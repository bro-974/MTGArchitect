using System.Text.Json;
using Microsoft.Extensions.Logging;
using MTGArchitect.Ingestor.Models;
using MTGArchitect.RAG.Data.Models;
using MTGArchitect.RAG.Data.Repositories;

namespace MTGArchitect.Ingestor.Services;

public sealed class CardIngestionService(
    ICardEmbeddingRepository repository,
    EmbeddingService embeddings,
    ILogger<CardIngestionService> logger)
{
    private const int BatchSize = 50;

    public async Task IngestAsync(string mtgJsonFilePath, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting ingestion from {Path}", mtgJsonFilePath);

        await using var stream = File.OpenRead(mtgJsonFilePath);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        // MTGJson AllPrintings format: root.data is an object keyed by set code
        // each set has a "cards" array
        if (!document.RootElement.TryGetProperty("data", out var setsElement))
        {
            logger.LogError("Expected root.data in MTGJson file");
            return;
        }

        var buffer = new List<MtgJsonCard>(BatchSize);
        var totalIngested = 0;

        foreach (var setEntry in setsElement.EnumerateObject())
        {
            if (!setEntry.Value.TryGetProperty("cards", out var cardsElement))
                continue;

            foreach (var cardElement in cardsElement.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var card = ParseCard(cardElement);
                if (card is null) continue;

                buffer.Add(card);

                if (buffer.Count >= BatchSize)
                {
                    await FlushBatchAsync(buffer, cancellationToken);
                    totalIngested += buffer.Count;
                    logger.LogInformation("Ingested {Count} cards so far", totalIngested);
                    buffer.Clear();
                }
            }
        }

        if (buffer.Count > 0)
        {
            await FlushBatchAsync(buffer, cancellationToken);
            totalIngested += buffer.Count;
        }

        logger.LogInformation("Ingestion complete. Total cards ingested: {Total}", totalIngested);
    }

    private async Task FlushBatchAsync(List<MtgJsonCard> batch, CancellationToken cancellationToken)
    {
        var texts = batch.Select(c => c.Text ?? c.Name ?? string.Empty).ToList();
        var vectors = await embeddings.GetEmbeddingsAsync(texts, cancellationToken);

        var entities = batch
            .Select((card, i) => new CardEmbedding
            {
                ScryfallId = Guid.TryParse(card.ScryfallId, out var guid) ? guid : Guid.NewGuid(),
                Name = card.Name ?? string.Empty,
                OracleText = card.Text ?? string.Empty,
                TypeLine = card.Type ?? string.Empty,
                Colors = card.Colors?.ToArray() ?? [],
                ManaValue = card.ManaValue ?? card.ConvertedManaCost ?? 0f,
                Embedding = i < vectors.Count ? vectors[i] : null,
            })
            .ToList();

        await repository.UpsertBatchAsync(entities, cancellationToken);
    }

    private static MtgJsonCard? ParseCard(JsonElement element)
    {
        // Skip tokens, planes, schemes — only process cards with a ScryfallId
        if (!element.TryGetProperty("identifiers", out var ids))
            return null;

        if (!ids.TryGetProperty("scryfallId", out var scryfallIdEl))
            return null;

        return new MtgJsonCard
        {
            ScryfallId = scryfallIdEl.GetString(),
            Name = element.TryGetProperty("name", out var n) ? n.GetString() : null,
            Text = element.TryGetProperty("text", out var t) ? t.GetString() : null,
            Type = element.TryGetProperty("type", out var ty) ? ty.GetString() : null,
            Colors = element.TryGetProperty("colors", out var c)
                ? c.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
                : null,
            ManaValue = element.TryGetProperty("manaValue", out var mv) ? (float)mv.GetDouble() : null,
            ConvertedManaCost = element.TryGetProperty("convertedManaCost", out var cmc) ? (float)cmc.GetDouble() : null,
        };
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MTGArchitect.RAG.Data.Models;
using MTGArchitect.RAG.Data.Repositories;

namespace MTGArchitect.RAG.Data.Services;

public sealed class CardSearchService(
    ICardEmbeddingRepository repository,
    EmbeddingService embeddingService,
    IConfiguration configuration,
    ILogger<CardSearchService> logger) : ICardSearchService
{
    private float Threshold =>
        float.TryParse(configuration["AiService:RagSimilarityThreshold"], out var t) ? t : 1.0f;

    public async Task<List<CardEmbedding>> GetSimilarCardsAsync(string userQuery, int limit = 5, CancellationToken ct = default)
    {
        // 1. Lancer la recherche texte ET l'embedding en parallèle
        var textTask = repository.SearchByTextAsync(userQuery, limit * 2, ct);
        var vectorTask = embeddingService.GetEmbeddingAsync(userQuery, ct);

        await Task.WhenAll(textTask, vectorTask);

        var textResults = await textTask;
        var vector = await vectorTask;
        List<CardEmbedding> vectorResults = [];

        if (vector != null)
        {
            vectorResults = await repository.SearchByVectorAsync(vector, limit * 2, Threshold, ct);
        }

        // 2. Fusion (simplified RRF): text results first, supplemented by vector
        var combined = textResults
            .Concat(vectorResults)
            .DistinctBy(x => x.Name)
            .ToList();

        // Exact name match always wins position 0
        var exactMatch = combined.FirstOrDefault(x =>
            string.Equals(x.Name, userQuery, StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            combined.Remove(exactMatch);
            combined.Insert(0, exactMatch);
        }

        return combined.Take(limit).ToList();
    }
}

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
        var vectorResults = new List<CardEmbedding>();

        if (vector != null)
        {
            vectorResults = await repository.SearchByVectorAsync(vector, limit * 2, Threshold, ct);
        }

        // 2. Fusion et Intersection (Reciprocal Rank Fusion simplifiée)
        // On donne la priorité au texte, mais on complète avec le vecteur
        var combined = textResults
            .Concat(vectorResults)
            .DistinctBy(x => x.Name)
            .Take(limit)
            .ToList();

        return combined;
    }
}

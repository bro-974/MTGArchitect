using MTGArchitect.RAG.Data.Models;

namespace MTGArchitect.RAG.Data.Services;

public interface ICardSearchService
{
    Task<List<CardEmbedding>> GetSimilarCardsAsync(string userQuery, int limit = 5, CancellationToken ct = default);
}

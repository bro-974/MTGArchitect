using MTGArchitect.RAG.Data.Models;
using Pgvector;

namespace MTGArchitect.RAG.Data.Repositories;

public interface ICardEmbeddingRepository
{
    Task UpsertBatchAsync(IEnumerable<CardEmbedding> cards, CancellationToken cancellationToken = default);
    Task<List<CardEmbedding>> SearchByVectorAsync(Vector queryVector, int topK = 10, float maxDistance = float.MaxValue, CancellationToken cancellationToken = default);
    Task<List<CardEmbedding>> SearchByTextAsync(string query, int topK = 10, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MTGArchitect.RAG.Data.Data;
using MTGArchitect.RAG.Data.Models;
using NpgsqlTypes;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace MTGArchitect.RAG.Data.Repositories;

public class CardEmbeddingRepository(RagDbContext db, ILogger<CardEmbeddingRepository> logger) : ICardEmbeddingRepository
{
    public async Task UpsertBatchAsync(IEnumerable<CardEmbedding> cards, CancellationToken cancellationToken = default)
    {
        foreach (var card in cards)
        {
            try
            {
                var exists = await db.CardEmbeddings
                    .AnyAsync(x => x.ScryfallId == card.ScryfallId, cancellationToken);

                if (exists)
                    db.CardEmbeddings.Update(card);
                else
                {
                    try
                    {
                        await db.CardEmbeddings.AddAsync(card, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to add card {ScryfallId} — skipping", card.ScryfallId);
                    }
                    
                }

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to upsert card {ScryfallId} — skipping", card.ScryfallId);
                db.ChangeTracker.Clear();
            }
        }
    }

    public async Task<List<CardEmbedding>> SearchByVectorAsync(Vector queryVector, int topK = 10, CancellationToken cancellationToken = default)
    {
        return await db.CardEmbeddings
            .OrderBy(x => x.Embedding!.L2Distance(queryVector))
            .Take(topK)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CardEmbedding>> SearchByTextAsync(string query, int topK = 10, CancellationToken cancellationToken = default)
    {
        var tsQuery = EF.Functions.ToTsQuery("english", query);

        return await db.CardEmbeddings
            .Where(x => x.SearchVector!.Matches(tsQuery))
            .OrderByDescending(x => x.SearchVector!.RankCoverDensity(tsQuery))
            .Take(topK)
            .ToListAsync(cancellationToken);
    }
}

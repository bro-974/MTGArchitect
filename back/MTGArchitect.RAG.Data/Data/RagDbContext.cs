using Microsoft.EntityFrameworkCore;
using MTGArchitect.RAG.Data.Models;

namespace MTGArchitect.RAG.Data.Data;

public class RagDbContext(DbContextOptions<RagDbContext> options) : DbContext(options)
{
    public DbSet<CardEmbedding> CardEmbeddings => Set<CardEmbedding>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("vector");
        builder.HasPostgresExtension("pg_trgm");

        builder.Entity<CardEmbedding>(entity =>
        {
            entity.HasKey(x => x.ScryfallId);

            entity.Property(x => x.Name).HasMaxLength(300).IsRequired();
            entity.Property(x => x.OracleText).HasColumnType("text").IsRequired();
            entity.Property(x => x.TypeLine).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Colors).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ManaValue).IsRequired();

            entity.Property(x => x.Embedding)
                .HasColumnType("vector(768)");

            entity.Property(x => x.SearchVector)
                .HasColumnType("tsvector")
                .IsGeneratedTsVectorColumn("english", nameof(CardEmbedding.Name), nameof(CardEmbedding.OracleText), nameof(CardEmbedding.TypeLine));

            entity.HasIndex(x => x.SearchVector)
                .HasMethod("GIN");
        });
    }
}

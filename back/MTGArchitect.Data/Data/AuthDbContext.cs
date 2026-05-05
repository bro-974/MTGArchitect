using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();
    public DbSet<QueryInfo> QueryInfos => Set<QueryInfo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Deck>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Note).HasColumnType("text");
            entity.Property(x => x.Commander).HasMaxLength(200);
            entity.Property(x => x.ColorIdentity).HasMaxLength(6);

            entity.HasOne(x => x.User)
                .WithMany(x => x.DeckWorkspace)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QueryInfo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Query).HasColumnType("text").IsRequired();
            entity.Property(x => x.SearchEngine).HasMaxLength(80).IsRequired();

            entity.HasOne(x => x.Deck)
                .WithMany(x => x.QuerySearches)
                .HasForeignKey(x => x.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DeckCard>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CardName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ScryFallId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Cost).HasMaxLength(50);
            entity.Property(x => x.IsSideBoard).HasDefaultValue(false).IsRequired();

            entity.HasOne(x => x.Deck)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

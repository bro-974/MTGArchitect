using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Deck>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Note).HasColumnType("text");
            entity.Property(x => x.QuerySearches).HasColumnType("text[]");

            entity.HasOne(x => x.User)
                .WithMany(x => x.DeckWorkspace)
                .HasForeignKey(x => x.UserId)
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

            entity.HasOne(x => x.Deck)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

namespace MTGArchitect.Data.Models;

public class DeckCard
{
    public Guid Id { get; set; }

    public string CardName { get; set; } = string.Empty;
    public string ScryFallId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Cost { get; set; }

    public Guid DeckId { get; set; }
    public Deck Deck { get; set; } = null!;
}

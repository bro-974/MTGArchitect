namespace MTGArchitect.Data.Models;

public class Deck
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Commander { get; set; }
    public string? ColorIdentity { get; set; }
    public ICollection<QueryInfo> QuerySearches { get; set; } = new List<QueryInfo>();

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<DeckCard> Cards { get; set; } = new List<DeckCard>();
}

namespace MTGArchitect.Data.Models;

public class QueryInfo
{
    public Guid Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public string SearchEngine { get; set; } = string.Empty;

    public Guid DeckId { get; set; }
    public Deck Deck { get; set; } = null!;
}

namespace MTGArchitect.AI.Contract;

public class DeckCardInput
{
    public DeckCardInput(string name, int quantity, bool isSideBoard, string? scryfallId = null)
    {
        Name = name;
        Quantity = quantity;
        IsSideBoard = isSideBoard;
        ScryfallId = scryfallId;
    }

    public string Name { get; }
    public int Quantity { get; }
    public bool IsSideBoard { get; }
    public string? ScryfallId { get; }
}

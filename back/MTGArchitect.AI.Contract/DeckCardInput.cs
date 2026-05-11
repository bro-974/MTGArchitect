namespace MTGArchitect.AI.Contract;

public class DeckCardInput
{
    public DeckCardInput(string name, int quantity, bool isSideBoard)
    {
        Name = name;
        Quantity = quantity;
        IsSideBoard = isSideBoard;
    }

    public string Name { get; }
    public int Quantity { get; }
    public bool IsSideBoard { get; }
}

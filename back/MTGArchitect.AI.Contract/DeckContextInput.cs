using System.Collections.Generic;

namespace MTGArchitect.AI.Contract;

public class DeckContextInput
{
    public DeckContextInput(string format, IEnumerable<DeckCardInput> cards)
    {
        Format = format;
        Cards = cards;
    }

    public string Format { get; }
    public IEnumerable<DeckCardInput> Cards { get; }
}

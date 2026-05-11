using MTGArchitect.AI.Contract;
using System.Text;

namespace MTGArchitect.AI.Client.Services;

public interface IDeckContextFormatter
{
    string Format(DeckContextInput deck);
}

public class DeckContextFormatter : IDeckContextFormatter
{
    public string Format(DeckContextInput deck)
    {
        var sb = new StringBuilder();
        foreach (var card in deck.Cards.Where(c => !c.IsSideBoard))
            sb.AppendLine($"{card.Quantity}x {card.Name}");
        var side = deck.Cards.Where(c => c.IsSideBoard).ToList();
        if (side.Count > 0)
        {
            sb.AppendLine("---Sideboard---");
            foreach (var card in side)
                sb.AppendLine($"{card.Quantity}x {card.Name}");
        }
        return sb.ToString().TrimEnd();
    }
}

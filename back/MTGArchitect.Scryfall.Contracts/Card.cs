using System;
using System.Collections.Generic;
using System.Text;

namespace MTGArchitect.Scryfall.Contracts
{
    public class Card
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ManaCost { get; set; } = string.Empty;
        public string TypeLine { get; set; } = string.Empty;
        public string SetCode { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CardSearchResult
    {
        public IReadOnlyList<Card> Cards { get; set; } = new List<Card>();
        public int TotalCount { get; set; }
    }
}

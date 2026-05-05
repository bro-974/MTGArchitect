using System;
using System.Collections.Generic;
using System.Text;

namespace MTGArchitect.Scryfall.Contracts
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ManaCost { get; set; }
        public string TypeLine { get; set; }
        public string SetCode { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CardSearchResult
    {
        public IReadOnlyList<Card> Cards { get; set; } = new List<Card>();
        public int TotalCount { get; set; }
    }
}

using System.Collections.Generic;

namespace MTGArchitect.Scryfall.Contracts
{
    public class CardDetail
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ManaCost { get; set; } = string.Empty;
        public float Cmc { get; set; }
        public string TypeLine { get; set; } = string.Empty;
        public string OracleText { get; set; } = string.Empty;
        public string Power { get; set; } = string.Empty;
        public string Toughness { get; set; } = string.Empty;
        public string Loyalty { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string SetCode { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public Dictionary<string, string> Legalities { get; set; } = new Dictionary<string, string>();
        public string FlavorText { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageLargeUrl { get; set; } = string.Empty;
        public List<CardPrinting> Printings { get; set; } = new List<CardPrinting>();
        public List<CardRuling> Rulings { get; set; } = new List<CardRuling>();
    }

    public class CardPrinting
    {
        public string Id { get; set; } = string.Empty;
        public string SetCode { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CardRuling
    {
        public string PublishedAt { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}

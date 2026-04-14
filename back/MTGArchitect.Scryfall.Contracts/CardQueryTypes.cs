namespace MTGArchitect.Scryfall.Contracts
{


/// <summary>Comparison operator used in numeric and range-based query filters.</summary>
public enum ComparisonOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

/// <summary>Card rarity values supported by Scryfall.</summary>
public enum CardRarity { Common, Uncommon, Rare, Special, Mythic, Bonus }

/// <summary>Game environments a card can exist in.</summary>
public enum CardGame { Paper, Mtgo, Arena }

/// <summary>Formats supported by Scryfall for legality queries.</summary>
public enum CardFormat
{
    Standard, Future, Historic, Timeless, Gladiator,
    Pioneer, Modern, Legacy, Pauper, Vintage, Penny,
    Commander, Oathbreaker, StandardBrawl, Brawl, Alchemy,
    PauperCommander, Duel, OldSchool, Premodern, Predh
}

/// <summary>Controls how duplicate cards are collapsed in results.</summary>
public enum UniqueStrategy { Cards, Prints, Art }

/// <summary>Sort order for search results.</summary>
public enum SortOrder
{
    Name, Set, Released, Rarity, Color,
    Usd, Tix, Eur, Cmc, Power, Toughness,
    EdhRec, Penny, Artist, Spoiled, Review
}

/// <summary>Sort direction for search results.</summary>
public enum SortDirection { Asc, Desc }
}

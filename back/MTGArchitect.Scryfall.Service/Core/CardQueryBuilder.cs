using MTGArchitect.Scryfall.Contracts;

namespace MTGArchitect.Scryfall.Service.Core;

/// <summary>
/// Fluent builder that produces a Scryfall-compatible query string.
/// Each method appends one or more query tokens; call <see cref="Build"/> to get the final string.
/// doc: https://scryfall.com/docs/syntax
/// </summary>
public class CardQueryBuilder
{
    private readonly List<string> _parts = [];

    // ── Name ──────────────────────────────────────────────────────────────────

    /// <summary>Adds a name search token. Pass <paramref name="exact"/> = true to use the ! prefix.</summary>
    public CardQueryBuilder WithName(string name, bool exact = false)
    {
        var sanitized = Quoted(name);
        _parts.Add(exact ? $"!{sanitized}" : sanitized);
        return this;
    }

    // ── Colors ────────────────────────────────────────────────────────────────

    /// <summary>Filters by card color (c:). Supports letters, names, and guild/shard aliases.</summary>
    public CardQueryBuilder WithColor(string color, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"c{Op(op)}{color}");
        return this;
    }

    /// <summary>Filters by color identity (id:), used for Commander deck building.</summary>
    public CardQueryBuilder WithColorIdentity(string colorIdentity, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"id{Op(op)}{colorIdentity}");
        return this;
    }

    // ── Types ─────────────────────────────────────────────────────────────────

    /// <summary>Adds a type filter (t:). Set <paramref name="exclude"/> to prefix with - (negation).</summary>
    public CardQueryBuilder WithType(string type, bool exclude = false)
    {
        _parts.Add($"{(exclude ? "-" : "")}t:{type}");
        return this;
    }

    // ── Card Text ─────────────────────────────────────────────────────────────

    /// <summary>Searches current Oracle text (o:).</summary>
    public CardQueryBuilder WithOracleText(string text)
    {
        _parts.Add($"o:{Quoted(text)}");
        return this;
    }

    /// <summary>Searches full Oracle text including reminder text (fo:).</summary>
    public CardQueryBuilder WithFullOracleText(string text)
    {
        _parts.Add($"fo:{Quoted(text)}");
        return this;
    }

    /// <summary>Searches for a keyword ability (kw:).</summary>
    public CardQueryBuilder WithKeyword(string keyword)
    {
        _parts.Add($"kw:{keyword}");
        return this;
    }

    // ── Mana ──────────────────────────────────────────────────────────────────

    /// <summary>Filters by mana cost in official text format, e.g. "2WW" or "{2}{G}" (m:).</summary>
    public CardQueryBuilder WithManaCost(string manaCost, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"m{Op(op)}{manaCost}");
        return this;
    }

    /// <summary>Filters by mana value / converted mana cost (mv:).</summary>
    public CardQueryBuilder WithManaValue(int value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"mv{Op(op)}{value}");
        return this;
    }

    /// <summary>Filters cards that produce the given mana colors, e.g. "wu" (produces:).</summary>
    public CardQueryBuilder WithProduces(string colors, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"produces{Op(op)}{colors}");
        return this;
    }

    // ── Power / Toughness / Loyalty ───────────────────────────────────────────

    /// <summary>Filters by power (pow:). Value can be a number or "tou" to compare against toughness.</summary>
    public CardQueryBuilder WithPower(string value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"pow{Op(op)}{value}");
        return this;
    }

    /// <summary>Filters by toughness (tou:). Value can be a number or "pow".</summary>
    public CardQueryBuilder WithToughness(string value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"tou{Op(op)}{value}");
        return this;
    }

    /// <summary>Filters planeswalkers by starting loyalty (loy:).</summary>
    public CardQueryBuilder WithLoyalty(int value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"loy{Op(op)}{value}");
        return this;
    }

    // ── Rarity ────────────────────────────────────────────────────────────────

    /// <summary>Filters by rarity (r:).</summary>
    public CardQueryBuilder WithRarity(CardRarity rarity, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"r{Op(op)}{RarityToString(rarity)}");
        return this;
    }

    // ── Sets & Blocks ─────────────────────────────────────────────────────────

    /// <summary>Filters by set code, e.g. "war" (s:/e:).</summary>
    public CardQueryBuilder WithSet(string setCode)
    {
        _parts.Add($"s:{setCode}");
        return this;
    }

    /// <summary>Filters by block using any set code in the block (b:).</summary>
    public CardQueryBuilder WithBlock(string blockCode)
    {
        _parts.Add($"b:{blockCode}");
        return this;
    }

    /// <summary>Filters by collector number within a set (cn:).</summary>
    public CardQueryBuilder WithCollectorNumber(string number)
    {
        _parts.Add($"cn:{number}");
        return this;
    }

    // ── Format Legality ───────────────────────────────────────────────────────

    /// <summary>Filters cards legal in a format (f:).</summary>
    public CardQueryBuilder WithFormat(CardFormat format)
    {
        _parts.Add($"f:{FormatToString(format)}");
        return this;
    }

    /// <summary>Filters cards banned in a format (banned:).</summary>
    public CardQueryBuilder WithBannedIn(CardFormat format)
    {
        _parts.Add($"banned:{FormatToString(format)}");
        return this;
    }

    /// <summary>Filters cards restricted in a format (restricted:).</summary>
    public CardQueryBuilder WithRestrictedIn(CardFormat format)
    {
        _parts.Add($"restricted:{FormatToString(format)}");
        return this;
    }

    // ── Prices ────────────────────────────────────────────────────────────────

    /// <summary>Filters by USD price (usd:).</summary>
    public CardQueryBuilder WithUsdPrice(decimal value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"usd{Op(op)}{value:F2}");
        return this;
    }

    /// <summary>Filters by EUR price (eur:).</summary>
    public CardQueryBuilder WithEurPrice(decimal value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"eur{Op(op)}{value:F2}");
        return this;
    }

    /// <summary>Filters by MTGO TIX price (tix:).</summary>
    public CardQueryBuilder WithTixPrice(decimal value, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"tix{Op(op)}{value:F2}");
        return this;
    }

    // ── Artist / Flavor / Watermark ───────────────────────────────────────────

    /// <summary>Filters by artist name or partial name (a:).</summary>
    public CardQueryBuilder WithArtist(string artist)
    {
        _parts.Add($"a:{Quoted(artist)}");
        return this;
    }

    /// <summary>Searches flavor text for a word or phrase (ft:).</summary>
    public CardQueryBuilder WithFlavorText(string text)
    {
        _parts.Add($"ft:{Quoted(text)}");
        return this;
    }

    /// <summary>Filters by watermark guild/faction name (wm:).</summary>
    public CardQueryBuilder WithWatermark(string watermark)
    {
        _parts.Add($"wm:{watermark}");
        return this;
    }

    // ── Border / Frame ────────────────────────────────────────────────────────

    /// <summary>Filters by border color: "black", "white", "silver", or "borderless" (border:).</summary>
    public CardQueryBuilder WithBorder(string border)
    {
        _parts.Add($"border:{border}");
        return this;
    }

    /// <summary>Filters by frame edition or effect, e.g. "2003", "legendary", "colorshifted" (frame:).</summary>
    public CardQueryBuilder WithFrame(string frame)
    {
        _parts.Add($"frame:{frame}");
        return this;
    }

    // ── Games ─────────────────────────────────────────────────────────────────

    /// <summary>Filters cards available in a specific game environment (game:).</summary>
    public CardQueryBuilder WithGame(CardGame game)
    {
        _parts.Add($"game:{GameToString(game)}");
        return this;
    }

    // ── Year / Date ───────────────────────────────────────────────────────────

    /// <summary>Filters by release year (year:).</summary>
    public CardQueryBuilder WithYear(int year, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"year{Op(op)}{year}");
        return this;
    }

    /// <summary>
    /// Filters by release date (date:). Accepts yyyy-mm-dd, set codes, or "now"/"today".
    /// </summary>
    public CardQueryBuilder WithDate(string date, ComparisonOperator op = ComparisonOperator.Equal)
    {
        _parts.Add($"date{Op(op)}{date}");
        return this;
    }

    // ── Language ──────────────────────────────────────────────────────────────

    /// <summary>Filters by language code or name, e.g. "japanese", "ko". Use "any" for all (lang:).</summary>
    public CardQueryBuilder WithLanguage(string language)
    {
        _parts.Add($"lang:{language}");
        return this;
    }

    // ── Tagger Tags ───────────────────────────────────────────────────────────

    /// <summary>Filters by art/illustration tag from Scryfall Tagger (art:).</summary>
    public CardQueryBuilder WithArtTag(string tag)
    {
        _parts.Add($"art:{tag}");
        return this;
    }

    /// <summary>Filters by Oracle function tag from Scryfall Tagger (function:).</summary>
    public CardQueryBuilder WithOracleTag(string tag)
    {
        _parts.Add($"function:{tag}");
        return this;
    }

    // ── Is / Not Flags ────────────────────────────────────────────────────────

    /// <summary>
    /// Appends an is: flag. Set <paramref name="positive"/> = false to negate it (-is:).
    /// Use this for flags not covered by named helpers, e.g. WithIs("bear").
    /// </summary>
    public CardQueryBuilder WithIs(string flag, bool positive = true)
    {
        _parts.Add($"{(positive ? "" : "-")}is:{flag}");
        return this;
    }

    // ── Display Options ───────────────────────────────────────────────────────

    /// <summary>Controls how duplicate results are collapsed (unique:).</summary>
    public CardQueryBuilder WithUnique(UniqueStrategy strategy)
    {
        _parts.Add($"unique:{UniqueToString(strategy)}");
        return this;
    }

    /// <summary>Sets the sort field for results (order:).</summary>
    public CardQueryBuilder WithOrder(SortOrder order)
    {
        _parts.Add($"order:{OrderToString(order)}");
        return this;
    }

    /// <summary>Sets the sort direction (direction:).</summary>
    public CardQueryBuilder WithDirection(SortDirection direction)
    {
        _parts.Add($"direction:{(direction == SortDirection.Asc ? "asc" : "desc")}");
        return this;
    }

    /// <summary>
    /// Sets the preferred printing to show (prefer:), e.g. "newest", "oldest",
    /// "usd-low", "promo", "universesbeyond", "atypical".
    /// </summary>
    public CardQueryBuilder WithPrefer(string prefer)
    {
        _parts.Add($"prefer:{prefer}");
        return this;
    }

    // ── SetFromQuery ──────────────────────────────────────────────────────────

    /// <summary>
    /// Populates the builder from a <see cref="CardQuerySearch"/> object,
    /// translating every non-null property into the corresponding fluent call.
    /// </summary>
    public CardQueryBuilder SetFromQuery(CardQuerySearch querySearch)
    {
        if (!string.IsNullOrWhiteSpace(querySearch.Name))
            WithName(querySearch.Name, querySearch.ExactName);

        if (!string.IsNullOrWhiteSpace(querySearch.Color))
            WithColor(querySearch.Color, querySearch.ColorOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.ColorIdentity))
            WithColorIdentity(querySearch.ColorIdentity, querySearch.ColorIdentityOperator);

        foreach (var type in querySearch.Types)
            WithType(type);

        foreach (var type in querySearch.ExcludedTypes)
            WithType(type, exclude: true);

        if (!string.IsNullOrWhiteSpace(querySearch.OracleText))
            WithOracleText(querySearch.OracleText);

        if (!string.IsNullOrWhiteSpace(querySearch.FullOracleText))
            WithFullOracleText(querySearch.FullOracleText);

        if (!string.IsNullOrWhiteSpace(querySearch.Keyword))
            WithKeyword(querySearch.Keyword);

        if (!string.IsNullOrWhiteSpace(querySearch.ManaCost))
            WithManaCost(querySearch.ManaCost);

        if (querySearch.ManaValue.HasValue)
            WithManaValue(querySearch.ManaValue.Value, querySearch.ManaValueOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.Produces))
            WithProduces(querySearch.Produces);

        if (!string.IsNullOrWhiteSpace(querySearch.Power))
            WithPower(querySearch.Power, querySearch.PowerOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.Toughness))
            WithToughness(querySearch.Toughness, querySearch.ToughnessOperator);

        if (querySearch.Loyalty.HasValue)
            WithLoyalty(querySearch.Loyalty.Value, querySearch.LoyaltyOperator);

        if (querySearch.Rarity.HasValue)
            WithRarity(querySearch.Rarity.Value, querySearch.RarityOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.SetCode))
            WithSet(querySearch.SetCode);

        if (!string.IsNullOrWhiteSpace(querySearch.Block))
            WithBlock(querySearch.Block);

        if (!string.IsNullOrWhiteSpace(querySearch.CollectorNumber))
            WithCollectorNumber(querySearch.CollectorNumber);

        if (querySearch.Format.HasValue)
            WithFormat(querySearch.Format.Value);

        if (querySearch.BannedIn.HasValue)
            WithBannedIn(querySearch.BannedIn.Value);

        if (querySearch.RestrictedIn.HasValue)
            WithRestrictedIn(querySearch.RestrictedIn.Value);

        if (querySearch.UsdPrice.HasValue)
            WithUsdPrice(querySearch.UsdPrice.Value, querySearch.UsdPriceOperator);

        if (querySearch.EurPrice.HasValue)
            WithEurPrice(querySearch.EurPrice.Value, querySearch.EurPriceOperator);

        if (querySearch.TixPrice.HasValue)
            WithTixPrice(querySearch.TixPrice.Value, querySearch.TixPriceOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.Artist))
            WithArtist(querySearch.Artist);

        if (!string.IsNullOrWhiteSpace(querySearch.FlavorText))
            WithFlavorText(querySearch.FlavorText);

        if (!string.IsNullOrWhiteSpace(querySearch.Watermark))
            WithWatermark(querySearch.Watermark);

        if (!string.IsNullOrWhiteSpace(querySearch.Border))
            WithBorder(querySearch.Border);

        if (!string.IsNullOrWhiteSpace(querySearch.Frame))
            WithFrame(querySearch.Frame);

        if (querySearch.Game.HasValue)
            WithGame(querySearch.Game.Value);

        if (querySearch.Year.HasValue)
            WithYear(querySearch.Year.Value, querySearch.YearOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.Date))
            WithDate(querySearch.Date, querySearch.DateOperator);

        if (!string.IsNullOrWhiteSpace(querySearch.Language))
            WithLanguage(querySearch.Language);

        if (!string.IsNullOrWhiteSpace(querySearch.ArtTag))
            WithArtTag(querySearch.ArtTag);

        if (!string.IsNullOrWhiteSpace(querySearch.OracleTag))
            WithOracleTag(querySearch.OracleTag);

        ApplyIsFlag("reprint", querySearch.IsReprint);
        ApplyIsFlag("foil", querySearch.IsFoil);
        ApplyIsFlag("nonfoil", querySearch.IsNonFoil);
        ApplyIsFlag("full", querySearch.IsFullArt);
        ApplyIsFlag("promo", querySearch.IsPromo);
        ApplyIsFlag("digital", querySearch.IsDigital);
        ApplyIsFlag("funny", querySearch.IsFunny);
        ApplyIsFlag("reserved", querySearch.IsReserved);
        ApplyIsFlag("commander", querySearch.IsCommander);
        ApplyIsFlag("partner", querySearch.IsPartner);
        ApplyIsFlag("hires", querySearch.IsHires);
        ApplyIsFlag("spell", querySearch.IsSpell);
        ApplyIsFlag("permanent", querySearch.IsPermanent);
        ApplyIsFlag("split", querySearch.IsSplit);
        ApplyIsFlag("transform", querySearch.IsTransform);
        ApplyIsFlag("mdfc", querySearch.IsMdfc);
        ApplyIsFlag("fetchland", querySearch.IsFetchland);
        ApplyIsFlag("shockland", querySearch.IsShockland);
        ApplyIsFlag("dual", querySearch.IsDualLand);

        if (querySearch.Unique.HasValue)
            WithUnique(querySearch.Unique.Value);

        if (querySearch.Order.HasValue)
            WithOrder(querySearch.Order.Value);

        if (querySearch.Direction.HasValue)
            WithDirection(querySearch.Direction.Value);

        if (!string.IsNullOrWhiteSpace(querySearch.Prefer))
            WithPrefer(querySearch.Prefer);

        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    /// <summary>Returns the assembled Scryfall query string.</summary>
    public string Build() => string.Join(" ", _parts);

    // ── Private helpers ───────────────────────────────────────────────────────

    private void ApplyIsFlag(string flag, bool? value)
    {
        if (value.HasValue)
            WithIs(flag, value.Value);
    }

    /// <summary>Wraps the value in quotes if it contains spaces or punctuation.</summary>
    private static string Quoted(string value) =>
        value.IndexOfAny([' ', ',', '\'']) >= 0 ? $"\"{value}\"" : value;

    private static string Op(ComparisonOperator op) => op switch
    {
        ComparisonOperator.Equal              => ":",
        ComparisonOperator.NotEqual           => "!=",
        ComparisonOperator.GreaterThan        => ">",
        ComparisonOperator.GreaterThanOrEqual => ">=",
        ComparisonOperator.LessThan           => "<",
        ComparisonOperator.LessThanOrEqual    => "<=",
        _                                     => ":"
    };

    private static string RarityToString(CardRarity rarity) => rarity switch
    {
        CardRarity.Common   => "common",
        CardRarity.Uncommon => "uncommon",
        CardRarity.Rare     => "rare",
        CardRarity.Special  => "special",
        CardRarity.Mythic   => "mythic",
        CardRarity.Bonus    => "bonus",
        _                   => "common"
    };

    private static string FormatToString(CardFormat format) => format switch
    {
        CardFormat.Standard        => "standard",
        CardFormat.Future          => "future",
        CardFormat.Historic        => "historic",
        CardFormat.Timeless        => "timeless",
        CardFormat.Gladiator       => "gladiator",
        CardFormat.Pioneer         => "pioneer",
        CardFormat.Modern          => "modern",
        CardFormat.Legacy          => "legacy",
        CardFormat.Pauper          => "pauper",
        CardFormat.Vintage         => "vintage",
        CardFormat.Penny           => "penny",
        CardFormat.Commander       => "commander",
        CardFormat.Oathbreaker     => "oathbreaker",
        CardFormat.StandardBrawl   => "standardbrawl",
        CardFormat.Brawl           => "brawl",
        CardFormat.Alchemy         => "alchemy",
        CardFormat.PauperCommander => "paupercommander",
        CardFormat.Duel            => "duel",
        CardFormat.OldSchool       => "oldschool",
        CardFormat.Premodern       => "premodern",
        CardFormat.Predh           => "predh",
        _                          => "standard"
    };

    private static string GameToString(CardGame game) => game switch
    {
        CardGame.Paper => "paper",
        CardGame.Mtgo  => "mtgo",
        CardGame.Arena => "arena",
        _              => "paper"
    };

    private static string UniqueToString(UniqueStrategy strategy) => strategy switch
    {
        UniqueStrategy.Cards  => "cards",
        UniqueStrategy.Prints => "prints",
        UniqueStrategy.Art    => "art",
        _                     => "cards"
    };

    private static string OrderToString(SortOrder order) => order switch
    {
        SortOrder.Name      => "name",
        SortOrder.Set       => "set",
        SortOrder.Released  => "released",
        SortOrder.Rarity    => "rarity",
        SortOrder.Color     => "color",
        SortOrder.Usd       => "usd",
        SortOrder.Tix       => "tix",
        SortOrder.Eur       => "eur",
        SortOrder.Cmc       => "cmc",
        SortOrder.Power     => "power",
        SortOrder.Toughness => "toughness",
        SortOrder.EdhRec    => "edhrec",
        SortOrder.Penny     => "penny",
        SortOrder.Artist    => "artist",
        SortOrder.Spoiled   => "spoiled",
        SortOrder.Review    => "review",
        _                   => "name"
    };
}


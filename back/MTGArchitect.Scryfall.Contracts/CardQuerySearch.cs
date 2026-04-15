using System.Collections.Generic;

namespace MTGArchitect.Scryfall.Contracts
{


    /// <summary>
    /// Structured input model for a Scryfall card search.
    /// All properties are optional; only non-null values are included in the query.
    /// doc: https://scryfall.com/docs/syntax
    /// </summary>
    public class CardQuerySearch
    {
        // ── Name ─────────────────────────────────────────────────────────────────

        /// <summary>Word(s) to search in the card name.</summary>
        public string? Name { get; set; }

        /// <summary>When true, <see cref="Name"/> must match the full card name exactly (uses ! prefix).</summary>
        public bool ExactName { get; set; }

        // ── Colors ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Color filter using single letters (w u b r g), color names, or guild/shard nicknames.
        /// Use "c" for colorless, "m" for multicolor.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>Comparison operator applied to <see cref="Color"/>. Defaults to <see cref="ComparisonOperator.Equal"/> (contains).</summary>
        public ComparisonOperator ColorOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>Color identity filter, same syntax as <see cref="Color"/>.</summary>
        public string? ColorIdentity { get; set; }

        /// <summary>Comparison operator applied to <see cref="ColorIdentity"/>.</summary>
        public ComparisonOperator ColorIdentityOperator { get; set; } = ComparisonOperator.Equal;

        // ── Types ─────────────────────────────────────────────────────────────────

        /// <summary>Card supertypes, types, or subtypes to include (each adds a t: clause).</summary>
        public IList<string> Types { get; set; } = new List<string>();

        /// <summary>Card types to explicitly exclude (each adds a -t: clause).</summary>
        public IList<string> ExcludedTypes { get; set; } = new List<string>();

        // ── Card Text ─────────────────────────────────────────────────────────────

        /// <summary>Phrase or word to search in the current Oracle text (o:).</summary>
        public string? OracleText { get; set; }

        /// <summary>Phrase to search in the full Oracle text including reminder text (fo:).</summary>
        public string? FullOracleText { get; set; }

        /// <summary>Keyword ability to search for (kw:).</summary>
        public string? Keyword { get; set; }

        // ── Mana ──────────────────────────────────────────────────────────────────

        /// <summary>Mana cost in official text format, e.g. "{2}{G}" or "2WW" (m:).</summary>
        public string? ManaCost { get; set; }

        /// <summary>Mana value (converted mana cost) to filter on (mv:).</summary>
        public int? ManaValue { get; set; }

        /// <summary>Comparison operator for <see cref="ManaValue"/>.</summary>
        public ComparisonOperator ManaValueOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>Colors of mana the card produces, e.g. "wu" (produces:).</summary>
        public string? Produces { get; set; }

        // ── Power / Toughness / Loyalty ───────────────────────────────────────────

        /// <summary>
        /// Power filter value. Accepts a number or "tou" to compare against toughness.
        /// </summary>
        public string? Power { get; set; }

        /// <summary>Comparison operator for <see cref="Power"/>.</summary>
        public ComparisonOperator PowerOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>
        /// Toughness filter value. Accepts a number or "pow" to compare against power.
        /// </summary>
        public string? Toughness { get; set; }

        /// <summary>Comparison operator for <see cref="Toughness"/>.</summary>
        public ComparisonOperator ToughnessOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>Starting loyalty value for planeswalkers (loy:).</summary>
        public int? Loyalty { get; set; }

        /// <summary>Comparison operator for <see cref="Loyalty"/>.</summary>
        public ComparisonOperator LoyaltyOperator { get; set; } = ComparisonOperator.Equal;

        // ── Rarity ────────────────────────────────────────────────────────────────

        /// <summary>Rarity to filter on (r:).</summary>
        public CardRarity? Rarity { get; set; }

        /// <summary>Comparison operator for <see cref="Rarity"/>.</summary>
        public ComparisonOperator RarityOperator { get; set; } = ComparisonOperator.Equal;

        // ── Sets & Blocks ─────────────────────────────────────────────────────────

        /// <summary>Set code to filter on, e.g. "war" for War of the Spark (s:/e:).</summary>
        public string? SetCode { get; set; }

        /// <summary>Block code; any set code within the block is accepted (b:).</summary>
        public string? Block { get; set; }

        /// <summary>Collector number within a set, supports ranges like ">50" (cn:).</summary>
        public string? CollectorNumber { get; set; }

        // ── Format Legality ───────────────────────────────────────────────────────

        /// <summary>Format the card must be legal in (f:).</summary>
        public CardFormat? Format { get; set; }

        /// <summary>Format in which the card is banned (banned:).</summary>
        public CardFormat? BannedIn { get; set; }

        /// <summary>Format in which the card is restricted (restricted:).</summary>
        public CardFormat? RestrictedIn { get; set; }

        // ── Prices ────────────────────────────────────────────────────────────────

        /// <summary>USD price filter (usd:).</summary>
        public decimal? UsdPrice { get; set; }

        /// <summary>Comparison operator for <see cref="UsdPrice"/>.</summary>
        public ComparisonOperator UsdPriceOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>EUR price filter (eur:).</summary>
        public decimal? EurPrice { get; set; }

        /// <summary>Comparison operator for <see cref="EurPrice"/>.</summary>
        public ComparisonOperator EurPriceOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>MTGO TIX price filter (tix:).</summary>
        public decimal? TixPrice { get; set; }

        /// <summary>Comparison operator for <see cref="TixPrice"/>.</summary>
        public ComparisonOperator TixPriceOperator { get; set; } = ComparisonOperator.Equal;

        // ── Artist / Flavor / Watermark ───────────────────────────────────────────

        /// <summary>Artist name or partial name (a:).</summary>
        public string? Artist { get; set; }

        /// <summary>Word or phrase in the flavor text (ft:).</summary>
        public string? FlavorText { get; set; }

        /// <summary>Guild or faction watermark, e.g. "orzhov" (wm:).</summary>
        public string? Watermark { get; set; }

        // ── Border / Frame ────────────────────────────────────────────────────────

        /// <summary>Border color: "black", "white", "silver", or "borderless" (border:).</summary>
        public string? Border { get; set; }

        /// <summary>Frame edition or effect, e.g. "2003", "legendary", "colorshifted" (frame:).</summary>
        public string? Frame { get; set; }

        // ── Games ─────────────────────────────────────────────────────────────────

        /// <summary>Game environment the card must be available in (game:).</summary>
        public CardGame? Game { get; set; }

        // ── Year / Date ───────────────────────────────────────────────────────────

        /// <summary>Release year filter (year:).</summary>
        public int? Year { get; set; }

        /// <summary>Comparison operator for <see cref="Year"/>.</summary>
        public ComparisonOperator YearOperator { get; set; } = ComparisonOperator.Equal;

        /// <summary>
        /// Release date filter in yyyy-mm-dd format, or a set code to stand for its release date (date:).
        /// Use "now" or "today" for the current date.
        /// </summary>
        public string? Date { get; set; }

        /// <summary>Comparison operator for <see cref="Date"/>.</summary>
        public ComparisonOperator DateOperator { get; set; } = ComparisonOperator.Equal;

        // ── Language ──────────────────────────────────────────────────────────────

        /// <summary>Language code or name, e.g. "japanese", "ko", "zhs". Use "any" for all languages (lang:).</summary>
        public string? Language { get; set; }

        // ── Tagger Tags ───────────────────────────────────────────────────────────

        /// <summary>Tag describing card illustration content, e.g. "squirrel" (art:).</summary>
        public string? ArtTag { get; set; }

        /// <summary>Oracle function tag describing card role, e.g. "removal" (function:).</summary>
        public string? OracleTag { get; set; }

        // ── Is / Not Flags ────────────────────────────────────────────────────────

        /// <summary>true → is:reprint, false → -is:reprint</summary>
        public bool? IsReprint { get; set; }

        /// <summary>true → is:foil</summary>
        public bool? IsFoil { get; set; }

        /// <summary>true → is:nonfoil</summary>
        public bool? IsNonFoil { get; set; }

        /// <summary>true → is:full (full-art cards)</summary>
        public bool? IsFullArt { get; set; }

        /// <summary>true → is:promo</summary>
        public bool? IsPromo { get; set; }

        /// <summary>true → is:digital</summary>
        public bool? IsDigital { get; set; }

        /// <summary>true → is:funny (Un-sets and holiday cards)</summary>
        public bool? IsFunny { get; set; }

        /// <summary>true → is:reserved (Reserved List)</summary>
        public bool? IsReserved { get; set; }

        /// <summary>true → is:commander (can be your commander)</summary>
        public bool? IsCommander { get; set; }

        /// <summary>true → is:partner</summary>
        public bool? IsPartner { get; set; }

        /// <summary>true → is:hires</summary>
        public bool? IsHires { get; set; }

        /// <summary>true → is:spell</summary>
        public bool? IsSpell { get; set; }

        /// <summary>true → is:permanent</summary>
        public bool? IsPermanent { get; set; }

        /// <summary>true → is:split, false → -is:split</summary>
        public bool? IsSplit { get; set; }

        /// <summary>true → is:transform</summary>
        public bool? IsTransform { get; set; }

        /// <summary>true → is:mdfc (modal double-faced)</summary>
        public bool? IsMdfc { get; set; }

        /// <summary>true → is:fetchland</summary>
        public bool? IsFetchland { get; set; }

        /// <summary>true → is:shockland</summary>
        public bool? IsShockland { get; set; }

        /// <summary>true → is:dual (original dual lands)</summary>
        public bool? IsDualLand { get; set; }

        // ── Display Options ───────────────────────────────────────────────────────

        /// <summary>Controls how duplicate results are eliminated (unique:).</summary>
        public UniqueStrategy? Unique { get; set; }

        /// <summary>Sort field for results (order:).</summary>
        public SortOrder? Order { get; set; }

        /// <summary>Sort direction (direction:).</summary>
        public SortDirection? Direction { get; set; }

        /// <summary>
        /// Preferred printing to show, e.g. "newest", "oldest", "usd-low", "promo",
        /// "universesbeyond", "atypical" (prefer:).
        /// </summary>
        public string? Prefer { get; set; }

        // ── Pagination ────────────────────────────────────────────────────────────

        /// <summary>Maximum number of cards to return. Clamped to 1–50. Defaults to 20.</summary>
        public int PageSize { get; set; } = 20;
    }
}

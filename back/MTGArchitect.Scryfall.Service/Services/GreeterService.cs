using Grpc.Core;
using MTGArchitect.Scryfall.Service.Contracts;
using SearchContracts = MTGArchitect.Scryfall.Contracts;

namespace MTGArchitect.Scryfall.Service.Services;

public class ScryfallCardSearchService(
    CardController cardController,
    ILogger<ScryfallCardSearchService> logger) : CardSearch.CardSearchBase
{
    public override async Task<SearchCardsReply> SearchCards(SearchCardsRequest request, ServerCallContext context)
    {
        var result = await cardController.SearchCards(request.Query, request.PageSize, context.CancellationToken);
        return ToReply(result);
    }

    public override async Task<SearchCardsReply> AdvanceSearchCards(AdvanceSearchCardsRequest request, ServerCallContext context)
    {
        var query = ToCardQuerySearch(request);
        var pageSize = request.PageSize > 0 ? request.PageSize : query.PageSize;
        var result = await cardController.AdvanceSearchCards(query, pageSize, context.CancellationToken);
        return ToReply(result);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static SearchCardsReply ToReply(CardController.SearchCardsResult result)
    {
        var reply = new SearchCardsReply();
        reply.Cards.AddRange(result.Cards.Select(c => new CardItem
        {
            Id = c.Id,
            Name = c.Name,
            ManaCost = c.ManaCost,
            TypeLine = c.TypeLine,
            SetCode = c.SetCode,
            ImageUrl = c.ImageUrl
        }));
        return reply;
    }

    private static SearchContracts.CardQuerySearch ToCardQuerySearch(AdvanceSearchCardsRequest r)
    {
        var q = new SearchContracts.CardQuerySearch();

        if (r.HasName)          q.Name = r.Name;
        if (r.HasExactName)     q.ExactName = r.ExactName;

        if (r.HasColor)         { q.Color = r.Color;         q.ColorOperator = MapOp(r.ColorOperator); }
        if (r.HasColorIdentity) { q.ColorIdentity = r.ColorIdentity; q.ColorIdentityOperator = MapOp(r.ColorIdentityOperator); }

        foreach (var t in r.IncludedTypes)        q.Types.Add(t);
        foreach (var t in r.ExcludedTypes) q.ExcludedTypes.Add(t);

        if (r.HasOracleText)     q.OracleText = r.OracleText;
        if (r.HasFullOracleText) q.FullOracleText = r.FullOracleText;
        if (r.HasKeyword)        q.Keyword = r.Keyword;

        if (r.HasManaCost)  q.ManaCost = r.ManaCost;
        if (r.HasManaValue) { q.ManaValue = r.ManaValue; q.ManaValueOperator = MapOp(r.ManaValueOperator); }
        if (r.HasProduces)  q.Produces = r.Produces;

        if (r.HasPower)     { q.Power = r.Power;         q.PowerOperator = MapOp(r.PowerOperator); }
        if (r.HasToughness) { q.Toughness = r.Toughness; q.ToughnessOperator = MapOp(r.ToughnessOperator); }
        if (r.HasLoyalty)   { q.Loyalty = r.Loyalty;     q.LoyaltyOperator = MapOp(r.LoyaltyOperator); }

        if (r.HasRarity) { q.Rarity = MapRarity(r.Rarity); q.RarityOperator = MapOp(r.RarityOperator); }

        if (r.HasSetCode)         q.SetCode = r.SetCode;
        if (r.HasBlock)           q.Block = r.Block;
        if (r.HasCollectorNumber) q.CollectorNumber = r.CollectorNumber;

        if (r.HasFormat)       q.Format = MapFormat(r.Format);
        if (r.HasBannedIn)     q.BannedIn = MapFormat(r.BannedIn);
        if (r.HasRestrictedIn) q.RestrictedIn = MapFormat(r.RestrictedIn);

        if (r.HasUsdPrice) { q.UsdPrice = (decimal)r.UsdPrice; q.UsdPriceOperator = MapOp(r.UsdPriceOperator); }
        if (r.HasEurPrice) { q.EurPrice = (decimal)r.EurPrice; q.EurPriceOperator = MapOp(r.EurPriceOperator); }
        if (r.HasTixPrice) { q.TixPrice = (decimal)r.TixPrice; q.TixPriceOperator = MapOp(r.TixPriceOperator); }

        if (r.HasArtist)    q.Artist = r.Artist;
        if (r.HasFlavorText) q.FlavorText = r.FlavorText;
        if (r.HasWatermark) q.Watermark = r.Watermark;

        if (r.HasBorder) q.Border = r.Border;
        if (r.HasFrame)  q.Frame = r.Frame;

        if (r.HasGame) q.Game = MapGame(r.Game);

        if (r.HasYear) { q.Year = r.Year; q.YearOperator = MapOp(r.YearOperator); }
        if (r.HasDate) { q.Date = r.Date; q.DateOperator = MapOp(r.DateOperator); }

        if (r.HasLanguage)  q.Language = r.Language;
        if (r.HasArtTag)    q.ArtTag = r.ArtTag;
        if (r.HasOracleTag) q.OracleTag = r.OracleTag;

        if (r.HasIsReprint)   q.IsReprint   = r.IsReprint;
        if (r.HasIsFoil)      q.IsFoil      = r.IsFoil;
        if (r.HasIsNonFoil)   q.IsNonFoil   = r.IsNonFoil;
        if (r.HasIsFullArt)   q.IsFullArt   = r.IsFullArt;
        if (r.HasIsPromo)     q.IsPromo     = r.IsPromo;
        if (r.HasIsDigital)   q.IsDigital   = r.IsDigital;
        if (r.HasIsFunny)     q.IsFunny     = r.IsFunny;
        if (r.HasIsReserved)  q.IsReserved  = r.IsReserved;
        if (r.HasIsCommander) q.IsCommander = r.IsCommander;
        if (r.HasIsPartner)   q.IsPartner   = r.IsPartner;
        if (r.HasIsHires)     q.IsHires     = r.IsHires;
        if (r.HasIsSpell)     q.IsSpell     = r.IsSpell;
        if (r.HasIsPermanent) q.IsPermanent = r.IsPermanent;
        if (r.HasIsSplit)     q.IsSplit     = r.IsSplit;
        if (r.HasIsTransform) q.IsTransform = r.IsTransform;
        if (r.HasIsMdfc)      q.IsMdfc      = r.IsMdfc;
        if (r.HasIsFetchland) q.IsFetchland = r.IsFetchland;
        if (r.HasIsShockland) q.IsShockland = r.IsShockland;
        if (r.HasIsDualLand)  q.IsDualLand  = r.IsDualLand;

        if (r.HasUnique)    q.Unique    = MapUnique(r.Unique);
        if (r.HasOrder)     q.Order     = MapOrder(r.Order);
        if (r.HasDirection) q.Direction = r.Direction == SortDirection.Desc ? SearchContracts.SortDirection.Desc : SearchContracts.SortDirection.Asc;
        if (r.HasPrefer)    q.Prefer    = r.Prefer;

        return q;
    }

    private static SearchContracts.ComparisonOperator MapOp(ComparisonOperator op) => op switch
    {
        ComparisonOperator.NotEqual          => SearchContracts.ComparisonOperator.NotEqual,
        ComparisonOperator.GreaterThan       => SearchContracts.ComparisonOperator.GreaterThan,
        ComparisonOperator.GreaterThanOrEqual => SearchContracts.ComparisonOperator.GreaterThanOrEqual,
        ComparisonOperator.LessThan          => SearchContracts.ComparisonOperator.LessThan,
        ComparisonOperator.LessThanOrEqual   => SearchContracts.ComparisonOperator.LessThanOrEqual,
        _                                    => SearchContracts.ComparisonOperator.Equal
    };

    private static SearchContracts.CardRarity MapRarity(CardRarity r) => r switch
    {
        CardRarity.Uncommon => SearchContracts.CardRarity.Uncommon,
        CardRarity.Rare     => SearchContracts.CardRarity.Rare,
        CardRarity.Special  => SearchContracts.CardRarity.Special,
        CardRarity.Mythic   => SearchContracts.CardRarity.Mythic,
        CardRarity.Bonus    => SearchContracts.CardRarity.Bonus,
        _                   => SearchContracts.CardRarity.Common
    };

    private static SearchContracts.CardGame MapGame(CardGame g) => g switch
    {
        CardGame.Mtgo  => SearchContracts.CardGame.Mtgo,
        CardGame.Arena => SearchContracts.CardGame.Arena,
        _              => SearchContracts.CardGame.Paper
    };

    private static SearchContracts.CardFormat MapFormat(CardFormat f) => f switch
    {
        CardFormat.FormatFuture         => SearchContracts.CardFormat.Future,
        CardFormat.FormatHistoric       => SearchContracts.CardFormat.Historic,
        CardFormat.FormatTimeless       => SearchContracts.CardFormat.Timeless,
        CardFormat.FormatGladiator      => SearchContracts.CardFormat.Gladiator,
        CardFormat.FormatPioneer        => SearchContracts.CardFormat.Pioneer,
        CardFormat.FormatModern         => SearchContracts.CardFormat.Modern,
        CardFormat.FormatLegacy         => SearchContracts.CardFormat.Legacy,
        CardFormat.FormatPauper         => SearchContracts.CardFormat.Pauper,
        CardFormat.FormatVintage        => SearchContracts.CardFormat.Vintage,
        CardFormat.FormatPenny          => SearchContracts.CardFormat.Penny,
        CardFormat.FormatCommander      => SearchContracts.CardFormat.Commander,
        CardFormat.FormatOathbreaker    => SearchContracts.CardFormat.Oathbreaker,
        CardFormat.FormatStandardBrawl  => SearchContracts.CardFormat.StandardBrawl,
        CardFormat.FormatBrawl          => SearchContracts.CardFormat.Brawl,
        CardFormat.FormatAlchemy        => SearchContracts.CardFormat.Alchemy,
        CardFormat.FormatPauperCommander => SearchContracts.CardFormat.PauperCommander,
        CardFormat.FormatDuel           => SearchContracts.CardFormat.Duel,
        CardFormat.FormatOldSchool      => SearchContracts.CardFormat.OldSchool,
        CardFormat.FormatPremodern      => SearchContracts.CardFormat.Premodern,
        CardFormat.FormatPredh          => SearchContracts.CardFormat.Predh,
        _                               => SearchContracts.CardFormat.Standard
    };

    private static SearchContracts.UniqueStrategy MapUnique(UniqueStrategy u) => u switch
    {
        UniqueStrategy.UniquePrints => SearchContracts.UniqueStrategy.Prints,
        UniqueStrategy.UniqueArt    => SearchContracts.UniqueStrategy.Art,
        _                           => SearchContracts.UniqueStrategy.Cards
    };

    private static SearchContracts.SortOrder MapOrder(SortOrder o) => o switch
    {
        SortOrder.SortSet       => SearchContracts.SortOrder.Set,
        SortOrder.SortReleased  => SearchContracts.SortOrder.Released,
        SortOrder.SortRarity    => SearchContracts.SortOrder.Rarity,
        SortOrder.SortColor     => SearchContracts.SortOrder.Color,
        SortOrder.SortUsd       => SearchContracts.SortOrder.Usd,
        SortOrder.SortTix       => SearchContracts.SortOrder.Tix,
        SortOrder.SortEur       => SearchContracts.SortOrder.Eur,
        SortOrder.SortCmc       => SearchContracts.SortOrder.Cmc,
        SortOrder.SortPower     => SearchContracts.SortOrder.Power,
        SortOrder.SortToughness => SearchContracts.SortOrder.Toughness,
        SortOrder.SortEdhrec    => SearchContracts.SortOrder.EdhRec,
        SortOrder.SortPenny     => SearchContracts.SortOrder.Penny,
        SortOrder.SortArtist    => SearchContracts.SortOrder.Artist,
        SortOrder.SortSpoiled   => SearchContracts.SortOrder.Spoiled,
        SortOrder.SortReview    => SearchContracts.SortOrder.Review,
        _                       => SearchContracts.SortOrder.Name
    };
}
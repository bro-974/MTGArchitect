using ProtoContracts = MTGArchitect.Scryfall.Service.Contracts;
using ScryfallContracts = MTGArchitect.Scryfall.Contracts;

namespace MTGArchitectServices.Scryfall.Client;

    public static class MappingHelpers
    {
        public static IEnumerable<ScryfallContracts.Card> MapCards(IEnumerable<ProtoContracts.CardItem> cards) =>
        cards.Select(card => new ScryfallContracts.Card()
        {
            Id = card.Id,
            Name = card.Name,
            ManaCost = card.ManaCost,
            TypeLine = card.TypeLine,
            SetCode = card.SetCode,
            ImageUrl = card.ImageUrl
        });

        public static ProtoContracts.AdvanceSearchCardsRequest ToProtoRequest(ScryfallContracts.CardQuerySearch q)
        {
            var req = new ProtoContracts.AdvanceSearchCardsRequest { PageSize = q.PageSize };

            if (q.Name is not null) { req.Name = q.Name; req.ExactName = q.ExactName; }
            if (q.Color is not null) { req.Color = q.Color; req.ColorOperator = MapOp(q.ColorOperator); }
            if (q.ColorIdentity is not null) { req.ColorIdentity = q.ColorIdentity; req.ColorIdentityOperator = MapOp(q.ColorIdentityOperator); }

            req.IncludedTypes.AddRange(q.Types);
            req.ExcludedTypes.AddRange(q.ExcludedTypes);

            if (q.OracleText is not null) req.OracleText = q.OracleText;
            if (q.FullOracleText is not null) req.FullOracleText = q.FullOracleText;
            if (q.Keyword is not null) req.Keyword = q.Keyword;

            if (q.ManaCost is not null) req.ManaCost = q.ManaCost;
            if (q.ManaValue.HasValue) { req.ManaValue = q.ManaValue.Value; req.ManaValueOperator = MapOp(q.ManaValueOperator); }
            if (q.Produces is not null) req.Produces = q.Produces;

            if (q.Power is not null) { req.Power = q.Power; req.PowerOperator = MapOp(q.PowerOperator); }
            if (q.Toughness is not null) { req.Toughness = q.Toughness; req.ToughnessOperator = MapOp(q.ToughnessOperator); }
            if (q.Loyalty.HasValue) { req.Loyalty = q.Loyalty.Value; req.LoyaltyOperator = MapOp(q.LoyaltyOperator); }

            if (q.Rarity.HasValue) { req.Rarity = (ProtoContracts.CardRarity)(int)q.Rarity.Value; req.RarityOperator = MapOp(q.RarityOperator); }
            if (q.SetCode is not null) req.SetCode = q.SetCode;
            if (q.Block is not null) req.Block = q.Block;
            if (q.CollectorNumber is not null) req.CollectorNumber = q.CollectorNumber;

            if (q.Format.HasValue) req.Format = (ProtoContracts.CardFormat)(int)q.Format.Value;
            if (q.BannedIn.HasValue) req.BannedIn = (ProtoContracts.CardFormat)(int)q.BannedIn.Value;
            if (q.RestrictedIn.HasValue) req.RestrictedIn = (ProtoContracts.CardFormat)(int)q.RestrictedIn.Value;

            if (q.UsdPrice.HasValue) { req.UsdPrice = (double)q.UsdPrice.Value; req.UsdPriceOperator = MapOp(q.UsdPriceOperator); }
            if (q.EurPrice.HasValue) { req.EurPrice = (double)q.EurPrice.Value; req.EurPriceOperator = MapOp(q.EurPriceOperator); }
            if (q.TixPrice.HasValue) { req.TixPrice = (double)q.TixPrice.Value; req.TixPriceOperator = MapOp(q.TixPriceOperator); }

            if (q.Artist is not null) req.Artist = q.Artist;
            if (q.FlavorText is not null) req.FlavorText = q.FlavorText;
            if (q.Watermark is not null) req.Watermark = q.Watermark;
            if (q.Border is not null) req.Border = q.Border;
            if (q.Frame is not null) req.Frame = q.Frame;

            if (q.Game.HasValue) req.Game = (ProtoContracts.CardGame)(int)q.Game.Value;

            if (q.Year.HasValue) { req.Year = q.Year.Value; req.YearOperator = MapOp(q.YearOperator); }
            if (q.Date is not null) { req.Date = q.Date; req.DateOperator = MapOp(q.DateOperator); }
            if (q.Language is not null) req.Language = q.Language;
            if (q.ArtTag is not null) req.ArtTag = q.ArtTag;
            if (q.OracleTag is not null) req.OracleTag = q.OracleTag;

            if (q.IsReprint.HasValue) req.IsReprint = q.IsReprint.Value;
            if (q.IsFoil.HasValue) req.IsFoil = q.IsFoil.Value;
            if (q.IsNonFoil.HasValue) req.IsNonFoil = q.IsNonFoil.Value;
            if (q.IsFullArt.HasValue) req.IsFullArt = q.IsFullArt.Value;
            if (q.IsPromo.HasValue) req.IsPromo = q.IsPromo.Value;
            if (q.IsDigital.HasValue) req.IsDigital = q.IsDigital.Value;
            if (q.IsFunny.HasValue) req.IsFunny = q.IsFunny.Value;
            if (q.IsReserved.HasValue) req.IsReserved = q.IsReserved.Value;
            if (q.IsCommander.HasValue) req.IsCommander = q.IsCommander.Value;
            if (q.IsPartner.HasValue) req.IsPartner = q.IsPartner.Value;
            if (q.IsHires.HasValue) req.IsHires = q.IsHires.Value;
            if (q.IsSpell.HasValue) req.IsSpell = q.IsSpell.Value;
            if (q.IsPermanent.HasValue) req.IsPermanent = q.IsPermanent.Value;
            if (q.IsSplit.HasValue) req.IsSplit = q.IsSplit.Value;
            if (q.IsTransform.HasValue) req.IsTransform = q.IsTransform.Value;
            if (q.IsMdfc.HasValue) req.IsMdfc = q.IsMdfc.Value;
            if (q.IsFetchland.HasValue) req.IsFetchland = q.IsFetchland.Value;
            if (q.IsShockland.HasValue) req.IsShockland = q.IsShockland.Value;
            if (q.IsDualLand.HasValue) req.IsDualLand = q.IsDualLand.Value;

            if (q.Unique.HasValue) req.Unique = (ProtoContracts.UniqueStrategy)(int)q.Unique.Value;
            if (q.Order.HasValue) req.Order = (ProtoContracts.SortOrder)(int)q.Order.Value;
            if (q.Direction.HasValue) req.Direction = (ProtoContracts.SortDirection)(int)q.Direction.Value;
            if (q.Prefer is not null) req.Prefer = q.Prefer;

            return req;
        }

        public static ProtoContracts.ComparisonOperator MapOp(ScryfallContracts.ComparisonOperator op) =>
            (ProtoContracts.ComparisonOperator)(int)op;
    }

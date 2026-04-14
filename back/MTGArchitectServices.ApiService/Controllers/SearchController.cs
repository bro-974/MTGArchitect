using MTGArchitect.Scryfall.Contracts;
using MTGArchitectServices.Scryfall.Client;
using ProtoContracts = MTGArchitect.Scryfall.Service.Contracts;

namespace MTGArchitectServices.ApiService.Controllers;

    public class SearchController
    {
        private readonly ProtoContracts.CardSearch.CardSearchClient cardSearchClient;

        public SearchController(ProtoContracts.CardSearch.CardSearchClient cardSearchClient)
        {
            this.cardSearchClient = cardSearchClient;
        }

        public async Task<IResult> QuerySearch(string q, int? pageSize,CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest(new { message = "Query parameter q is required." });

            var reply = await cardSearchClient.SearchCardsAsync(new ProtoContracts.SearchCardsRequest
            {
                Query = q,
                PageSize = pageSize.GetValueOrDefault(20)
            }, cancellationToken: cancellationToken);

            return Results.Ok(MappingHelpers.MapCards(reply.Cards));
        }

        public async Task<IResult> AdvancedSearch(CardQuerySearch body, CancellationToken cancellationToken)
        {
            var request = MappingHelpers.ToProtoRequest(body);
            var reply = await cardSearchClient.AdvanceSearchCardsAsync(request, cancellationToken: cancellationToken);
            return Results.Ok(MappingHelpers.MapCards(reply.Cards));
        }
}

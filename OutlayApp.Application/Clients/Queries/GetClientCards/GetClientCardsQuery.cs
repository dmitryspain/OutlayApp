using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Clients.Queries.GetClientCards;

public sealed record GetClientCardsQuery(Guid ClientId) : IQuery<List<ClientCardDto>>;

public class GetClientCardsQueryHandler : IQueryHandler<GetClientCardsQuery, List<ClientCardDto>>
{
    private readonly IClientCardsRepository _cards;

    public GetClientCardsQueryHandler(IClientCardsRepository cards)
    {
        _cards = cards;
    }

    public async Task<Result<List<ClientCardDto>>> Handle(GetClientCardsQuery request, CancellationToken cancellationToken)
    {
        var cards = await _cards.GetAll(request.ClientId, cancellationToken);
        return cards.Select(c => c.ToDto()).ToList();
    }
}

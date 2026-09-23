using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class GetClientQueryHandler : IQueryHandler<GetClientQuery, ClientDto>
{
    private readonly IClientRepository _clientRepository;

    public GetClientQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<ClientDto>> Handle(GetClientQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdWithCards(request.ClientId, cancellationToken);
        return client is null
            ? Result.Failure<ClientDto>(new Error("Client.NotFound", "Клієнта не знайдено."))
            : client.ToDto();
    }
}

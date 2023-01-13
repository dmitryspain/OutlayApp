using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class GetClientQueryHandler : IQueryHandler<GetClientQuery, ClientDto>
{
    private readonly IMapper _mapper;
    private readonly IClientRepository _clientRepository;

    public GetClientQueryHandler(IMapper mapper, IClientRepository clientRepository)
    {
        _mapper = mapper;
        _clientRepository = clientRepository;
    }

    public async Task<Result<ClientDto>> Handle(GetClientQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdWithCards(request.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(new Error("Client.NotFound",
                $"Client with Id {request.ClientId} does not found"));

        return _mapper.Map<ClientDto>(client);
    }
}
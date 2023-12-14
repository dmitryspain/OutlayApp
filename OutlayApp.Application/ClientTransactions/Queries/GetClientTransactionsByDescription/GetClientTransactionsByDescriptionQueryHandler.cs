using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public class GetClientTransactionsByDescriptionQueryHandler : IQueryHandler<GetClientTransactionsByDescriptionQuery,
    List<ClientTransactionByDescriptionResponse>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly IMapper _mapper;

    public GetClientTransactionsByDescriptionQueryHandler(IClientTransactionRepository clientTransactionRepository,
        IMapper mapper)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ClientTransactionByDescriptionResponse>>> Handle(GetClientTransactionsByDescriptionQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _clientTransactionRepository
            .GetByDescription(request.ClientCardId, request.Description, cancellationToken);

        var result = _mapper.Map<List<ClientTransactionByDescriptionResponse>>(transactions);
        return result;
    }
}
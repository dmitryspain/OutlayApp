using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Backfill;

/// <summary>Where the stored history of a card begins (unix seconds); "now" when nothing is stored.</summary>
public sealed record GetHistoryStartQuery(Guid CardId) : IQuery<long>;

public class GetHistoryStartQueryHandler : IQueryHandler<GetHistoryStartQuery, long>
{
    private readonly IClientTransactionRepository _transactionRepository;

    public GetHistoryStartQueryHandler(IClientTransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<long>> Handle(GetHistoryStartQuery request, CancellationToken cancellationToken)
    {
        var earliest = await _transactionRepository.GetEarliest(request.CardId, cancellationToken);
        var start = earliest is null ? DateTimeOffset.Now : new DateTimeOffset(earliest.DateOccured);
        return Result.Success(start.ToUnixTimeSeconds() - 1);
    }
}

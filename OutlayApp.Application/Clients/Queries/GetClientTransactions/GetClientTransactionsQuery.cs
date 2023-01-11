using MediatR;
using OutlayApp.Application.Transactions;

namespace OutlayApp.Application.Clients.Queries.GetClientTransactions;

public class GetClientTransactionsQuery : IRequest<List<MonobankTransaction>>
{
    public string CardId { get; }
    //todo: make sure its not more then DateTo
    public DateTime? DateFrom { get; }
    public DateTime? DateTo { get; }

    public GetClientTransactionsQuery(string cardId, DateTime? dateFrom, DateTime? dateTo)
    {
        CardId = cardId;
        DateFrom = dateFrom;
        DateTo = dateTo;
    }
}
using MediatR;

namespace OutlayApp.Application.Clients.Queries.GetCardHistoryByDescription;

public class GetCardHistoryByDescriptionQuery : IRequest<List<TransactionByDescriptionDto>>
{
    public string CardId { get; }
    public string Description { get; }
    public DateTime? DateFrom { get; }
    public DateTime? DateTo { get; }

    public GetCardHistoryByDescriptionQuery(string cardId, string description, DateTime? dateFrom, DateTime? dateTo)
    {
        CardId = cardId;
        Description = description;
        DateFrom = dateFrom;
        DateTo = dateTo;
    }
}
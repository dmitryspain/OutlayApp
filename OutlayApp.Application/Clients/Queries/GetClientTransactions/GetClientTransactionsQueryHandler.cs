using System.Net.Http.Json;
using MediatR;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions;

namespace OutlayApp.Application.Clients.Queries.GetClientTransactions;

public class GetClientTransactionsQueryHandler : IRequestHandler<GetClientTransactionsQuery, List<MonobankTransaction>>
{
    private readonly HttpClient _httpClient;
    private const int MaxDaysPeriod = 30;

    public GetClientTransactionsQueryHandler(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }

    public async Task<List<MonobankTransaction>> Handle(GetClientTransactionsQuery request, CancellationToken cancellationToken)
    {
        var from = DateTime.Now.AddDays(-MaxDaysPeriod);
        var to = DateTime.Now;

        if (request.DateFrom.HasValue)
            from = request.DateFrom.Value;

        if (request.DateTo.HasValue)
            to = request.DateTo.Value;

        var unixTimeFrom = ((DateTimeOffset)from).ToUnixTimeSeconds();
        var unixTimeTo = ((DateTimeOffset)to).ToUnixTimeSeconds();

        var url = $"/personal/statement/{request.CardId}/{unixTimeFrom}/{unixTimeTo}";
        var result = await _httpClient.GetAsync(url, cancellationToken);
        var cardHistories = (await result.Content.ReadFromJsonAsync<IEnumerable<MonobankTransaction>>(cancellationToken:
            cancellationToken) ?? Array.Empty<MonobankTransaction>()).ToList();
        return cardHistories;
    }
}
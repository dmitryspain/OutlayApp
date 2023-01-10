// using System.Net.Http.Json;
// using AutoMapper;
// using MediatR;
// using OutlayApp.Application.Configuration.Monobank;
// using OutlayApp.Domain.Clients;
//
// namespace OutlayApp.Application.Clients.GetCardHistoryByDescription;
//
// public class GetCardHistoryByDescriptionQueryHandler : IRequestHandler<GetCardHistoryByDescriptionQuery, List<TransactionByDescriptionDto>>
// {
//     private readonly HttpClient _httpClient;
//     private readonly IMapper _mapper;
//
//     public GetCardHistoryByDescriptionQueryHandler(IHttpClientFactory factory, IMapper mapper)
//     {
//         _mapper = mapper;
//         _httpClient = factory.CreateClient(MonobankConstants.Client);
//     }
//     
//     public async Task<List<TransactionByDescriptionDto>> Handle(GetCardHistoryByDescriptionQuery request, CancellationToken cancellationToken)
//     {
//         var allTransactions = await _httpClient.GetAsync($"/personal/statement/{request.CardId}/{unixSeconds}",
//             cancellationToken);
//
//         var filtered = (await allTransactions.Content.ReadFromJsonAsync<IEnumerable<ClientTransaction>>(cancellationToken:
//             cancellationToken))!.Where(x => x.Description == request.Description);
//
//         return _mapper.Map<IEnumerable<TransactionByDescriptionDto>>(filtered);
//     }
// }










// Client(UserId, Name)
// ClientCards(ClientId, CardId)
// Cards(ClientId, Balance, 
// Transactions(CardId, DateOccured, Description, Amount, BalanceAfter?, CurrencyCode, Type, MaskedCardNumber)


































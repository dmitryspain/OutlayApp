using System.Net.Http.Json;
using AutoMapper;
using MediatR;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Infrastructure.Models;

namespace OutlayApp.Application.Clients.GetClientInfo;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, ClientInfoDto>
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;

    public GetClientsQueryHandler(IHttpClientFactory factory, IMapper mapper)
    {
        _mapper = mapper;
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }
    
    public async Task<ClientInfoDto> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync("/personal/client-info", cancellationToken);
        var clientInfo = await result.Content.ReadFromJsonAsync<ClientInfo>(cancellationToken: cancellationToken);
        return _mapper.Map<ClientInfoDto>(clientInfo);
    }
}
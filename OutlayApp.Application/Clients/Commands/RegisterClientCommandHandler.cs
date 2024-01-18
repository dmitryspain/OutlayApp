using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Clients.Commands;

public class RegisterClientCommandHandler : ICommandHandler<RegisterClientCommand, Guid>
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IOptions<MonobankSettings> _monobankSettings;
    // private readonly HttpClient _httpClient;

    public RegisterClientCommandHandler(IClientRepository clientRepository,
        IUnitOfWork unitOfWork, IOptions<MonobankSettings> monobankSettings)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _monobankSettings = monobankSettings;

        // _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }

    public async Task<Result<Guid>> Handle(RegisterClientCommand request, CancellationToken cancellationToken)
    {
        var exist = await _clientRepository.GetByPersonalToken(request.ClientToken, cancellationToken);
        if (exist is not null)
        {
            return Result.Success(exist.Cards.MaxBy(x => x.Balance)!.Id);
        }

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_monobankSettings.Value.BaseUrl);
        httpClient.DefaultRequestHeaders.Add(MonobankConstants.TokenHeader, request.ClientToken);
        // var settings = builder.Configuration.GetSection(MonobankConstants.Name)
        //             .Get<MonobankSettings>();
        //         httpClient.BaseAddress = new Uri(settings!.BaseUrl);
        //         httpClient.DefaultRequestHeaders.Add(MonobankConstants.TokenHeader, settings.PersonalToken);
        var result = await httpClient.GetAsync("/personal/client-info", cancellationToken);
        var clientInfo = await result.Content.ReadFromJsonAsync<ClientInfo>(cancellationToken: cancellationToken);

        var client = Client.Create(clientInfo!.Name, request.ClientToken);
        foreach (var account in clientInfo.Accounts)
        {
            client.AddCard(account.Balance, account.Type, account.Id, account.CreditLimit, account.CurrencyCode);
        }

        await _clientRepository.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var mostUsedCard = client.Cards.MaxBy(x => x.Balance);
        return Result.Success(mostUsedCard!.Id);
    }
}
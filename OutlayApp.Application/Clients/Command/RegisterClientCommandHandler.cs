using System.Net.Http.Json;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions.Commands;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;
using OutlayApp.Infrastructure.Models;

namespace OutlayApp.Application.Clients.Command;

public class RegisterClientCommandHandler : ICommandHandler<RegisterClientCommand>
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;

    public RegisterClientCommandHandler(IClientRepository clientRepository, IHttpClientFactory factory,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }

    public async Task<Result> Handle(RegisterClientCommand request, CancellationToken cancellationToken)
    {
        var exist = await _clientRepository.GetByPersonalToken(request.ClientToken, cancellationToken);
        if (exist is not null)
            return Result.Failure(new Error("Client.AlreadyExists", "Client is already exists"));

        var result = await _httpClient.GetAsync("/personal/client-info", cancellationToken);
        var clientInfo = await result.Content.ReadFromJsonAsync<ClientInfo>(cancellationToken: cancellationToken);

        var client = Client.Create(clientInfo!.Name, request.ClientToken);
        foreach (var account in clientInfo.Accounts)
        {
            client.AddCard(account.Balance, account.Type, account.Id, account.CreditLimit, account.CurrencyCode);
        }

        await _clientRepository.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
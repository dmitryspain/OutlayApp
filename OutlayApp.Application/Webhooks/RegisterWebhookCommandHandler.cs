using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Webhooks;

public class RegisterWebhookCommandHandler : ICommandHandler<RegisterWebhookCommand, string>
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly MonobankSettings _settings;

    public RegisterWebhookCommandHandler(IClientRepository clientRepository, IUnitOfWork unitOfWork,
        IHttpClientFactory factory, IOptions<MonobankSettings> settings)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
        _settings = settings.Value;
    }

    public async Task<Result<string>> Handle(RegisterWebhookCommand request, CancellationToken cancellationToken)
    {
        var url = WebhookUrl.Build(_settings);
        if (url is null)
            return Result.Failure<string>(new Error("Webhook.NotConfigured",
                "Webhooks are not configured on the server (Monobank:WebhookBaseUrl / WebhookSecret)."));

        var client = await _clientRepository.GetByPersonalToken(request.ClientToken, cancellationToken);
        if (client is null)
            return Result.Failure<string>(new Error("Client.NotFound", "Connect the token first."));

        // Monobank calls the URL (GET) before accepting it, so it must already be reachable
        using var message = new HttpRequestMessage(HttpMethod.Post, "/personal/webhook")
        {
            Content = JsonContent.Create(new { webHookUrl = url })
        };
        message.Headers.Add(MonobankConstants.TokenHeader, request.ClientToken);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure<string>(new Error("Webhook.Rejected",
                $"Monobank refused the webhook ({(int)response.StatusCode}): {body}"));
        }

        client.SetWebhook(url);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(url);
    }
}

using Microsoft.Extensions.Options;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Monobank;
using OutlayApp.Application.Security;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Webhooks;

public class RegisterWebhookCommandHandler : ICommandHandler<RegisterWebhookCommand, string>
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMonobankClient _monobank;
    private readonly ITokenProtector _protector;
    private readonly MonobankSettings _settings;

    public RegisterWebhookCommandHandler(IClientRepository clientRepository, IUnitOfWork unitOfWork,
        IMonobankClient monobank, ITokenProtector protector, IOptions<MonobankSettings> settings)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _monobank = monobank;
        _protector = protector;
        _settings = settings.Value;
    }

    public async Task<Result<string>> Handle(RegisterWebhookCommand request, CancellationToken cancellationToken)
    {
        var url = WebhookUrl.Build(_settings);
        if (url is null)
            return Result.Failure<string>(new Error("Webhook.NotConfigured",
                "На сервері не налаштовано публічну адресу для webhook (Monobank:WebhookBaseUrl / WebhookSecret)."));

        var client = await _clientRepository.GetById(request.ClientId, cancellationToken);
        if (client?.EncryptedToken is null)
            return Result.Failure<string>(new Error("Client.NotFound", "Клієнта не знайдено."));

        // Monobank calls the URL (GET) before accepting it, so it must already be reachable
        try
        {
            await _monobank.SetWebhook(_protector.Unprotect(client.EncryptedToken), url, cancellationToken);
        }
        catch (MonobankException ex)
        {
            return Result.Failure<string>(MonobankErrors.From(ex));
        }

        client.SetWebhook(url);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(url);
    }
}

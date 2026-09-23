using Microsoft.Extensions.Options;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Webhooks;

public class GetWebhookStatusQueryHandler : IQueryHandler<GetWebhookStatusQuery, WebhookStatus>
{
    private readonly IClientRepository _clientRepository;
    private readonly MonobankSettings _settings;

    public GetWebhookStatusQueryHandler(IClientRepository clientRepository, IOptions<MonobankSettings> settings)
    {
        _clientRepository = clientRepository;
        _settings = settings.Value;
    }

    public async Task<Result<WebhookStatus>> Handle(GetWebhookStatusQuery request, CancellationToken cancellationToken)
    {
        // read from our own records: /personal/client-info is rate-limited to once a minute
        var url = WebhookUrl.Build(_settings);
        var client = await _clientRepository.GetById(request.ClientId, cancellationToken);
        var enabled = url is not null && client?.WebhookUrl == url;
        return Result.Success(new WebhookStatus(url is not null, enabled));
    }
}

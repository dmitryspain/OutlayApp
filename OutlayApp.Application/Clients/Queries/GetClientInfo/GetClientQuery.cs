using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public record GetClientQuery(Guid ClientId) : IQuery<ClientDto>;

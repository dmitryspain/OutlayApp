namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class ClientDto
{
    public string FullName { get; set; } = string.Empty;
    public IReadOnlyCollection<ClientCardDto> Cards { get; set; } = Array.Empty<ClientCardDto>();
}

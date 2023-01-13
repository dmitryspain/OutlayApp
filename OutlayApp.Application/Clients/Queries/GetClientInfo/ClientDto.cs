namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class ClientDto
{
    public string FullName { get; set; }
    public IReadOnlyCollection<ClientCardDto> Cards { get; set; }
}
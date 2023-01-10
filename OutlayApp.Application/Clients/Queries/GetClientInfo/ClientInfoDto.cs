namespace OutlayApp.Application.Clients.GetClientInfo;

public class ClientInfoDto
{
    public string Name { get; set; }
    public IEnumerable<ClientCardDto> Cards { get; set; }
}
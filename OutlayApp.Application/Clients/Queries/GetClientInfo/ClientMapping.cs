using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;

namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public static class ClientMapping
{
    public static ClientCardDto ToDto(this ClientCard card) => new()
    {
        Id = card.Id,
        Balance = card.Balance,
        CreditLimit = card.CreditLimit,
        Type = card.Type,
        CurrencyCode = card.CurrencyCode,
        MaskedCardNumber = card.MaskedPan,
        Iban = card.Iban,
    };

    public static ClientDto ToDto(this Client client) => new()
    {
        FullName = client.Name,
        Cards = client.Cards.Select(ToDto).ToList(),
    };
}

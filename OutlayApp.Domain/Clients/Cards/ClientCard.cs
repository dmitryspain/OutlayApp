using OutlayApp.Domain.SeedWork;

namespace OutlayApp.Domain.Clients.Cards;

public class ClientCard : Entity, IAggregateRoot
{
    public Guid ClientId { get; private set; }
    // public CardId CardId { get; set; }
    // public ClientId ClientId { get; private set; }

    public decimal Balance { get; private set; }
    public string Type { get; private set; }
    public int CreditLimit { get; private set; }
    public int CurrencyCode { get; private set; }
    public string ExternalCardId { get; private set; }

    private ClientCard()
        : base(Guid.NewGuid())
    {
    }

    internal ClientCard(Guid id, Guid clientId, decimal balance, string type, string externalCardId,
        int creditLimit, int currencyCode)
        : base(id)
    {
        ClientId = clientId;
        Balance = balance;
        Type = type;
        ExternalCardId = externalCardId;
        CreditLimit = creditLimit;
        CurrencyCode = currencyCode;
    }
}
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients.Events;
using OutlayApp.Domain.Primitives;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Domain.Clients;

public sealed class Client : Entity, IAggregateRoot
{
    public string Name { get; private set; }

    /// <summary>The Monobank token, encrypted (the plain value never reaches the database).</summary>
    public string? EncryptedToken { get; private set; }

    /// <summary>SHA-256 of the Monobank token, to find the client by it.</summary>
    public string? TokenHash { get; private set; }

    /// <summary>Plain token of rows created before encryption; emptied by the one-time migration at startup.</summary>
    public string? LegacyPlainToken { get; private set; }

    /// <summary>The URL Monobank pushes new transactions to, once registered.</summary>
    public string? WebhookUrl { get; private set; }

    private readonly List<ClientCard> _cards = new();
    public IReadOnlyCollection<ClientCard> Cards => _cards;

    private Client() : base(Guid.NewGuid())
    {
        Name = string.Empty;
    }

    private Client(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public Result<ClientCard> AddCard(string externalCardId, string type, decimal balance, decimal creditLimit,
        int currencyCode, string? maskedPan, string? iban)
    {
        if (_cards.Any(x => x.ExternalCardId == externalCardId))
            return Result.Failure<ClientCard>(new Error("ClientCard.AlreadyAdded", "This card already has been added"));

        var card = new ClientCard(Guid.NewGuid(), Id, balance, type, externalCardId, creditLimit, currencyCode,
            maskedPan, iban);

        _cards.Add(card);
        AddDomainEvent(new CardsHasBeenAddedEvent(card.Id));
        return card;
    }

    /// <param name="plainToken">the Monobank token (only its hash is kept readable)</param>
    /// <param name="encryptedToken">the same token, encrypted by the caller</param>
    public void SetToken(string plainToken, string encryptedToken)
    {
        TokenHash = Shared.TokenHash.Of(plainToken);
        EncryptedToken = encryptedToken;
        LegacyPlainToken = null;
    }

    public void SetWebhook(string url)
    {
        WebhookUrl = url;
    }

    public static Client Create(string name, string plainToken, string encryptedToken)
    {
        var client = new Client(Guid.NewGuid(), name);
        client.SetToken(plainToken, encryptedToken);
        return client;
    }
}

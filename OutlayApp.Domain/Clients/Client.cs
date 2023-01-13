using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients.Events;
using OutlayApp.Domain.SeedWork;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Domain.Clients;

public sealed class Client : Entity, IAggregateRoot
{
    public string Name { get; private set; } 

    public string PersonalToken { get; private set; }

    private readonly List<ClientCard> _cards = new();
    public IReadOnlyCollection<ClientCard> Cards => _cards;

    private Client() : base(Guid.NewGuid())
    {
    }

    private Client(Guid id, string name, string personalToken)
        : base(id)
    {
        Name = name;
        PersonalToken = personalToken;
    }

    public Result<ClientCard> AddCard(decimal balance, string type, string externalCardId,
        int creditLimit, int currencyCode)
    {
        if (_cards.Any(x => x.ExternalCardId == externalCardId))
            return Result.Failure<ClientCard>(new Error("ClientCard.AlreadyAdded", "This card already has been added"));
        
        var card = new ClientCard(Guid.NewGuid(), Id, balance,  type, 
            externalCardId, creditLimit, currencyCode);
        
        _cards.Add(card);
        AddDomainEvent(new CardsHasBeenAddedEvent(Id));
        return card;
    }

    public static Client Create(string name, string personalToken)
    {
        return new Client(Guid.NewGuid(), name, personalToken);
    }
}
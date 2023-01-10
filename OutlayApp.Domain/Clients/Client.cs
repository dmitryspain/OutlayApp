using OutlayApp.Domain.Clients.Cards;
using OutlayApp.Domain.Clients.Transactions;
using OutlayApp.Domain.SeedWork;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Domain.Clients;

public sealed class Client : Entity, IAggregateRoot
{
    public string Name { get; private set; } // todo try remove 2nd section

    public string PersonalToken { get; private set; }

    private readonly List<ClientCard> _cards = new();
    public IReadOnlyCollection<ClientCard> Cards => _cards;

    private readonly List<ClientTransaction> _transactions = new();
    public IReadOnlyCollection<ClientTransaction> Transactions => _transactions;

    private Client() : base(Guid.NewGuid())
    {
    }

    private Client(Guid id, string name, string personalToken)
        : base(id)
    {
        Name = name;
        PersonalToken = personalToken;
    }

    public Result<ClientTransaction> AddTransaction(Guid clientId, string description,
        Guid cardId,
        decimal amount, DateTime dateOccured)
    {
        var transaction = ClientTransaction.Create(Guid.NewGuid(), clientId, cardId, description, amount, dateOccured);
        _transactions.Add(transaction);
        return transaction;
    }

    public Result<ClientCard> AddCard(decimal balance, string type, string externalCardId,
        int creditLimit, int currencyCode)
    {
        if (_cards.Any(x => x.ExternalCardId == externalCardId))
            return Result.Failure<ClientCard>(new Error("ClientCard.AlreadyAdded", "This card already has been added"));
        
        var card = new ClientCard(Guid.NewGuid(), Id, balance,  type, 
            externalCardId, creditLimit, currencyCode);
        
        _cards.Add(card);
        return card;
    }

    public static Client Create(string name, string personalToken)
    {
        return new Client(Guid.NewGuid(), name, personalToken);
    }
}
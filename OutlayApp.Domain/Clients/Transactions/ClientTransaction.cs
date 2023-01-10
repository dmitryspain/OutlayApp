using OutlayApp.Domain.SeedWork;

namespace OutlayApp.Domain.Clients.Transactions;

public class ClientTransaction : Entity, IAggregateRoot
{
    public Guid ClientId { get; private set; }
    public Guid CardId { get; private set; }
    public DateTime DateOccured { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    
    private ClientTransaction()
        : base(Guid.NewGuid())
    {
    }

    private ClientTransaction(Guid id, Guid clientId, Guid cardId, string description,
        decimal amount, DateTime dateOccured)
        : base(id)
    {
        ClientId = clientId;
        Description = description;
        Amount = amount;
        DateOccured = dateOccured;
        CardId = cardId;
        //todo balanceAfter
    }

    public static ClientTransaction Create(Guid id,
        Guid clientId, Guid cardId, string description,
        decimal amount, DateTime dateOccured)
    {
        return new ClientTransaction(id, clientId, cardId, description, amount, dateOccured);
    }
}
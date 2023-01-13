using OutlayApp.Domain.SeedWork;

namespace OutlayApp.Domain.ClientTransactions;

public class ClientTransaction : Entity, IAggregateRoot
{
    public Guid ClientCardId { get; private set; }
    public long DateOccured { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    
    private ClientTransaction()
        : base(Guid.NewGuid())
    {
    }

    private ClientTransaction(Guid id, Guid clientCardId, string description, decimal balanceAfter,
        decimal amount, long dateOccured)
        : base(id)
    {
        Description = description;
        Amount = amount;
        DateOccured = dateOccured;
        ClientCardId = clientCardId;
        //todo balanceAfter
    }

    public static ClientTransaction Create(Guid cardId, string description, decimal balanceAfter,
        decimal amount, long dateOccured)
    {
        return new ClientTransaction(Guid.NewGuid(), cardId, description, balanceAfter, amount, dateOccured);
    }
}
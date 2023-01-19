using OutlayApp.Domain.Primitives;

namespace OutlayApp.Domain.ClientTransactions;

public class ClientTransaction : Entity, IAggregateRoot
{
    public Guid ClientCardId { get; private set; }
    public long DateOccured { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public int Mcc { get; private set; }

    private ClientTransaction()
        : base(Guid.NewGuid())
    {
    }

    private ClientTransaction(Guid id, Guid clientCardId, string description, decimal amount,
        decimal balanceAfter, long dateOccured, int mcc)
        : base(id)
    {
        ClientCardId = clientCardId;
        Description = description;
        Amount = amount;
        BalanceAfter = balanceAfter;
        DateOccured = dateOccured;
        Mcc = mcc;
    }

    public static ClientTransaction Create(Guid cardId, string description, decimal amount,
        decimal balanceAfter, long dateOccured, int mcc)
    {
        return new ClientTransaction(Guid.NewGuid(), cardId, description, amount, balanceAfter, dateOccured, mcc);
    }
}
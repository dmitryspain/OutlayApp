using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.SeedWork;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Domain.ClientCards;

public class ClientCard : Entity, IAggregateRoot
{
    public Guid ClientId { get; private set; }
    public decimal Balance { get; private set; }
    public string Type { get;  private set; } 
    public int CreditLimit { get; private set; }
    public int CurrencyCode { get; private set; }
    public string ExternalCardId { get; private set; }
    
    private readonly List<ClientTransaction> _transactions = new();
    public IReadOnlyCollection<ClientTransaction> Transactions => _transactions;

    private ClientCard() : base(Guid.NewGuid())
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
    
    public Result<ClientTransaction> AddTransaction(string description,
        decimal amount, decimal balanceAfter, long dateOccured)
    {
        var transaction = ClientTransaction.Create(Id, description, amount, balanceAfter, dateOccured);
       _transactions.Add(transaction);
        return transaction;
    }
}
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Primitives;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Domain.ClientCards;

public class ClientCard : Entity, IAggregateRoot
{
    public Guid ClientId { get; private set; }
    /// <summary>In whole currency units (hryvnias), like transaction amounts.</summary>
    public decimal Balance { get; private set; }
    public string Type { get; private set; } = string.Empty;
    /// <summary>In whole currency units.</summary>
    public decimal CreditLimit { get; private set; }
    public int CurrencyCode { get; private set; }
    public string ExternalCardId { get; private set; } = string.Empty;
    public string? MaskedPan { get; private set; }
    public string? Iban { get; private set; }

    private readonly List<ClientTransaction> _transactions = new();
    public IReadOnlyCollection<ClientTransaction> Transactions => _transactions;

    private ClientCard() : base(Guid.NewGuid())
    {
    }

    internal ClientCard(Guid id, Guid clientId, decimal balance, string type, string externalCardId,
        decimal creditLimit, int currencyCode, string? maskedPan, string? iban)
        : base(id)
    {
        ClientId = clientId;
        Balance = balance;
        Type = type;
        ExternalCardId = externalCardId;
        CreditLimit = creditLimit;
        CurrencyCode = currencyCode;
        MaskedPan = maskedPan;
        Iban = iban;
    }

    public Result<ClientTransaction> AddTransaction(TransactionDetails details)
    {
        var transaction = ClientTransaction.Create(Id, details);
        _transactions.Add(transaction);
        return transaction;
    }

    public void UpdateBalance(decimal newBalance)
    {
        Balance = newBalance;
    }

    /// <summary>Refreshes what the bank reports about the account.</summary>
    public void UpdateAccount(decimal balance, decimal creditLimit, string type, string? maskedPan, string? iban)
    {
        Balance = balance;
        CreditLimit = creditLimit;
        Type = type;
        MaskedPan = maskedPan ?? MaskedPan;
        Iban = iban ?? Iban;
    }
}

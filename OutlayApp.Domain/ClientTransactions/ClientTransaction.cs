using OutlayApp.Domain.Primitives;

namespace OutlayApp.Domain.ClientTransactions;

public class ClientTransaction : Entity, IAggregateRoot
{
    public Guid ClientCardId { get; private set; }
    /// <summary>UTC.</summary>
    public DateTime DateOccured { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public int Mcc { get; private set; }
    /// <summary>Monobank's own statement item id. Null for rows imported before it was stored.</summary>
    public string? ExternalId { get; private set; }
    /// <summary>The other party of a transfer, as the bank names it.</summary>
    public string? CounterName { get; private set; }
    /// <summary>The payer's note on a transfer.</summary>
    public string? Comment { get; private set; }
    public decimal Cashback { get; private set; }
    /// <summary>Still being processed by the bank (authorised, not settled).</summary>
    public bool Hold { get; private set; }

    private ClientTransaction()
        : base(Guid.NewGuid())
    {
    }

    private ClientTransaction(Guid id, Guid clientCardId, TransactionDetails d)
        : base(id)
    {
        ClientCardId = clientCardId;
        Apply(d);
    }

    public static ClientTransaction Create(Guid cardId, TransactionDetails details)
    {
        if (details.DateOccuredUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateOccured must be UTC", nameof(details));
        return new ClientTransaction(Guid.NewGuid(), cardId, details);
    }

    /// <summary>A row stored before ExternalId existed was matched to this bank item: take over its id and details.</summary>
    public void AdoptBankItem(TransactionDetails details)
    {
        Apply(details);
    }

    /// <summary>The bank finished processing it.</summary>
    public void Settle()
    {
        Hold = false;
    }

    private void Apply(TransactionDetails d)
    {
        Description = d.Description;
        Amount = d.Amount;
        BalanceAfter = d.BalanceAfter;
        DateOccured = d.DateOccuredUtc;
        Mcc = d.Mcc;
        ExternalId = d.ExternalId;
        CounterName = string.IsNullOrWhiteSpace(d.CounterName) ? null : d.CounterName;
        Comment = string.IsNullOrWhiteSpace(d.Comment) ? null : d.Comment;
        Cashback = d.Cashback;
        Hold = d.Hold;
    }
}

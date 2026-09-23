using OutlayApp.Application.Configuration.Extensions;
using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Application.Transactions;

/// <summary>A statement item as Monobank sends it (money in minor units, time in unix seconds).</summary>
public class MonobankTransaction
{
    public string Id { get; set; } = string.Empty;
    public long Time { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Mcc { get; set; }
    public int OriginalMcc { get; set; }
    public bool Hold { get; set; }
    public long Amount { get; set; }
    public long OperationAmount { get; set; }
    public int CurrencyCode { get; set; }
    public long CommissionRate { get; set; }
    public long CashbackAmount { get; set; }
    public long Balance { get; set; }
    public string? Comment { get; set; }
    public string? ReceiptId { get; set; }
    public string? InvoiceId { get; set; }
    public string? CounterEdrpou { get; set; }
    public string? CounterIban { get; set; }
    public string? CounterName { get; set; }

    public DateTime TimeUtc => DateTimeOffset.FromUnixTimeSeconds(Time).UtcDateTime;

    public TransactionDetails ToDetails() => new(
        Description,
        Amount.ToDecimal(),
        Balance.ToDecimal(),
        TimeUtc,
        Mcc,
        Id,
        CounterName,
        Comment,
        CashbackAmount.ToDecimal(),
        Hold);
}

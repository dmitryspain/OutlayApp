namespace OutlayApp.Application.Transactions;

public class MonobankTransaction
{
    public string Id { get; set; }
    public int Time { get; set; }
    public string Description { get; set; }
    public int Mcc { get; set; }
    public int OriginalMcc { get; set; }
    public bool Hold { get; set; }
    public int Amount { get; set; }
    public int OperationAmount { get; set; }
    public int CurrencyCode { get; set; }
    public int CommissionRate { get; set; }
    public int CashbackAmount { get; set; }
    public int Balance { get; set; }
    public string? Comment { get; set; }
    public string? ReceiptId { get; set; }
    public string? InvoiceId { get; set; }
    public string? CounterEdrpou { get; set; }
    public string? CounterIban { get; set; }

    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");

    /// <summary>When it happened, in Kyiv time — the way transactions are stored.</summary>
    public DateTime LocalTime =>
        TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(Time).DateTime, TimeZoneInfo.Utc, Kyiv);
}
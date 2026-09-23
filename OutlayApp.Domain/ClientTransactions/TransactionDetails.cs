namespace OutlayApp.Domain.ClientTransactions;

/// <summary>Everything the bank tells about one statement item. Money in whole units, time in UTC.</summary>
public sealed record TransactionDetails(
    string Description,
    decimal Amount,
    decimal BalanceAfter,
    DateTime DateOccuredUtc,
    int Mcc,
    string? ExternalId,
    string? CounterName = null,
    string? Comment = null,
    decimal Cashback = 0,
    bool Hold = false);

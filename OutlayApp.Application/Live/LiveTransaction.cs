namespace OutlayApp.Application.Live;

/// <summary>Payload of a <see cref="LiveEventTypes.Transaction"/> event.</summary>
public sealed record LiveTransaction(string Description, decimal Amount, DateTime DateOccured, int Mcc,
    decimal BalanceAfter);

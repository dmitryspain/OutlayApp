using OutlayApp.Application.Transactions;

namespace OutlayApp.Application.Webhooks;

/// <summary>What Monobank POSTs to the webhook: <c>{ type: "StatementItem", data: { account, statementItem } }</c>.</summary>
public sealed class MonobankWebhookPayload
{
    public const string StatementItemType = "StatementItem";

    public string Type { get; set; } = string.Empty;
    public MonobankWebhookData? Data { get; set; }
}

public sealed class MonobankWebhookData
{
    public string Account { get; set; } = string.Empty;
    public MonobankTransaction? StatementItem { get; set; }
}

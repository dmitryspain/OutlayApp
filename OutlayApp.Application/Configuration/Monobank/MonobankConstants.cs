namespace OutlayApp.Application.Configuration.Monobank;

public static class MonobankConstants
{
    public const string HttpClient = "MonobankClient";
    public const string Name = "Monobank";
    public const string TokenHeader = "X-Token";
    /// <summary>Longest period a single statement request may cover (31 days + 1 hour).</summary>
    public const int MaxStatementSeconds = 31 * 24 * 3600 + 3600;
    /// <summary>A statement page holds at most this many items; a full page means there are older ones.</summary>
    public const int StatementPageSize = 500;
    /// <summary>Each personal endpoint may be called once per this interval per token.</summary>
    public static readonly TimeSpan RequestInterval = TimeSpan.FromSeconds(61);
}

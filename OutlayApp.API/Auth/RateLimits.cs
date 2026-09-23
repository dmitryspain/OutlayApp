namespace OutlayApp.API.Auth;

public static class RateLimits
{
    /// <summary>Connecting takes a Monobank token: slow down guessing.</summary>
    public const string Connect = "connect";
}

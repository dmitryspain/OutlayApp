using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Monobank;

/// <summary>Turns Monobank failures into <see cref="Error"/>s the UI can show as they are.</summary>
public static class MonobankErrors
{
    public const string RateLimited = "Monobank.RateLimited";
    public const string Unauthorized = "Monobank.Unauthorized";
    public const string Failed = "Monobank.Failed";

    public static Error From(MonobankException ex) => ex switch
    {
        MonobankRateLimitException r => new Error(RateLimited,
            $"Банк дозволяє один запит на хвилину — спробуйте через {Math.Ceiling(r.RetryAfter.TotalSeconds)} с."),
        MonobankUnauthorizedException => new Error(Unauthorized, "Монобанк не прийняв токен. Перевірте або створіть новий."),
        _ => new Error(Failed, $"Монобанк не відповів як слід ({ex.Status?.ToString() ?? "мережа"})."),
    };
}

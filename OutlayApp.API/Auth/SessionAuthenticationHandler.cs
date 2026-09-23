using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Sessions;
using OutlayApp.Domain.Shared;

namespace OutlayApp.API.Auth;

/// <summary>
/// "Authorization: Bearer ots_…" → the session's client. Only /api/events may pass it as ?access_token=,
/// because EventSource cannot send headers. Sessions are cached briefly to spare a query per request.
/// </summary>
public sealed class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Session";
    public const string ClientIdClaim = "client_id";
    private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(1);

    private readonly IClientSessionRepository _sessions;
    private readonly IMemoryCache _cache;

    public SessionAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, IClientSessionRepository sessions, IMemoryCache cache)
        : base(options, logger, encoder)
    {
        _sessions = sessions;
        _cache = cache;
    }

    public static string CacheKey(string tokenHash) => $"session:{tokenHash}";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = ReadToken();
        if (token is null)
            return AuthenticateResult.NoResult();
        if (!token.StartsWith(ClientSession.TokenPrefix, StringComparison.Ordinal))
            return AuthenticateResult.Fail("Not a session token");

        var hash = TokenHash.Of(token);
        if (!_cache.TryGetValue(CacheKey(hash), out Guid? clientId))
        {
            var session = await _sessions.GetByTokenHash(hash, Context.RequestAborted);
            clientId = session is not null && session.IsActive(DateTime.UtcNow) ? session.ClientId : null;
            _cache.Set(CacheKey(hash), clientId, CacheFor);
        }
        if (clientId is null)
            return AuthenticateResult.Fail("Session expired or revoked");

        var identity = new ClaimsIdentity(new[] { new Claim(ClientIdClaim, clientId.Value.ToString()) }, SchemeName);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
    }

    private string? ReadToken()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return header["Bearer ".Length..].Trim();
        if (Request.Path.StartsWithSegments("/api/events") && Request.Query.TryGetValue("access_token", out var q))
            return q.ToString();
        return null;
    }
}

public static class ClaimsPrincipalExtensions
{
    public static Guid ClientId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(SessionAuthenticationHandler.ClientIdClaim)
                   ?? throw new InvalidOperationException("Not signed in"));
}

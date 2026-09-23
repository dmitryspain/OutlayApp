using System.Security.Cryptography;
using OutlayApp.Domain.Primitives;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Domain.Sessions;

/// <summary>
/// A signed-in browser. The Monobank token is sent once to open a session; afterwards the browser only
/// holds this session's own token (only its hash is stored), which can be revoked without touching the bank.
/// </summary>
public sealed class ClientSession : Entity, IAggregateRoot
{
    public const string TokenPrefix = "ots_";
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(180);

    public Guid ClientId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private ClientSession() : base(Guid.NewGuid())
    {
    }

    private ClientSession(Guid id, Guid clientId, string tokenHash, DateTime now) : base(id)
    {
        ClientId = clientId;
        TokenHash = tokenHash;
        CreatedAtUtc = now;
        ExpiresAtUtc = now + Lifetime;
    }

    /// <summary>Opens a session; the plain token is returned once and never stored.</summary>
    public static (ClientSession Session, string Token) Open(Guid clientId, DateTime nowUtc)
    {
        var token = TokenPrefix + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (new ClientSession(Guid.NewGuid(), clientId, Shared.TokenHash.Of(token), nowUtc), token);
    }

    public bool IsActive(DateTime nowUtc) => nowUtc < ExpiresAtUtc;
}

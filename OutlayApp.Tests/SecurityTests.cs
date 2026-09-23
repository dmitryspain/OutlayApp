using OutlayApp.Application.Monobank;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Sessions;
using OutlayApp.Domain.Shared;
using OutlayApp.Infrastructure.Monobank;

namespace OutlayApp.Tests;

public class SecurityTests
{
    [Fact]
    public void Session_tokens_are_random_and_only_their_hash_is_kept()
    {
        var now = DateTime.UtcNow;
        var (a, tokenA) = ClientSession.Open(Guid.NewGuid(), now);
        var (_, tokenB) = ClientSession.Open(Guid.NewGuid(), now);

        Assert.StartsWith(ClientSession.TokenPrefix, tokenA);
        Assert.NotEqual(tokenA, tokenB);
        Assert.Equal(TokenHash.Of(tokenA), a.TokenHash);
        Assert.DoesNotContain(tokenA, a.TokenHash);
        Assert.True(a.IsActive(now.AddDays(179)));
        Assert.False(a.IsActive(now.AddDays(181)));
    }

    [Fact]
    public void Client_keeps_the_token_hashed_and_encrypted_only()
    {
        var client = Client.Create("Name", "plain-token", "ENCRYPTED");
        Assert.Equal(TokenHash.Of("plain-token"), client.TokenHash);
        Assert.Equal("ENCRYPTED", client.EncryptedToken);
        Assert.Null(client.LegacyPlainToken);
    }

    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 9, 23, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void Rate_limiter_allows_one_call_a_minute_per_token_and_endpoint()
    {
        var time = new ManualTime();
        var limiter = new MonobankRateLimiter(time);

        limiter.Acquire("t1", "statement");
        var ex = Assert.Throws<MonobankRateLimitException>(() => limiter.Acquire("t1", "statement"));
        Assert.True(ex.RetryAfter > TimeSpan.FromSeconds(55));

        limiter.Acquire("t1", "client-info"); // another endpoint
        limiter.Acquire("t2", "statement");   // another token

        time.Now = time.Now.AddSeconds(62);
        limiter.Acquire("t1", "statement");
    }
}

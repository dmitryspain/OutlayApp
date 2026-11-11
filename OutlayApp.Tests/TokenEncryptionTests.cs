using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.DataProtection;
using OutlayApp.Infrastructure.Security;

namespace OutlayApp.Tests;

/// <summary>
/// Monobank tokens at rest: <see cref="TokenProtector"/> over ASP.NET Core Data Protection (in-memory keys here,
/// keys from the database in the app), and the patched crypto packages that Data Protection pulls in.
/// </summary>
public class TokenEncryptionTests
{
    private const string Token = "monobank-token-0123456789abcdefghijklmnopqrstu";

    // Purpose the tokens already in the database were encrypted with
    private const string StoredPurpose = "Outlay.MonobankToken.v1";

    // Fixes GHSA-r955-8q5j-3h83, GHSA-7hqp-7c5m-4vq6, GHSA-c399-9853-6jmj (Xml) and GHSA-846f-f764-76mx,
    // GHSA-2fmv-rv2c-59x7 (Pkcs)
    private static readonly Version Patched = new(10, 0, 13);

    [Fact]
    public void Decrypts_what_it_encrypted()
    {
        var protector = new TokenProtector(new EphemeralDataProtectionProvider());

        Assert.Equal(Token, protector.Unprotect(protector.Protect(Token)));
    }

    [Fact]
    public void Encrypted_value_does_not_reveal_the_token()
    {
        var protector = new TokenProtector(new EphemeralDataProtectionProvider());

        var first = protector.Protect(Token);
        var second = protector.Protect(Token);

        Assert.DoesNotContain(Token, first);
        Assert.NotEqual(first, second); // randomized: an encoding, or equal tokens of two clients, would match
    }

    [Fact]
    public void Reads_tokens_stored_under_the_v1_purpose()
    {
        // Renaming the purpose would make every token in the database unreadable
        var provider = new EphemeralDataProtectionProvider();
        var stored = provider.CreateProtector(StoredPurpose).Protect(Token);

        Assert.Equal(Token, new TokenProtector(provider).Unprotect(stored));
    }

    [Fact]
    public void Another_purpose_cannot_decrypt_the_token()
    {
        var provider = new EphemeralDataProtectionProvider();
        var stored = new TokenProtector(provider).Protect(Token);

        Assert.ThrowsAny<CryptographicException>(() =>
            provider.CreateProtector("Outlay.MonobankToken.v2").Unprotect(stored));
    }

    [Fact]
    public void Other_keys_cannot_decrypt_the_token()
    {
        var stored = new TokenProtector(new EphemeralDataProtectionProvider()).Protect(Token);
        var otherKeys = new TokenProtector(new EphemeralDataProtectionProvider());

        Assert.ThrowsAny<CryptographicException>(() => otherKeys.Unprotect(stored));
    }

    [Theory]
    [InlineData(typeof(SignedXml))]
    [InlineData(typeof(SignedCms))]
    public void Loads_the_patched_crypto_packages(Type type)
    {
        // What counts is the copy the process loads, not the restored package: Microsoft.AspNetCore.App 8.0 has
        // its own older ones (assembly 8.0.0.0). 10.0.12 and 10.0.13 are both assembly 10.0.0.0, so compare the
        // product version.
        var assembly = type.Assembly;
        var name = assembly.GetName();
        var product = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        var version = Version.Parse(product.Split('+', '-')[0]);

        Assert.True(version >= Patched, $"{name.Name} {product} from {assembly.Location} is older than {Patched}");
    }
}

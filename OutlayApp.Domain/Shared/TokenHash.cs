using System.Security.Cryptography;
using System.Text;

namespace OutlayApp.Domain.Shared;

/// <summary>SHA-256 of a secret, hex — lets us look a token up without storing it readable.</summary>
public static class TokenHash
{
    public static string Of(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}

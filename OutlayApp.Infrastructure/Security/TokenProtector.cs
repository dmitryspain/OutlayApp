using Microsoft.AspNetCore.DataProtection;
using OutlayApp.Application.Security;

namespace OutlayApp.Infrastructure.Security;

/// <summary>ASP.NET Core Data Protection (keys kept in the database, so every instance can decrypt).</summary>
public sealed class TokenProtector : ITokenProtector
{
    private readonly IDataProtector _protector;

    public TokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Outlay.MonobankToken.v1");
    }

    public string Protect(string plain) => _protector.Protect(plain);
    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}

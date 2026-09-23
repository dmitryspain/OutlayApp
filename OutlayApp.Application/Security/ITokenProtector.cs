namespace OutlayApp.Application.Security;

/// <summary>Encrypts Monobank tokens at rest.</summary>
public interface ITokenProtector
{
    string Protect(string plain);
    string Unprotect(string protectedValue);
}

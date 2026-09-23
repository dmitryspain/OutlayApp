using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using OutlayApp.Application.Auth;
using OutlayApp.Domain.Shared;

namespace OutlayApp.API.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMemoryCache _cache;

    public AuthController(ISender sender, IMemoryCache cache)
    {
        _sender = sender;
        _cache = cache;
    }

    public sealed record ConnectRequest(string Token);

    /// <summary>Exchanges a Monobank token (sent in the body, never in the URL) for a session token.</summary>
    [HttpPost("session")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Connect)]
    public async Task<IActionResult> Connect([FromBody] ConnectRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConnectCommand(request.Token ?? string.Empty), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Signs this browser out (revokes its session).</summary>
    [HttpDelete("session")]
    [Authorize]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        var token = Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();
        await _sender.Send(new SignOutCommand(token), cancellationToken);
        _cache.Remove(SessionAuthenticationHandler.CacheKey(TokenHash.Of(token)));
        return NoContent();
    }
}

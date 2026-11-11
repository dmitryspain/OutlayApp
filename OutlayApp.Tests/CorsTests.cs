using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace OutlayApp.Tests;

/// <summary>
/// CORS of the API in Development: Cors:Origins comes from the real appsettings files (not a copy, so a broken
/// config fails here), registered and applied with the same calls as Program.cs. The iOS app's WebView
/// (Capacitor) calls the API from capacitor://localhost.
/// </summary>
public class CorsTests
{
    private const string IosApp = "capacitor://localhost";
    private const string OtherSite = "https://example.com";

    [Theory]
    [InlineData(IosApp)]
    [InlineData("http://localhost:4200")]
    [InlineData("http://localhost:4300")]
    public async Task Development_allows_the_ios_app_and_both_local_uis(string origin)
    {
        Assert.True((await DefaultPolicy()).IsOriginAllowed(origin));
    }

    [Fact]
    public async Task Development_does_not_allow_other_sites()
    {
        var policy = await DefaultPolicy();

        // IsOriginAllowed ignores a lone "*", which CorsService treats as "any origin"
        Assert.False(policy.AllowAnyOrigin);
        Assert.False(policy.IsOriginAllowed(OtherSite));
    }

    [Fact]
    public async Task Preflight_from_the_ios_app_is_allowed_with_the_session_header()
    {
        var response = await Send(Preflight(IosApp));

        Assert.Equal(StatusCodes.Status204NoContent, response.StatusCode);
        Assert.Equal(IosApp, response.Headers[HeaderNames.AccessControlAllowOrigin].ToString());
        Assert.Contains("authorization", response.Headers[HeaderNames.AccessControlAllowHeaders].ToString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(HttpMethods.Get, response.Headers[HeaderNames.AccessControlAllowMethods].ToString());
    }

    [Fact]
    public async Task Preflight_from_another_site_gets_no_allow_origin()
    {
        // ASP.NET Core still answers 204; it is the missing header that makes the browser block the call
        var response = await Send(Preflight(OtherSite));

        Assert.False(response.Headers.ContainsKey(HeaderNames.AccessControlAllowOrigin));
    }

    [Fact]
    public async Task A_401_for_the_ios_app_still_carries_its_origin()
    {
        // without the header the WebView hides the 401 and the UI shows "no network" instead of "session expired"
        var response = await Send(request =>
        {
            request.Method = HttpMethods.Get;
            request.Path = "/api/clients/cards";
            request.Headers.Origin = IosApp;
        });

        Assert.Equal(StatusCodes.Status401Unauthorized, response.StatusCode);
        Assert.Equal(IosApp, response.Headers[HeaderNames.AccessControlAllowOrigin].ToString());
    }

    private static Action<HttpRequest> Preflight(string origin) => request =>
    {
        request.Method = HttpMethods.Options;
        request.Path = "/api/clients/cards";
        request.Headers.Origin = origin;
        request.Headers.AccessControlRequestMethod = HttpMethods.Get;
        request.Headers.AccessControlRequestHeaders = "authorization";
    };

    /// <summary>The policy UseCors() resolves for every endpoint.</summary>
    private static async Task<CorsPolicy> DefaultPolicy() =>
        await Services().GetRequiredService<ICorsPolicyProvider>().GetPolicyAsync(new DefaultHttpContext(), null)
        ?? throw new InvalidOperationException("No default CORS policy");

    /// <summary>
    /// Program.cs order: UseCors, then authentication and authorization. Here the latter is a stand-in that rejects
    /// the anonymous call the way the fallback policy does.
    /// </summary>
    private static async Task<FakeResponseFeature> Send(Action<HttpRequest> setUp)
    {
        var services = Services();
        var app = new ApplicationBuilder(services);
        app.UseCors();
        app.Run(context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        });

        var response = new FakeResponseFeature();
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature());
        features.Set<IHttpResponseFeature>(response);
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(Stream.Null));
        var context = new DefaultHttpContext(features) { RequestServices = services };
        setUp(context.Request);

        await app.Build()(context);
        await response.Start(); // the server sends the headers
        return response;
    }

    /// <summary>The CORS registration of Program.cs, fed with the Development configuration.</summary>
    private static IServiceProvider Services()
    {
        var origins = DevelopmentConfiguration().GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
        return new ServiceCollection()
            .AddLogging()
            .AddCors(o => o.AddDefaultPolicy(p => p
                .WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader()))
            .BuildServiceProvider();
    }

    /// <summary>The same files in the same order as WebApplication.CreateBuilder in Development (arrays merge by index).</summary>
    private static IConfiguration DevelopmentConfiguration()
    {
        var api = Path.Combine(RepositoryRoot(), "OutlayApp.API");
        return new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(api, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(api, "appsettings.Development.json"), optional: false)
            .Build();
    }

    private static string RepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "OutlayApp.API", "appsettings.Development.json")))
                return dir.FullName;
        }
        throw new InvalidOperationException($"OutlayApp.API/appsettings.Development.json not found above {AppContext.BaseDirectory}");
    }
}

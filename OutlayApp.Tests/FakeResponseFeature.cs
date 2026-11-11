using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace OutlayApp.Tests;

/// <summary>
/// Response without a server: OnStarting callbacks (CORS adds its headers in one) run only when
/// <see cref="Start"/> is called, as a server runs them right before sending the headers.
/// </summary>
internal sealed class FakeResponseFeature : IHttpResponseFeature
{
    private readonly List<(Func<object, Task> Callback, object State)> _onStarting = new();

    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public string? ReasonPhrase { get; set; }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public Stream Body { get; set; } = Stream.Null;
    public bool HasStarted { get; private set; }

    public void OnStarting(Func<object, Task> callback, object state) => _onStarting.Add((callback, state));

    public void OnCompleted(Func<object, Task> callback, object state)
    {
    }

    public async Task Start()
    {
        // like Kestrel: the last registered callback runs first
        for (var i = _onStarting.Count - 1; i >= 0; i--)
            await _onStarting[i].Callback(_onStarting[i].State);
        HasStarted = true;
    }
}

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Kunet.AspNetCore.Plugable;

internal sealed class ActionDescriptorChangeProvider : IActionDescriptorChangeProvider
{
    public static readonly ActionDescriptorChangeProvider Instance = new();

    private CancellationTokenSource _tokenSource = new();

    public IChangeToken GetChangeToken()
    {
        if (_tokenSource.IsCancellationRequested)
        {
            _tokenSource = new CancellationTokenSource();
        }
        return new CancellationChangeToken(_tokenSource.Token);
    }

    public void NotifyChanged() => _tokenSource.Cancel();
}
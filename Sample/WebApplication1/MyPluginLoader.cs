using Kunet.AspNetCore.Plugable;
using System.Reflection;

namespace WebApplication1;

public sealed class MyPluginLoader : PluginLoader
{
    private readonly ILogger<MyPluginLoader> _logger;

    public MyPluginLoader(ILogger<MyPluginLoader> logger)
    {
        _logger = logger;
    }

    public override async Task<Assembly> LoadAsync(HttpContext httpContext)
    {
        var request = httpContext.Request;

        if (request.Query.TryGetValue("area", out var assemblyFullName) && TryGetLoaded(assemblyFullName, out var assembly))
        {
            return assembly;
        }

        if (request.Query.TryGetValue("pluginId", out var pluginId))
        {
            return await LoadPluginFromAnywhere(pluginId);
        }

        return null;
    }

    // TODO: thread safe
    public async Task<Assembly> LoadPluginFromAnywhere(string pluginId)
    {
        await Task.Delay(1000);

        return pluginId switch
        {
            "1" => Assembly.LoadFile(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\RazorClassLibrary1\\bin\\Debug\\net8.0\\RazorClassLibrary1.dll")),
            _ => null
        };
    }
}
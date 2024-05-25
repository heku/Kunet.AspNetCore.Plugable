using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Kunet.AspNetCore.Plugable;

public abstract class PluginLoader
{
    internal static ConcurrentDictionary<string, Assembly> LoadedPlugins { get; } = new(StringComparer.Ordinal);

    protected internal static bool TryGetLoaded(string assemblyFullName, [NotNullWhen(true)] out Assembly? assembly)
    {
        return LoadedPlugins.TryGetValue(assemblyFullName, out assembly);
    }

    public abstract Task<Assembly?> LoadAsync(HttpContext httpContext);
}
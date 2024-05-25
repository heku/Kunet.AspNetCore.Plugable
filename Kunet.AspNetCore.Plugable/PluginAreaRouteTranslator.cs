using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Kunet.AspNetCore.Plugable;

internal sealed class PluginAreaRouteTranslator : DynamicRouteValueTransformer
{
    private readonly PluginLoader _pluginLoader;

    public PluginAreaRouteTranslator(PluginLoader pluginLoader)
    {
        _pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
    }

    public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        if (!values.ContainsKey("area") && await LoadPlugin(httpContext) is { } assembly)
        {
            values["area"] = assembly.FullName;
        }

        if (values["page"] is string page && !page.StartsWith("/"))
        {
            values["page"] = "/" + page;
        }

        return values;
    }

    private async Task<Assembly?> LoadPlugin(HttpContext httpContext)
    {
        if (await _pluginLoader.LoadAsync(httpContext) is { } assembly)
        {
            httpContext.RequestServices.AddApplicationParts(assembly);
            return assembly;
        }
        return null;
    }
}
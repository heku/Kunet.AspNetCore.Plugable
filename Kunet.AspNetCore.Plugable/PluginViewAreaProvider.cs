using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Kunet.AspNetCore.Plugable;

internal sealed class PluginViewAreaProvider : IApplicationFeatureProvider<ViewsFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
    {
        foreach (var descriptor in feature.ViewDescriptors)
        {
            if (descriptor.Type?.Assembly.FullName is string assembly && PluginLoader.TryGetLoaded(assembly, out _))
            {
                descriptor.RelativePath = $"/Areas/{assembly}{descriptor.RelativePath}";
            }
        }
    }
}
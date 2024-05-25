using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Kunet.AspNetCore.Plugable;

internal sealed class PluginControllerAreaProvider : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType.Assembly.FullName is string assembly && PluginLoader.TryGetLoaded(assembly, out _))
        {
            controller.RouteValues["area"] = assembly;
        }
    }
}
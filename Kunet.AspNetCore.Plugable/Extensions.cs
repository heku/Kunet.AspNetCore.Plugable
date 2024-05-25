using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Kunet.AspNetCore.Plugable;

public static class Extensions
{
    public static IMvcBuilder AddPluginLoader<TPluginLoader>(this IMvcBuilder builder) where TPluginLoader : PluginLoader
    {
        builder.AddMvcOptions(options => options.Conventions.Add(new PluginControllerAreaProvider()));
        builder.ConfigureApplicationPartManager(apm => apm.FeatureProviders.Add(new PluginViewAreaProvider()));

        builder.Services.AddSingleton<PluginAreaRouteTranslator>();
        builder.Services.AddSingleton<PluginLoader, TPluginLoader>();
        builder.Services.AddSingleton<IActionDescriptorChangeProvider>(ActionDescriptorChangeProvider.Instance);

        return builder;
    }

    public static void UsePluginStaticFiles(this IApplicationBuilder app)
    {
        app.UseMiddleware<PluginStaticFileMiddleware>();
    }

    public static void UsePluginStaticFiles(this IApplicationBuilder app, StaticFileOptions options)
    {
        Debug.Assert(options.FileProvider is null, "The FileProvider will be dynamically created per plugin!");
        app.UseMiddleware<PluginStaticFileMiddleware>(Options.Create(options));
    }

    public static void MapPluginControllerRoute(this IEndpointRouteBuilder app, [StringSyntax("Route")] string pattern, int order = -1)
    {
        app.MapDynamicControllerRoute<PluginAreaRouteTranslator>(pattern, null!, order);
    }

    public static void MapPluginPageRoute(this IEndpointRouteBuilder app, [StringSyntax("Route")] string pattern, int order = -1)
    {
        app.MapDynamicPageRoute<PluginAreaRouteTranslator>(pattern, null!, order);
    }

    public static void AddApplicationParts(this IServiceProvider serviceProvider, Assembly assembly)
    {
        if (PluginLoader.LoadedPlugins.TryAdd(assembly.FullName!, assembly))
        {
            var apm = serviceProvider.GetRequiredService<ApplicationPartManager>();
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            lock (apm)
            {
                foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
                {
                    apm.ApplicationParts.Add(applicationPart);
                }
            }
            serviceProvider.NotifyApplicationPartsChanged();
        }
    }

    public static void RemoveApplicationParts(this IServiceProvider serviceProvider, string assemblyFullName)
    {
        if (PluginLoader.LoadedPlugins.TryRemove(assemblyFullName, out var assembly))
        {
            var apm = serviceProvider.GetRequiredService<ApplicationPartManager>();
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            lock (apm)
            {
                foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
                {
                    var registeredApplicationPart = apm.ApplicationParts.First(x => x.Name == applicationPart.Name && x.GetType() == applicationPart.GetType());
                    apm.ApplicationParts.Remove(registeredApplicationPart);
                }
            }
            serviceProvider.NotifyApplicationPartsChanged();
        }
    }

    // https://github.com/dotnet/aspnetcore/blob/83573c72d12e1c2018805bc5d461d730bcb9dcc4/src/Mvc/Mvc.Razor/src/Compilation/DefaultViewCompiler.cs#L82
    private static Action? ClearCompiledViewsCache;

    private static void NotifyApplicationPartsChanged(this IServiceProvider service)
    {
        if (ClearCompiledViewsCache is null)
        {
            var viewProvider = service.GetRequiredService<IViewCompilerProvider>();
            var viewCompiler = viewProvider.GetCompiler();
            var clearCacheMethod = viewCompiler.GetType().GetMethod("ClearCache", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(clearCacheMethod is not null, "This check would alert us once the MVC internal implementation changed!");
            ClearCompiledViewsCache = clearCacheMethod.CreateDelegate<Action>(viewCompiler);
        }
        ClearCompiledViewsCache();
        ActionDescriptorChangeProvider.Instance.NotifyChanged();
    }
}
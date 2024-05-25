## This repo is created for PoC and hasn't been fully tested!

# Kunet.AspNetCore.Plugable
ASP.NET Core extension for dynamic plugins support.

By default, ASP.NET Core only supports _static_ plugins via [Razor Class Library](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-8.0&tabs=visual-studio)
and [ApplicationParts](https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-8.0).
This extension is developed based on these fundamentals, but provides ability to load a Razor Class Library at runtime.

## Support
- ASP.NET Core MVC
- ASP.NET Core Web API
- ASP.NET Core Razor Pages
- Embedded static files under wwwroot

NOTE: Runtime compilation is not supported.

## Sample

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc()
                .AddPluginLoader<MyPluginLoader>(); // for plugins

var app = builder.Build();

app.UsePluginStaticFiles(); // for plugins
app.UseStaticFiles();

app.UseRouting();

app.MapPluginControllerRoute("{controller=Home}/{action=Index}/{id?}"); // for plugins
app.MapPluginPageRoute("{page}"); // for plugins
app.MapDefaultControllerRoute();

app.Run();
```

Implement the plugin loader
```csharp
public abstract class PluginLoader
{
    public abstract Task<Assembly?> LoadAsync(HttpContext httpContext);
}

// e.g.
public override async Task<Assembly?> LoadAsync(HttpContext httpContext)
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
```

Then requests below will be routed to plugin:

- `/site/css?pluginId=<pluginId>`
- `/home/index?pluginId=<pluginId>`
- `/page?pluginId=<pluginId>`
- `/site/css?area=<plugin assembly full name>`
- `/home/index?area=<plugin assembly full name>`
- `/page?area=<plugin assembly full name>`


## Known problem
Links generation in/for plugins may not work as expected, you'd better test it carefully.
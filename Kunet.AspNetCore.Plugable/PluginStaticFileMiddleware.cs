using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Kunet.AspNetCore.Plugable;

internal sealed class PluginStaticFileMiddleware
{
    public const string BASE_NAMESPACE = "{0}.wwwroot";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _hostingEnv;
    private readonly ILoggerFactory _loggerFactory;
    private readonly PluginLoader _pluginLoader;
    private readonly StaticFileOptions _options;
    private readonly ConditionalWeakTable<Assembly, StaticFileMiddleware> _middlewares = new();

    public PluginStaticFileMiddleware(RequestDelegate next,
                                      IWebHostEnvironment hostingEnv,
                                      ILoggerFactory loggerFactory,
                                      PluginLoader pluginLoader,
                                      IOptions<StaticFileOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _hostingEnv = hostingEnv ?? throw new ArgumentNullException(nameof(hostingEnv));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        if (await _pluginLoader.LoadAsync(context) is { } assembly)
        {
            if (!_middlewares.TryGetValue(assembly, out var middleware))
            {
                middleware = CreateStaticFileMiddleware(assembly);
                _middlewares.Add(assembly, middleware);
            }
            await middleware.Invoke(context);
        }
        else
        {
            await _next.Invoke(context);
        }
    }

    private StaticFileMiddleware CreateStaticFileMiddleware(Assembly assembly)
    {
        var options = CreateOptionsCopy();
        var baseNamespace = string.Format(BASE_NAMESPACE, assembly.GetName().Name);
        options.Value.FileProvider = new EmbeddedFileProvider(assembly, baseNamespace);
        return new StaticFileMiddleware(_next, _hostingEnv, options, _loggerFactory);
    }

    private IOptions<StaticFileOptions> CreateOptionsCopy() => Options.Create(new StaticFileOptions
    {
        ContentTypeProvider = _options.ContentTypeProvider,
        DefaultContentType = _options.DefaultContentType,
        HttpsCompression = _options.HttpsCompression,
        OnPrepareResponse = _options.OnPrepareResponse,
        OnPrepareResponseAsync = _options.OnPrepareResponseAsync,
        RedirectToAppendTrailingSlash = _options.RedirectToAppendTrailingSlash,
        RequestPath = _options.RequestPath,
        ServeUnknownFileTypes = _options.ServeUnknownFileTypes
    });
}
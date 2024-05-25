using Kunet.AspNetCore.Plugable;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc()
                .AddPluginLoader<MyPluginLoader>(); // for plugins

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UsePluginStaticFiles(); // for plugins
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapPluginControllerRoute("{controller=Home}/{action=Index}/{id?}"); // for plugins
app.MapPluginPageRoute("{page}"); // for plugins
app.MapDefaultControllerRoute();

app.Run();
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var name = context.File.Name;
        if (name is "sw.js" or "index.html" or "manifest.json")
        {
            context.Context.Response.Headers.CacheControl = "no-cache";
        }
    }
});

app.Run();

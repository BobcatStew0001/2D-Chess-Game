using Chess_Backend.Logic;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<MoveValidator>();
builder.Services.AddSingleton<BoardEvaluator>();
builder.Services.AddSingleton<ChessAI>();
builder.Services.AddSingleton<PlayerSessions>();
builder.Services.AddSingleton<GameLibrary>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChessGamePolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                {
                    return false;
                }

                return uri.Host is "localhost" or "127.0.0.1" || uri.Port is 5253 or 7166;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("ChessGamePolicy");
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (GameException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(exception.Message);
    }
});
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();

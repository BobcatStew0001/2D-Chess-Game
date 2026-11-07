using Chess_Backend.Logic;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<GameState>();
builder.Services.AddSingleton<MoveValidator>();
builder.Services.AddSingleton<ChessAI>();
builder.Services.AddSingleton<BoardEvaluator>();

var app = builder.Build();
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
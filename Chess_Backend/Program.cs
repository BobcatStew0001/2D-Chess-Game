using Chess_Backend.Logic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<GameState>();
builder.Services.AddSingleton<MoveValidator>();
builder.Services.AddSingleton<ChessAI>();
builder.Services.AddSingleton<BoardEvaluator>();

var app = builder.Build();
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Chess_Backend.Logic;
using Microsoft.OpenApi.Writers;

namespace Chess_Backend.Controllers; 
[ApiController]
[Route("api/[controller]")]

public class SaveController:ControllerBase
{
    private readonly GameState _gameState;
    public SaveController(GameState gameState)
    {
        _gameState = gameState;
    }

    

    [HttpPost("save")]
    public IActionResult SaveGame()
    {
        Directory.CreateDirectory("SavedGames");
        string json = JsonSerializer.Serialize(_gameState);
        System.IO.File.WriteAllText("SavedGames/save.json", json);
        return Ok("Game saved successfully");
    }

    [HttpGet("load")]
    public IActionResult LoadGame()
    {
        string myFilePath = "SavedGames/save.json";
        if (System.IO.File.Exists(myFilePath))
        {
            string jsonstring = System.IO.File.ReadAllText(myFilePath);
            var data = JsonSerializer.Deserialize<GameState>(jsonstring);
            return Ok("Game Loaded");
        }
        else
        {
            return BadRequest("No game found");
        }
    }
}
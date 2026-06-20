using Chess_Backend.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;

public class GameController:ControllerBase
{
    private readonly GameState _gameState;
    public GameController(GameState gameState)
    {
        _gameState = gameState;
    }
    [HttpGet("state")]
    public IActionResult GetGameState()
    {
        return Ok(_gameState);
    }
    [HttpPost("start")]
    public IActionResult StartNewGame()
    {
        return Ok(_gameState);
    }
    [HttpPost("resign")]
    public IActionResult Resign()
    {
        _gameState.Resign();
        return Ok(_gameState);
    }

}
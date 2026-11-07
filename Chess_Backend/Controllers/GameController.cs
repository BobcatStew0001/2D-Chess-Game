using Chess_Backend.Logic;
using Chess_Backend.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;
[ApiController]
[Route("api/[controller]")]

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
        return Ok(GameStateDTO.FromGameState(_gameState));
    }
    [HttpPost("start")]
    public IActionResult StartNewGame()
    {
        _gameState.StartNewGame();
        return Ok(GameStateDTO.FromGameState(_gameState));
    }
    [HttpPost("resign")]
    public IActionResult Resign()
    {
        _gameState.Resign();
        return Ok(GameStateDTO.FromGameState(_gameState));
    }

}
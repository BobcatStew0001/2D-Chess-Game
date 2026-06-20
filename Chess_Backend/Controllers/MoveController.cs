using Microsoft.AspNetCore.Mvc;
using Chess_Backend.Logic;
using Chess_Backend.Models;

namespace Chess_Backend.Controllers;
[ApiController]
[Route("api/[controller]")]

public class MoveController:ControllerBase
{
    private readonly GameState _gameState;
    
    private readonly MoveValidator _moveValidator;
    
    public MoveController(GameState gameState, MoveValidator moveValidator)
    {
        _gameState = gameState;
        _moveValidator = moveValidator;
    }

    [HttpPost("validate")]
    public IActionResult ValidateMove([FromBody] Move move)
    {
        bool isValid = _moveValidator.IsValidMove(move, _gameState.Board);
        return Ok(isValid);
    }
    [HttpPost("make")]
    public IActionResult MakeMove([FromBody] Move move)
    {
        bool isValid = _moveValidator.IsValidMove(move, _gameState.Board);
        if (isValid)
        {
            _gameState.MakeMove(move);
            return Ok(_gameState);
        }
        else
        {
            return BadRequest("Invalid move");
        }
    }

}
using Chess_Backend.Logic;
using Chess_Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;
[ApiController]
[Route("api/[controller]")]

public class AIController:ControllerBase
{
    private readonly ChessAI _ai;
    private readonly GameState _gameState;
    
    public AIController(GameState gameState, ChessAI ai)
    {
        _gameState = gameState;
        _ai = ai;
      
        
    }
[HttpPost("aimove")]
public IActionResult AIMove()
{
    var bestMove = _ai.GetBestMove(_gameState.Board, PieceColor.Black);
    _gameState.MakeMove(bestMove);
    return Ok(bestMove);


}
    
}
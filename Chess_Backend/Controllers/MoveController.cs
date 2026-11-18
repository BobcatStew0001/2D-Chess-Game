using Microsoft.AspNetCore.Mvc;
using Chess_Backend.Logic;
using Chess_Backend.Models;
using Chess_Backend.DTOS;

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

    [HttpGet("legalmoves/{row}/{column}")]
    public IActionResult LegalMoves(int row, int column)
    {
        Position from = new Position(row, column);
        Piece piece = _gameState.Board.GetPieceAt(from);

        if (piece == null || piece.Color != _gameState.WhoseTurn)
        {
            return Ok(new List<Position>());
        }

        List<Position> legalMoves = piece.GetMove(_gameState.Board)
            .Where(target => _moveValidator.IsValidMove(new Move(from, target), _gameState.Board, _gameState.WhoseTurn))
            .ToList();

        return Ok(legalMoves);
    }

    [HttpPost("validate")]
    public IActionResult ValidateMove([FromBody] Move move)
    {
        bool isValid = _moveValidator.IsValidMove(move, _gameState.Board, _gameState.WhoseTurn);
        return Ok(isValid);
    }
    [HttpPost("make")]
    public IActionResult MakeMove([FromBody] Move move)
    {
        bool isValid = _moveValidator.IsValidMove(move, _gameState.Board, _gameState.WhoseTurn);
        if (isValid)
        {
            _gameState.MakeMove(move);
            return Ok(GameStateDTO.FromGameState(_gameState));
        }
        else
        {
            return BadRequest("Invalid move");
        }
    }

}
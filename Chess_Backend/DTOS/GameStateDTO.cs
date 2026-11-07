using Chess_Backend.Logic;
using Chess_Backend.Models;

namespace Chess_Backend.DTOS;

public class GameStateDTO
{
    public PieceColor WhoseTurn { get; set; }
    public List<Move> MoveHistory { get; set; } = new();
    public CurrentState State { get; set; }
    public List<PieceDTO> Pieces { get; set; } = new();

    public static GameStateDTO FromGameState(GameState gameState)
    {
        return new GameStateDTO
        {
            WhoseTurn = gameState.WhoseTurn,
            MoveHistory = gameState.MoveHistory,
            State = gameState.State,
            Pieces = gameState.Board.GetAllPieces(PieceColor.White)
                .Concat(gameState.Board.GetAllPieces(PieceColor.Black))
                .Select(piece => new PieceDTO
                {
                    Type = piece.Type,
                    Color = piece.Color,
                    Row = piece.Position.Row,
                    Column = piece.Position.Column
                })
                .ToList()
        };
    }
}
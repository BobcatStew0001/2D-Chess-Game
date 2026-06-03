
namespace Chess_Backend.Models;

public abstract class Piece
{
    public PieceColor Color { get; set;}
    public PieceType Type { get; set;}
    public Position Position { get; set; }
    public bool HasMoved { get; set; }

    public Piece(PieceColor color, PieceType type, Position position, bool hasMoved = false)
    {
        Color = color;
        Type = type;
        Position = position;
    }
    
    public abstract Position[] GetMove(Board board);
    
}
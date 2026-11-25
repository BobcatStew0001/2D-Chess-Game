namespace Chess_Backend.Models;

public class StoredPiece
{
    public PieceColor Color { get; set; }
    public PieceType Type { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public bool HasMoved { get; set; }
}

public class StoredMove
{
    public int FromRow { get; set; }
    public int FromColumn { get; set; }
    public int ToRow { get; set; }
    public int ToColumn { get; set; }
    public bool IsEnPassant { get; set; }
    public bool IsCastle { get; set; }
    public bool IsCapture { get; set; }
    public PieceType? Promotion { get; set; }

    public Move ToMove()
    {
        return new Move(
            new Position(FromRow, FromColumn),
            new Position(ToRow, ToColumn),
            IsEnPassant,
            IsCastle,
            IsCapture,
            Promotion);
    }

    public static StoredMove FromMove(Move move)
    {
        return new StoredMove
        {
            FromRow = move.From.Row,
            FromColumn = move.From.Column,
            ToRow = move.To.Row,
            ToColumn = move.To.Column,
            IsEnPassant = move.IsEnPassant,
            IsCastle = move.IsCastle,
            IsCapture = move.IsCapture,
            Promotion = move.Promotion
        };
    }
}

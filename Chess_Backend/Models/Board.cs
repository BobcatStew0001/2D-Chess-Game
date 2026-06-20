using Chess_Backend.Models.Pieces;

namespace Chess_Backend.Models;

public class Board
{
    Piece[,] _board = new Piece[8, 8];

    public void SetupBoard()
    {
        for (int col = 0; col < 8; col++)
        {
            _board[1, col] = new Pawn(PieceColor.Black, PieceType.Pawn, new Position(1, col));
            _board[6, col] = new Pawn(PieceColor.White, PieceType.Pawn, new Position(6, col));
        }

        PieceType[] backRank =
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

        for (int col = 0; col < 8; col++)
        {
            _board[0, col] = CreatePiece(backRank[col], PieceColor.Black, new Position(0, col));
            _board[7, col] = CreatePiece(backRank[col], PieceColor.White, new Position(7, col));
        }
    }

    private Piece CreatePiece(PieceType type, PieceColor color, Position position)
    {
        return type switch
        {
            PieceType.Rook => new Rook(color, type, position),
            PieceType.Knight => new Knight(color, type, position),
            PieceType.Bishop => new Bishop(color, type, position),
            PieceType.Queen => new Queen(color, type, position),
            PieceType.King => new King(color, type, position),
            _ => throw new ArgumentException($"{type} cannot be placed on the back rank")
        };
    }

    public Piece[] GetAllPieces(PieceColor color)
    {
        List<Piece> pieces = new List<Piece>();
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece piece = _board[row, col];
                if (piece != null && piece.Color == color)
                {
                    pieces.Add(piece);
                }
            }
        }
        return pieces.ToArray();
    }

    public Piece GetPieceAt(Position position)
    {
        if (!IsWithinBounds(position))
        {
            return null;
        }
        return _board[position.Row, position.Column];
    }

    public void MovePiece(Move move)
    {
        Piece piece = GetPieceAt(move.From);
        _board[move.From.Row, move.From.Column] = null;
        _board[move.To.Row, move.To.Column] = piece;
        piece.Position = move.To;
    }

    public void RemovePiece(Position position)
    {
        _board[position.Row, position.Column] = null;
    }

    public void RestorePiece(Piece piece, Position position)
    {
        if (piece != null)
        {
            piece.Position = position;
        }
        _board[position.Row, position.Column] = piece;
    }

    public bool IsWithinBounds(Position position)
    {
        return position.Row >= 0 && position.Row < 8 && position.Column >= 0 && position.Column < 8;
    }
}
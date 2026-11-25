using Chess_Backend.Models.Pieces;

namespace Chess_Backend.Models;

public class Board
{
    Piece[,] _board = new Piece[8, 8];
    private readonly Stack<MoveUndo> _undoStack = new();

    public Move? LastMove { get; set; }

    public Board()
    {
        SetupBoard();
    }

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

    public void ReplaceAll(IEnumerable<StoredPiece> pieces)
    {
        _board = new Piece[8, 8];
        foreach (StoredPiece stored in pieces)
        {
            Position position = new Position(stored.Row, stored.Column);
            if (!IsWithinBounds(position))
            {
                throw new ArgumentOutOfRangeException(nameof(pieces), "A saved piece is off the board.");
            }

            Piece piece = CreatePiece(stored.Type, stored.Color, position);
            piece.HasMoved = stored.HasMoved;
            _board[stored.Row, stored.Column] = piece;
        }
    }

    private Piece CreatePiece(PieceType type, PieceColor color, Position position)
    {
        return type switch
        {
            PieceType.Pawn => new Pawn(color, type, position),
            PieceType.Rook => new Rook(color, type, position),
            PieceType.Knight => new Knight(color, type, position),
            PieceType.Bishop => new Bishop(color, type, position),
            PieceType.Queen => new Queen(color, type, position),
            PieceType.King => new King(color, type, position),
            _ => throw new ArgumentException($"{type} is not a chess piece")
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
        MoveUndo undo = new MoveUndo
        {
            PreviousLastMove = LastMove,
            MovedHadMoved = piece.HasMoved
        };

        if (move.IsEnPassant)
        {
            Position capturedAt = new Position(move.From.Row, move.To.Column);
            undo.EnPassantVictim = GetPieceAt(capturedAt);
            undo.EnPassantSquare = capturedAt;
            RemovePiece(capturedAt);
        }

        if (move.IsCastle)
        {
            int direction = move.To.Column > move.From.Column ? 1 : -1;
            Position rookFrom = new Position(move.From.Row, direction == 1 ? 7 : 0);
            Position rookTo = new Position(move.From.Row, move.To.Column - direction);
            Piece rook = GetPieceAt(rookFrom);
            undo.Rook = rook;
            undo.RookFrom = rookFrom;
            undo.RookTo = rookTo;
            undo.RookHadMoved = rook != null && rook.HasMoved;
            _board[rookFrom.Row, rookFrom.Column] = null;
            _board[rookTo.Row, rookTo.Column] = rook;
            if (rook != null)
            {
                rook.Position = rookTo;
                rook.HasMoved = true;
            }
        }

        _board[move.From.Row, move.From.Column] = null;
        Piece placed = piece;
        if (piece.Type == PieceType.Pawn && (move.To.Row == 0 || move.To.Row == 7))
        {
            PieceType promotedType = move.Promotion is PieceType.Queen or PieceType.Rook or PieceType.Bishop or PieceType.Knight
                ? move.Promotion.Value
                : PieceType.Queen;
            placed = CreatePiece(promotedType, piece.Color, move.To);
        }

        placed.HasMoved = true;
        placed.Position = move.To;
        _board[move.To.Row, move.To.Column] = placed;
        LastMove = move;
        _undoStack.Push(undo);
    }

    public void UndoMove(Move move, Piece moved, Piece? capturedOnTarget)
    {
        MoveUndo undo = _undoStack.Pop();
        _board[move.From.Row, move.From.Column] = moved;
        moved.Position = move.From;
        moved.HasMoved = undo.MovedHadMoved;
        _board[move.To.Row, move.To.Column] = capturedOnTarget;
        if (capturedOnTarget != null)
        {
            capturedOnTarget.Position = move.To;
        }

        if (undo.Rook != null && undo.RookFrom != null && undo.RookTo != null)
        {
            _board[undo.RookTo.Row, undo.RookTo.Column] = null;
            _board[undo.RookFrom.Row, undo.RookFrom.Column] = undo.Rook;
            undo.Rook.Position = undo.RookFrom;
            undo.Rook.HasMoved = undo.RookHadMoved;
        }

        if (undo.EnPassantVictim != null && undo.EnPassantSquare != null)
        {
            _board[undo.EnPassantSquare.Row, undo.EnPassantSquare.Column] = undo.EnPassantVictim;
            undo.EnPassantVictim.Position = undo.EnPassantSquare;
        }

        LastMove = undo.PreviousLastMove;
    }

    private sealed class MoveUndo
    {
        public Move? PreviousLastMove { get; set; }
        public bool MovedHadMoved { get; set; }
        public Piece? EnPassantVictim { get; set; }
        public Position? EnPassantSquare { get; set; }
        public Piece? Rook { get; set; }
        public Position? RookFrom { get; set; }
        public Position? RookTo { get; set; }
        public bool RookHadMoved { get; set; }
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
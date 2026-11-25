using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public class MoveValidator
{
    public void Annotate(Move move, Board board)
    {
        Piece piece = board.GetPieceAt(move.From);
        if (piece == null)
        {
            return;
        }

        move.IsCastle = piece.Type == PieceType.King
            && move.From.Row == move.To.Row
            && Math.Abs(move.From.Column - move.To.Column) == 2;
        move.IsEnPassant = IsEnPassant(piece, move, board);
        move.IsCapture = move.IsEnPassant || board.GetPieceAt(move.To) != null;

        if (piece.Type == PieceType.Pawn && (move.To.Row == 0 || move.To.Row == 7))
        {
            move.Promotion = move.Promotion is PieceType.Queen or PieceType.Rook or PieceType.Bishop or PieceType.Knight
                ? move.Promotion
                : PieceType.Queen;
        }
        else
        {
            move.Promotion = null;
        }
    }

    public bool IsValidMove(Move move, Board board, PieceColor whoseTurn)
    {
        if (!board.IsWithinBounds(move.From) || !board.IsWithinBounds(move.To))
        {
            return false;
        }

        Piece piece = board.GetPieceAt(move.From);
        if (piece == null || piece.Color != whoseTurn)
        {
            return false;
        }

        Annotate(move, board);
        if (move.IsCastle)
        {
            if (!CanCastle(piece, move, board) || IsInCheck(piece.Color, board))
            {
                return false;
            }

            int direction = move.To.Column > move.From.Column ? 1 : -1;
            Move step = new Move(move.From, new Position(move.From.Row, move.From.Column + direction));
            if (WouldLeaveKingInCheck(step, board))
            {
                return false;
            }
        }
        else if (!piece.GetMove(board).Contains(move.To))
        {
            return false;
        }

        return !WouldLeaveKingInCheck(move, board);
    }

    public bool IsInCheck(PieceColor color, Board board)
    {
        Piece king = board.GetAllPieces(color).FirstOrDefault(piece => piece.Type == PieceType.King);
        if (king == null)
        {
            return false;
        }

        PieceColor enemyColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        return board.GetAllPieces(enemyColor).Any(piece => Attacks(piece, king.Position, board));
    }

    public bool WouldLeaveKingInCheck(Move move, Board board)
    {
        Piece movingPiece = board.GetPieceAt(move.From);
        if (movingPiece == null)
        {
            return true;
        }

        Piece targetPiece = board.GetPieceAt(move.To);
        board.MovePiece(move);
        try
        {
            return IsInCheck(movingPiece.Color, board);
        }
        finally
        {
            board.UndoMove(move, movingPiece, targetPiece);
        }
    }

    public bool HasAnyLegalMoves(PieceColor color, Board board)
    {
        foreach (Piece piece in board.GetAllPieces(color))
        {
            foreach (Move move in CandidateMoves(piece, board))
            {
                if (IsValidMove(move, board, color))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public IEnumerable<Move> CandidateMoves(Piece piece, Board board)
    {
        foreach (Position target in piece.GetMove(board))
        {
            yield return new Move(piece.Position, target);
        }

        if (piece.Type == PieceType.King && !piece.HasMoved)
        {
            yield return new Move(piece.Position, new Position(piece.Position.Row, piece.Position.Column + 2));
            yield return new Move(piece.Position, new Position(piece.Position.Row, piece.Position.Column - 2));
        }
    }

    private static bool IsEnPassant(Piece piece, Move move, Board board)
    {
        if (piece.Type != PieceType.Pawn || board.LastMove == null)
        {
            return false;
        }

        Move lastMove = board.LastMove;
        if (Math.Abs(lastMove.From.Row - lastMove.To.Row) != 2 || lastMove.From.Column != lastMove.To.Column)
        {
            return false;
        }

        Piece enemyPawn = board.GetPieceAt(lastMove.To);
        if (enemyPawn == null || enemyPawn.Type != PieceType.Pawn || enemyPawn.Color == piece.Color)
        {
            return false;
        }

        int forward = piece.Color == PieceColor.White ? -1 : 1;
        return piece.Position.Row == lastMove.To.Row
            && Math.Abs(piece.Position.Column - lastMove.To.Column) == 1
            && move.To.Row == piece.Position.Row + forward
            && move.To.Column == lastMove.To.Column;
    }

    private bool CanCastle(Piece king, Move move, Board board)
    {
        if (king.HasMoved || king.Position.Row != move.To.Row)
        {
            return false;
        }

        int direction = move.To.Column > king.Position.Column ? 1 : -1;
        Position rookSquare = new Position(king.Position.Row, direction == 1 ? 7 : 0);
        Piece rook = board.GetPieceAt(rookSquare);
        if (rook == null || rook.Type != PieceType.Rook || rook.Color != king.Color || rook.HasMoved)
        {
            return false;
        }

        int first = Math.Min(king.Position.Column, rookSquare.Column) + 1;
        int last = Math.Max(king.Position.Column, rookSquare.Column) - 1;
        for (int column = first; column <= last; column++)
        {
            if (column == king.Position.Column)
            {
                continue;
            }

            if (board.GetPieceAt(new Position(king.Position.Row, column)) != null)
            {
                return false;
            }
        }

        return true;
    }

    private bool Attacks(Piece piece, Position target, Board board)
    {
        int rowDelta = target.Row - piece.Position.Row;
        int columnDelta = target.Column - piece.Position.Column;
        if (rowDelta == 0 && columnDelta == 0)
        {
            return false;
        }

        return piece.Type switch
        {
            PieceType.Pawn => rowDelta == (piece.Color == PieceColor.White ? -1 : 1) && Math.Abs(columnDelta) == 1,
            PieceType.Knight => Math.Abs(rowDelta) == 1 && Math.Abs(columnDelta) == 2
                || Math.Abs(rowDelta) == 2 && Math.Abs(columnDelta) == 1,
            PieceType.King => Math.Max(Math.Abs(rowDelta), Math.Abs(columnDelta)) == 1,
            PieceType.Bishop => Math.Abs(rowDelta) == Math.Abs(columnDelta) && RayIsClear(piece.Position, target, board),
            PieceType.Rook => (rowDelta == 0 || columnDelta == 0) && RayIsClear(piece.Position, target, board),
            PieceType.Queen => (rowDelta == 0 || columnDelta == 0 || Math.Abs(rowDelta) == Math.Abs(columnDelta))
                && RayIsClear(piece.Position, target, board),
            _ => false
        };
    }

    private bool RayIsClear(Position from, Position to, Board board)
    {
        int rowStep = Math.Sign(to.Row - from.Row);
        int columnStep = Math.Sign(to.Column - from.Column);
        int row = from.Row + rowStep;
        int column = from.Column + columnStep;
        while (row != to.Row || column != to.Column)
        {
            if (board.GetPieceAt(new Position(row, column)) != null)
            {
                return false;
            }

            row += rowStep;
            column += columnStep;
        }

        return true;
    }
}

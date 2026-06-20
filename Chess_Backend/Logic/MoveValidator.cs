using Chess_Backend.Models;
using Chess_Backend.Models.Pieces;

namespace Chess_Backend.Logic;

public class MoveValidator
{
    public bool IsValidMove(Move move, Board board)
    {
        Piece piece = board.GetPieceAt(move.From); 
        if (piece == null) return false;
        if (!piece.GetMove(board).Contains(move.To) || WouldLeaveKingInCheck(move, board))
        {
            return false; 
        } 
        return true;
    }

    public bool IsInCheck(PieceColor color, Board board)
    {
        Piece king = board.GetAllPieces(color).FirstOrDefault(p => p.Type == PieceType.King);
        PieceColor enemyColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        return board.GetAllPieces(enemyColor).Any(p => p.GetMove(board).Contains(king.Position));        
    }

    public bool WouldLeaveKingInCheck(Move move, Board board)
    {
        Piece movingPiece = board.GetPieceAt(move.From);
        Piece targetPiece = board.GetPieceAt(move.To);
        board.MovePiece(move);
        bool isInCheck = IsInCheck(movingPiece.Color, board);
        board.RestorePiece(movingPiece,move.From);
        board.RestorePiece(targetPiece,move.To);
        return isInCheck;
    }

    public bool HasAnyLegalMoves(PieceColor color, Board board)
    {
        foreach (Piece piece in board.GetAllPieces(color))
        {
            foreach (Position target in piece.GetMove(board))
            {
                Move move = new Move(piece.Position, target);
                if (IsValidMove(move, board))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
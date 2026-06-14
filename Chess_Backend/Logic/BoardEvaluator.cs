using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public class BoardEvaluator
{
    public int EvaluateBoard(Board board)
    { 
        int whiteValue = board.GetAllPieces(PieceColor.White).Sum(PieceValue);
        int blackValue = board.GetAllPieces(PieceColor.Black).Sum(PieceValue);
        return blackValue - whiteValue;
    }

    public int PieceValue(Piece piece)
    {
        switch (piece.Type)
        {
           case PieceType.Pawn:
               return 1;
           case PieceType.Knight:
               return 3;
           case PieceType.Bishop:
               return 3;
           case PieceType.Rook:
               return 5;
           case PieceType.Queen:
               return 9;
           case PieceType.King:
               return 100;
           default:
               return 0;
        }
        
    }
}
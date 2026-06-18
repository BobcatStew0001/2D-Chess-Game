using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public class ChessAI
{
    private BoardEvaluator _evaluator = new BoardEvaluator();
    
    public Move GetBestMove(Board board, PieceColor color)
    {
        Piece[] pieces = board.GetAllPieces(color);
        List<Move> allMoves = new List<Move>();
        foreach (Piece piece in pieces)
        {
            foreach (Position move in piece.GetMove(board))
            {
                allMoves.Add(new Move(piece.Position, move));
            }
        }
        var bestScore = int.MinValue;
        Move bestMove = null;
        foreach (Move move in allMoves)
        {
            Piece movingPiece = board.GetPieceAt(move.From); 
            Piece targetPiece = board.GetPieceAt(move.To); 
            board.MovePiece(move);
            int score = MiniMax(board, 3, false);
            board.RestorePiece(movingPiece, move.From);
            board.RestorePiece(targetPiece, move.To);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        return bestMove;
        
    }

    public int MiniMax(Board board, int depth, bool isMaximizing)
    {
        if (depth == 0)
        {
            return _evaluator.EvaluateBoard(board);
        }

        PieceColor currentColor = isMaximizing ? PieceColor.Black : PieceColor.White;
        Piece[] pieces = board.GetAllPieces(currentColor);
        List<Move> allMoves = new List<Move>();
        foreach (Piece piece in pieces)
        {
            foreach (Position move in piece.GetMove(board))
            {
                allMoves.Add(new Move(piece.Position, move));
            }
        }

        int bestscore = isMaximizing ? int.MinValue : int.MaxValue;
        foreach (Move move in allMoves)
        {

            Piece movingPiece = board.GetPieceAt(move.From);
            Piece targetPiece = board.GetPieceAt(move.To);


            board.MovePiece(move);


            int score = MiniMax(board, depth - 1, !isMaximizing);

            board.RestorePiece(movingPiece, move.From);
            board.RestorePiece(targetPiece, move.To);

            if (isMaximizing && score > bestscore)
            {
                bestscore = score;
            }

            if (!isMaximizing && score < bestscore)
            {
                bestscore = score;
            }
            
        }
        return bestscore;
    }; 

}
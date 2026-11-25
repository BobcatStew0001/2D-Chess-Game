using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public class ChessAI
{
    private readonly BoardEvaluator _evaluator;
    private readonly MoveValidator _moveValidator;

    public ChessAI(MoveValidator moveValidator, BoardEvaluator evaluator)
    {
        _moveValidator = moveValidator;
        _evaluator = evaluator;
    }
    
    public Move? GetBestMove(Board board, PieceColor color)
    {
        List<Move> legalMoves = new List<Move>();
        foreach (Piece piece in board.GetAllPieces(color))
        {
            foreach (Move candidate in _moveValidator.CandidateMoves(piece, board))
            {
                if (_moveValidator.IsValidMove(candidate, board, color))
                {
                    legalMoves.Add(candidate);
                }
            }
        }

        Move? bestMove = null;
        int bestScore = int.MinValue;
        foreach (Move move in legalMoves)
        {
            Piece movingPiece = board.GetPieceAt(move.From);
            Piece targetPiece = board.GetPieceAt(move.To);
            board.MovePiece(move);
            int score = MiniMax(board, 3, false);
            board.UndoMove(move, movingPiece, targetPiece);
            if (bestMove == null || score > bestScore)
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

            _moveValidator.Annotate(move, board);
            Piece movingPiece = board.GetPieceAt(move.From);
            Piece targetPiece = board.GetPieceAt(move.To);
            board.MovePiece(move);
            int score = MiniMax(board, depth - 1, !isMaximizing);
            board.UndoMove(move, movingPiece, targetPiece);

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
    }

}
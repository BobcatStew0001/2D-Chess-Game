using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public class GameState
{
    public PieceColor WhoseTurn { get; set; }
    public CurrentState State { get; set; }
    public List<Move> MoveHistory { get; set; }
    public Board Board { get; set; }

    public GameState()
    {
        WhoseTurn = PieceColor.White;
        State = CurrentState.InProgress;
        MoveHistory = new List<Move>();
        Board = new Board();

    }
    
    public void MakeMove(Move move)
    {
        if (State == CurrentState.InProgress || State == CurrentState.Check)
        {
            Board.MovePiece(move);
            MoveHistory.Add(move);
            WhoseTurn = WhoseTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }
    }
    
}

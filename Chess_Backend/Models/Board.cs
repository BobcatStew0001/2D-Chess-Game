namespace Chess_Backend.Models;

public class Board
{
    Piece[,] _board = new Piece[8, 8];

    public void SetupBoard()
    {
        
    }
    public Piece[] GetAllPieces(PieceColor piece)
    {

        return new Piece[0];
    }
    public Piece GetPieceAt(Position position)
    {
        
        return null; //Placeholder for now
    }

    public void MovePiece(Move move)
    {
        
    }
    public void  RemovePiece(Position position)
    {
        
    }

    public bool IsWithinBounds(Position position)
    {
        return true;
    }
   
}
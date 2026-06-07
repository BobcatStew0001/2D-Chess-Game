namespace Chess_Backend.Models.Pieces;

public class Knight:Piece
{
    public Knight(PieceColor color, PieceType type, Position position, bool hasMoved = false) : base(color, type, position, hasMoved)
    {
    }

    public override Position[] GetMove(Board board)
    {
        int[,] offsets = new int[8, 2]
        {
            { -2, -1 },
            { -2,  1 },
            {  2, -1 },
            {  2,  1 },
            { -1, -2 },
            { -1,  2 },
            {  1, -2 },
            {  1,  2 }
        };
        List<Position> validMoves = new List<Position>();
        for (var i = 0; i < offsets.GetLength(0); i++)
        {
            Position newPosition = new Position(Position.Row + offsets[i, 0], Position.Column + offsets[i, 1]);
            if (board.IsWithinBounds(newPosition))
            {
                Piece pieceAtSquare = board.GetPieceAt(newPosition);
                if (pieceAtSquare == null || pieceAtSquare.Color != Color)
                {
                    validMoves.Add(newPosition);    
                }
                
            }
        }
        return validMoves.ToArray();
    }
}


namespace Chess_Backend.Models.Pieces;

public class Rook:Piece
{
    public Rook(PieceColor color, PieceType type, Position position, bool hasMoved = false) : base(color, type, position, hasMoved)
    {
    }

    public override Position[] GetMove(Board board)
    {
        int[,] offsets = new int[4,2]
        {
            {-1, 0},
            {1, 0}, 
            {0,-1},
            {0, 1}
        };
        List<Position> validMoves = new List<Position>();
        for (var i = 0; i < offsets.GetLength(0); i++)
        {
            int step = 1; 
            while (true)
            {
                Position newPosition = new Position(Position.Row + offsets[i, 0] * step, 
                    Position.Column + offsets[i, 1] * step);

                if(!board.IsWithinBounds(newPosition))
                {
                    break;
                }

                if(board.GetPieceAt(newPosition) == null)
                {
                    validMoves.Add(newPosition);
                    step++;
                    continue;
                }

                if(board.GetPieceAt(newPosition) != null && board.GetPieceAt(newPosition).Color != Color)
                {
                   validMoves.Add(newPosition);
                   break;
                }
                else
                {
                    break;
                }
                
            }
            
        }
        return validMoves.ToArray();
    } 
}
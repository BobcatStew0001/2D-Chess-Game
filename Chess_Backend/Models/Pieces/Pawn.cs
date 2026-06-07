namespace Chess_Backend.Models.Pieces;

public class Pawn : Piece
{
    public Pawn(PieceColor color, PieceType type, Position position, bool hasMoved = false) : base(color, type,
        position, hasMoved)
    {
    }

    public override Position[] GetMove(Board board)
    {
        int[,] offsets = new int[,] { };
        
        if (PieceColor.White == Color && HasMoved == false)
        {
            offsets = new int[4, 2]
            {
                { -2, 0 },
                { -1, 0 },
                { -1, 1 },
                { -1, -1 }
            };
        }
        
        else if (PieceColor.White == Color && HasMoved == true)
        {
            offsets = new int[3, 2]
            {
                { -1, 0 },
                { -1, 1 },
                { -1, -1 }
            };
        }

        else if (PieceColor.Black == Color && HasMoved == false)
        {
            offsets = new int[4, 2]
            {
                { 2, 0 },
                { 1, 0 },
                { 1, 1 },
                { 1, -1 }
            };
        }

        else if (PieceColor.Black == Color && HasMoved == true)
        {
            offsets = new int[3, 2]
            {
                { 1, 0 },
                { 1, 1 },
                { 1, -1 }
            };
        }

        List<Position> validMoves = new List<Position>();
        for (var i = 0; i < offsets.GetLength(0); i++)
        {
            Position newPosition = new Position(Position.Row + offsets[i, 0], Position.Column + offsets[i, 1]);
            if (board.IsWithinBounds(newPosition))
            {
                Piece pieceAtSquare = board.GetPieceAt(newPosition);
                if (pieceAtSquare != null && pieceAtSquare.Color == Color)
                {
                    continue; 
                }

                if (offsets[i, 1] == 0 && pieceAtSquare == null)
                {
                    validMoves.Add(newPosition);
                }

                if (offsets[i, 1] != 0 && pieceAtSquare != null)
                {
                    validMoves.Add(newPosition);
                }
            }
        }
        return validMoves.ToArray();
    }
}
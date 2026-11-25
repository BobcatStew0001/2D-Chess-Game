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
                    if (Math.Abs(offsets[i, 0]) == 2)
                    {
                        int step = offsets[i, 0] > 0 ? 1 : -1;
                        Position between = new Position(Position.Row + step, Position.Column);
                        if (board.GetPieceAt(between) != null)
                        {
                            continue;
                        }
                    }

                    validMoves.Add(newPosition);
                }

                if (offsets[i, 1] != 0 && pieceAtSquare != null)
                {
                    validMoves.Add(newPosition);
                }
            }
        }

        Move? lastMove = board.LastMove;
        if (lastMove != null
            && Math.Abs(lastMove.From.Row - lastMove.To.Row) == 2
            && lastMove.From.Column == lastMove.To.Column)
        {
            Piece enemyPawn = board.GetPieceAt(lastMove.To);
            if (enemyPawn != null
                && enemyPawn.Type == PieceType.Pawn
                && enemyPawn.Color != Color
                && enemyPawn.Position.Row == Position.Row
                && Math.Abs(enemyPawn.Position.Column - Position.Column) == 1)
            {
                int forward = Color == PieceColor.White ? -1 : 1;
                Position landing = new Position(Position.Row + forward, enemyPawn.Position.Column);
                if (board.IsWithinBounds(landing) && board.GetPieceAt(landing) == null)
                {
                    validMoves.Add(landing);
                }
            }
        }

        return validMoves.ToArray();
    }
}
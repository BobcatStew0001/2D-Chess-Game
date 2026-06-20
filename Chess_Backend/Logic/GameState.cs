using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public class GameState
{   
    public PieceColor WhoseTurn { get; set; }
    public CurrentState State { get; set; }
    public List<Move> MoveHistory { get; set; }
    public Board Board { get; set; }

    private readonly MoveValidator _moveValidator;
    private readonly Dictionary<string, int> _positionCounts;
    private int _halfMoveClock;

    public GameState()
    {
        WhoseTurn = PieceColor.White;
        State = CurrentState.InProgress;
        MoveHistory = new List<Move>();
        Board = new Board();
        _moveValidator = new MoveValidator();
        _positionCounts = new Dictionary<string, int>();
        _halfMoveClock = 0;
    }

    public void Resign()
    {
        if (State == CurrentState.InProgress || State == CurrentState.Check)
        {
            State = CurrentState.Resigned;
        }
    }

    public void MakeMove(Move move)
    {
        if (State == CurrentState.InProgress || State == CurrentState.Check)
        {
            Piece piece = Board.GetPieceAt(move.From);
            bool resetsHalfMoveClock = piece.Type == PieceType.Pawn || move.IsCapture;

            Board.MovePiece(move);
            piece.HasMoved = true;
            MoveHistory.Add(move);
            WhoseTurn = WhoseTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

            _halfMoveClock = resetsHalfMoveClock ? 0 : _halfMoveClock + 1;
            RecordPosition();
            UpdateGameState();
        }
    }

    private void UpdateGameState()
    {
        bool inCheck = _moveValidator.IsInCheck(WhoseTurn, Board);
        bool hasLegalMoves = _moveValidator.HasAnyLegalMoves(WhoseTurn, Board);

        if (inCheck && !hasLegalMoves)
        {
            State = CurrentState.Checkmate;
        }
        else if (!inCheck && !hasLegalMoves)
        {
            State = CurrentState.Stalemate;
        }
        else if (_halfMoveClock >= 100 || IsThreefoldRepetition() || IsInsufficientMaterial())
        {
            State = CurrentState.Draw;
        }
        else if (inCheck)
        {
            State = CurrentState.Check;
        }
        else
        {
            State = CurrentState.InProgress;
        }
    }

    private void RecordPosition()
    {
        string key = GetPositionKey();
        _positionCounts[key] = _positionCounts.GetValueOrDefault(key) + 1;
    }

    private bool IsThreefoldRepetition()
    {
        return _positionCounts.GetValueOrDefault(GetPositionKey()) >= 3;
    }

    private string GetPositionKey()
    {
        IEnumerable<Piece> pieces = Board.GetAllPieces(PieceColor.White)
            .Concat(Board.GetAllPieces(PieceColor.Black))
            .OrderBy(p => p.Position.Row)
            .ThenBy(p => p.Position.Column);

        string piecesKey = string.Join("|", pieces.Select(p => $"{p.Color}{p.Type}{p.Position.Row}{p.Position.Column}{p.HasMoved}"));
        return $"{WhoseTurn}:{piecesKey}";
    }

    private bool IsInsufficientMaterial()
    {
        List<Piece> nonKingPieces = Board.GetAllPieces(PieceColor.White)
            .Concat(Board.GetAllPieces(PieceColor.Black))
            .Where(p => p.Type != PieceType.King)
            .ToList();

        if (nonKingPieces.Count == 0)
        {
            return true;
        }

        if (nonKingPieces.Count == 1 &&
            (nonKingPieces[0].Type == PieceType.Knight || nonKingPieces[0].Type == PieceType.Bishop))
        {
            return true;
        }

        if (nonKingPieces.Count == 2 &&
            nonKingPieces.All(p => p.Type == PieceType.Bishop) &&
            nonKingPieces[0].Color != nonKingPieces[1].Color)
        {
            int squareColorA = (nonKingPieces[0].Position.Row + nonKingPieces[0].Position.Column) % 2;
            int squareColorB = (nonKingPieces[1].Position.Row + nonKingPieces[1].Position.Column) % 2;
            if (squareColorA == squareColorB)
            {
                return true;
            }
        }

        return false;
    }
}

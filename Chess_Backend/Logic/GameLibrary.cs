using System.Text.Json;
using System.Text.Json.Serialization;
using Chess_Backend.DTOS;
using Chess_Backend.Models;

namespace Chess_Backend.Logic;

public sealed class GameLibrary
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;
    private readonly MoveValidator _moveValidator;
    private readonly ChessAI _ai;
    private readonly object _gate = new();
    private LiveGame? _friend;
    private readonly Dictionary<string, LiveGame> _computer = new(StringComparer.OrdinalIgnoreCase);

    public GameLibrary(IHostEnvironment environment, MoveValidator moveValidator, ChessAI ai)
    {
        _path = Path.Combine(environment.ContentRootPath, "SavedGames", "games.json");
        _moveValidator = moveValidator;
        _ai = ai;
        Load();
    }

    public GameStateDTO JoinFriend(string username)
    {
        lock (_gate)
        {
            _friend ??= NewGame(Accounts.FriendMode);
            if (_friend.WhitePlayer == null)
            {
                _friend.WhitePlayer = username;
            }
            else if (!IsSeated(_friend, username))
            {
                if (_friend.BlackPlayer != null)
                {
                    throw new GameException("Both seats are already taken.");
                }

                _friend.BlackPlayer = username;
            }

            Save();
            return Describe(_friend, username);
        }
    }

    public GameStateDTO JoinComputer(string username)
    {
        lock (_gate)
        {
            if (!_computer.TryGetValue(username, out LiveGame? game))
            {
                game = NewGame(Accounts.ComputerMode);
                game.WhitePlayer = username;
                game.BlackPlayer = Accounts.ComputerName;
                _computer[username] = game;
            }

            bool changed = ReplyForComputer(game);
            if (changed)
            {
                Save();
            }

            return Describe(game, username);
        }
    }

    public GameStateDTO View(string username, string mode)
    {
        lock (_gate)
        {
            LiveGame game = RequireGame(username, mode);
            if (ReplyForComputer(game))
            {
                Save();
            }

            return Describe(game, username);
        }
    }

    public GameStateDTO Start(string username, string mode)
    {
        lock (_gate)
        {
            LiveGame game = RequireGame(username, mode);
            RequireSeat(game, username);
            game.State.StartNewGame();
            game.ResignedBy = null;
            Save();
            return Describe(game, username);
        }
    }

    public GameStateDTO Resign(string username, string mode)
    {
        lock (_gate)
        {
            LiveGame game = RequireGame(username, mode);
            RequireSeat(game, username);
            if (game.State.State != CurrentState.InProgress && game.State.State != CurrentState.Check)
            {
                throw new GameException("This game is already over.");
            }

            game.ResignedBy = username;
            game.State.Resign();
            Save();
            return Describe(game, username);
        }
    }

    public bool Validate(string username, string mode, Move move)
    {
        lock (_gate)
        {
            LiveGame game = RequireGame(username, mode);
            if (!CanMove(game, username))
            {
                return false;
            }

            return _moveValidator.IsValidMove(move, game.State.Board, game.State.WhoseTurn);
        }
    }

    public GameStateDTO MakeMove(string username, string mode, Move move)
    {
        lock (_gate)
        {
            LiveGame game = RequireGame(username, mode);
            if (!CanMove(game, username))
            {
                throw new GameException(WaitingMessage(game, username));
            }

            if (!_moveValidator.IsValidMove(move, game.State.Board, game.State.WhoseTurn))
            {
                throw new GameException("Invalid move");
            }

            if (game.State.Board.GetPieceAt(move.To) != null)
            {
                move.IsCapture = true;
            }

            game.State.MakeMove(move);
            ReplyForComputer(game);
            Save();
            return Describe(game, username);
        }
    }

    public void Flush()
    {
        lock (_gate)
        {
            Save();
        }
    }

    private LiveGame RequireGame(string username, string mode)
    {
        if (mode == Accounts.FriendMode)
        {
            if (_friend == null || !IsSeated(_friend, username))
            {
                throw new GameException("Join the friend game before playing.");
            }

            return _friend;
        }

        if (mode == Accounts.ComputerMode)
        {
            if (!_computer.TryGetValue(username, out LiveGame? game))
            {
                throw new GameException("Start a computer game before playing.");
            }

            return game;
        }

        throw new GameException("Choose a friend game or a computer game.");
    }

    private static bool CanMove(LiveGame game, string username)
    {
        if (game.State.State != CurrentState.InProgress && game.State.State != CurrentState.Check)
        {
            return false;
        }

        PieceColor? color = ColorOf(game, username);
        if (color == null || game.State.WhoseTurn != color)
        {
            return false;
        }

        if (game.Mode == Accounts.FriendMode && MissingFriend(game))
        {
            return false;
        }

        return true;
    }

    private static bool MissingFriend(LiveGame game)
    {
        return string.IsNullOrWhiteSpace(game.WhitePlayer) || string.IsNullOrWhiteSpace(game.BlackPlayer);
    }

    private static string WaitingMessage(LiveGame game, string username)
    {
        if (game.State.State != CurrentState.InProgress && game.State.State != CurrentState.Check)
        {
            return "This game is already over.";
        }

        if (game.Mode == Accounts.FriendMode && MissingFriend(game))
        {
            return $"Waiting for {Accounts.OtherPlayer(username)} to join.";
        }

        PieceColor? color = ColorOf(game, username);
        if (color == null)
        {
            return "You do not have a seat in this game.";
        }

        return "It is not your turn.";
    }

    private bool ReplyForComputer(LiveGame game)
    {
        if (game.Mode != Accounts.ComputerMode || game.State.WhoseTurn != PieceColor.Black)
        {
            return false;
        }

        if (game.State.State != CurrentState.InProgress && game.State.State != CurrentState.Check)
        {
            return false;
        }

        Move? move = _ai.GetBestMove(game.State.Board, PieceColor.Black);
        if (move == null)
        {
            return false;
        }

        game.State.MakeMove(move);
        return true;
    }

    private static LiveGame NewGame(string mode)
    {
        return new LiveGame
        {
            Mode = mode,
            State = new GameState()
        };
    }

    private static void RequireSeat(LiveGame game, string username)
    {
        if (!IsSeated(game, username))
        {
            throw new GameException("You do not have a seat in this game.");
        }
    }

    private static bool IsSeated(LiveGame game, string username)
    {
        return string.Equals(game.WhitePlayer, username, StringComparison.OrdinalIgnoreCase)
            || string.Equals(game.BlackPlayer, username, StringComparison.OrdinalIgnoreCase);
    }

    private static PieceColor? ColorOf(LiveGame game, string username)
    {
        if (string.Equals(game.WhitePlayer, username, StringComparison.OrdinalIgnoreCase))
        {
            return PieceColor.White;
        }

        if (string.Equals(game.BlackPlayer, username, StringComparison.OrdinalIgnoreCase))
        {
            return PieceColor.Black;
        }

        return null;
    }

    private static GameStateDTO Describe(LiveGame game, string username)
    {
        PieceColor? color = ColorOf(game, username);
        bool active = game.State.State == CurrentState.InProgress || game.State.State == CurrentState.Check;
        bool opponentJoined = game.Mode == Accounts.ComputerMode || !MissingFriend(game);
        GameStateDTO dto = GameStateDTO.FromGameState(game.State);
        dto.Mode = game.Mode;
        dto.You = username;
        dto.YourColor = color;
        dto.YourTurn = active && opponentJoined && color == game.State.WhoseTurn;
        dto.OpponentJoined = opponentJoined;
        dto.WhitePlayer = game.WhitePlayer;
        dto.BlackPlayer = game.BlackPlayer;
        dto.ResignedBy = game.ResignedBy;
        return dto;
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(_path);
            GameFile? file = JsonSerializer.Deserialize<GameFile>(json, JsonOptions);
            if (file == null)
            {
                return;
            }

            if (file.Friend != null)
            {
                _friend = Restore(file.Friend);
            }

            foreach (KeyValuePair<string, StoredMatch> pair in file.Computer ?? [])
            {
                _computer[pair.Key] = Restore(pair.Value);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Could not read saved games: {exception.Message}");
        }
    }

    private void Save()
    {
        GameFile file = new GameFile
        {
            Friend = _friend?.ToStoredMatch(),
            Computer = _computer.ToDictionary(pair => pair.Key, pair => pair.Value.ToStoredMatch(), StringComparer.OrdinalIgnoreCase)
        };

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        string tempPath = _path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(file, JsonOptions));
        File.Move(tempPath, _path, true);
    }

    private sealed class LiveGame
    {
        public required string Mode { get; init; }
        public required GameState State { get; init; }
        public string? WhitePlayer { get; set; }
        public string? BlackPlayer { get; set; }
        public string? ResignedBy { get; set; }

        public StoredMatch ToStoredMatch()
        {
            IEnumerable<Piece> pieces = State.Board.GetAllPieces(PieceColor.White)
                .Concat(State.Board.GetAllPieces(PieceColor.Black));

            return new StoredMatch
            {
                Mode = Mode,
                WhitePlayer = WhitePlayer,
                BlackPlayer = BlackPlayer,
                ResignedBy = ResignedBy,
                WhoseTurn = State.WhoseTurn,
                State = State.State,
                HalfMoveClock = State.HalfMoveClock,
                MoveHistory = State.MoveHistory.Select(StoredMove.FromMove).ToList(),
                Pieces = pieces.Select(piece => new StoredPiece
                {
                    Color = piece.Color,
                    Type = piece.Type,
                    Row = piece.Position.Row,
                    Column = piece.Position.Column,
                    HasMoved = piece.HasMoved
                }).ToList(),
                PositionCounts = State.CopyPositionCounts()
            };
        }
    }

    private static LiveGame Restore(StoredMatch saved)
    {
        GameState state = new GameState();
        state.WhoseTurn = saved.WhoseTurn;
        state.State = saved.State;
        state.MoveHistory = saved.MoveHistory.Select(move => move.ToMove()).ToList();
        state.Board.ReplaceAll(saved.Pieces);
        state.RestoreProgress(saved.HalfMoveClock, saved.PositionCounts);
        if (saved.MoveHistory.Count > 0)
        {
            state.Board.LastMove = saved.MoveHistory[^1].ToMove();
        }
        return new LiveGame
        {
            Mode = saved.Mode,
            State = state,
            WhitePlayer = saved.WhitePlayer,
            BlackPlayer = saved.BlackPlayer,
            ResignedBy = saved.ResignedBy
        };
    }
}

public class StoredMatch
{
    public string Mode { get; set; } = Accounts.FriendMode;
    public string? WhitePlayer { get; set; }
    public string? BlackPlayer { get; set; }
    public string? ResignedBy { get; set; }
    public PieceColor WhoseTurn { get; set; }
    public CurrentState State { get; set; }
    public int HalfMoveClock { get; set; }
    public List<StoredMove> MoveHistory { get; set; } = new();
    public List<StoredPiece> Pieces { get; set; } = new();
    public Dictionary<string, int> PositionCounts { get; set; } = new();
}

public class GameFile
{
    public StoredMatch? Friend { get; set; }
    public Dictionary<string, StoredMatch> Computer { get; set; } = new();
}

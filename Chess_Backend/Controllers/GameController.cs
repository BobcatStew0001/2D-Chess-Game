using Chess_Backend.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameLibrary _games;
    private readonly PlayerSessions _sessions;

    public GameController(GameLibrary games, PlayerSessions sessions)
    {
        _games = games;
        _sessions = sessions;
    }

    [HttpPost("play/friend")]
    public IActionResult PlayFriend()
    {
        Session? session = _sessions.Find(Request);
        if (session == null)
        {
            return Unauthorized("Sign in first.");
        }

        session.Mode = Accounts.FriendMode;
        return Ok(_games.JoinFriend(session.Username));
    }

    [HttpPost("play/computer")]
    public IActionResult PlayComputer()
    {
        Session? session = _sessions.Find(Request);
        if (session == null)
        {
            return Unauthorized("Sign in first.");
        }

        session.Mode = Accounts.ComputerMode;
        return Ok(_games.JoinComputer(session.Username));
    }

    [HttpGet("state")]
    public IActionResult GetGameState()
    {
        return WithGame((session) => _games.View(session.Username, session.Mode!));
    }

    [HttpPost("start")]
    public IActionResult StartNewGame()
    {
        return WithGame((session) => _games.Start(session.Username, session.Mode!));
    }

    [HttpPost("resign")]
    public IActionResult Resign()
    {
        return WithGame((session) => _games.Resign(session.Username, session.Mode!));
    }

    private IActionResult WithGame(Func<Session, object> action)
    {
        Session? session = _sessions.Find(Request);
        if (session == null)
        {
            return Unauthorized("Sign in first.");
        }

        if (session.Mode != Accounts.FriendMode && session.Mode != Accounts.ComputerMode)
        {
            return BadRequest("Choose a friend game or a computer game.");
        }

        return Ok(action(session));
    }
}

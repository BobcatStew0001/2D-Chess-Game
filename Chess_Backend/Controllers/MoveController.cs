using Microsoft.AspNetCore.Mvc;
using Chess_Backend.Logic;
using Chess_Backend.Models;

namespace Chess_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoveController : ControllerBase
{
    private readonly GameLibrary _games;
    private readonly PlayerSessions _sessions;

    public MoveController(GameLibrary games, PlayerSessions sessions)
    {
        _games = games;
        _sessions = sessions;
    }

    [HttpPost("validate")]
    public IActionResult ValidateMove([FromBody] Move move)
    {
        Session? session = _sessions.Find(Request);
        if (session == null)
        {
            return Unauthorized("Sign in first.");
        }

        if (session.Mode != Accounts.FriendMode && session.Mode != Accounts.ComputerMode)
        {
            return Ok(false);
        }

        return Ok(_games.Validate(session.Username, session.Mode, move));
    }

    [HttpPost("make")]
    public IActionResult MakeMove([FromBody] Move move)
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

        return Ok(_games.MakeMove(session.Username, session.Mode, move));
    }
}

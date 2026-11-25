using Chess_Backend.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly GameLibrary _games;
    private readonly PlayerSessions _sessions;

    public AIController(GameLibrary games, PlayerSessions sessions)
    {
        _games = games;
        _sessions = sessions;
    }

    [HttpPost("aimove")]
    public IActionResult AIMove()
    {
        Session? session = _sessions.Find(Request);
        if (session == null)
        {
            return Unauthorized("Sign in first.");
        }

        if (session.Mode != Accounts.ComputerMode)
        {
            return BadRequest("The computer only moves in a computer game.");
        }

        return Ok(_games.View(session.Username, Accounts.ComputerMode));
    }
}

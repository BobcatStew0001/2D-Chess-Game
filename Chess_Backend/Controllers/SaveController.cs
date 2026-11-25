using Chess_Backend.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaveController : ControllerBase
{
    private readonly GameLibrary _games;
    private readonly PlayerSessions _sessions;

    public SaveController(GameLibrary games, PlayerSessions sessions)
    {
        _games = games;
        _sessions = sessions;
    }

    [HttpPost("save")]
    public IActionResult SaveGame()
    {
        if (_sessions.Find(Request) == null)
        {
            return Unauthorized("Sign in first.");
        }

        _games.Flush();
        return Ok("Game saved successfully");
    }

    [HttpGet("load")]
    public IActionResult LoadGame()
    {
        if (_sessions.Find(Request) == null)
        {
            return Unauthorized("Sign in first.");
        }

        return Ok("Saved games load when the server starts, and they stay up to date after every move.");
    }
}

using Chess_Backend.DTOS;
using Chess_Backend.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Chess_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PlayerSessions _sessions;

    public AuthController(PlayerSessions sessions)
    {
        _sessions = sessions;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest? request)
    {
        string? username = Accounts.Authenticate(request?.Username, request?.Password);
        if (username == null)
        {
            return Unauthorized("The name or password is wrong.");
        }

        string token = _sessions.SignIn(username);
        return Ok(new { token, username });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _sessions.SignOut(Request);
        return Ok("Signed out");
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        Session? session = _sessions.Find(Request);
        if (session == null)
        {
            return Unauthorized("Sign in first.");
        }

        return Ok(new { username = session.Username });
    }
}

namespace Chess_Backend.Logic;

public sealed class Session
{
    public required string Username { get; init; }
    public string? Mode { get; set; }
}

public sealed class PlayerSessions
{
    private readonly Dictionary<string, Session> _sessions = new();
    private readonly object _gate = new();

    public string SignIn(string username)
    {
        string token = Guid.NewGuid().ToString("N");
        lock (_gate)
        {
            _sessions[token] = new Session { Username = username };
        }

        return token;
    }

    public void SignOut(HttpRequest request)
    {
        string? token = ReadToken(request);
        if (token == null)
        {
            return;
        }

        lock (_gate)
        {
            _sessions.Remove(token);
        }
    }

    public Session? Find(HttpRequest request)
    {
        string? token = ReadToken(request);
        if (token == null)
        {
            return null;
        }

        lock (_gate)
        {
            return _sessions.TryGetValue(token, out Session? session) ? session : null;
        }
    }

    private static string? ReadToken(HttpRequest request)
    {
        string header = request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string token = header[prefix.Length..].Trim();
        return token.Length == 0 ? null : token;
    }
}

namespace Chess_Backend.Logic;

public static class Accounts
{
    public const string FriendMode = "Friend";
    public const string ComputerMode = "Computer";
    public const string ComputerName = "Computer";

    private static readonly string[] Names = ["Bub", "Friend"];
    private const string Password = "King2026";

    public static string? Authenticate(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || password != Password)
        {
            return null;
        }

        return Names.FirstOrDefault(name => string.Equals(name, username.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static string OtherPlayer(string username)
    {
        return string.Equals(username, "Bub", StringComparison.OrdinalIgnoreCase) ? "Friend" : "Bub";
    }
}

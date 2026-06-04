namespace Chess_Backend.Models;

public class Move
{
    public Position From { get; set; }
    public Position To { get; set; }
    public bool IsEnPassant { get; set; }
    public bool IsCastle { get; set; }
    public bool IsCapture { get; set; }
    

    public Move(Position from, Position to,bool isEnPassant = false, bool isCastle = false, bool isCapture= false)
    {
        
        From = from;
        To = to;
        IsEnPassant = isEnPassant;
        IsCapture = isCapture;
        IsCastle = isCastle;
    }
}

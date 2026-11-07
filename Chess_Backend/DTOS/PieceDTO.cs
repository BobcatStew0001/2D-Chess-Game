using System;
using Chess_Backend.Models;

namespace Chess_Backend.DTOS;

public class PieceDTO
{
    public PieceColor Color { get; set; }
    public PieceType Type { get; set; }
    public int  Row { get; set; }
    public int Column { get; set; }
}
namespace PokerAnalysis.Domain;
using PokerAnalysis.Domain.Enums;

public class HandParticipant
{
    public Player Player { get; set; }
    public int SeatNumber { get; set; }
    public Position Position { get; set; }
    public int StartingStack { get; set; }
    public string[]? HoleCards { get; set; }

    public HandParticipant(Player player, int seatNumber, Position position, int startingStack)
    {
        Player = player;
        SeatNumber = seatNumber;
        Position = position;
        StartingStack = startingStack;
    }

}
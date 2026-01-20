namespace PokerAnalysis.Application.Services;
using PokerAnalysis.Domain;

public class PlayerStats
{
    public Player Player { get; set; }
    public int HandsWon { get; set; }


    public PlayerStats(Player player, int handsWon)
    {
        Player = player;
        HandsWon = handsWon;
    }


}
namespace PokerAnalysis.Application.Models;
using PokerAnalysis.Domain;

public class PlayerStats
{
    public Player Player { get; set; }
    public int HandsWon { get; set; }
    public int HandsPlayed { get; set; }
    public int VpipEvents { get; set; }
    public double Vpip => HandsPlayed > 0 ? (double)VpipEvents / HandsPlayed * 100 : 0; 


    public PlayerStats(Player player, int handsWon, int handsPlayed, int vpipEvents)
    {
        Player = player;
        HandsWon = handsWon;
        HandsPlayed = handsPlayed;
        VpipEvents = vpipEvents;

    }

}

//vpip
// everytime a player makes a calls or raises on the street preflop 
//the number of calls + raises on the street preflop / hands participated in.

// firt stat we need is hands participated in
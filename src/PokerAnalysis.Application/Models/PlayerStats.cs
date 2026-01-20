namespace PokerAnalysis.Application.Models;

using PokerAnalysis.Domain;

public class PlayerStats
{
    public Player Player { get; set; }
    public int HandsPlayed { get; set; }
    public int HandsWon { get; set; }

    // --- True C-bet tracking ---
    public int CBetOpportunities { get; set; }   // was preflop aggressor AND saw flop
    public int CBetsMade { get; set; }           // bet on flop in those spots

    public double CBetPercentage
    {
        get
        {
            if (CBetOpportunities == 0) return 0.0;
            return (double)CBetsMade / CBetOpportunities * 100.0;
        }
    }

    // --- VPIP tracking ---
    public int VpipHands { get; set; }           // hands where player VPIP'd

    public double VpipPercentage
    {
        get
        {
            if (HandsPlayed == 0) return 0.0;
            return (double)VpipHands / HandsPlayed * 100.0;
        }
    }

    public PlayerStats(Player player)
    {
        Player = player;
        HandsPlayed = 0;
        HandsWon = 0;

        CBetOpportunities = 0;
        CBetsMade = 0;

        VpipHands = 0;
    }
}

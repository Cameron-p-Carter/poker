namespace PokerAnalysis.Application.Services;

using PokerAnalysis.Domain;
using PokerAnalysis.Application.Models;

public class StatCalculator
{
    public List<PlayerStats> CalculateStats(Session session)
    {
        Dictionary<string, PlayerStats> statsByPlayerId = new();

        foreach (Hand hand in session.Hands)
        {
            foreach (HandResult result in hand.Results)
            {
                if (!statsByPlayerId.TryGetValue(result.Player.Id, out var stats))
                {
                    stats = new PlayerStats(result.Player, 0);
                    statsByPlayerId[result.Player.Id] = stats;
                }
                stats.HandsWon++;
            }
        }

        return statsByPlayerId.Values.ToList();
    }
}
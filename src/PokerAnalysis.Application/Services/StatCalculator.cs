namespace PokerAnalysis.Application.Services;

using PokerAnalysis.Domain;
using PokerAnalysis.Application.Models;
using PokerAnalysis.Domain.Enums;


public class StatCalculator
{
    public List<PlayerStats> CalculateStats(Session session)
    {
        Dictionary<string, PlayerStats> statsByPlayerId = new();

        foreach (Hand hand in session.Hands)
        {
            HashSet<string> vpipCounted = new();

            foreach (HandResult result in hand.Results)
            {
                if (!statsByPlayerId.TryGetValue(result.Player.Id, out var stats))
                {
                    stats = new PlayerStats(result.Player, 0, 0, 0);
                    statsByPlayerId[result.Player.Id] = stats;
                }
                stats.HandsWon++;
            }

            foreach (HandParticipant participant in hand.Participants)
            {
                if (!statsByPlayerId.TryGetValue(participant.Player.Id, out var stats))
                {
                    stats = new PlayerStats(participant.Player, 0, 0, 0);
                    statsByPlayerId[participant.Player.Id] = stats;
                }
                stats.HandsPlayed++;
            }

            foreach (PlayerAction action in hand.Actions)
            {
                if (!statsByPlayerId.TryGetValue(action.Player.Id, out var stats))
                {
                    stats = new PlayerStats(action.Player, 0, 0, 0);
                    statsByPlayerId[action.Player.Id] = stats;
                }

                if (action.StreetType == StreetType.Preflop &&
                    (action.ActionType == ActionType.Call || action.ActionType == ActionType.Raise) &&
                    !vpipCounted.Contains(action.Player.Id))
                {
                    stats.VpipEvents++;
                    vpipCounted.Add(action.Player.Id);
                }
            }
        }

        return statsByPlayerId.Values.ToList();
    }
}
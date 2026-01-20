namespace PokerAnalysis.Application.Services;

using PokerAnalysis.Domain;
using PokerAnalysis.Application.Models;
using PokerAnalysis.Domain.Enums;

public class StatCalculator
{
    public List<PlayerStats> Calculate(Session session)
    {
        Dictionary<string, PlayerStats> statsDict = new();

        foreach (Player player in session.Players)
        {
            statsDict[player.Id] = new PlayerStats(player);
        }

        foreach (Hand hand in session.Hands)
        {
            // Hands played
            foreach (HandParticipant participant in hand.Participants)
            {
                if (statsDict.TryGetValue(participant.Player.Id, out var ps))
                {
                    ps.HandsPlayed++;
                }
            }

            // Hands won
            foreach (HandResult result in hand.Results)
            {
                if (statsDict.TryGetValue(result.Player.Id, out var ps))
                {
                    ps.HandsWon++;
                }
            }

            // ----- VPIP -----
            foreach (HandParticipant participant in hand.Participants)
            {
                if (!statsDict.TryGetValue(participant.Player.Id, out var ps))
                    continue;

                // VPIP if they voluntarily put money in preflop (call/raise/bet).
                // Posting blinds does NOT count by itself.
                bool vpipThisHand = hand.Actions.Any(a =>
                    a.Player.Id == participant.Player.Id &&
                    a.StreetType == StreetType.Preflop &&
                    (a.ActionType == ActionType.Call ||
                     a.ActionType == ActionType.Raise ||
                     a.ActionType == ActionType.Bet)
                );

                if (vpipThisHand)
                {
                    ps.VpipHands++;
                }
            }

            // ----- TRUE C-BET LOGIC -----
            bool reachedFlop = hand.Streets.Any(s =>
                s.StreetType == StreetType.Flop && !s.IsSecondRun);

            if (!reachedFlop)
                continue;

            var lastPreflopRaise = hand.Actions
                .Where(a => a.StreetType == StreetType.Preflop &&
                            a.ActionType == ActionType.Raise)
                .LastOrDefault();

            if (lastPreflopRaise == null)
                continue; // limp pot → no c-bet opportunity

            var aggressor = lastPreflopRaise.Player;

            if (!statsDict.TryGetValue(aggressor.Id, out var aps))
                continue;

            aps.CBetOpportunities++;

            bool cBetMade = hand.Actions.Any(a =>
                a.Player.Id == aggressor.Id &&
                a.StreetType == StreetType.Flop &&
                a.ActionType == ActionType.Bet
            );

            if (cBetMade)
            {
                aps.CBetsMade++;
            }
        }

        return statsDict.Values.ToList();
    }
}

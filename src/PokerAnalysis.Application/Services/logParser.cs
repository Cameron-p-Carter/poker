using System.IO;
using System.Text.RegularExpressions;
using PokerAnalysis.Domain;
using PokerAnalysis.Domain.Enums;

namespace PokerAnalysis.Application.Services;

public class LogParser
{
    public Session ParseSession(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        Array.Reverse(lines);

        Dictionary<string, Player> players = new();
        List<Hand> hands = new();
        Hand? currentHand = null;
        StreetType currentStreet = StreetType.Preflop;

        // We need to track dealer info temporarily until we parse player stacks
        string? dealerPlayerId = null;
        int dealerSeat = 0;

        int smallBlind = 0;
        int bigBlind = 0;
        DateTime? sessionStart = null;
        DateTime? sessionEnd = null;
        string sessionId = "";

        foreach (string rawLine in lines)
        {
            if (!TryParseTimestampFromLogRow(rawLine, out DateTime timestamp))
                continue;

            if (!TryGetMessageField(rawLine, out string message))
                continue;

            // Track session time boundaries
            if (sessionStart == null) sessionStart = timestamp;
            sessionEnd = timestamp;

            if (message.Contains("-- starting hand"))
            {
                // Parse hand number
                Match handNumMatch = Regex.Match(message, @"hand #(\d+)");
                int handNumber = int.Parse(handNumMatch.Groups[1].Value);

                // Parse hand id
                Match idMatch = Regex.Match(message, @"\(id: (.+?)\)");
                string handId = idMatch.Groups[1].Value;

                // Parse dealer info - we need this to calculate positions later
                Match dealerMatch = Regex.Match(message, @"dealer: ""(.+?) @ (.+?)""");
                if (dealerMatch.Success)
                {
                    dealerPlayerId = dealerMatch.Groups[2].Value;  // Store the ID
                }

                // Create new hand (dealer seat will be set when we parse player stacks)
                currentHand = new Hand(handNumber, handId, timestamp, 0);

                // Reset street for new hand
                currentStreet = StreetType.Preflop;

                // Use first hand's ID as session ID if not set
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = handId;
                }
            }
            else if (message.Contains("-- ending hand"))
            {
                if (currentHand != null)
                {
                    hands.Add(currentHand);
                    currentHand = null;
                }
            }
            else if (message.Contains("Player stacks:") && currentHand != null)
            {
                var playerStacks = ParsePlayerStacks(message);
                List<int> activeSeats = playerStacks.Select(p => p.seat).ToList();

                // Find dealer seat by matching dealer player ID
                foreach (var (seat, name, id, stack) in playerStacks)
                {
                    if (id == dealerPlayerId)
                    {
                        dealerSeat = seat;
                        currentHand.DealerSeatNumber = seat;
                        break;
                    }
                }

                // Now create participants for each player
                foreach (var (seat, name, id, stack) in playerStacks)
                {
                    // Add to players dictionary if new
                    if (!players.ContainsKey(id))
                    {
                        players[id] = new Player(id, name);
                    }

                    // Calculate position
                    Position position = CalculatePosition(seat, dealerSeat, activeSeats);

                    // Create hand participant
                    HandParticipant participant = new HandParticipant(
                        players[id],
                        seat,
                        position,
                        stack
                    );
                    currentHand.Participants.Add(participant);
                }
            }
            else if (message.Contains("posts a small blind") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);
                int amount = ParseBlindAmount(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                if (smallBlind == 0) smallBlind = amount;

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.PostSmallBlind,
                    StreetType.Preflop,
                    false
                );
                action.Amount = amount;
                currentHand.Actions.Add(action);
            }
            else if (message.Contains("posts a big blind") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);
                int amount = ParseBlindAmount(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                if (bigBlind == 0) bigBlind = amount;

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.PostBigBlind,
                    StreetType.Preflop,
                    false
                );
                action.Amount = amount;
                currentHand.Actions.Add(action);
            }
            else if (message.Contains("\" folds") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.Fold,
                    currentStreet,
                    false
                );
                currentHand.Actions.Add(action);
            }
            else if (message.Contains("\" checks") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.Check,
                    currentStreet,
                    false
                );
                currentHand.Actions.Add(action);
            }
            else if (message.Contains("\" calls") && !message.Contains("Uncalled") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);
                int amount = ParseActionAmount(message);
                bool isAllIn = IsAllIn(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.Call,
                    currentStreet,
                    isAllIn
                );
                currentHand.Actions.Add(action);
                action.Amount = amount;
            }
            else if (message.Contains("\" bets") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);
                int amount = ParseActionAmount(message);
                bool isAllIn = IsAllIn(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.Bet,
                    currentStreet,
                    isAllIn
                );
                action.Amount = amount;
                currentHand.Actions.Add(action);
            }
            else if (message.Contains("\" raises") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);
                int amount = ParseActionAmount(message);
                bool isAllIn = IsAllIn(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                PlayerAction action = new PlayerAction(
                    players[id],
                    ActionType.Raise,
                    currentStreet,
                    isAllIn
                );
                action.Amount = amount;
                currentHand.Actions.Add(action);
            }
            else if (message.Contains("Flop:") && currentHand != null)
            {
                string[] cards = ParseCommunityCards(message);
                bool isSecondRun = IsSecondRun(message);

                Street street = new Street(StreetType.Flop, cards, isSecondRun);
                currentHand.Streets.Add(street);

                if (!isSecondRun)
                {
                    currentStreet = StreetType.Flop;
                }
            }
            else if (message.Contains("Turn:") && currentHand != null)
            {
                string[] cards = ParseCommunityCards(message);
                bool isSecondRun = IsSecondRun(message);

                Street street = new Street(StreetType.Turn, cards, isSecondRun);
                currentHand.Streets.Add(street);

                if (!isSecondRun)
                {
                    currentStreet = StreetType.Turn;
                }
            }
            else if (message.Contains("River:") && currentHand != null)
            {
                string[] cards = ParseCommunityCards(message);
                bool isSecondRun = IsSecondRun(message);

                Street street = new Street(StreetType.River, cards, isSecondRun);
                currentHand.Streets.Add(street);

                if (!isSecondRun)
                {
                    currentStreet = StreetType.River;
                }
            }
            else if (message.Contains("\" shows a") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);
                string[] holeCards = ParseHoleCards(message);

                HandParticipant? participant = currentHand.Participants
                    .FirstOrDefault(p => p.Player.Id == id);

                if (participant != null)
                {
                    participant.HoleCards = holeCards;
                }
            }
            else if (message.Contains("collected") && message.Contains("from pot") && currentHand != null)
            {
                var (name, id) = ParsePlayer(message);

                int amount = ParseCollectedAmount(message);

                string? winningHand = ParseWinningHand(message);
                bool isSecondRun = IsSecondRun(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }

                HandResult result = new HandResult(players[id], amount, isSecondRun);
                result.WinningHand = winningHand;
                currentHand.Results.Add(result);
            }
            else if (message.Contains("joined the game"))
            {
                var (name, id) = ParsePlayer(message);

                if (!players.ContainsKey(id))
                {
                    players[id] = new Player(id, name);
                }
            }
        }

        Session session = new Session(
            sessionId,
            sessionStart ?? DateTime.MinValue,
            sessionEnd ?? DateTime.MaxValue,
            smallBlind,
            bigBlind,
            0,      // ante (would need to parse if present)
            false   // allowRunItTwice (would need to detect from logs)
        );

        session.Hands = hands;
        session.Players = players.Values.ToList();

        return session;
    }

    private static bool TryParseTimestampFromLogRow(string line, out DateTime timestamp)
    {
        timestamp = default;
        if (string.IsNullOrWhiteSpace(line)) return false;

        int lastComma = line.LastIndexOf(',');
        if (lastComma < 0) return false;

        int secondLastComma = line.LastIndexOf(',', lastComma - 1);
        if (secondLastComma < 0) return false;

        string timestampStr = line.Substring(secondLastComma + 1, lastComma - secondLastComma - 1);
        return DateTime.TryParse(timestampStr, out timestamp);
    }

    private static bool TryGetMessageField(string line, out string message)
    {
        message = "";
        if (string.IsNullOrWhiteSpace(line)) return false;

        int lastComma = line.LastIndexOf(',');
        if (lastComma < 0) return false;

        int secondLastComma = line.LastIndexOf(',', lastComma - 1);
        if (secondLastComma < 0) return false;

        message = line.Substring(0, secondLastComma);

        // Strip outer quotes if present
        if (message.Length >= 2 && message[0] == '"' && message[^1] == '"')
            message = message.Substring(1, message.Length - 2);

        // Unescape CSV quotes: "" -> "
        message = message.Replace("\"\"", "\"");

        return true;
    }

    private (string name, string id) ParsePlayer(string text)
    {
        // expects clean message text, not raw CSV row
        var match = Regex.Match(text, @"""([^""]+?) @ ([^""]+?)""");
        if (match.Success)
            return (match.Groups[1].Value, match.Groups[2].Value);

        return ("", "");
    }

    private string[] ParseCommunityCards(string text)
    {
        Match match = Regex.Match(text, @"\[(.+?)\]");
        if (match.Success)
        {
            string cards = match.Groups[1].Value;
            return cards.Split(", ");
        }
        return Array.Empty<string>();
    }

    private string[] ParseHoleCards(string text)
    {
        Match match = Regex.Match(text, @"shows a (.+)\.");
        if (match.Success)
        {
            string cards = match.Groups[1].Value;
            return cards.Split(", ");
        }
        return Array.Empty<string>();
    }

    // ============ ADDITIONAL HELPERS ============

    private List<(int seat, string name, string id, int stack)> ParsePlayerStacks(string text)
    {
        var result = new List<(int seat, string name, string id, int stack)>();
        string[] playerParts = text.Split('|');

        foreach (string part in playerParts)
        {
            Match match = Regex.Match(part, @"#(\d+)\s+""([^""]+?) @ ([^""]+?)""\s+\((\d+)\)");
            if (match.Success)
            {
                int seat = int.Parse(match.Groups[1].Value);
                string name = match.Groups[2].Value;
                string id = match.Groups[3].Value;
                int stack = int.Parse(match.Groups[4].Value);

                result.Add((seat, name, id, stack));
            }
        }

        return result;
    }

    private Position CalculatePosition(int seatNumber, int dealerSeat, List<int> activeSeats)
    {
        activeSeats.Sort();
        int dealerIndex = activeSeats.IndexOf(dealerSeat);
        int playerIndex = activeSeats.IndexOf(seatNumber);
        int playerCount = activeSeats.Count;
        int positionsFromDealer = (playerIndex - dealerIndex + playerCount) % playerCount;

        return positionsFromDealer switch
        {
            0 => Position.Button,
            1 => Position.SmallBlind,
            2 => Position.BigBlind,
            3 => Position.UTG,
            4 => Position.UTG1,
            5 => Position.UTG2,
            6 => Position.UTG3,
            7 => Position.Middle,
            8 => Position.Lojack,
            9 => Position.Hijack,
            10 => Position.Cutoff,
            _ => Position.Cutoff
        };
    }

    private string? ParseWinningHand(string text)
    {
        Match match = Regex.Match(text, @"with (.+?) \(combination");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return null;
    }

    private int ParseCollectedAmount(string text)
    {
        Match match = Regex.Match(text, @"collected (\d+) from");
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        return 0;
    }

    private bool IsAllIn(string text)
    {
        return text.Contains("all in");
    }

    private bool IsSecondRun(string text)
    {
        return text.Contains("(second run)");
    }
    private int ParseBlindAmount(string text)
    {
        var m = Regex.Match(text, @"posts a (?:small|big) blind(?: of)?\s+(\d+)", RegexOptions.IgnoreCase);
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }
    private int ParseActionAmount(string text)
{
    // Prefer "raises to X" (final bet size)
    var raiseTo = Regex.Match(text, @"\braises\s+to\s+(\d+)\b", RegexOptions.IgnoreCase);
    if (raiseTo.Success) return int.Parse(raiseTo.Groups[1].Value);

    // Some logs use "raises X to Y"
    var raiseXtoY = Regex.Match(text, @"\braises\s+\d+\s+to\s+(\d+)\b", RegexOptions.IgnoreCase);
    if (raiseXtoY.Success) return int.Parse(raiseXtoY.Groups[1].Value);

    // Calls / Bets: "calls 41", "bets 15", "bets 41 and go all in"
    var call = Regex.Match(text, @"\bcalls\s+(\d+)\b", RegexOptions.IgnoreCase);
    if (call.Success) return int.Parse(call.Groups[1].Value);

    var bet = Regex.Match(text, @"\bbets\s+(\d+)\b", RegexOptions.IgnoreCase);
    if (bet.Success) return int.Parse(bet.Groups[1].Value);

    // Fallback: last number in line (safer than first)
    var matches = Regex.Matches(text, @"\d+");
    return matches.Count > 0 ? int.Parse(matches[^1].Value) : 0;
}

}


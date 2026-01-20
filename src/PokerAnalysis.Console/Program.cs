using PokerAnalysis.Application.Services;
using PokerAnalysis.Application.Models;
using PokerAnalysis.Domain;

// Change this to your actual CSV file path
string filePath = @"C:\Users\cz\Downloads\nit.csv";

Console.WriteLine("poker\n");

// Parse the session
LogParser parser = new LogParser();
Session session = parser.ParseSession(filePath);

// Print session info
Console.WriteLine($"Session ID: {session.SessionId}");
Console.WriteLine($"Blinds: {session.SmallBlind}/{session.BigBlind}");
Console.WriteLine($"Total Hands: {session.Hands.Count}");
Console.WriteLine($"Total Players: {session.Players.Count}");
Console.WriteLine();

// Calculate and print stats
StatCalculator calculator = new StatCalculator();
List<PlayerStats> stats = calculator.Calculate(session);

Console.WriteLine("stats\n");

foreach (PlayerStats ps in stats)
{
    Console.WriteLine($"Player: {ps.Player.DisplayName}");

    Console.WriteLine($"  Hands Played: {ps.HandsPlayed}");
    Console.WriteLine($"  Hands Won: {ps.HandsWon}");

    Console.WriteLine($"  C-Bet Opportunities (Preflop Aggressor + Flop): {ps.CBetOpportunities}");
    Console.WriteLine($"  C-Bets Made: {ps.CBetsMade}");
    Console.WriteLine($"  C-Bet %: {ps.CBetPercentage:F1}%");

    Console.WriteLine();
    Console.WriteLine($"  VPIP Hands: {ps.VpipHands}");
Console.WriteLine($"  VPIP %: {ps.VpipPercentage:F1}%");
}

Console.WriteLine("=== Done ===");

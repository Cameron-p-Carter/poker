using PokerAnalysis.Application.Services;
using PokerAnalysis.Application.Models;
using PokerAnalysis.Domain;

Console.WriteLine("Welcome to Polker");
Console.WriteLine("");

Console.WriteLine("Enter The full filepath for the CSV file: ");
string? filePath = Console.ReadLine();

LogParser logParser= new LogParser();
Session session = logParser.ParseSession(filePath);
StatCalculator statCalculator = new StatCalculator();
List<PlayerStats> stats = statCalculator.CalculateStats(session);

foreach (PlayerStats statsItem in stats)
{
    Console.WriteLine("Name:");
    Console.WriteLine(statsItem.Player.DisplayName);
    Console.WriteLine("Hands Won:");
    Console.WriteLine(statsItem.HandsWon);
    Console.WriteLine("Hands Played");
    Console.WriteLine(statsItem.HandsPlayed);
    Console.WriteLine("Vpip Events:");
    Console.WriteLine(statsItem.VpipEvents);
    Console.WriteLine("Vpip:");
    Console.WriteLine(statsItem.Vpip);
    Console.WriteLine("");
    
}





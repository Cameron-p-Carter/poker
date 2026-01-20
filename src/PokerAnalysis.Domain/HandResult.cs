namespace PokerAnalysis.Domain;

public class HandResult
{
    public Player Player { get; set;}
    public int AmountWon { get; set; }
    public string? WinningHand { get; set; }
    public bool IsSecondRun { get; set; }

    public HandResult(Player player, int amountWon, bool isSecondRun)
    {
        Player = player;
        AmountWon = amountWon;
        IsSecondRun = isSecondRun;
    }
}
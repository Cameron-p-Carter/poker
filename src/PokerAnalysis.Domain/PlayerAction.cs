namespace PokerAnalysis.Domain;
using PokerAnalysis.Domain.Enums;

public class PlayerAction
{
    public Player Player { get; set; }
    public ActionType ActionType { get; set; }
    public int? Amount { get; set; }
    public StreetType StreetType { get; set; }
    public bool IsAllIn { get; set; }

    public PlayerAction(Player player, ActionType actionType, StreetType streetType, bool isAllIn)
    {
        Player = player;
        ActionType = actionType;
        StreetType = streetType;
        IsAllIn = isAllIn;
    }
}
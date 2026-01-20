namespace PokerAnalysis.Domain;
using PokerAnalysis.Domain.Enums;

public class Street
{
    public StreetType StreetType { get; set; }
    public string[] CommunityCards { get; set; }
    public bool IsSecondRun { get; set; }

    public Street(StreetType streetType, bool isSecondRun)
    {
        StreetType = streetType;
        CommunityCards = Array.Empty<string>();
        IsSecondRun = isSecondRun;
    }

    public Street(StreetType streetType, string[] communityCards, bool isSecondRun)
    {
        StreetType = streetType;
        CommunityCards = communityCards;
        IsSecondRun = isSecondRun;
    }
}
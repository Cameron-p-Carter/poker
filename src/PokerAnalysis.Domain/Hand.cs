namespace PokerAnalysis.Domain;

public class Hand
{
    public int HandNumber { get; set; }
    public string HandId { get; set; }
    public DateTime TimeStamp { get; set; }
    public int DealerSeatNumber { get; set; }
    public List<HandParticipant> Participants { get; set; }
    public List<PlayerAction> Actions { get; set; }
    public List<Street> Streets { get; set; }
    public List<HandResult> Results { get; set; }

    public Hand(int handNumber, string handId, DateTime timeStamp, int dealerSeatNumber)
    {
        HandNumber = handNumber;
        HandId = handId;
        TimeStamp = timeStamp;
        DealerSeatNumber = dealerSeatNumber;
        Participants = new List<HandParticipant>();
        Actions = new List<PlayerAction>();
        Streets = new List<Street>();
        Results = new List<HandResult>();

    }
}
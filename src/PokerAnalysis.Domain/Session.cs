namespace PokerAnalysis.Domain;

public class Session
{
    public string SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int SmallBlind { get; set; }
    public int BigBlind { get; set; }
    public int Ante { get; set; }
    public bool AllowRunItTwice { get; set; }
    public List<Hand> Hands { get; set; }
    public List<Player> Players { get; set; }

    public Session(string sessionId, DateTime startTime, DateTime endTime, int smallBlind, int bigBlind, int ante, bool allowRunItTwice)
    {
        SessionId = sessionId;
        StartTime = startTime;
        EndTime = endTime;
        SmallBlind = smallBlind;
        BigBlind = bigBlind;
        Ante = ante;
        AllowRunItTwice = allowRunItTwice;
        Hands = new List<Hand>();
        Players = new List<Player>();
    }
}
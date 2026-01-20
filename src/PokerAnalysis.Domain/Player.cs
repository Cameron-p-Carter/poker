namespace PokerAnalysis.Domain;

public class Player
{
    public string Id { get; set;}
    public string DisplayName { get; set;}
    public List<string> Aliases{ get; set;}

    public Player(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
        Aliases = new List<string>();
    }
}
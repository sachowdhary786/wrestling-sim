using System.Collections.Generic;

/// <summary>
/// Tracks the state of a match across all simulation phases
/// </summary>
public class MatchState
{
    public List<Wrestler> wrestlers;
    public Dictionary<string, WrestlerStats> wrestlerStats;
    public Dictionary<Wrestler, float> scores;
    public Dictionary<Wrestler, float> momentum;
    public Match match;
    public GameData data;
    public (float tech, float brawl, float psych, float aerial) weights;
    public float BookingModifier { get; }

    public MatchState(List<Wrestler> wrestlers, WrestlerStats stats1, WrestlerStats stats2, Match match, GameData data, (float, float, float, float) weights, float bookingModifier)
    {
        this.wrestlers = wrestlers;
        this.match = match;
        this.data = data;
        this.weights = weights;
        this.BookingModifier = bookingModifier;
        this.scores = new Dictionary<Wrestler, float>();
        this.momentum = new Dictionary<Wrestler, float>();
        this.wrestlerStats = new Dictionary<string, WrestlerStats>
        {
            { wrestlers[0].id.ToString(), stats1 },
            { wrestlers[1].id.ToString(), stats2 }
        };
    }
}

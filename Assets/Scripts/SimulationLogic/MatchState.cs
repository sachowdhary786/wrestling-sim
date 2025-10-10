using System.Collections.Generic;

/// <summary>
/// Tracks the state of a match across all simulation phases
/// </summary>
public class MatchState
{
    public List<Wrestler> wrestlers;
    public Dictionary<Wrestler, float> scores;
    public Dictionary<Wrestler, float> momentum;
    public Match match;
    public GameData data;
    public (float tech, float brawl, float psych, float aerial) weights;

    public MatchState(List<Wrestler> wrestlers, Match match, GameData data, (float, float, float, float) weights)
    {
        this.wrestlers = wrestlers;
        this.match = match;
        this.data = data;
        this.weights = weights;
        this.scores = new Dictionary<Wrestler, float>();
        this.momentum = new Dictionary<Wrestler, float>();
    }
}

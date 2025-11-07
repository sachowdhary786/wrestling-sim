using System;
using System.Collections.Generic;

[Serializable]
public class Match
{
    public Guid id;
    public Guid? titleId;
    public string location;
    public string matchType;
    public bool titleMatch;
    public Referee referee;
    public MatchAim matchAim;
    public bool isMainEvent;

    public List<Guid> participants = new List<Guid>();
    public Dictionary<Guid, Guid> managers = new Dictionary<Guid, Guid>(); // Key: Wrestler ID, Value: Manager's Wrestler ID
    public Guid? roadAgentId;
    public Guid winnerId;
    public int rating;
    public FinishType finishType;

    public Match(Wrestler wrestler1, Wrestler wrestler2, GameData gameData)
    {
        id = Guid.NewGuid();
        participants.Add(wrestler1.id);
        participants.Add(wrestler2.id);
        matchType = "Singles";
        RefereeManager.AssignReferee(this, gameData);
    }

    public Match(TagTeam team1, TagTeam team2, GameData gameData)
    {
        id = Guid.NewGuid();
        participants.AddRange(team1.members);
        participants.AddRange(team2.members);
        matchType = "TagTeam";
        RefereeManager.AssignReferee(this, gameData);
    }

    public Match() { }

}

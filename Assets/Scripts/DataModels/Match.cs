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
}

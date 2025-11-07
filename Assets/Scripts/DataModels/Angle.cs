using System;
using System.Collections.Generic;

public enum AngleType
{
    Attack,          // One wrestler attacks another
    Promo,           // A wrestler cuts a promo on another
    Turn,            // A wrestler turns on their partner/friend
    Respect,         // Wrestlers show respect after a great match
    Debut,           // A new wrestler debuts
    Interference     // Someone interferes in the match/aftermath
}

public enum AngleOutcome
{
    NewFeud,
    RivalryEscalated,
    NewAlliance,
    HeelTurn,
    FaceTurn,
    Injury,
    MomentumShift
}

[Serializable]
public class Angle
{
    public Guid id;
    public AngleType type;
    public List<Guid> participants = new List<Guid>();
    public string description;
    public List<AngleOutcome> outcomes = new List<AngleOutcome>();
}

using System;
using System.Collections.Generic;

[Serializable]
public class Match
{
    public int id;
    public string titleId;
    public string location;
    public string matchType;
    public bool titleMatch;
    public Referee referee;

    public List<string> participants = new List<string>();
    public string winnerId;
    public int rating;
    public string finishType;
}

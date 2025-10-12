using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TagTeam
{
    public Guid id;
    public string name;
    public List<Guid> members = new List<Guid>(); // wrestler IDs
    public int chemistry; // fixed value, e.g. 0–20 boost
}

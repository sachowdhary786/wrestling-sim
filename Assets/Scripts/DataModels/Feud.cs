using System;
using System.Collections.Generic;

[System.Serializable]
public class Feud
{
    public Guid id;
    public List<Guid> participants = new List<Guid>(); // 2–4 wrestlers
    public int heat; // 0–100
    public int durationWeeks;
    public bool active;
}

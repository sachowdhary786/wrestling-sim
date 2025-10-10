[System.Serializable]
public class Referee
{
    public string id;
    public string name;
    
    // Core Stats (0-100)
    public int strictness;      // Higher = more DQs, count outs, rule enforcement
    public int corruption;      // Higher = more easily influenced, biased finishes
    public int experience;      // Affects match pacing, rating bonus
    public int consistency;     // Affects how predictable their calls are
    
    // Status
    public bool isFavoredByCompany;  // Company-backed referee
    public bool isActive;
    public bool isInjured;
    public int injuryWeeksRemaining;
    
    // Career
    public int age;
    public int yearsExperience;
    public RefereeStats stats;
    
    // Fatigue & Scheduling
    public int fatigue;         // 0-100, increases with matches
    public int matchesThisWeek;
    public int consecutiveWeeks;
    
    // Specializations
    public bool isMainEventRef;      // Better at handling big matches
    public bool isHardcoreSpecialist; // Better at extreme rules matches

    public Referee(string name, int strictness, int corruption, int experience)
    {
        this.name = name;
        this.strictness = strictness;
        this.corruption = corruption;
        this.experience = experience;
        this.consistency = 70; // Default
        this.isActive = true;
        this.age = 30;
        this.yearsExperience = 5;
        this.fatigue = 0;
        this.stats = new RefereeStats(this.id);
    }

    public Referee() 
    {
        this.stats = new RefereeStats(this.id);
    }

    /// <summary>
    /// Gets the referee's overall quality rating
    /// </summary>
    public int GetQualityRating()
    {
        // Higher experience and consistency = better referee
        // Lower corruption = better referee
        return (experience + consistency + (100 - corruption)) / 3;
    }

    /// <summary>
    /// Checks if this referee is suitable for a specific match type
    /// </summary>
    public bool IsSuitableFor(string matchType)
    {
        // Hardcore specialist check
        if (isHardcoreSpecialist)
        {
            return matchType switch
            {
                "Hardcore" => true,
                "NoDisqualification" => true,
                "StreetFight" => true,
                "FallsCountAnywhere" => true,
                "LastManStanding" => true,
                _ => true // Can still do regular matches
            };
        }

        // Main event ref check
        if (isMainEventRef && experience < 70)
            return false; // Main event refs need high experience

        return isActive;
    }
}

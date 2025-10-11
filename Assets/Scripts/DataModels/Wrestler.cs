using System.Collections.Generic;

[System.Serializable]
public class wrestler
{
    public int id;
    public string name;
    public string hometown;
    public int age;

    // Stats
    public int charisma;
    public int micSkill;
    public int psychology;
    public int technical;
    public int brawling;
    public int aerial;
    public int stamina;
    public int toughness;

    // Current State
    public bool injured;
    public bool active = true;
    public int injurySeverity; // 1 = Minor, 2 = Moderate, 3 = Severe
    public string injuryType;
    public int recoveryWeeksRemaining;
    
    // Booking & Performance State
    public int fatigue; // 0-100, increases with matches
    public int morale; // 0-100, affects performance
    public int popularity; // 0-100, overall popularity
    public int momentum; // -100 to +100, recent booking strength
    public int matchesThisWeek;
    public int matchesThisMonth;
    public int daysRestSinceLastMatch;

    // Career
    public Dictionary<string, int> popularityByLocation;
    public Alignment alignment;
    public string gimmick;
    public string companyId;
    public string imagePath;

    // Relationships
    public List<string> friends = new List<string>();
    public List<string> rivals = new List<string>();\

    // Traits
    public List<string> traits = new List<string>();

    public List<string> teamIds = new List<string>();
    public int teamwork;

    // Chemistry Modifiers
    public Dictionary<string, int> chemistry = new Dictionary<string, int>();

    public wrestler()
    {
        popularityByLocation = new Dictionary<string, int>();
        morale = 70; // Start at decent morale
        popularity = 50; // Start at average popularity
        fatigue = 0; // Start fresh
        momentum = 0; // Start neutral
        daysRestSinceLastMatch = 7;
    }

    // Helper method to get popularity for a specific location
    public int GetPopularity(string location)
    {
        if (popularityByLocation.ContainsKey(location))
        {
            return popularityByLocation[location];
        }
        return 0; // Default popularity if location not found
    }

    // Helper method to set popularity for a specific location
    public void SetPopularity(string location, int value)
    {
        popularityByLocation[location] = value;
    }
}

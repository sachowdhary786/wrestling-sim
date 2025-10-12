using System.Collections.Generic;

[System.Serializable]
public class Wrestler
{
    public Guid id;
    public string name;
    public int popularity; // Overall popularity, used for signing logic and main event status
    public string hometown;
    public int age;

    // The wrestler's current employment contract. Null if a free agent.
    public Contract contract;

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
    public bool isRetired = false;
    public bool isManager = false;
    public Guid? trainerId;
    public int injurySeverity; // 1 = Minor, 2 = Moderate, 3 = Severe
    public string injuryType;
    public int recoveryWeeksRemaining;
    
    // Booking & Performance State
    public int fatigue; // 0-100, increases with matches
    public int morale; // 0-100, affects performance
    public int momentum; // -100 to +100, recent booking strength
    public int matchesThisWeek;
    public int matchesThisMonth;
    public int daysRestSinceLastMatch;

    // Career
    public Dictionary<string, int> popularityByLocation;
    public Dictionary<string, int> crowdFatigue; // Tracks over-exposure in a location
    public Alignment alignment;
    public string gimmick;
    public string imagePath;

    // Relationships
    public List<Guid> friends = new List<Guid>();
    public List<Guid> rivals = new List<Guid>();

    // Traits
    public List<Guid> traits = new List<Guid>();

    public List<Guid> teamIds = new List<Guid>();
    public int teamwork;

    // Chemistry Modifiers
    public Dictionary<Guid, int> chemistry = new Dictionary<Guid, int>();

    public Wrestler()
    {
        id = Guid.NewGuid();
        popularityByLocation = new Dictionary<string, int>();
        crowdFatigue = new Dictionary<string, int>();
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

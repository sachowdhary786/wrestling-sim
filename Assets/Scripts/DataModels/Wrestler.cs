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

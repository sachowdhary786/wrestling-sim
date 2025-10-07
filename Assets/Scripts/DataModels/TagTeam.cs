[System.Serializable]
public class TagTeam
{
    public string id;
    public string name;
    public List<string> members = new List<string>(); // wrestler IDs
    public int chemistry; // fixed value, e.g. 0–20 boost
}

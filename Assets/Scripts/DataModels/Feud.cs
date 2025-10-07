[System.Serializable]
public class Feud
{
    public string id;
    public List<string> participants; // 2–4 wrestlers
    public int heat; // 0–100
    public int durationWeeks;
    public bool active;
}

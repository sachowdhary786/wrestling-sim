public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Wrestler> roster;
    public List<Show> shows;
    public Dictionary<string, Wrestler> champions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}

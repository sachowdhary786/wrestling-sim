public static class SaveManager
{
    private static string path = Application.persistentDataPath + "/save.json";

    public static void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static GameData Load()
    {
        if (!File.Exists(path))
            return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<GameData>(json);
    }
}

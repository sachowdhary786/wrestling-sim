using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string path = Application.persistentDataPath + "/save.json";

    public static void Save(GameData data)
    {
        string json = UnityEngine.JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static GameData Load()
    {
        if (!File.Exists(path))
            return null;
        string json = File.ReadAllText(path);
        return UnityEngine.JsonUtility.FromJson<GameData>(json);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Handles loading all game data from JSON files in the Resources folder.
/// </summary>
public static class DataLoader
{
    // Wrapper for deserializing root JSON arrays from Resources
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }

    /// <summary>
    /// Loads all data from JSON files and populates a GameData object.
    /// </summary>
    public static GameData LoadGameData()
    {
        var gameData = new GameData();

        // Load all game data, initializing with empty lists if files are not found.
        try
        {
            gameData.companies = LoadData<Company>("Companies").ToDictionary(c => c.id, c => c);
            gameData.wrestlers = LoadData<Wrestler>("Wrestlers").ToDictionary(w => w.id, w => w);
            gameData.titles = LoadData<Title>("Titles").ToDictionary(t => t.id, t => t);
            gameData.feuds = LoadData<Feud>("Feuds");
            gameData.teams = LoadData<TagTeam>("TagTeams");
            gameData.referees = LoadData<Referee>("Referees").ToDictionary(r => r.id, r => r);
            gameData.traits = LoadData<Trait>("Traits").ToDictionary(t => t.id, t => t);
        }
        catch (ArgumentException ex)
        {
            UnityEngine.Debug.LogError(
                $"Duplicate key while building game data: {ex.Message}\n{ex.StackTrace}"
            );
            throw;
        }

        // Assign contracts based on the loaded data
        AssignContracts(gameData);

        return gameData;
    }

    public static GameData LoadSavedGame(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            UnityEngine.Debug.LogError($"[DataLoader] Save file not found at {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            var serializableData = JsonUtility.FromJson<SerializableGameData>(json);

            var gameData = new GameData
            {
                companies = serializableData.companies.ToDictionary(c => c.id, c => c),
                wrestlers = serializableData.wrestlers.ToDictionary(w => w.id, w => w),
                titles = serializableData.titles.ToDictionary(t => t.id, t => t),
                feuds = serializableData.feuds,
                teams = serializableData.teams,
                referees = serializableData.referees.ToDictionary(r => r.id, r => r),
                traits = serializableData.traits.ToDictionary(t => t.id, t => t),
            };

            UnityEngine.Debug.Log($"[DataLoader] Game data successfully loaded from {path}");
            return gameData;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[DataLoader] Failed to load saved game data: {e.Message}");
            return null;
        }
    }

    private static List<T> LoadData<T>(string folderName) where T : class
    {
        var path = Path.Combine(Application.streamingAssetsPath, folderName);
        if (!Directory.Exists(path))
        {
            UnityEngine.Debug.LogWarning($"Data folder not found: {path}");
            return new List<T>();
        }

        var dataList = new List<T>();
        var files = Directory.GetFiles(path, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var item = JsonUtility.FromJson<T>(json);
                if (item != null)
                    dataList.Add(item);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to load {typeof(T).Name} from {file}: {ex.Message}");
            }
        }

        // ✅ Run duplicate/empty ID protection before returning
        CleanIds(dataList);

        return dataList;
    }

    private static void CleanIds<T>(List<T> list)
    {
        var idField = typeof(T).GetField("id");
        if (idField == null || idField.FieldType != typeof(Guid))
            return; // Nothing to clean

        var seenIds = new HashSet<Guid>();
        foreach (var item in list)
        {
            var id = (Guid)idField.GetValue(item);

            // Auto-generate new ID if empty
            if (id == Guid.Empty)
            {
                var newId = Guid.NewGuid();
                idField.SetValue(item, newId);
                UnityEngine.Debug.LogWarning($"{typeof(T).Name} had empty ID — assigned new ID: {newId}");
                id = newId;
            }

            // Detect duplicates
            if (!seenIds.Add(id))
            {
                var newId = Guid.NewGuid();
                idField.SetValue(item, newId);
                UnityEngine.Debug.LogWarning($"{typeof(T).Name} had duplicate ID — assigned new ID: {newId}");
            }
        }
    }



    private static void AssignContracts(GameData gameData)
    {
        if (gameData.companies == null || gameData.wrestlers == null)
            return;

        foreach (var wrestler in gameData.wrestlers.Values)
        {
            if (wrestler.companyId.HasValue)
            {
                if (gameData.companies.TryGetValue(wrestler.companyId.Value, out var company))
                {
                    // Create a default 12-month contract with a salary based on popularity
                    int salary = 500 * wrestler.popularity;
                    wrestler.contract = new Contract(company.id, salary, 12);
                    company.roster.Add(wrestler.id);
                }
            }
        }
        UnityEngine.Debug.Log("[DataLoader] Assigned contracts and rosters.");
    }
}

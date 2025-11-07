using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles saving all game data to a JSON file.
/// </summary>
public static class DataSaver
{
    /// <summary>
    /// Saves the provided GameData object to a JSON file.
    /// </summary>
    /// <param name="gameData">The GameData object to save.</param>
    /// <param name="fileName">The name of the save file (e.g., "savegame.json").</param>
    public static void SaveGameData(GameData gameData, string fileName)
    {
        // Convert dictionaries to lists for serialization
        var serializableData = new SerializableGameData
        {
            companies = gameData.companies.Values.ToList(),
            wrestlers = gameData.wrestlers.Values.ToList(),
            titles = gameData.titles.Values.ToList(),
            feuds = gameData.feuds.ToList(), // feuds is already a list
            teams = gameData.teams.ToList(), // teams is already a list
            referees = gameData.referees.Values.ToList(),
            traits = gameData.traits.Values.ToList()
        };

        try
        {
            string json = JsonUtility.ToJson(serializableData, true);
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, json);
            Debug.Log($"[DataSaver] Game data successfully saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataSaver] Failed to save game data: {e.Message}");
        }
    }
}

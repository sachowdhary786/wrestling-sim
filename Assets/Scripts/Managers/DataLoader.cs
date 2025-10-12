using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles loading all game data from JSON files in the Resources folder.
/// </summary>
public static class DataLoader
{
    // Generic wrapper for deserializing a root JSON array
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }

    // Temporary class to handle deserialization of wrestlers with the old companyId field
    [Serializable]
    private class WrestlerWithTempId : Wrestler
    {
        public Guid? companyId;
    }

    /// <summary>
    /// Loads all data from JSON files and populates a GameData object.
    /// </summary>
    public static GameData LoadGameData()
    {
        var gameData = new GameData();

        // Load all game data, initializing with empty lists if files are not found.
        gameData.companies = LoadData<Company>("Companies");
        var wrestlersWithId = LoadData<WrestlerWithTempId>("Wrestlers");
        gameData.wrestlers = wrestlersWithId.Cast<Wrestler>().ToList(); // Cast to the base type
        gameData.titles = LoadData<Title>("Titles");
        gameData.feuds = LoadData<Feud>("Feuds");
        gameData.teams = LoadData<TagTeam>("TagTeams");
        gameData.referees = LoadData<Referee>("Referees");
        gameData.traits = LoadData<Trait>("Traits");

        // Assign contracts based on the loaded data
        AssignContracts(wrestlersWithId, gameData);

        return gameData;
    }

    private static List<T> LoadData<T>(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            string wrappedJson = "{\"items\":" + jsonFile.text + "}";
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            Debug.Log($"[DataLoader] Loaded {wrapper.items.Count} items from {fileName}.json.");
            return wrapper.items;
        }
        else
        {
            Debug.Log($"[DataLoader] {fileName}.json not found. Initializing empty list.");
            return new List<T>();
        }
    }

    private static void AssignContracts(List<WrestlerWithTempId> wrestlersWithId, GameData gameData)
    {
        if (gameData.companies == null)
            return;

        foreach (var tempWrestler in wrestlersWithId)
        {
            if (tempWrestler.companyId.HasValue)
            {
                var wrestler = gameData.wrestlers.First(w => w.id == tempWrestler.id);
                var company = gameData.companies.FirstOrDefault(c =>
                    c.id == tempWrestler.companyId.Value
                );
                if (company != null)
                {
                    // Create a default 12-month contract with a salary based on popularity
                    int salary = 500 * wrestler.popularity;
                    wrestler.contract = new Contract(company.id, salary, 12);
                    company.roster.Add(wrestler.id);
                }
            }
        }
        Debug.Log("[DataLoader] Assigned contracts and rosters.");
    }
}

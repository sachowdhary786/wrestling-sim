using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Handles loading all game data from JSON files in the Resources folder.
/// </summary>
public static class DataLoader
{
    // Helper classes to wrap JSON arrays for deserialization with JsonUtility
    [Serializable]
    private class CompanyList { public List<Company> companies; }
    [Serializable]
    private class WrestlerList { public List<WrestlerWithTempId> wrestlers; }

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

        // Load Companies
        TextAsset companiesJson = Resources.Load<TextAsset>("Companies");
        if (companiesJson != null)
        {
            // A trick to deserialize a root array with JsonUtility
            string wrappedJson = "{\"companies\":" + companiesJson.text + "}";
            gameData.companies = JsonUtility.FromJson<CompanyList>(wrappedJson).companies;
            Debug.Log($"[DataLoader] Loaded {gameData.companies.Count} companies.");
        }
        else
        {
            Debug.LogError("[DataLoader] Companies.json not found in Resources folder!");
        }

        // Load Wrestlers
        TextAsset wrestlersJson = Resources.Load<TextAsset>("Wrestlers");
        if (wrestlersJson != null)
        {
            string wrappedJson = "{\"wrestlers\":" + wrestlersJson.text + "}";
            // Deserialize into the temporary class
            var wrestlersWithId = JsonUtility.FromJson<WrestlerList>(wrappedJson).wrestlers;
            // Convert to the proper Wrestler class
            gameData.wrestlers = wrestlersWithId.Cast<Wrestler>().ToList();
            // Now assign contracts based on the temporary ID
            AssignContracts(wrestlersWithId, gameData);
            Debug.Log($"[DataLoader] Loaded {gameData.wrestlers.Count} wrestlers.");
        }
        else
        {
            Debug.LogError("[DataLoader] Wrestlers.json not found in Resources folder!");
        }


        return gameData;
    }

    private static void AssignContracts(List<WrestlerWithTempId> wrestlersWithId, GameData gameData)
    {
        if (gameData.companies == null) return;

        foreach (var tempWrestler in wrestlersWithId)
        {
            if (tempWrestler.companyId.HasValue)
            {
                var wrestler = gameData.wrestlers.First(w => w.id == tempWrestler.id);
                var company = gameData.companies.FirstOrDefault(c => c.id == tempWrestler.companyId.Value);
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

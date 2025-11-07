using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameData gameData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadGame()
    {
        gameData = DataLoader.LoadGameData();
        if (gameData != null)
        {
            Debug.Log("[GameManager] Game data loaded successfully.");
        }
        else
        {
            Debug.LogError("[GameManager] Failed to load game data.");
        }
    }

    public void SaveGame(string saveFileName = "savegame.json")
    {
        if (gameData != null)
        {
            DataSaver.SaveGameData(gameData, saveFileName);
        }
        else
        {
            Debug.LogError("[GameManager] No game data to save.");
        }
    }

    public void AdvanceWeek()
    {
        if (gameData == null)
        {
            Debug.LogError("[GameManager] Cannot advance week, GameData is not loaded.");
            return;
        }

        Debug.Log("[GameManager] Advancing game state by one week...");

        // Advance referees
        foreach (var referee in gameData.referees.Values)
        {
            RefereeCareerManager.AdvanceWeek(referee);
        }

        // Advance wrestlers
        WrestlerStateManager.WeeklyReset(gameData);

        // Advance feuds
        foreach (var feud in gameData.feuds)
        {
            // FeudManager.AdvanceWeek(feud); // Placeholder for future feud progression logic
        }

        Debug.Log("[GameManager] Week advanced.");
    }
}

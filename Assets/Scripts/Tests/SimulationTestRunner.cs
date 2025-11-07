using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// An automated test runner to validate core simulation logic.
/// </summary>
public class SimulationTestRunner : MonoBehaviour
{
    private void Start()
    {
        // Use a coroutine to ensure the GameManager is initialized first.
        StartCoroutine(RunTests());
    }

    private IEnumerator RunTests()
    {
        Debug.Log("--- STARTING SIMULATION TEST RUN ---");

        // Wait a frame to ensure GameManager.Awake() has run.
        yield return null;

        if (GameManager.Instance == null || GameManager.Instance.gameData == null)
        {
            Debug.LogError("[TEST FAILED] GameManager or GameData not initialized.");
            yield break;
        }

        var gameData = GameManager.Instance.gameData;
        Debug.Log($"[TEST] Initial data loaded. Wrestlers: {gameData.wrestlers.Count}, Referees: {gameData.referees.Count}");

        // --- Test 1: Simulate a Singles Match ---
        Debug.Log("--- Running Test 1: Singles Match ---");
        var wrestler1 = gameData.wrestlers.Values.FirstOrDefault();
        var wrestler2 = gameData.wrestlers.Values.Skip(1).FirstOrDefault();
        if (wrestler1 != null && wrestler2 != null)
        {
            var singlesMatch = new Match(wrestler1, wrestler2, gameData);
            MatchSimulatorExtensions.SimulateAndBroadcast(singlesMatch, gameData);
        }
        else
        {
            Debug.LogWarning("[TEST SKIPPED] Not enough wrestlers for a singles match.");
        }
        yield return new WaitForSeconds(1); // Pause for readability

        // --- Test 2: Simulate a Tag Team Match ---
        Debug.Log("--- Running Test 2: Tag Team Match ---");
        var team1 = gameData.teams.FirstOrDefault();
        var team2 = gameData.teams.Skip(1).FirstOrDefault();
        if (team1 != null && team2 != null)
        {
            var tagMatch = new Match(team1, team2, gameData);
            MatchSimulatorExtensions.SimulateAndBroadcast(tagMatch, gameData);
        }
        else
        {
            Debug.LogWarning("[TEST SKIPPED] Not enough teams for a tag match.");
        }
        yield return new WaitForSeconds(1);

        // --- Test 3: Advance Game Week ---
        Debug.Log("--- Running Test 3: Advance Week ---");
        GameManager.Instance.AdvanceWeek();
        yield return new WaitForSeconds(1);

        // --- Test 4: Save and Load Data ---
        Debug.Log("--- Running Test 4: Save/Load Verification ---");
        string testSaveFile = "test_save.json";
        GameManager.Instance.SaveGame(testSaveFile);
        var loadedData = DataLoader.LoadSavedGame(testSaveFile);
        if (loadedData != null && loadedData.wrestlers.Count == gameData.wrestlers.Count)
        {
            Debug.Log("[TEST PASSED] Save/Load successful. Wrestler count matches.");
        }
        else
        {
            Debug.LogError("[TEST FAILED] Wrestler count mismatch after loading.");
        }

        Debug.Log("--- SIMULATION TEST RUN COMPLETED ---");
    }
}

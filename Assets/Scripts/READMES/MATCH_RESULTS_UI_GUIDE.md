# Match Results UI System - Usage Guide

## Overview

The Match Results UI system provides an event-based architecture to pipe simulation results to UI instead of Debug.Log. This allows for dynamic UI updates, progress tracking, and better user experience.

---

## Architecture

```
MatchSimulator.Simulate()
        ↓
MatchSimulatorExtensions.SimulateAndBroadcast()
        ↓
MatchResultsEvent (Static Event Bus)
        ↓
MatchResultsUI (UI Component - Auto-subscribes)
```

---

## Quick Start

### 1. Setup UI Component

1. Create a Canvas in your scene
2. Add the `MatchResultsUI` component to a GameObject
3. Assign UI references in the Inspector:
   - Results Panel (GameObject)
   - Results Text (TextMeshProUGUI)
   - Progress Bar (Slider)
   - Bulk Progress Panel (GameObject)
   - etc.

### 2. Use in Your Code

#### Option A: Simulate Single Match (with UI)
```csharp
using UnityEngine;

public class YourMatchController : MonoBehaviour
{
    public void RunMatch(Match match, GameData data)
    {
        // This will automatically broadcast to UI
        MatchSimulatorExtensions.SimulateAndBroadcast(
            match, 
            data, 
            MatchSimulationMode.Advanced
        );
    }
}
```

#### Option B: Simulate Bulk Matches
```csharp
public void RunTournament(Match[] matches, GameData data)
{
    // This broadcasts progress updates and final summary
    SimulationModeHelper.SimulateBulkMatches(
        matches, 
        data, 
        MatchSimulationMode.Simple
    );
}
```

#### Option C: Run Benchmark
```csharp
public void BenchmarkPerformance(Match testMatch, GameData data)
{
    // This broadcasts benchmark results to UI
    SimulationModeHelper.BenchmarkModes(testMatch, data, iterations: 100);
}
```

---

## Events Available

### 1. OnMatchCompleted
Fired when a single match finishes.

```csharp
void OnEnable()
{
    MatchResultsEvent.OnMatchCompleted += HandleMatchResult;
}

void HandleMatchResult(MatchResult result)
{
    Debug.Log($"Match finished! Winner: {result.winnerName}");
    Debug.Log($"Rating: {result.rating}/100");
    Debug.Log($"Finish: {result.finishType}");
    
    // Check for injuries
    foreach (var evt in result.events)
    {
        if (evt.eventType == "Injury")
        {
            ShowInjuryAlert(evt.wrestlerName, evt.description);
        }
    }
}
```

### 2. OnBulkProgress
Fired during bulk simulation for each completed match.

```csharp
void HandleBulkProgress(BulkSimulationProgress progress)
{
    float percent = progress.percentComplete;
    int completed = progress.completedMatches;
    int total = progress.totalMatches;
    float timeLeft = progress.estimatedTimeRemaining;
    
    UpdateProgressBar(percent);
    UpdateProgressText($"{completed}/{total} - {timeLeft:F0}s remaining");
}
```

### 3. OnBulkCompleted
Fired when bulk simulation finishes.

```csharp
void HandleBulkCompleted(BulkSimulationSummary summary)
{
    string message = $"Simulated {summary.totalMatches} matches in {summary.totalTime:F1}s\n" +
                    $"Average Rating: {summary.averageRating}/100\n" +
                    $"Injuries: {summary.totalInjuries}";
    
    ShowSummaryPopup(message);
}
```

### 4. OnBenchmarkCompleted
Fired when benchmark finishes.

```csharp
void HandleBenchmarkCompleted(BenchmarkResult result)
{
    Debug.Log($"Simple Mode: {result.simpleModeAvgTime:F5}s per match");
    Debug.Log($"Advanced Mode: {result.advancedModeAvgTime:F5}s per match");
    Debug.Log($"Speed Improvement: {result.speedMultiplier:F2}x");
}
```

---

## Custom UI Implementation

You can create your own UI handler instead of using `MatchResultsUI`:

```csharp
using UnityEngine;

public class MyCustomResultsUI : MonoBehaviour
{
    void OnEnable()
    {
        MatchResultsEvent.OnMatchCompleted += OnMatchDone;
    }

    void OnDisable()
    {
        MatchResultsEvent.OnMatchCompleted -= OnMatchDone;
    }

    void OnMatchDone(MatchResult result)
    {
        // Your custom UI logic here
        ShowCustomPopup(result);
        PlayVictorySound();
        UpdateLeaderboard();
    }
}
```

---

## MatchResult Data Structure

```csharp
public class MatchResult
{
    Match match;                    // Full match object
    string winnerName;              // Winner's display name
    int rating;                     // Match rating (0-100)
    string finishType;              // "Pinfall", "Submission", etc.
    MatchSimulationMode mode;       // Simple or Advanced
    float simulationTime;           // How long it took to simulate
    List<string> participantNames;  // All wrestler names
    bool wasTitleMatch;             // Was it a title match?
    string titleName;               // Name of title (if applicable)
    List<MatchEvent> events;        // Injuries, referee bumps, etc.
}
```

---

## Example: Tournament Manager

```csharp
public class TournamentManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject resultsPanel;

    private void OnEnable()
    {
        MatchResultsEvent.OnBulkProgress += UpdateProgress;
        MatchResultsEvent.OnBulkCompleted += ShowResults;
    }

    private void OnDisable()
    {
        MatchResultsEvent.OnBulkProgress -= UpdateProgress;
        MatchResultsEvent.OnBulkCompleted -= ShowResults;
    }

    public void StartTournament(Match[] matches, GameData data)
    {
        statusText.text = "Tournament in progress...";
        resultsPanel.SetActive(false);
        
        // This will trigger progress events
        SimulationModeHelper.SimulateBulkMatches(
            matches, 
            data, 
            MatchSimulationMode.Simple
        );
    }

    private void UpdateProgress(BulkSimulationProgress progress)
    {
        progressBar.value = progress.percentComplete / 100f;
        statusText.text = $"Match {progress.completedMatches}/{progress.totalMatches}";
    }

    private void ShowResults(BulkSimulationSummary summary)
    {
        statusText.text = "Tournament Complete!";
        resultsPanel.SetActive(true);
        
        // Display winner bracket, stats, etc.
        DisplayTournamentWinner(summary.results);
    }
}
```

---

## Example: Real-Time Match Display

```csharp
public class LiveMatchDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchStatusText;
    [SerializeField] private Image winnerPortrait;
    [SerializeField] private GameObject injuryWarning;

    private void OnEnable()
    {
        MatchResultsEvent.OnMatchCompleted += DisplayMatchResult;
    }

    private void OnDisable()
    {
        MatchResultsEvent.OnMatchCompleted -= DisplayMatchResult;
    }

    public void WatchMatch(Match match, GameData data)
    {
        matchStatusText.text = "Match in progress...";
        
        // Use Advanced mode for watched matches
        MatchSimulatorExtensions.SimulateAndBroadcast(
            match, 
            data, 
            MatchSimulationMode.Advanced
        );
    }

    private void DisplayMatchResult(MatchResult result)
    {
        // Animate the result display
        StartCoroutine(AnimateResult(result));
    }

    private IEnumerator AnimateResult(MatchResult result)
    {
        // Show winner
        matchStatusText.text = $"{result.winnerName} WINS!";
        yield return new WaitForSeconds(2f);
        
        // Show rating
        matchStatusText.text = $"Match Rating: {result.rating}/100";
        yield return new WaitForSeconds(1.5f);
        
        // Check for injuries
        foreach (var evt in result.events)
        {
            if (evt.eventType == "Injury")
            {
                injuryWarning.SetActive(true);
                matchStatusText.text = $"{evt.wrestlerName} is injured!";
                yield return new WaitForSeconds(2f);
            }
        }
        
        injuryWarning.SetActive(false);
    }
}
```

---

## Testing

The `MatchResultsUI` component includes test methods (Editor only):

1. Select the GameObject with `MatchResultsUI` in the Hierarchy
2. Right-click on the component
3. Choose:
   - "Test Single Match Event" - Fires a fake match result
   - "Test Bulk Complete Event" - Fires a fake bulk summary

---

## Performance Considerations

### Event vs. Polling
✅ **Events** - Efficient, only fires when needed  
❌ **Polling** - Wasteful, checks every frame

### Bulk Simulations
- Progress events fire **per match** (can be hundreds)
- Consider throttling updates for very large batches:

```csharp
private float lastUpdateTime = 0f;

void HandleBulkProgress(BulkSimulationProgress progress)
{
    // Only update UI every 0.1 seconds
    if (Time.time - lastUpdateTime > 0.1f)
    {
        UpdateUI(progress);
        lastUpdateTime = Time.time;
    }
}
```

---

## Migration from Debug.Log

### Before
```csharp
Match result = MatchSimulator.Simulate(match, data, mode);
Debug.Log($"Winner: {result.winnerId}, Rating: {result.rating}");
```

### After
```csharp
// Option 1: Use extension (auto-broadcasts)
MatchSimulatorExtensions.SimulateAndBroadcast(match, data, mode);

// Option 2: Manual broadcast
Match result = MatchSimulator.Simulate(match, data, mode);
var matchResult = CreateMatchResult(result, data, mode, simTime);
MatchResultsEvent.BroadcastMatchResult(matchResult);
```

---

## Benefits

✅ **Separation of Concerns** - Logic separate from presentation  
✅ **Flexible UI** - Multiple listeners can react to same event  
✅ **Easy Testing** - Mock events without running simulations  
✅ **Progress Tracking** - Real-time updates for bulk operations  
✅ **Extensible** - Add custom event handlers without modifying core code  

---

## Troubleshooting

### UI Not Updating
1. Check that `MatchResultsUI` is active in scene
2. Verify UI references are assigned in Inspector
3. Ensure you're using `SimulateAndBroadcast()` or manually broadcasting events
4. Check that events are being subscribed in `OnEnable()`

### Events Not Firing
```csharp
// Verify subscription
void OnEnable()
{
    MatchResultsEvent.OnMatchCompleted += MyHandler;
    Debug.Log("Subscribed to match events");
}

// Verify unsubscription
void OnDisable()
{
    MatchResultsEvent.OnMatchCompleted -= MyHandler;
    Debug.Log("Unsubscribed from match events");
}
```

### Missing Data in MatchResult
Check that `GameData` contains:
- All wrestlers referenced in match
- Title data (if title match)
- Valid wrestler IDs

---

## Next Steps

1. ✅ Add `MatchResultsUI` component to your scene
2. ✅ Assign UI references
3. ✅ Replace `MatchSimulator.Simulate()` with `MatchSimulatorExtensions.SimulateAndBroadcast()`
4. ⬜ Create custom UI layouts as needed
5. ⬜ Add sound effects for match results
6. ⬜ Implement animation sequences
7. ⬜ Add match replay functionality

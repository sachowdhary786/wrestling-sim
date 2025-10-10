# Simulation Modes Guide

## Overview

The match simulation system supports two modes optimized for different scenarios:

| Mode | Speed | Detail | Logging | Injury Rate | Best For |
|------|-------|--------|---------|-------------|----------|
| **Advanced** | Normal | Full phases | Verbose | 100% | Player-watched matches |
| **Simple** | 2-3x faster | Streamlined | Minimal | 50% | Bulk/background sims |

---

## When to Use Each Mode

### Use **Advanced Mode** when:
- ✅ Player is actively watching the match
- ✅ Important story/title matches
- ✅ You need detailed phase-by-phase breakdown
- ✅ Debugging match simulation
- ✅ Single match focus

### Use **Simple Mode** when:
- ✅ Simulating tournaments (many matches)
- ✅ Season mode (off-screen shows)
- ✅ Background/house show matches
- ✅ Simulating multiple shows at once
- ✅ Performance is critical (mobile, large rosters)

---

## Technical Differences

### Advanced Mode Features
```
1. Opening Phase
   - Detailed performance calculation
   - Chemistry logging
   - Trait bonus application
   
2. Mid Phase
   - 2-5 momentum shifts
   - Near fall chances
   - Dynamic back-and-forth
   
3. Climax Phase
   - Momentum-influenced winner
   - Detailed finish type selection
   - Match-specific finish logic
   
4. Aftermath Phase
   - Full injury system
   - Post-match angle chance
   - Detailed injury logging
```

### Simple Mode Features
```
- Single-pass calculation
- No phase separation
- Reduced injury chance (50%)
- Minimal logging
- Same core algorithms
- Faster execution
```

---

## Performance Comparison

Based on typical match simulation:

```
Advanced Mode: ~0.0015s per match
Simple Mode:   ~0.0005s per match
Speed Gain:    3x faster
```

**Example Scenarios:**

| Scenario | Matches | Advanced Time | Simple Time | Savings |
|----------|---------|---------------|-------------|---------|
| Single Match | 1 | 0.0015s | 0.0005s | 0.001s |
| Show (7 matches) | 7 | 0.0105s | 0.0035s | 0.007s |
| Tournament (32) | 32 | 0.048s | 0.016s | 0.032s |
| Season (500) | 500 | 0.75s | 0.25s | 0.5s |
| Full Year (2000) | 2000 | 3.0s | 1.0s | 2.0s |

*Times are approximate and will vary based on hardware*

---

## Code Examples

### Basic Usage

```csharp
// Default (Advanced mode)
Match result = MatchSimulator.Simulate(booking, gameData);

// Explicit Advanced
Match result = MatchSimulator.Simulate(booking, gameData, MatchSimulationMode.Advanced);

// Simple mode
Match result = MatchSimulator.Simulate(booking, gameData, MatchSimulationMode.Simple);
```

### Show Simulation

```csharp
// Player's show - use Advanced for detail
Show playerShow = ShowSimulator.SimulateShow(show, gameData, MatchSimulationMode.Advanced);

// Rival promotion show - use Simple for speed
Show rivalShow = ShowSimulator.SimulateShow(competitorShow, gameData, MatchSimulationMode.Simple);
```

### Smart Mode Selection

```csharp
public Match SimulateMatch(Match booking, GameData data, bool isPlayerWatching)
{
    var mode = SimulationModeHelper.GetRecommendedMode(
        isPlayerWatching: isPlayerWatching,
        isBulkSimulation: false
    );
    
    return MatchSimulator.Simulate(booking, data, mode);
}
```

### Bulk Tournament Simulation

```csharp
public void SimulateTournament(Match[] tournamentMatches, GameData data)
{
    Debug.Log($"Simulating {tournamentMatches.Length} tournament matches...");
    
    // Use Simple mode for speed
    SimulationModeHelper.SimulateBulkMatches(
        tournamentMatches, 
        data, 
        MatchSimulationMode.Simple
    );
}
```

### Season Mode Example

```csharp
public void AdvanceWeek(GameData data)
{
    // Player's show - Advanced mode
    var playerShow = GetPlayerShow();
    ShowSimulator.SimulateShow(playerShow, data, MatchSimulationMode.Advanced);
    
    // All other promotions - Simple mode for speed
    var otherShows = GetOtherPromotionShows();
    foreach (var show in otherShows)
    {
        ShowSimulator.SimulateShow(show, data, MatchSimulationMode.Simple);
    }
}
```

---

## Injury Rate Differences

### Advanced Mode
- Base injury chance: 1%
- Match type multipliers: Full (Hardcore 3x, Ladder 2.5x, etc.)
- Severity: Minor (1-3 weeks), Moderate (3-6 weeks), Major (6-12 weeks)
- Detailed injury types per severity

### Simple Mode
- Base injury chance: 0.5% (50% reduction)
- Match type multipliers: Reduced (Hardcore 7%, Ladder 10%, etc.)
- Severity: Always Minor (1-3 weeks)
- Generic "minor injury" type
- No stat penalties beyond stamina

**Rationale**: Off-screen matches shouldn't decimate your roster with injuries.

---

## Logging Differences

### Advanced Mode Logs
```
[OPENING] Singles match begins!
  John Cena opens with performance score: 78.3
  Randy Orton opens with performance score: 75.1
  Chemistry: John Cena & Randy Orton (negative)
[MID PHASE] Match intensifies with momentum shifts!
  Momentum shift #1: John Cena gains control (+18.2)
  Momentum shift #2: Randy Orton gains control (+21.5)
  NEAR FALL! Randy Orton almost had it!
[CLIMAX] Heading into the finish sequence!
  Building to the finish...
  FINISH: John Cena wins via Pinfall!
  Final Match Rating: 82/100
[AFTERMATH] Match concluded. Winner: John Cena
```

### Simple Mode Logs
```
(No logs by default - silent execution)
```

---

## Migration Guide

If you have existing code calling the simulator:

### Before
```csharp
Match result = MatchSimulator.Simulate(booking, gameData);
```

### After (No changes needed!)
```csharp
// Still works - defaults to Advanced mode
Match result = MatchSimulator.Simulate(booking, gameData);

// Or explicitly choose mode
Match result = MatchSimulator.Simulate(booking, gameData, MatchSimulationMode.Simple);
```

**Backward Compatible**: All existing code continues to work without changes.

---

## Benchmarking

Use the built-in benchmark tool to test performance on your hardware:

```csharp
// Create a test match
Match testMatch = new Match
{
    participants = new List<string> { "wrestler1", "wrestler2" },
    matchType = "Singles",
    location = "Arena"
};

// Run benchmark (100 iterations of each mode)
SimulationModeHelper.BenchmarkModes(testMatch, gameData, iterations: 100);
```

**Output:**
```
=== SIMULATION MODE BENCHMARK (100 iterations) ===
Simple Mode:   0.052s total, 0.00052s per match
Advanced Mode: 0.148s total, 0.00148s per match
Speed Improvement: 2.85x faster with Simple mode
===========================================
```

---

## Best Practices

1. **Default to Advanced** for quality unless performance is an issue
2. **Use Simple for bulk** operations (tournaments, season sim)
3. **Profile your game** to find performance bottlenecks
4. **Consider player experience** - they should see Advanced mode matches
5. **Use the helper** - `SimulationModeHelper.GetRecommendedMode()` makes smart choices

---

## Future Considerations

Potential additions:
- **Hybrid Mode**: Simple for most of match, Advanced for finish
- **Configurable Simple Mode**: Adjust injury rates, logging level
- **Replay System**: Store Advanced mode data for later review
- **Async Simulation**: Run Simple mode matches in background threads

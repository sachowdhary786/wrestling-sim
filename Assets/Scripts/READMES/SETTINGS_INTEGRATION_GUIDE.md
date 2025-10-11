# GameSettings Integration Guide

## Setup Steps

### 1. Create the GameSettings Asset
1. In Unity Editor, navigate to `Assets/Resources` folder (create if doesn't exist)
2. Right-click → `Create` → `Wrestling Sim` → `Game Settings`
3. Name it `GameSettings` (must be exact name for auto-loading)
4. Configure all values in the Inspector

### 2. Access Settings in Code

#### Method 1: Direct Access (Recommended)
```csharp
using UnityEngine;

public class YourClass
{
    public void YourMethod()
    {
        // Access via static property
        GameSettings settings = GameSettingsManager.Settings;
        
        // Use settings
        float bonus = settings.hometownBonus;
        bool injuriesOn = settings.injuriesEnabled;
    }
}
```

#### Method 2: Quick Access
```csharp
// One-liner access
float randomness = GameSettingsManager.Get().performanceRandomness;
```

---

## Example Integration: MatchInjurySystem

### BEFORE (Hardcoded Values)
```csharp
private static float CalculateInjuryChance(Wrestler wrestler, string matchType, float fatigue)
{
    float chance = 1f; // base 1%
    
    chance += matchType switch
    {
        "Hardcore" => 15f,  // HARDCODED
        "Ladder" => 20f,    // HARDCODED
        "Cage" => 10f,      // HARDCODED
        "TLC" => 25f,       // HARDCODED
        _ => 2f             // HARDCODED
    };
    
    return chance;
}
```

### AFTER (Using GameSettings)
```csharp
private static float CalculateInjuryChance(Wrestler wrestler, string matchType, float fatigue)
{
    var settings = GameSettingsManager.Settings;
    
    // Check if injuries are disabled globally
    if (!settings.injuriesEnabled)
        return 0f;
    
    float chance = settings.baseInjuryChance;
    
    // Use configured risk values
    chance += settings.GetMatchTypeRisk(matchType);
    
    return chance;
}

private static void ApplyInjury(Wrestler wrestler, float injuryChance)
{
    var settings = GameSettingsManager.Settings;
    
    int severity = settings.DetermineInjurySeverity(injuryChance);
    
    wrestler.injured = true;
    wrestler.injurySeverity = severity;
    wrestler.injuryType = GetRandomInjuryType(severity);
    
    // Use configured recovery ranges
    var recoveryRange = settings.GetRecoveryRange(severity);
    wrestler.recoveryWeeksRemaining = UnityEngine.Random.Range(
        recoveryRange.x, 
        recoveryRange.y
    );
    
    if (settings.logInjuries)
    {
        Debug.Log($"⚠️ {wrestler.name} suffers a {wrestler.injuryType}!");
    }
    
    // Use configured stat penalties
    ApplyInjuryStatPenalty(wrestler, settings.GetInjuryStatPenalty(severity));
}
```

---

## Example Integration: MatchPerformanceCalculator

### BEFORE
```csharp
public static float CalculateBasePerformance(...)
{
    float hometownBonus = match.location.Contains(wrestler.hometown)
        ? 1.05f  // HARDCODED
        : 1.0f;
    
    float formFactor = 1.0f + Random.Range(-0.05f, 0.05f); // HARDCODED
    
    // ... calculation
}
```

### AFTER
```csharp
public static float CalculateBasePerformance(...)
{
    var settings = GameSettingsManager.Settings;
    
    float hometownBonus = match.location.Contains(wrestler.hometown)
        ? settings.hometownBonus
        : 1.0f;
    
    float formFactor = 1.0f + Random.Range(-settings.formVariance, settings.formVariance);
    
    // ... rest of calculation
}
```

---

## Example Integration: SimpleMatchSimulator

### BEFORE
```csharp
private static void ApplyChemistryModifiers(Dictionary<Wrestler, float> scores, List<Wrestler> wrestlers)
{
    // ...
    if (a.friends.Contains(b.id.ToString()))
        chemistry += 3; // HARDCODED
    if (a.rivals.Contains(b.id.ToString()))
        chemistry -= 3; // HARDCODED
    // ...
}
```

### AFTER
```csharp
private static void ApplyChemistryModifiers(Dictionary<Wrestler, float> scores, List<Wrestler> wrestlers)
{
    var settings = GameSettingsManager.Settings;
    
    // ...
    if (a.friends.Contains(b.id.ToString()))
        chemistry += settings.simpleChemistryBonus;
    if (a.rivals.Contains(b.id.ToString()))
        chemistry -= settings.simpleChemistryBonus;
    // ...
}
```

---

## Benefits

### 1. **Easy Balancing**
- Tweak all game values in one place
- No recompilation needed
- Test different configurations quickly

### 2. **Designer-Friendly**
- Non-programmers can adjust gameplay
- Clear tooltips explain each setting
- Visual sliders for ranges

### 3. **Multiple Configurations**
- Create different settings for Easy/Normal/Hard modes
- Tournament settings vs. Season settings
- Testing configurations

### 4. **Runtime Flexibility**
```csharp
// Disable injuries for casual mode
GameSettingsManager.Settings.injuriesEnabled = false;

// Increase action for special events
GameSettingsManager.Settings.performanceRandomness = 20f;
```

---

## Creating Multiple Settings Profiles

You can create different settings for different scenarios:

1. Create `Resources/GameSettings.asset` (default)
2. Create `Resources/TournamentSettings.asset`
3. Create `Resources/CasualModeSettings.asset`

Load specific settings:
```csharp
GameSettings tournament = Resources.Load<GameSettings>("TournamentSettings");
// Use tournament-specific configuration
```

---

## Debugging Tips

### Inspector Shortcuts
- Select the GameSettings asset
- Adjust values while game is running
- See changes in real-time

### Editor Menu Commands
- Right-click GameSettingsManager in Hierarchy
- "Reload Settings" - refresh from disk
- "Print Current Settings" - debug current values

### Logging
```csharp
if (GameSettingsManager.Settings.logPerformance)
{
    Debug.Log($"Performance calculated: {performance}");
}
```

---

## Next Steps

1. ✅ Create GameSettings asset in Resources folder
2. ✅ Configure initial values
3. ⬜ Refactor MatchInjurySystem to use settings
4. ⬜ Refactor MatchPerformanceCalculator to use settings
5. ⬜ Refactor SimpleMatchSimulator to use settings
6. ⬜ Refactor MatchPhaseSimulator to use settings
7. ⬜ Test and balance values
8. ⬜ Create alternative settings profiles (optional)

---

## Common Patterns

### Safe Access with Fallback
```csharp
float bonus = GameSettingsManager.Settings?.hometownBonus ?? 1.05f;
```

### Conditional Features
```csharp
if (GameSettingsManager.Settings.refereeEventsEnabled)
{
    // Process referee events
}
```

### Caching for Performance
```csharp
private GameSettings _cachedSettings;

void Start()
{
    _cachedSettings = GameSettingsManager.Settings;
}

void Update()
{
    // Use _cachedSettings instead of accessing manager every frame
}
```

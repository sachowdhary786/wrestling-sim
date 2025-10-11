## Booking System - AI-Driven Match Card Creation

## Overview

The Booking Manager is an AI-powered system that automatically creates balanced, story-driven match cards for wrestling shows. It considers active feuds, wrestler states (fatigue, morale, popularity), and card structure to create compelling shows.

---

## Architecture

```
BookingManager (AI Logic)
    ↓
Analyzes:
- Active Feuds (heat levels)
- Wrestler States (fatigue, morale, momentum)
- Available Talent (injuries, availability)
- Card Balance (opener → main event)
    ↓
Creates Matches:
1. Main Event (feuds/titles/top talent)
2. Semi-Main (mid-tier feuds/titles)
3. Midcard (feuds/alignment matches)
4. Opener (high-energy talent)
    ↓
WrestlerStateManager
    ↓
Updates After Matches:
- Fatigue (increases)
- Morale (win/loss)
- Momentum (booking strength)
- Popularity (match quality)
```

---

## Quick Start

### Basic Usage

```csharp
using UnityEngine;

public class ShowController : MonoBehaviour
{
    public void RunWeeklyShow()
    {
        // Load your game data
        GameData gameData = LoadGameData();
        
        // Create a show
        Show show = new Show
        {
            id = "show_001",
            showName = "Monday Night Raw",
            date = System.DateTime.Now.ToString(),
            companyId = "wwe"
        };
        
        // Book the show with AI
        show = BookingManager.BookShow(
            show, 
            gameData, 
            targetMatchCount: 6,
            priority: BookingPriority.Balanced
        );
        
        // Show is now booked with 6 matches!
        Debug.Log($"Show booked with {show.matches.Count} matches");
        
        // Simulate the matches
        foreach (var match in show.matches)
        {
            SimulateAndUpdateStates(match, gameData);
        }
        
        // Weekly reset
        WrestlerStateManager.WeeklyReset(gameData);
    }
    
    private void SimulateAndUpdateStates(Match match, GameData gameData)
    {
        // Simulate match
        MatchSimulator.Simulate(match, gameData, MatchSimulationMode.Advanced);
        
        // Update wrestler states
        foreach (var participantId in match.participants)
        {
            var wrestler = gameData.wrestlers.Find(w => w.id.ToString() == participantId);
            if (wrestler == null) continue;
            
            bool won = participantId == match.winnerId;
            WrestlerStateManager.UpdateWrestlerAfterMatch(wrestler, match, won, gameData);
        }
    }
}
```

---

## Booking Priorities

The system supports different booking philosophies:

### **Balanced** (Default)
- Mix of feuds, titles, and star power
- Best for weekly TV shows

```csharp
BookingManager.BookShow(show, data, 6, BookingPriority.Balanced);
```

### **Storyline**
- Prioritizes active feuds and ongoing angles
- Best for PPV events

```csharp
BookingManager.BookShow(show, data, 6, BookingPriority.Storyline);
```

### **StarPower**
- Features top popular wrestlers
- Best for special events

```csharp
BookingManager.BookShow(show, data, 6, BookingPriority.StarPower);
```

### **WorkRate**
- Features best in-ring performers
- Best for "wrestling-focused" shows

```csharp
BookingManager.BookShow(show, data, 6, BookingPriority.WorkRate);
```

### **Development**
- Gives opportunities to newer talent
- Best for developmental shows

```csharp
BookingManager.BookShow(show, data, 6, BookingPriority.Development);
```

---

## Card Structure

The AI automatically creates a balanced card:

### **1. Main Event**
Priority order:
1. **High-heat feuds** (70+ heat)
2. **Title matches** with top talent
3. **Top two popular wrestlers**

Characteristics:
- Most important match
- Often includes title
- Highest star power

### **2. Semi-Main**
Priority order:
1. **Mid-tier feuds** (50+ heat)
2. **Secondary title matches**

Characteristics:
- Strong but not quite main event
- Can include midcard titles
- Builds to main event

### **3. Midcard** (Multiple Matches)
Priority order:
1. **Any active feuds** (30+ heat)
2. **Alignment-based matches** (Face vs Heel)
3. **Random pairings**

Characteristics:
- Story continuation
- Character development
- Filler content

### **4. Opener**
Priority order:
1. **High-energy wrestlers** (aerial > 70 or charisma > 75)
2. **Low-fatigue talent**

Characteristics:
- Gets crowd excited
- Fast-paced action
- Sets tone for show

---

## Wrestler State Management

### **Fatigue (0-100)**
Increases with matches, especially gimmick matches.

**Effects:**
- High fatigue (>80) = Won't be booked
- Medium fatigue (50-80) = Reduced performance
- Low fatigue (<50) = Optimal performance

**Recovery:**
- 10 points per day base
- +5 per day with high stamina
- Halved if injured

**Example:**
```csharp
// After match
WrestlerStateManager.ApplyMatchFatigue(wrestler, match, data);
// wrestler.fatigue increases by 15-30 depending on match type

// Daily recovery
WrestlerStateManager.RecoverFatigue(wrestler, days: 1);
// wrestler.fatigue decreases by 10-15
```

### **Morale (0-100)**
Based on booking decisions and match results.

**Increases from:**
- Winning matches (+5 regular, +10 title)
- High-rated matches (+3)

**Decreases from:**
- Losing matches (-2 regular, -5 title)
- Poor matches (-3)
- High fatigue (-2)

**Example:**
```csharp
bool won = match.winnerId == wrestler.id.ToString();
WrestlerStateManager.UpdateMoraleAfterMatch(wrestler, match, won, data);
```

### **Momentum (-100 to +100)**
Reflects recent booking strength.

**Positive Momentum:**
- +20 for title wins
- +15 for great matches (80+ rating)
- +10 for good matches
- +5 for regular wins

**Negative Momentum:**
- -10 for title losses
- -5 for regular losses

**Decay:**
- 10% per week if not used

**Example:**
```csharp
WrestlerStateManager.UpdateMomentum(wrestler, match, won, data);
// wrestler.momentum = 45 (building momentum)
```

### **Popularity (0-100)**
Overall fan support level.

**Increases from:**
- High-rated matches (+1 to +3)
- Title match exposure (+1)
- Charisma multiplier (x1.5 if charisma > 80)

**Example:**
```csharp
WrestlerStateManager.UpdatePopularity(wrestler, match, data);
```

---

## Booking Logic Deep Dive

### Main Event Selection

```csharp
// 1. Check for high-heat feuds
var feudMatch = BookFeudMatch(data, available, used, show, minHeat: 70);

// 2. If no feud, book title match
var titleMatch = BookTitleMatch(data, available, used, show, isMainEvent: true);

// 3. If no title available, use top talent
var topMatch = BookTopTalentMatch(data, available, used, show);
```

### Availability Checks

Wrestlers are **unavailable** if:
- `injured == true`
- `active == false`
- `fatigue >= 90`
- `morale <= 20`

```csharp
private static bool IsAvailable(Wrestler w)
{
    return w.active 
        && !w.injured 
        && w.fatigue < 90
        && w.morale > 20;
}
```

### Feud Integration

```csharp
// Active feuds automatically prioritized
var activeFeud = data.feuds
    .Where(f => f.active && f.heat >= minHeat)
    .OrderByDescending(f => f.heat)
    .FirstOrDefault();

// Creates match with feud participants
var feudWrestlers = GetFeudWrestlers(feud, data, available, used);
var match = CreateMatch(feudWrestlers.Take(2).ToList(), data, show);

// High heat = gimmick match chance
if (feud.heat > 80)
{
    // 30% chance for Hardcore, Cage, etc.
    match.matchType = "Hardcore";
}
```

---

## Complete Workflow Example

```csharp
public class SeasonManager : MonoBehaviour
{
    private GameData gameData;
    private int currentWeek = 1;

    void Start()
    {
        gameData = LoadGameData();
        InitializeWrestlerStates();
    }

    public void RunWeek()
    {
        Debug.Log($"=== WEEK {currentWeek} ===");
        
        // 1. Book Monday show
        Show mondayShow = new Show
        {
            showName = "Monday Night Raw",
            companyId = "wwe"
        };
        mondayShow = BookingManager.BookShow(mondayShow, gameData, 6);
        
        // 2. Simulate Monday show
        SimulateShow(mondayShow);
        
        // 3. Book Friday show
        Show fridayShow = new Show
        {
            showName = "Friday Night SmackDown",
            companyId = "wwe"
        };
        fridayShow = BookingManager.BookShow(fridayShow, gameData, 5);
        
        // 4. Simulate Friday show
        SimulateShow(fridayShow);
        
        // 5. Weekly reset
        WrestlerStateManager.WeeklyReset(gameData);
        
        // 6. Print wrestler states
        PrintTopWrestlers();
        
        currentWeek++;
    }

    private void SimulateShow(Show show)
    {
        foreach (var match in show.matches)
        {
            // Simulate
            MatchSimulator.Simulate(match, gameData, MatchSimulationMode.Advanced);
            
            // Update states
            foreach (var participantId in match.participants)
            {
                var wrestler = gameData.wrestlers.Find(w => w.id.ToString() == participantId);
                if (wrestler == null) continue;
                
                bool won = participantId == match.winnerId;
                WrestlerStateManager.UpdateWrestlerAfterMatch(wrestler, match, won, gameData);
            }
        }
    }

    private void PrintTopWrestlers()
    {
        var topWrestlers = gameData.wrestlers
            .OrderByDescending(w => w.popularity + w.momentum / 2)
            .Take(5);
        
        Debug.Log("=== TOP 5 WRESTLERS ===");
        foreach (var wrestler in topWrestlers)
        {
            Debug.Log(WrestlerStateManager.GetStateReport(wrestler));
        }
    }

    private void InitializeWrestlerStates()
    {
        foreach (var wrestler in gameData.wrestlers)
        {
            if (wrestler.morale == 0) wrestler.morale = 70;
            if (wrestler.popularity == 0) wrestler.popularity = 50;
        }
    }
}
```

---

## Utility Methods

### Check Wrestler Condition

```csharp
int condition = WrestlerStateManager.GetConditionRating(wrestler);
// 80+ = Great condition
// 60-80 = Good condition
// 40-60 = Fair condition
// <40 = Poor condition
```

### Check if Should Rest

```csharp
if (WrestlerStateManager.ShouldRest(wrestler))
{
    Debug.Log($"{wrestler.name} needs rest!");
    // Don't book them this week
}
```

### Get Push Level

```csharp
string pushLevel = WrestlerStateManager.GetPushLevel(wrestler);
// Returns: "Mega Push", "Strong Push", "Mid Push", "Neutral", "Cooling Off", "Buried"
```

### Get State Report

```csharp
string report = WrestlerStateManager.GetStateReport(wrestler);
// "John Cena: Fatigue 30/100, Morale 85/100, Popularity 95/100, Momentum +45, Matches: 2/week, Rest: 3 days, Push: Strong Push"
```

---

## Advanced: Custom Booking Logic

You can extend the system with custom booking rules:

```csharp
public static class CustomBookingRules
{
    public static Match BookRivalryMatch(GameData data, Show show)
    {
        // Find wrestlers with highest rivalry heat
        var rivalPairs = new List<(Wrestler, Wrestler, int)>();
        
        foreach (var wrestler in data.wrestlers)
        {
            foreach (var rivalId in wrestler.rivals)
            {
                var rival = data.wrestlers.Find(w => w.id.ToString() == rivalId);
                if (rival != null)
                {
                    int heat = CalculateRivalryHeat(wrestler, rival);
                    rivalPairs.Add((wrestler, rival, heat));
                }
            }
        }
        
        var bestRivalry = rivalPairs.OrderByDescending(r => r.Item3).FirstOrDefault();
        
        if (bestRivalry.Item1 != null)
        {
            return CreateCustomMatch(bestRivalry.Item1, bestRivalry.Item2, data, show);
        }
        
        return null;
    }
}
```

---

## Integration with Match Results UI

```csharp
public class ShowRunner : MonoBehaviour
{
    void OnEnable()
    {
        MatchResultsEvent.OnMatchCompleted += OnMatchComplete;
    }

    void OnMatchComplete(MatchResult result)
    {
        // Update wrestler states automatically
        var wrestler = FindWrestler(result.winnerName);
        if (wrestler != null)
        {
            bool won = true;
            WrestlerStateManager.UpdateWrestlerAfterMatch(
                wrestler, 
                result.match, 
                won, 
                gameData
            );
        }
    }
}
```

---

## Best Practices

1. **Always check availability** before manual booking
2. **Run WeeklyReset()** after each week
3. **Monitor fatigue** to avoid overusing talent
4. **Balance morale** by rotating winners
5. **Build momentum** gradually over multiple wins
6. **Use feuds** to create compelling stories
7. **Rest injured wrestlers** until recovered

---

## Troubleshooting

### "Not enough available wrestlers"
- Check `wrestler.active`, `wrestler.injured`, `wrestler.fatigue`
- Ensure roster has at least 10-15 active wrestlers
- Run `WrestlerStateManager.WeeklyReset()` to recover fatigue

### "No feuds being booked"
- Ensure `feud.active == true`
- Check `feud.heat >= 30` (minimum for midcard)
- Verify feud participants are available

### "Same wrestlers booked repeatedly"
- Increase roster size
- Lower `targetMatchCount`
- Check fatigue thresholds

### "Low morale wrestlers"
- Book them in winning matches
- Give them rest weeks
- Check for injury/high fatigue

---

## Future Enhancements

Potential additions:

1. **Tag Team Booking** - Automatic tag team match creation
2. **Tournament Booking** - Bracket-style tournaments
3. **Angle System** - Non-match segments (promos, attacks)
4. **Contract System** - Wrestler happiness and contract status
5. **Crowd Reactions** - Dynamic crowd heat affecting bookings
6. **Authority Figure** - AI GM making decisions
7. **Booking Restrictions** - Company-specific rules (no blood, etc.)

---

## Summary

✅ **AI-driven booking** - Automatically creates balanced cards  
✅ **Feud integration** - Prioritizes active storylines  
✅ **State management** - Tracks fatigue, morale, momentum, popularity  
✅ **Card balance** - Opener → Midcard → Main Event structure  
✅ **Availability checks** - Prevents overuse and injury bookings  
✅ **Extensible** - Easy to add custom booking logic  

The system provides a solid foundation for realistic wrestling promotion simulation with minimal manual intervention.

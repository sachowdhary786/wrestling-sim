# Complete Referee System Guide

## Overview

The referee system is a comprehensive career management and match officiating system with:
- ✅ Career progression and development
- ✅ Statistics tracking
- ✅ Fatigue and injury management
- ✅ Special events and incidents
- ✅ Scheduling and workload balancing
- ✅ Reputation and achievements
- ✅ Training and promotions

---

## System Components

### 1. **Referee Model** (`Referee.cs`)
Core referee data with stats, status, and career info.

**Key Properties:**
- Stats: `experience`, `consistency`, `strictness`, `corruption`
- Status: `isActive`, `isInjured`, `fatigue`
- Career: `age`, `yearsExperience`, `stats`
- Specializations: `isMainEventRef`, `isHardcoreSpecialist`

### 2. **RefereeStats** (`RefereeStats.cs`)
Detailed career statistics tracking.

**Tracks:**
- Match counts (total, title, hardcore, main event)
- Finish types (pinfalls, submissions, DQs, etc.)
- Quality metrics (average rating, perfect matches)
- Incidents (knockouts, bumps, controversies)
- Reputation and achievements

### 3. **RefereeManager** (`RefereeManager.cs`)
Core match officiating logic.

**Features:**
- Smart referee assignment
- Rating influence calculation
- Finish type influence
- Phase-specific effects
- Default referee pool creation

### 4. **RefereeCareerManager** (`RefereeCareerManager.cs`)
Career progression and development.

**Features:**
- Week advancement
- Match recording
- Fatigue management
- Injury system
- Training system
- Promotions
- Suspensions
- Retirement

### 5. **RefereeEventSystem** (`RefereeEventSystem.cs`)
Special incidents during matches.

**Events:**
- Referee bumps
- Referee knockouts
- Fast/slow counts
- Missed calls
- Wrong calls
- Arguments with wrestlers
- Ejections

### 6. **RefereeScheduler** (`RefereeScheduler.cs`)
Workload management and scheduling.

**Features:**
- Show-wide referee assignment
- Workload balancing
- Replacement referee finding
- Schedule reports
- Season planning

### 7. **RefereeUtilities** (`RefereeUtilities.cs`)
Helper functions and reports.

**Features:**
- Comprehensive referee reports
- Top referee lists
- Power rankings
- League statistics
- Training recommendations
- Promotion candidates

---

## Quick Start Guide

### Initialize Referee System

```csharp
// Create default referee pool
gameData.referees = RefereeManager.CreateDefaultReferees();

// Or create custom referees
Referee customRef = new Referee("John Smith", 
    strictness: 60, 
    corruption: 20, 
    experience: 50)
{
    id = "ref_john",
    age = 35,
    yearsExperience = 10,
    isActive = true
};
gameData.referees.Add(customRef);
```

### Simulate Matches with Referees

```csharp
// Single match - auto-assigns referee
Match match = new Match { /* ... */ };
Match result = MatchSimulator.Simulate(match, gameData);

// Assign specific referee
match.referee = gameData.referees.Find(r => r.name == "Earl Hebner");
Match result = MatchSimulator.Simulate(match, gameData);

// Assign referees to entire show
RefereeScheduler.AssignRefereesToShow(show, gameData);
ShowSimulator.SimulateShow(show, gameData);
```

### Advance Time

```csharp
// Advance all referees by one week
RefereeUtilities.AdvanceAllRefereesWeek(gameData);

// Or individually
foreach (var referee in gameData.referees)
{
    RefereeCareerManager.AdvanceWeek(referee);
}
```

---

## Career Management

### Training Referees

```csharp
// Train specific skill
RefereeCareerManager.TrainReferee(referee, "consistency", weeks: 4);
RefereeCareerManager.TrainReferee(referee, "experience", weeks: 2);
RefereeCareerManager.TrainReferee(referee, "corruption_reduction", weeks: 3);

// Available focus areas:
// - "strictness"
// - "consistency"
// - "experience"
// - "corruption_reduction"
```

### Promotions

```csharp
// Promote to main event status
bool success = RefereeCareerManager.PromoteToMainEvent(referee);
// Requires: 70+ experience, 50+ matches

// Assign hardcore specialist
bool success = RefereeCareerManager.AssignHardcoreSpecialist(referee);
// Requires: 20+ hardcore matches

// Find refs ready for promotion
var candidates = RefereeUtilities.GetRefereesReadyForPromotion(gameData);
```

### Suspensions & Retirement

```csharp
// Suspend referee
RefereeCareerManager.SuspendReferee(referee, weeks: 4, reason: "Controversial calls");

// Retire referee
RefereeCareerManager.RetireReferee(referee, reason: "Age");
```

---

## Scheduling & Workload

### Assign Referees to Shows

```csharp
// Assign to single show
RefereeScheduler.AssignRefereesToShow(show, gameData);

// Balance workload across multiple shows
List<Show> upcomingShows = GetNextMonthShows();
RefereeScheduler.BalanceWorkload(upcomingShows, gameData);

// Generate schedule report
string report = RefereeScheduler.GenerateScheduleReport(gameData);
Debug.Log(report);
```

### Check Availability

```csharp
// Check if referee can work
bool canWork = RefereeCareerManager.CanWorkMatch(referee);

// Get effectiveness (reduced by fatigue)
float effectiveness = RefereeCareerManager.GetEffectiveness(referee);
// Returns 0.7 to 1.0 (70-100%)

// Get suitable referees for match
var suitable = RefereeUtilities.GetSuitableReferees(
    gameData, 
    matchType: "Hardcore", 
    titleMatch: false
);
```

---

## Reports & Analytics

### Individual Referee Report

```csharp
string report = RefereeUtilities.GetRefereeReport(referee);
Debug.Log(report);

// Output includes:
// - Basic info (age, experience)
// - Attributes (experience, consistency, etc.)
// - Career stats (matches, ratings)
// - Finish type breakdown
// - Incidents
// - Current status (fatigue, availability)
// - Achievements
```

### League Statistics

```csharp
// Get league-wide stats
string stats = RefereeUtilities.GetLeagueStatistics(gameData);

// Power rankings
string rankings = RefereeUtilities.GeneratePowerRankings(gameData);

// Top referees by criteria
var topByQuality = RefereeUtilities.GetTopReferees(gameData, "quality", 10);
var topByMatches = RefereeUtilities.GetTopReferees(gameData, "matches", 10);
var topByRating = RefereeUtilities.GetTopReferees(gameData, "rating", 10);

// Compare two referees
string comparison = RefereeUtilities.CompareReferees(ref1, ref2);
```

### Training & Development Reports

```csharp
// Find refs needing training
var needTraining = RefereeUtilities.GetRefereesNeedingTraining(gameData);

// Find promotion candidates
var readyForPromotion = RefereeUtilities.GetRefereesReadyForPromotion(gameData);
```

---

## Special Events

### Referee Events During Matches

Events automatically trigger during Advanced mode matches:

```csharp
// Events are checked in Mid Phase and Climax
// Examples:
// - Referee Bump (-2 rating)
// - Referee Knockout (-5 rating, needs replacement)
// - Fast Count (-3 rating, controversial)
// - Missed Call (-2 rating)
// - Wrong Call (-3 rating)
// - Argument with Wrestler (0 rating, adds drama)
```

### Manual Event Triggering

```csharp
// Check for event
var refEvent = RefereeEventSystem.CheckForEvent(
    referee, 
    match, 
    phase: 3, // Climax
    state
);

if (refEvent != null)
{
    RefereeEventSystem.ApplyEvent(refEvent, referee, match, state);
}

// Get all possible events for a referee
var possibleEvents = RefereeEventSystem.GetPossibleEvents(referee, match);
```

---

## Integration with Match Simulation

### Automatic Integration

The referee system is fully integrated with both simulation modes:

**Advanced Mode:**
- Referee logged at match start
- Phase-specific influences
- Events can occur
- Stats recorded after match

**Simple Mode:**
- Referee affects rating
- Referee affects finish type
- Stats recorded after match
- No events (for performance)

### Recording Match Results

```csharp
// Automatically done in MatchPhaseSimulator.SimulateAftermath()
RefereeCareerManager.RecordMatch(referee, match, wasKnockedOut, wasBumped);

// This updates:
// - Total matches
// - Finish type stats
// - Average rating
// - Reputation
// - Fatigue
// - Achievements
```

---

## Achievements System

Achievements are automatically awarded:

### Milestone Achievements
- **100 Matches** - 100 career matches
- **500 Matches** - 500 career matches
- **1000 Matches - Legend** - 1000 career matches

### Quality Achievements
- **50 Perfect Matches** - 50 matches with 85+ rating, no incidents
- **Elite Referee** - 80+ average rating over 50+ matches

### Specialty Achievements
- **Title Match Specialist** - 100+ title matches
- **Promoted to Main Event Referee** - Promoted to main event status
- **Became Hardcore Specialist** - Assigned hardcore specialist

### Controversial Achievements
- **Controversial Figure** - 25+ controversial finishes

---

## Best Practices

### 1. Initialize Properly
```csharp
// Always create referee pool at game start
gameData.referees = RefereeManager.CreateDefaultReferees();
```

### 2. Advance Time Regularly
```csharp
// Call once per week
RefereeUtilities.AdvanceAllRefereesWeek(gameData);
```

### 3. Balance Workload
```csharp
// Don't overwork referees
if (referee.matchesThisWeek >= 5)
{
    // Find different referee
}
```

### 4. Monitor Fatigue
```csharp
// High fatigue reduces effectiveness
if (referee.fatigue > 80)
{
    // Give referee time off
}
```

### 5. Train Young Refs
```csharp
// Develop rookies
if (referee.experience < 60)
{
    RefereeCareerManager.TrainReferee(referee, "experience", 2);
}
```

### 6. Use Reports
```csharp
// Regular monitoring
string report = RefereeScheduler.GenerateScheduleReport(gameData);
// Check for issues
```

---

## Example Workflows

### Weekly Show Workflow

```csharp
// 1. Create show
Show weeklyShow = new Show { /* ... */ };

// 2. Assign referees
RefereeScheduler.AssignRefereesToShow(weeklyShow, gameData);

// 3. Simulate show
ShowSimulator.SimulateShow(weeklyShow, gameData, MatchSimulationMode.Advanced);

// 4. Advance week
RefereeUtilities.AdvanceAllRefereesWeek(gameData);

// 5. Check for issues
var needTraining = RefereeUtilities.GetRefereesNeedingTraining(gameData);
var injured = gameData.referees.Where(r => r.isInjured).ToList();
```

### Season Planning Workflow

```csharp
// 1. Get upcoming shows
List<Show> season = GetNextSeasonShows(); // e.g., 52 shows

// 2. Balance workload
RefereeScheduler.BalanceWorkload(season, gameData);

// 3. Simulate season
foreach (var show in season)
{
    ShowSimulator.SimulateShow(show, gameData, MatchSimulationMode.Simple);
    RefereeUtilities.AdvanceAllRefereesWeek(gameData);
}

// 4. Review season
string stats = RefereeUtilities.GetLeagueStatistics(gameData);
string rankings = RefereeUtilities.GeneratePowerRankings(gameData);
```

### Referee Development Workflow

```csharp
// 1. Identify development needs
var needTraining = RefereeUtilities.GetRefereesNeedingTraining(gameData);

// 2. Train referees
foreach (var ref in needTraining)
{
    if (ref.experience < 60)
        RefereeCareerManager.TrainReferee(ref, "experience", 2);
    if (ref.consistency < 60)
        RefereeCareerManager.TrainReferee(ref, "consistency", 2);
    if (ref.corruption > 40)
        RefereeCareerManager.TrainReferee(ref, "corruption_reduction", 2);
}

// 3. Check for promotions
var readyForPromotion = RefereeUtilities.GetRefereesReadyForPromotion(gameData);
foreach (var ref in readyForPromotion)
{
    RefereeCareerManager.PromoteToMainEvent(ref);
}
```

---

## Performance Considerations

- **Fatigue System**: Prevents referee overuse
- **Injury System**: Adds realism, forces roster depth
- **Event System**: Adds variety without performance cost
- **Statistics**: Minimal overhead, tracked incrementally
- **Scheduling**: O(n log n) complexity, efficient for large rosters

---

## Future Expansion Ideas

- Referee personalities/traits
- Referee-wrestler relationships
- Referee factions/alliances
- International referee exchanges
- Referee tournaments/competitions
- Referee mentorship system
- Custom referee creation UI
- Referee contract system
- Referee morale system

---

## Troubleshooting

### No Referees Available
```csharp
if (gameData.referees.Count == 0)
{
    gameData.referees = RefereeManager.CreateDefaultReferees();
}
```

### All Referees Injured/Fatigued
```csharp
// Force recovery
foreach (var ref in gameData.referees)
{
    ref.isInjured = false;
    ref.fatigue = 0;
}
```

### Referee Not Improving
```csharp
// Manual training
RefereeCareerManager.TrainReferee(referee, "experience", 10);
```

### Too Many Controversies
```csharp
// Reduce corruption
referee.corruption = Mathf.Max(0, referee.corruption - 20);
```

---

## Summary

The referee system provides:
- ✅ **Realistic Career Progression**
- ✅ **Detailed Statistics Tracking**
- ✅ **Dynamic Match Events**
- ✅ **Smart Scheduling**
- ✅ **Comprehensive Reports**
- ✅ **Training & Development**
- ✅ **Full Integration with Match Simulation**

All features work seamlessly with both Simple and Advanced match simulation modes!

# Referee System Documentation

## Overview

The Referee system adds realistic officiating to match simulations, with referees influencing match quality, finish types, and overall presentation.

---

## Referee Stats (0-100)

### **Strictness**
- **High (70+)**: More DQs, count outs, strict rule enforcement
- **Low (30-)**: Lenient, lets wrestlers get away with more
- **Effect**: Increases chance of DQ/Count Out finishes by up to 15%

### **Corruption**
- **High (60+)**: Easily influenced, biased calls, screwjobs
- **Low (20-)**: Fair and impartial officiating
- **Effect**: 
  - Can create "Controversial Finish" outcomes (up to 12% chance)
  - Negative impact on match rating (up to -4 points)

### **Experience**
- **High (80+)**: Veteran referee, smooth match flow
- **Low (30-)**: Rookie, may struggle with pacing
- **Effect**: 
  - Positive impact on match rating (up to +5 points)
  - Better at handling big matches

### **Consistency**
- **High (80+)**: Predictable, reliable calls
- **Low (40-)**: Unpredictable, may botch finishes
- **Effect**: 
  - Positive impact on match rating (up to +3 points)
  - Low consistency can cause "Botched Finish" (up to 8% chance)

---

## Referee Specializations

### **Main Event Referee**
- Required experience: 70+
- Gets +2 rating bonus for title matches
- Automatically assigned to title matches when available

### **Hardcore Specialist**
- Gets +3 rating bonus for hardcore-style matches
- Preferred for: Hardcore, No DQ, Street Fight, Falls Count Anywhere, Last Man Standing

---

## Referee Influence on Matches

### **Match Rating Impact**
```
Rating Modifier = 
  + (Experience / 100) * 5        // Up to +5
  + (Consistency / 100) * 3       // Up to +3
  - (Corruption / 100) * 4        // Up to -4
  + Main Event Bonus (if title)   // +2
  + Hardcore Specialist Bonus     // +3
```

**Example:**
- Earl Hebner (Experience: 95, Consistency: 85, Corruption: 40)
- Rating Modifier: +4.75 + 2.55 - 1.6 = **+5.7 points**

### **Finish Type Influence**

#### High Strictness (70+)
- Up to 15% chance to change finish to DQ or Count Out
- Enforces rules more rigidly

#### High Corruption (60+)
- Up to 12% chance for "Controversial Finish"
- Company-backed referees favor certain wrestlers
- Can create storyline moments

#### Low Consistency (40-)
- Up to 8% chance for "Botched Finish"
- Unpredictable calls add realism

---

## Referee Assignment Logic

### Automatic Assignment
If no referee is assigned to a match, the system automatically assigns one based on:

1. **Match Importance**
   - Title matches get experienced refs (70+ experience)
   - Regular matches get random suitable refs

2. **Match Type Suitability**
   - Hardcore matches prefer hardcore specialists
   - All refs must be active

3. **Quality Priority**
   - Higher quality refs (GetQualityRating()) preferred for big matches

### Manual Assignment
```csharp
// Assign specific referee
match.referee = myReferee;

// Or let system auto-assign
RefereeManager.AssignReferee(match, gameData);
```

---

## Integration with Simulation Modes

### Advanced Mode
- Full referee influence across all phases
- Detailed logging of referee actions
- Phase-specific influences:
  - **Opening**: Experience improves match flow
  - **Mid Phase**: Consistency maintains pacing
  - **Climax**: Corruption can influence outcome

### Simple Mode
- Referee affects rating calculation
- Referee affects finish type
- No phase-by-phase logging
- Same core influence, streamlined execution

---

## Default Referee Pool

The system includes 7 pre-made referees for testing:

| Name | Strictness | Corruption | Experience | Consistency | Specialization |
|------|------------|------------|------------|-------------|----------------|
| Earl Hebner | 60 | 40 | 95 | 85 | Main Event |
| Mike Chioda | 70 | 20 | 90 | 90 | Main Event |
| Charles Robinson | 50 | 15 | 85 | 80 | Main Event |
| Nick Patrick | 40 | 70 | 75 | 60 | Company Favorite |
| Tommy Young | 80 | 10 | 80 | 85 | Strict |
| Bryce Remsburg | 30 | 5 | 70 | 75 | Hardcore Specialist |
| Rookie Ref | 60 | 20 | 25 | 40 | Learning |

### Create Default Pool
```csharp
gameData.referees = RefereeManager.CreateDefaultReferees();
```

---

## Usage Examples

### Basic Match Simulation
```csharp
// Referee auto-assigned if not set
Match match = new Match
{
    participants = new List<string> { "wrestler1", "wrestler2" },
    matchType = "Singles",
    titleMatch = false
};

// Simulate - referee assigned automatically
Match result = MatchSimulator.Simulate(match, gameData);
```

### Manual Referee Assignment
```csharp
// Assign specific referee
var earlHebner = gameData.referees.Find(r => r.name == "Earl Hebner");
match.referee = earlHebner;

Match result = MatchSimulator.Simulate(match, gameData);
```

### Title Match with Main Event Ref
```csharp
Match titleMatch = new Match
{
    participants = new List<string> { "champion", "challenger" },
    matchType = "Singles",
    titleMatch = true
};

// System automatically assigns experienced main event ref
RefereeManager.AssignReferee(titleMatch, gameData);

Match result = MatchSimulator.Simulate(titleMatch, gameData);
```

### Hardcore Match with Specialist
```csharp
Match hardcoreMatch = new Match
{
    participants = new List<string> { "wrestler1", "wrestler2" },
    matchType = "Hardcore"
};

// System prefers hardcore specialist
RefereeManager.AssignReferee(hardcoreMatch, gameData);

Match result = MatchSimulator.Simulate(hardcoreMatch, gameData);
```

### Get Referee Style Description
```csharp
string style = RefereeManager.GetRefereeStyle(match.referee);
Debug.Log($"Referee Style: {style}");
// Output: "Strict, Fair, Veteran, Consistent, Main Event"
```

---

## Advanced Features

### Storyline Integration

#### Company-Backed Referees
```csharp
referee.isFavoredByCompany = true;
referee.corruption = 70;

// This referee may favor company wrestlers
// Creates "Controversial Finish" outcomes
```

#### Corrupt Referee Angles
```csharp
if (match.finishType == "Controversial Finish")
{
    // Trigger storyline event
    // Wrestler complains about biased officiating
    // Rematch demanded
}
```

#### Botched Finishes
```csharp
if (match.finishType == "Botched Finish")
{
    // Referee made a mistake
    // Could lead to referee suspension
    // Wrestler demands different ref for rematch
}
```

### Custom Referee Creation
```csharp
Referee customRef = new Referee("My Referee", 
    strictness: 50, 
    corruption: 10, 
    experience: 60)
{
    id = "ref_custom",
    consistency = 75,
    isMainEventRef = false,
    isHardcoreSpecialist = false,
    isActive = true
};

gameData.referees.Add(customRef);
```

---

## Finish Types

### Standard Finishes
- **Pinfall** (most common)
- **Submission**
- **Knockout**
- **Count Out**
- **DQ** (Disqualification)

### Referee-Influenced Finishes
- **Controversial Finish** - Corrupt referee makes biased call
- **Botched Finish** - Inconsistent referee makes mistake

---

## Match Log Examples

### Advanced Mode with Referee
```
[OPENING] Singles match begins!
  Referee: Earl Hebner (Veteran, Consistent, Main Event)
  John Cena opens with performance score: 78.3
  Randy Orton opens with performance score: 75.1
  Referee Earl Hebner's experience improves match flow
[MID PHASE] Match intensifies with momentum shifts!
  ...
[CLIMAX] Heading into the finish sequence!
  Building to the finish...
  FINISH: John Cena wins via Pinfall!
  Referee Earl Hebner modifier: +5.7
  Final Match Rating: 85/100
```

### Controversial Finish Example
```
[CLIMAX] Heading into the finish sequence!
  Referee Nick Patrick may influence the outcome...
  ⚠️ Referee Nick Patrick makes a controversial call!
  FINISH: Randy Orton wins via Controversial Finish!
```

### Botched Finish Example
```
[CLIMAX] Heading into the finish sequence!
  ⚠️ Referee Rookie Ref botches the finish!
  FINISH: John Cena wins via Botched Finish!
```

---

## Performance Impact

Referee system adds minimal overhead:
- **Advanced Mode**: ~0.0001s per match
- **Simple Mode**: ~0.00005s per match
- Negligible impact on simulation speed

---

## Best Practices

1. **Always populate referees list** in GameData
   ```csharp
   gameData.referees = RefereeManager.CreateDefaultReferees();
   ```

2. **Let system auto-assign** for most matches
   - System makes smart choices based on match importance

3. **Manually assign** for storyline purposes
   - Corrupt ref for heel advantage
   - Strict ref for rule-heavy matches

4. **Use referee stats for storylines**
   - High corruption = heel manager can bribe
   - Low experience = rookie mistakes
   - High strictness = technical wrestlers benefit

5. **Track controversial finishes** for rematch booking
   ```csharp
   if (match.finishType == "Controversial Finish")
   {
       BookRematch(match);
   }
   ```

---

## Future Enhancements

Potential additions:
- **Referee fatigue** - Multiple matches reduce performance
- **Referee injuries** - Can be knocked out during match
- **Referee reputation** - Builds over time
- **Referee rivalries** - Certain wrestlers/refs don't get along
- **Special referee matches** - Wrestler acts as referee
- **Referee interference** - Actively helps/hinders wrestlers
- **Referee training** - Improve stats over time

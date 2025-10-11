# Competitive Mode - Player vs AI Rival Company

## Quick Start

### **1. Setup Scene**
```csharp
// Attach RatingsWarExample.cs to a GameObject
// Configure in Inspector:
- Player Company Name: "Your Company Name"
- Rival Company Name: "Rival Name"
- Rival Philosophy: Balanced/WorkRate/StarPower/Storyline/Chaos
- Season Length: 52 weeks
- Same Night Competition: true (Monday Night Wars style)
```

### **2. Press Play**
```
Game starts → Roster divided 50/50
```

### **3. Controls**
- **SPACE**: Run one competitive week
- **M**: Monthly report
- **R**: Roster comparison
- **T**: Sign free agent

### **4. Play**
```
Week 1 → Your show vs AI show → Winner declared
Week 2 → Continue...
Week 52 → Season finale, overall winner declared
```

---

## What You Get

### **Automatic Competition**
- AI books its own shows
- AI responds to your booking
- AI signs talent
- AI creates feuds
- AI adapts strategy

### **Head-to-Head Battle**
- Same night = direct competition
- Ratings compared
- Winner/Loser/Draw system
- Season-long tracking

### **Roster Management**
- Split roster (10 each)
- Free agency every 4 weeks
- Unhappy wrestlers leave
- Sign new talent

### **AI Personalities**
- **Balanced**: Mix of everything
- **WorkRate**: Quality matches (7 per show)
- **StarPower**: Big names (5 per show)
- **Storyline**: Feud-focused
- **Chaos**: Unpredictable

---

## Example Output

```
==================== WEEK 1 ====================
RATINGS WAR!
============================================

[PLAYER] World Wrestling Entertainment airs...
  [MAIN EVENT] Stone Cold vs The Rock - 92/100
  [MIDCARD] Triple H vs Kurt Angle - 85/100
  [OPENER] Undertaker vs Edge - 78/100
[PLAYER] Show Average: 85/100

[AI - TCW] Booking Total Championship Wrestling...
[AI - TCW] Main Event: Jericho vs Cena (Title Match)
[AI - TCW] Show Average: 82/100

🏆 PLAYER WINS THE WEEK! (85.0 vs 82.0)
📊 STANDINGS: Player 1W-0L-0D | Rival 0W-1L-0D

=== TOP 5 WRESTLERS ===
YOUR ROSTER:
  1. Stone Cold - Pop: 98, Mom: +25 🔥
  2. The Rock - Pop: 97, Mom: +20 🔥
  3. Undertaker - Pop: 96, Mom: +10

RIVAL ROSTER:
  1. Chris Jericho - Pop: 90, Mom: +15
  2. John Cena - Pop: 95, Mom: +18
  3. Kurt Angle - Pop: 92, Mom: +5
```

---

## Game Flow

### **Weekly Cycle**
```
1. Press SPACE
   ↓
2. You book show (auto or manual)
   ↓
3. AI books show (analyzes yours if same night)
   ↓
4. Both shows simulate
   ↓
5. Ratings compared
   ↓
6. Winner declared
   ↓
7. Standings updated
   ↓
8. Wrestler states update
   ↓
9. Back to step 1
```

### **Monthly Cycle** (Every 4 Weeks)
```
Week 4, 8, 12, etc.
   ↓
1. Talent movement phase
   ↓
2. Unhappy wrestlers → free agents
   ↓
3. AI signs talent
   ↓
4. You can sign talent (press T)
   ↓
5. Monthly report
   ↓
6. AI adjusts strategy if needed
```

---

## Win Conditions

### **Week Winner**
- Beat rival by **2+** rating points = WIN
- Lose by **2+** rating points = LOSS
- Within 2 points = DRAW

### **Season Winner**
After 52 weeks:
- Most weeks won = **OVERALL WINNER**
- Tiebreaker: Average rating

---

## Strategy Guide

### **Early Game (Weeks 1-13)**
- Establish your stars
- Build momentum for top talent
- Create strong feuds
- Don't overwork wrestlers

### **Mid Game (Weeks 14-39)**
- Rotate talent to avoid fatigue
- Sign free agents
- Counter AI's strategy
- Maintain morale

### **Late Game (Weeks 40-52)**
- Push for championship
- Use your best talent
- Create special attractions
- Close strong

---

## AI Counter-Programming

When `sameNightCompetition = true`:

### **AI Analyzes Your Show**
```
[AI] Analyzing player show: 2 title matches detected
[AI] Player has title matches! Increasing star power!
```

### **AI Responds**
- Sees your title matches → Books title match
- Sees your main event quality → Matches it
- Aggressive AI → Adds special attractions

---

## Customization

### **Easy Mode**
```csharp
rivalPhilosophy = BookingPhilosophy.Balanced;
rivalAI.aggression = AggressionLevel.Passive;
competition.DivideRoster(gameData, 0.6f); // You get 60%
```

### **Hard Mode**
```csharp
rivalPhilosophy = BookingPhilosophy.StarPower;
rivalAI.aggression = AggressionLevel.Aggressive;
competition.DivideRoster(gameData, 0.4f); // You get 40%
```

### **Chaos Mode**
```csharp
rivalPhilosophy = BookingPhilosophy.Chaos;
rivalAI.aggression = AggressionLevel.Aggressive;
sameNightCompetition = true;
```

---

## Files Created

### **Core Systems**
- `RivalCompanyAI.cs` - AI opponent logic
- `CompetitionManager.cs` - Tracks competition
- `RatingsWarExample.cs` - Playable example

### **Existing Integration**
- Uses `BookingManager` for AI booking
- Uses `WrestlerStateManager` for states
- Uses `MatchSimulator` for matches
- Uses `RefereeManager` for referees

---

## Next Steps

1. **Play the example** - Run `RatingsWarExample` scene
2. **Try different AI personalities** - Change in Inspector
3. **Extend with UI** - Create visual interface
4. **Add manual booking** - Let player book matches
5. **Add financial system** - Budget management
6. **Add PPV events** - Special shows
7. **Add contracts** - Limited-time deals

---

## Architecture

```
RatingsWarExample (Unity MonoBehaviour)
    ↓
Creates CompetitionManager
    ↓
    ├─→ Manages Player Roster
    ├─→ Manages RivalCompanyAI
    │       ↓
    │       ├─→ Uses BookingManager (AI books shows)
    │       ├─→ Uses MatchSimulator (simulates)
    │       ├─→ Uses WrestlerStateManager (updates)
    │       └─→ Adapts Strategy
    ↓
Weekly Competition Loop:
    Player Show → Simulated
    Rival Show → AI Books & Simulates
    Compare Ratings → Declare Winner
    Update States → Talent Movement
    Repeat
```

---

## Benefits Over Single-Player

### **Single Player Mode**
- Simulate seasons alone
- No competition
- Less strategic depth

### **Competitive Mode**
- Direct rivalry
- Counter-programming
- Talent wars
- Strategic booking matters
- Win/loss pressure
- More engaging

---

## Summary

This system transforms your wrestling sim into a **competitive management game** where:

✅ You compete against intelligent AI  
✅ Every booking decision affects standings  
✅ Talent management matters  
✅ Strategy evolves over time  
✅ Clear win/loss conditions  
✅ Season-long progression  
✅ Fully automated AI opponent  
✅ Plugs into existing systems seamlessly  

**It's like playing GM Mode against a smart CPU opponent that actually tries to beat you!**

# Ratings War System - Player vs AI Competition

## Overview

The **Ratings War System** transforms your wrestling simulation into a competitive experience where you battle an AI-controlled rival company for ratings supremacy. Think **WWE vs WCW's Monday Night Wars** - both companies air shows on the same night, competing for viewers and talent.

---

## Core Concept

```
┌────────────────────────────────────────────────────┐
│                  MONDAY NIGHT                      │
├────────────────────────────────────────────────────┤
│                                                    │
│  YOUR COMPANY              vs        RIVAL AI      │
│  Monday Night Raw                    Nitro         │
│                                                    │
│  Books 6 matches                     Books 6       │
│  Your wrestlers                      AI wrestlers  │
│  Your decisions                      AI strategy   │
│                                                    │
│  Show Rating: 85/100                 Rating: 82    │
│                                                    │
│           🏆 YOU WIN THE WEEK! 🏆                  │
│                                                    │
└────────────────────────────────────────────────────┘
```

---

## Game Mode Features

### **1. Split Roster**
At the start, the 20 wrestlers are divided:
- **10 wrestlers** to your company
- **10 wrestlers** to rival company
- Balanced by popularity (alternating draft)

### **2. Weekly Head-to-Head**
Every week, both companies air shows:
- **Same Night Mode**: Monday Night Wars style (AI can counter-program)
- **Different Nights**: Separate nights (no counter-programming)

### **3. Win Conditions**
- **Win**: Beat rival by 2+ rating points
- **Loss**: Lose by 2+ rating points
- **Draw**: Within 2 points of each other

### **4. Season Length**
- Default: **52 weeks** (1 year)
- Winner: Most weeks won
- Tracks: Wins, Losses, Draws, Average Rating

---

## AI Rival Personalities

### **Balanced** (Default)
- Mix of storylines, star power, and match quality
- Adapts based on performance
- Medium aggression

### **WorkRate**
- Books 7 matches per show
- Focuses on in-ring quality
- Showcases best workers
- Uses Advanced simulation mode

### **StarPower**
- Books 5 big matches
- Features top popular wrestlers
- Less matches, bigger names
- High production value

### **Storyline**
- Prioritizes feuds and drama
- Creates storylines aggressively
- Books 6 matches focused on angles
- Builds long-term narratives

### **Chaos**
- Unpredictable and wild
- Random match counts (4-9)
- Random booking priorities
- Can do anything

---

## AI Aggression Levels

### **Passive**
- Doesn't analyze your shows
- Books independently
- No counter-programming
- No talent raids

### **Medium** (Default)
- Analyzes your booking
- Responds to your title matches
- Attempts talent signings
- Creates feuds

### **Aggressive**
- Actively counter-programs
- Tries to destroy you
- Aggressively signs talent
- Adds special attractions
- Goes for the kill

**AI Adapts**: If losing badly (5+ weeks behind), AI becomes Aggressive

---

## Competition Mechanics

### **Weekly Competition**
```csharp
Week 1 - HEAD TO HEAD
├─ Your Show: Monday Night Raw
│  ├─ Opener: Austin vs Rock
│  ├─ Midcard: Triple H vs Angle
│  ├─ Main Event: Undertaker vs Michaels (TITLE)
│  └─ Rating: 87/100
│
└─ Rival Show: TCW Monday Night
   ├─ Opener: Jericho vs Edge
   ├─ Midcard: CM Punk vs AJ Styles
   ├─ Main Event: Orton vs Cena (TITLE)
   └─ Rating: 84/100

Result: YOU WIN! (87 vs 84)
Standings: You 1-0-0 | Rival 0-1-0
```

### **Talent Movement**
Every 4 weeks, talent can change companies:

**Unhappy Wrestlers Leave:**
- Morale < 30 + Momentum < -30 = 30% chance to become free agent
- Applies to both your roster and AI's roster

**Free Agent Signings:**
- AI automatically tries to sign talent
- You can press **T** to sign best free agent
- Signing cost based on popularity + momentum

**Example:**
```
Week 4 - TALENT MOVEMENT
⚠️ Randy Orton is unhappy and becomes a free agent!
  (Your company - Morale: 25, Momentum: -45)

💰 Chris Jericho leaves TCW and becomes available!
  (Rival company - Morale: 28, Momentum: -35)

[AI] Signed Chris Benoit for $95,000!
[YOU] Press T to sign Jericho!
```

### **Feud Creation**
AI creates new feuds automatically:
- 20% chance per week
- Picks top Face vs top Heel
- Sets heat (50-80)
- Adds to feud system

### **Strategy Adaptation**
AI adjusts based on performance:
```
Week 15: AI is 8-7 (losing)
[AI] Strategy shift: Going AGGRESSIVE!
- Increases aggression level
- Adds special attractions
- Counter-programs harder
- Goes for star power

Week 30: AI is 18-12 (winning)
[AI] Strategy shift: Maintaining balanced approach
- Returns to medium aggression
- Sticks with current strategy
```

---

## Player Experience

### **Controls**
- **SPACE** - Run one competitive week
- **M** - Monthly report (standings, top performers, etc.)
- **R** - Roster report (your roster vs rival's roster)
- **T** - Try to sign best free agent

### **Auto vs Manual Booking**
```csharp
// Demo Mode: Auto-book player shows
autoBookPlayer = true;
playerPriority = BookingPriority.Balanced;

// Manual Mode: You book manually
autoBookPlayer = false;
// Call your own booking logic
```

Currently uses auto-booking for demo purposes, but you can integrate your own booking UI.

---

## Example Gameplay Session

### **Week 1**
```
==================== WEEK 1 ====================
RATINGS WAR!
============================================

[PLAYER] World Wrestling Entertainment Monday Night airs...
  Stone Cold Steve Austin vs The Rock
  Triple H vs Kurt Angle
  Undertaker vs Shawn Michaels (TITLE)
[PLAYER] Show Average: 87/100

[AI - Total Championship Wrestling] Booking Monday Night...
[AI - TCW] Booked 6 matches
  Chris Jericho vs Edge
  Eddie Guerrero vs CM Punk
  Randy Orton vs John Cena (TITLE)
[AI - TCW] Show Average: 84/100

🏆 PLAYER WINS THE WEEK! (87.0 vs 84.0)

📊 STANDINGS: Player 1W-0L-0D | Rival 0W-1L-0D
```

### **Week 4 - Monthly Report**
```
========== MONTHLY REPORT ==========

Last 4 Weeks:
  Week 1: 🏆 YOU (87.0 vs 84.0)
  Week 2: 🏆 YOU (85.5 vs 83.2)
  Week 3: 💀 THEM (81.0 vs 88.5)
  Week 4: 🤝 DRAW (86.0 vs 87.5)

Overall Record:
  You: 2W - 1L - 1D
  Total Championship Wrestling: 1W - 2L - 1D

Average Show Ratings:
  You: 84.9/100
  Rival: 85.8/100

Your Top Performer:
  Stone Cold Steve Austin - Popularity: 98/100

Rival's Top Performer:
  Chris Jericho - Popularity: 90/100

Rival AI Status:
[Total Championship Wrestling]
  Record: 1W - 2L
  Avg Rating: 85.8/100
  Shows Run: 4
  Budget: $980,000
  Philosophy: Balanced
  Aggression: Medium
===================================
```

### **Week 8 - Talent Movement**
```
[TALENT MOVEMENT] Contract negotiations phase...

⚠️ Randy Orton is unhappy and becomes a free agent!
💰 Eddie Guerrero leaves Total Championship Wrestling!

[AI - TCW] Signed Chris Benoit for $95,000!

[ROSTERS] Player: 9 | Rival: 10 | Free Agents: 2

💡 Press T to sign a free agent!
```

### **Week 52 - Season Finale**
```
************************************************************
🏁 SEASON FINALE - RATINGS WAR CONCLUDED
************************************************************

🎉🎉🎉 YOU WIN THE RATINGS WAR! 🎉🎉🎉

Final Record:
  World Wrestling Entertainment: 28W - 20L - 4D
  Total Championship Wrestling: 20W - 28L - 4D

Total Shows:
  You ran: 52 shows
  Rival ran: 52 shows

Average Ratings:
  You: 84.3/100
  Rival: 82.1/100

Longest Win Streaks:
  You: 7 weeks
  Rival: 5 weeks

************************************************************
```

---

## Strategy Tips

### **For Players**

**1. Monitor Your Roster**
- Check morale and momentum regularly
- Rest fatigued wrestlers
- Rotate talent to avoid burnout

**2. Build Stars**
- Push wrestlers with momentum
- Feature popular talent in main events
- Create compelling feuds

**3. Sign Smart**
- Grab free agents when available
- Focus on high popularity wrestlers
- Don't let top talent become unhappy

**4. Book Strategically**
- Open with high-energy talent
- Save best matches for main event
- Use title matches wisely

**5. Counter the AI**
- If AI goes star power, match it
- If AI books many matches, go quality over quantity
- Adapt to their philosophy

### **Understanding AI Behavior**

**Balanced AI:**
- Predictable but solid
- Books sensibly
- Hard to beat consistently

**WorkRate AI:**
- Many matches = diluted talent
- Opportunity to showcase your stars in fewer matches
- Counter with quality main events

**StarPower AI:**
- Fewer matches but bigger
- Need to match star power
- Title matches crucial

**Chaos AI:**
- Unpredictable
- Can have bad weeks
- Capitalize on their mistakes

---

## Integration with Existing Systems

### **Uses BookingManager**
```csharp
// AI uses the same booking system
Show rivalShow = BookingManager.BookShow(
    show, 
    gameData, 
    matchCount, 
    priority
);
```

### **Uses WrestlerStateManager**
```csharp
// Both companies track wrestler states
WrestlerStateManager.UpdateWrestlerAfterMatch(wrestler, match, won, gameData);
WrestlerStateManager.WeeklyReset(gameData);
```

### **Uses MatchSimulator**
```csharp
// Matches simulated same way
MatchSimulator.Simulate(match, gameData, mode);
```

### **Uses RefereeManager**
```csharp
// Referees assigned automatically
RefereeManager.AssignReferee(match, gameData);
```

---

## Customization Options

### **Modify AI Difficulty**
```csharp
// Easy: Passive aggression, Balanced philosophy
rivalAI.aggression = AggressionLevel.Passive;

// Medium: Default settings
rivalAI.aggression = AggressionLevel.Medium;

// Hard: Aggressive, WorkRate or StarPower
rivalAI.aggression = AggressionLevel.Aggressive;
rivalAI.philosophy = BookingPhilosophy.StarPower;
```

### **Change Competition Style**
```csharp
// Head-to-head same night (harder)
sameNightCompetition = true;

// Different nights (easier)
sameNightCompetition = false;
```

### **Adjust Season Length**
```csharp
seasonLength = 26; // Half season
seasonLength = 52; // Full season (default)
seasonLength = 104; // Two years
```

### **Modify Roster Split**
```csharp
// Even split (default)
competition.DivideRoster(gameData, 0.5f);

// Player advantage (60/40)
competition.DivideRoster(gameData, 0.6f);

// Rival advantage (40/60)
competition.DivideRoster(gameData, 0.4f);
```

---

## Extending the System

### **Add Manual Booking UI**
```csharp
private Show ManualBooking()
{
    // Create your booking UI
    // Let player pick matches
    // Return completed show
}

// In RunWeek():
if (!autoBookPlayer)
{
    playerShow = ManualBooking();
}
```

### **Add Financial System**
```csharp
public class CompanyFinances
{
    public float revenue; // From ratings
    public float expenses; // Wrestler salaries
    public float budget;
    
    public void CalculateRevenue(Show show)
    {
        revenue += show.averageRating * 10000f; // $10k per rating point
    }
}
```

### **Add Talent Contracts**
```csharp
public class WrestlerContract
{
    public string wrestlerId;
    public float salary;
    public int weeksRemaining;
    public bool canNegotiate;
}
```

### **Add Special Events**
```csharp
// PPVs, Invasions, Debuts
if (currentWeek == 13) // WrestleMania
{
    playerShow.specialEvent = true;
    playerShow.ratingBonus = 10;
}
```

---

## Success Metrics

### **Win Conditions**
1. **Rating Victory**: Most weeks won
2. **Quality Victory**: Highest average rating
3. **Dominance Victory**: Win by 10+ weeks
4. **Perfect Season**: Win every week

### **Achievements**
- **🔥 Hot Streak**: Win 5 weeks in a row
- **💪 Comeback Kid**: Win after being 5+ weeks behind
- **⭐ Star Maker**: Have wrestler reach 100 popularity
- **📺 Rating King**: Achieve 95+ show rating
- **🏆 Undefeated**: Finish season undefeated

---

## Troubleshooting

### **AI Dominates Every Week**
- Lower AI aggression
- Give yourself more roster (60/40 split)
- Check if your booking strategy matches talent
- Sign free agents aggressively

### **Too Many Free Agents**
- Wrestlers leaving due to low morale
- Rotate talent more
- Don't overuse same wrestlers
- Give losing wrestlers wins occasionally

### **AI Never Creates Feuds**
- AI has 20% chance per week
- May need more talent of opposite alignments
- Check that AI has both Faces and Heels

### **Ratings Too Close Every Week**
- Increase win margin requirement (current: 2 points)
- Adjust match quality variance
- Give more distinct philosophies

---

## Future Enhancements

Potential additions:

1. **PPV Events** - Special shows worth more
2. **Brand Split** - Two shows per company
3. **Invasion Angles** - Wrestlers jump ship dramatically
4. **Contracts** - Limited-time deals, bidding wars
5. **Storyline Tracking** - AI creates ongoing narratives
6. **Fan Base** - Tracking viewership numbers
7. **TV Deal** - Better time slots, bigger budgets
8. **Multiplayer** - Player vs Player competition

---

## Summary

✅ **Fully functional ratings war**  
✅ **AI opponent with personality**  
✅ **Automatic talent management**  
✅ **Weekly head-to-head competition**  
✅ **Roster splits and free agency**  
✅ **Strategy adaptation**  
✅ **Season-long tracking**  
✅ **Win/Loss/Draw system**  
✅ **Integration with all existing systems**  

The Ratings War System transforms your simulation into a **competitive management game** where every booking decision matters and you're fighting for dominance against an intelligent AI opponent!

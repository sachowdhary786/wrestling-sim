# Match Simulation System

This directory contains the modular match simulation system for the wrestling game.

## Simulation Modes

The system supports two simulation modes for different use cases:

### **Advanced Mode** (Default)
- Full phase-by-phase simulation (Opening → Mid → Climax → Aftermath)
- Detailed Debug logging for each phase
- Dynamic momentum system
- Complete injury simulation
- Best for: Matches the player is watching, important story matches

### **Simple Mode**
- Fast, streamlined simulation without phases
- Minimal logging for performance
- Reduced injury chance (50% of normal)
- Same core calculations, simplified flow
- Best for: Bulk simulations, off-screen matches, tournaments, season mode

**Performance**: Simple mode is typically **2-3x faster** than Advanced mode.

## Architecture Overview

The match simulation has been refactored into focused, single-responsibility modules:

### Core Files

#### **MatchSimulator.cs** (Main Orchestrator)
- **Purpose**: Coordinates the entire match simulation flow
- **Responsibilities**:
  - Validates match participants
  - Initializes match state
  - Orchestrates the 4 phases of match simulation
  - Returns completed match with results
- **Size**: ~80 lines (down from 469 lines)

#### **MatchState.cs** (Data Structure)
- **Purpose**: Tracks all match state across simulation phases
- **Contains**:
  - Wrestler list
  - Performance scores
  - Momentum values
  - Match and game data references
  - Match type weights

#### **MatchPhaseSimulator.cs** (Phase Logic)
- **Purpose**: Simulates the individual phases of a match
- **Phases**:
  1. **Opening Phase** - Feeling out process, initial performance calculation
  2. **Mid Phase** - Momentum swings, near falls, back-and-forth action
  3. **Climax Phase** - Finish sequence, winner determination
  4. **Aftermath Phase** - Injury checks, post-match angles

#### **MatchPerformanceCalculator.cs** (Performance & Rating)
- **Purpose**: Handles all performance and rating calculations
- **Functions**:
  - Calculate base performance
  - Apply trait bonuses
  - Calculate feud heat bonuses
  - Apply chemistry modifiers
  - Calculate tag team chemistry
  - Calculate final match rating

#### **MatchInjurySystem.cs** (Injury Management)
- **Purpose**: Handles injury simulation and management
- **Functions**:
  - Check for injuries based on match conditions
  - Calculate injury chance (match type, fatigue, toughness)
  - Determine injury severity (minor, moderate, major)
  - Apply injury stat penalties
  - Generate realistic injury types

#### **SimpleMatchSimulator.cs** (Fast Simulation)
- **Purpose**: Optimized simulation for bulk processing
- **Features**:
  - Skips phase-by-phase simulation
  - Minimal logging overhead
  - Reduced injury rates
  - Same core algorithms, streamlined execution

#### **SimulationModeHelper.cs** (Utilities)
- **Purpose**: Helper methods for working with simulation modes
- **Functions**:
  - Recommend appropriate mode based on context
  - Bulk simulation utilities
  - Performance benchmarking
  - Mode descriptions

## Match Flow

```
MatchSimulator.Simulate()
    ↓
1. Validate & fetch wrestlers
    ↓
2. Initialize MatchState
    ↓
3. MatchPhaseSimulator.SimulateOpeningPhase()
   - Calculate base performance
   - Apply bonuses (traits, feuds, chemistry)
   - Initialize momentum
    ↓
4. MatchPhaseSimulator.SimulateMidPhase()
   - 2-5 momentum shifts
   - Near fall chances
   - Dynamic back-and-forth
    ↓
5. MatchPhaseSimulator.SimulateClimaxPhase()
   - Apply final momentum
   - Determine winner
   - Set finish type (Pinfall, Submission, etc.)
    ↓
6. MatchPerformanceCalculator.CalculateMatchRating()
   - Average performance
   - Psychology & popularity bonuses
   - Tag chemistry bonus
    ↓
7. MatchPhaseSimulator.SimulateAftermath()
   - MatchInjurySystem.CheckForInjuries()
   - Post-match angle chance (15%)
    ↓
8. Return completed match
```

## Benefits of Modular Design

### **Readability**
- Each file has a clear, focused purpose
- Easy to find specific functionality
- Well-documented with XML comments

### **Maintainability**
- Changes to one system don't affect others
- Easy to test individual components
- Clear separation of concerns

### **Extensibility**
- Easy to add new match phases
- Simple to add new performance factors
- Can add new injury types without touching other code

### **Reusability**
- Performance calculator can be used elsewhere
- Injury system can apply to training/events
- Phase simulator can be extended for different match types

## Usage Examples

```csharp
// Advanced mode (default) - Full phase-by-phase simulation
Match result = MatchSimulator.Simulate(booking, gameData);
// or explicitly:
Match result = MatchSimulator.Simulate(booking, gameData, MatchSimulationMode.Advanced);

// Simple mode - Fast simulation for bulk processing
Match result = MatchSimulator.Simulate(booking, gameData, MatchSimulationMode.Simple);

// Simulate entire show with mode
Show result = ShowSimulator.SimulateShow(show, gameData, MatchSimulationMode.Simple);

// Use helper to determine best mode
var mode = SimulationModeHelper.GetRecommendedMode(
    isPlayerWatching: false,
    isBulkSimulation: true,
    matchCount: 50
);
Match result = MatchSimulator.Simulate(booking, gameData, mode);

// Bulk simulate many matches efficiently
Match[] matches = GetTournamentMatches();
SimulationModeHelper.SimulateBulkMatches(matches, gameData, MatchSimulationMode.Simple);

// Benchmark performance difference
SimulationModeHelper.BenchmarkModes(testMatch, gameData, iterations: 100);
```

## Additional Systems

### **Referee System** ✅ IMPLEMENTED
- Full referee assignment and influence system
- 11 different referee event types (bumps, knockouts, controversies)
- Referee stats tracking (strictness, corruption, experience, consistency)
- Affects match ratings and finish types
- Career progression and reputation system
- See: `RefereeManager.cs`, `RefereeEventSystem.cs`, `RefereeCareerManager.cs`

### **Booking System** ✅ IMPLEMENTED
- AI-driven match card creation
- Prioritizes active feuds for headline matches
- Balances card structure (opener → midcard → main event)
- Tracks wrestler fatigue, morale, popularity, momentum
- Availability checking and workload management
- Multiple booking priorities (Balanced, Storyline, StarPower, etc.)
- See: `BookingManager.cs`, `WrestlerStateManager.cs`

### **Match Results UI** ✅ IMPLEMENTED
- Event-based system for piping results to UI
- Progress tracking for bulk simulations
- Detailed match result displays with ratings and events
- Benchmark reporting for performance comparison
- See: `MatchResultsEvent.cs`, `MatchResultsUI.cs`

## Future Enhancements

Potential additions to consider:

1. **Crowd Reactions** - Dynamic crowd heat affecting match flow
2. **Signature Moves** - Special moves that affect momentum/ratings
3. **Tag Team Booking** - Automatic tag team match creation
4. **Commentary System** - Generate play-by-play commentary
5. **Replay System** - Store key moments for review
6. **Angle System** - Non-match segments (promos, attacks, run-ins)
7. **Contract System** - Wrestler happiness and contract negotiations

## File Sizes

- **MatchSimulator.cs**: 81 lines (orchestrator)
- **MatchState.cs**: 24 lines (data structure)
- **MatchPhaseSimulator.cs**: 165 lines (phase logic)
- **MatchPerformanceCalculator.cs**: 178 lines (calculations)
- **MatchInjurySystem.cs**: 118 lines (injury system)

**Total**: 566 lines across 5 focused files (vs. 469 lines in one monolithic file)

The slight increase in total lines is due to:
- XML documentation comments
- Better code organization
- Clearer separation between concerns
- More maintainable structure

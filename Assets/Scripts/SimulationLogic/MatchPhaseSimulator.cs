using UnityEngine;

/// <summary>
/// Simulates the individual phases of a wrestling match
/// </summary>
public static class MatchPhaseSimulator
{
    // Phase 1: Opening - Feeling Out Process
    public static void SimulateOpeningPhase(MatchState state)
    {
        Debug.Log($"[OPENING] {state.match.matchType} match begins!");
        
        // Log referee assignment
        if (state.match.referee != null)
        {
            string refStyle = RefereeManager.GetRefereeStyle(state.match.referee);
            Debug.Log($"  Referee: {state.match.referee.name} ({refStyle})");
        }

        // Calculate base performance for each wrestler
        foreach (var wrestler in state.wrestlers)
        {
            float performance = MatchPerformanceCalculator.CalculateBasePerformance(
                wrestler,
                state.weights,
                state.match
            );

            performance = MatchPerformanceCalculator.ApplyTraitBonuses(
                wrestler,
                state.match,
                state.data,
                performance
            );

            performance += MatchPerformanceCalculator.GetFeudHeatBonus(wrestler, state.data);

            state.scores[wrestler] = performance;
            state.momentum[wrestler] = 50f; // Start neutral

            Debug.Log($"  {wrestler.name} opens with performance score: {performance:F1}");
        }

        // Apply chemistry modifiers
        MatchPerformanceCalculator.ApplyChemistryModifiers(state);
        
        // Apply referee influence
        RefereeManager.ApplyPhaseInfluence(state, "Opening");
    }

    // Phase 2: Mid Phase - Momentum Swings
    public static void SimulateMidPhase(MatchState state)
    {
        Debug.Log($"[MID PHASE] Match intensifies with momentum shifts!");

        int momentumShifts = UnityEngine.Random.Range(2, 5);

        for (int i = 0; i < momentumShifts; i++)
        {
            // Pick a random wrestler to gain momentum
            Wrestler wrestler = state.wrestlers[UnityEngine.Random.Range(0, state.wrestlers.Count)];
            float momentumGain = UnityEngine.Random.Range(10f, 25f);

            state.momentum[wrestler] = Mathf.Min(100f, state.momentum[wrestler] + momentumGain);
            state.scores[wrestler] += momentumGain * 0.3f; // Momentum affects overall score

            Debug.Log($"  Momentum shift #{i + 1}: {wrestler.name} gains control (+{momentumGain:F1})");

            // Others lose some momentum
            foreach (var other in state.wrestlers)
            {
                if (other != wrestler)
                {
                    state.momentum[other] = Mathf.Max(0f, state.momentum[other] - (momentumGain * 0.5f));
                }
            }
        }

        // Near falls / close calls add excitement
        if (UnityEngine.Random.value > 0.5f)
        {
            Wrestler nearFallWrestler = state.wrestlers[UnityEngine.Random.Range(0, state.wrestlers.Count)];
            state.scores[nearFallWrestler] += 5f;
            Debug.Log($"  NEAR FALL! {nearFallWrestler.name} almost had it!");
        }
        
        // Check for referee events during mid phase
        var refEvent = RefereeEventSystem.CheckForEvent(state.match.referee, state.match, 2, state);
        if (refEvent != null)
        {
            RefereeEventSystem.ApplyEvent(refEvent, state.match.referee, state.match, state);
        }
    }

    // Phase 3: Climax - Finish Sequence
    public static Wrestler SimulateClimaxPhase(MatchState state)
    {
        Debug.Log($"[CLIMAX] Heading into the finish sequence!");
        Debug.Log($"  Building to the finish...");
        
        // Apply referee influence during climax
        RefereeManager.ApplyPhaseInfluence(state, "Climax");

        // Final momentum matters more
        foreach (var kvp in state.momentum)
        {
            state.scores[kvp.Key] += kvp.Value * 0.2f;
        }

        // Pick winner based on final scores (with randomness)
        Wrestler winner = state.wrestlers[0];
        float highScore = 0;

        foreach (var kvp in state.scores)
        {
            float adjusted = kvp.Value + UnityEngine.Random.Range(-10f, 10f);
            if (adjusted > highScore)
            {
                highScore = adjusted;
                winner = kvp.Key;
            }
        }

        // Check for referee events during climax
        var refEvent = RefereeEventSystem.CheckForEvent(state.match.referee, state.match, 3, state);
        if (refEvent != null)
        {
            RefereeEventSystem.ApplyEvent(refEvent, state.match.referee, state.match, state);
            
            // Handle referee knockout - need replacement
            if (refEvent.requiresReplacement)
            {
                var replacement = RefereeScheduler.FindReplacementReferee(state.match, state.match.referee, state.data);
                if (replacement != null)
                {
                    state.match.referee = replacement;
                }
            }
        }

        // Determine finish type
        string finishType = DetermineFinishType(state);
        
        // Apply referee influence on finish type
        finishType = RefereeManager.ApplyRefereeInfluence(finishType, state.match.referee, state.match);
        state.match.finishType = finishType;

        Debug.Log($"  FINISH: {winner.name} wins via {finishType}!");

        return winner;
    }

    // Phase 4: Aftermath - Post-match consequences
    public static void SimulateAftermath(MatchState state, Wrestler winner)
    {
        Debug.Log($"[AFTERMATH] Match concluded. Winner: {winner.name}");

        // Record referee performance
        if (state.match.referee != null)
        {
            bool wasKnockedOut = state.match.referee.stats.timesKnockedOut > 0 && 
                                 state.match.finishType == "Controversial Finish";
            bool wasBumped = state.match.referee.stats.timesBumped > 0;
            
            RefereeCareerManager.RecordMatch(state.match.referee, state.match, wasKnockedOut, wasBumped);
        }

        // Check for injuries
        float fatigue = (100 - MatchPerformanceCalculator.AverageStat(state.wrestlers, w => w.stamina)) / 100f;
        MatchInjurySystem.CheckForInjuries(state.wrestlers, state.match, state.match.matchType, fatigue, state.data);

        // Post-match angle chance
        if (UnityEngine.Random.value < 0.15f) // 15% chance
        {
            Debug.Log($"  POST-MATCH: Something is happening after the bell!");
            // Could trigger storyline events here
        }
    }

    private static string DetermineFinishType(MatchState state)
    {
        string[] finishes = { "Pinfall", "Submission", "Knockout", "Count Out", "DQ" };
        float[] weights = { 60f, 20f, 10f, 5f, 5f };

        // Adjust based on match type
        if (state.match.matchType == "Hardcore")
        {
            weights[2] += 20f; // More knockouts
        }

        float total = 0;
        foreach (float w in weights)
            total += w;
        float roll = UnityEngine.Random.Range(0f, total);

        float cumulative = 0;
        for (int i = 0; i < finishes.Length; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return finishes[i];
        }

        return "Pinfall";
    }
}

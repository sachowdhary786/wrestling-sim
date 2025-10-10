using UnityEngine;

/// <summary>
/// Helper utilities for choosing and working with simulation modes
/// </summary>
public static class SimulationModeHelper
{
    /// <summary>
    /// Determines the appropriate simulation mode based on context
    /// </summary>
    public static MatchSimulationMode GetRecommendedMode(bool isPlayerWatching, bool isBulkSimulation, int matchCount = 1)
    {
        // Use Advanced mode if:
        // - Player is watching the match
        // - Single important match
        if (isPlayerWatching)
            return MatchSimulationMode.Advanced;

        // Use Simple mode if:
        // - Bulk simulation (tournaments, season sim)
        // - Off-screen matches
        // - Many matches at once
        if (isBulkSimulation || matchCount > 5)
            return MatchSimulationMode.Simple;

        // Default to Advanced for quality
        return MatchSimulationMode.Advanced;
    }

    /// <summary>
    /// Simulates multiple matches efficiently
    /// </summary>
    public static void SimulateBulkMatches(Match[] matches, GameData data, MatchSimulationMode mode = MatchSimulationMode.Simple)
    {
        Debug.Log($"[BULK SIMULATION] Simulating {matches.Length} matches in {mode} mode...");
        
        var startTime = Time.realtimeSinceStartup;

        foreach (var match in matches)
        {
            MatchSimulator.Simulate(match, data, mode);
        }

        var elapsed = Time.realtimeSinceStartup - startTime;
        Debug.Log($"[BULK SIMULATION] Completed {matches.Length} matches in {elapsed:F3} seconds ({elapsed / matches.Length:F4}s per match)");
    }

    /// <summary>
    /// Benchmarks the performance difference between modes
    /// </summary>
    public static void BenchmarkModes(Match testMatch, GameData data, int iterations = 100)
    {
        Debug.Log($"=== SIMULATION MODE BENCHMARK ({iterations} iterations) ===");

        // Benchmark Simple mode
        var startSimple = Time.realtimeSinceStartup;
        for (int i = 0; i < iterations; i++)
        {
            var matchCopy = CopyMatch(testMatch);
            MatchSimulator.Simulate(matchCopy, data, MatchSimulationMode.Simple);
        }
        var simpleTime = Time.realtimeSinceStartup - startSimple;

        // Benchmark Advanced mode
        var startAdvanced = Time.realtimeSinceStartup;
        for (int i = 0; i < iterations; i++)
        {
            var matchCopy = CopyMatch(testMatch);
            MatchSimulator.Simulate(matchCopy, data, MatchSimulationMode.Advanced);
        }
        var advancedTime = Time.realtimeSinceStartup - startAdvanced;

        // Results
        Debug.Log($"Simple Mode:   {simpleTime:F3}s total, {simpleTime / iterations:F5}s per match");
        Debug.Log($"Advanced Mode: {advancedTime:F3}s total, {advancedTime / iterations:F5}s per match");
        Debug.Log($"Speed Improvement: {advancedTime / simpleTime:F2}x faster with Simple mode");
        Debug.Log($"===========================================");
    }

    private static Match CopyMatch(Match original)
    {
        // Simple shallow copy for benchmarking
        return new Match
        {
            id = original.id,
            titleId = original.titleId,
            location = original.location,
            matchType = original.matchType,
            titleMatch = original.titleMatch,
            participants = new System.Collections.Generic.List<string>(original.participants)
        };
    }

    /// <summary>
    /// Gets a description of what each mode does
    /// </summary>
    public static string GetModeDescription(MatchSimulationMode mode)
    {
        return mode switch
        {
            MatchSimulationMode.Simple => 
                "Fast simulation without phases. Best for bulk processing, off-screen matches, and season simulations. " +
                "Reduced injury chance and minimal logging for performance.",

            MatchSimulationMode.Advanced => 
                "Full phase-by-phase simulation with detailed logging. Includes Opening, Mid Phase, Climax, and Aftermath. " +
                "Best for matches the player is watching or important story matches.",

            _ => "Unknown mode"
        };
    }
}

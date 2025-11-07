using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper utilities for choosing and working with simulation modes
/// </summary>
public static class SimulationModeHelper
{
    /// <summary>
    /// Determines the appropriate simulation mode based on context
    /// </summary>
    public static MatchSimulationMode GetRecommendedMode(
        bool isPlayerWatching,
        bool isBulkSimulation,
        int matchCount = 1
    )
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
    /// Simulates multiple matches efficiently with progress reporting
    /// </summary>
    public static void SimulateBulkMatches(
        Match[] matches,
        GameData data,
        MatchSimulationMode mode = MatchSimulationMode.Simple
    )
    {
        var startTime = Time.realtimeSinceStartup;
        var results = new List<MatchResult>();
        int totalInjuries = 0;
        int totalRating = 0;
        int highestRating = 0;
        int lowestRating = 100;

        for (int i = 0; i < matches.Length; i++)
        {
            var matchStartTime = Time.realtimeSinceStartup;
            var match = matches[i];

            // Simulate the match
            MatchSimulator.Simulate(match, data, mode);
            var matchTime = Time.realtimeSinceStartup - matchStartTime;

            // Create result object
            var result = CreateMatchResult(match, data, mode, matchTime);
            results.Add(result);

            // Track statistics
            totalRating += result.rating;
            highestRating = Mathf.Max(highestRating, result.rating);
            lowestRating = Mathf.Min(lowestRating, result.rating);
            totalInjuries += result.events?.FindAll(e => e.eventType == "Injury").Count ?? 0;

            // Broadcast progress
            var elapsed = Time.realtimeSinceStartup - startTime;
            var avgTimePerMatch = elapsed / (i + 1);
            var estimatedRemaining = avgTimePerMatch * (matches.Length - i - 1);

            MatchResultsEvent.BroadcastBulkProgress(
                new BulkSimulationProgress
                {
                    completedMatches = i + 1,
                    totalMatches = matches.Length,
                    percentComplete = ((i + 1) / (float)matches.Length) * 100f,
                    estimatedTimeRemaining = estimatedRemaining,
                    lastCompletedMatch = result,
                }
            );
        }

        var totalElapsed = Time.realtimeSinceStartup - startTime;

        // Broadcast completion summary
        MatchResultsEvent.BroadcastBulkComplete(
            new BulkSimulationSummary
            {
                totalMatches = matches.Length,
                totalTime = totalElapsed,
                averageTimePerMatch = totalElapsed / matches.Length,
                mode = mode,
                results = results,
                totalInjuries = totalInjuries,
                averageRating = totalRating / matches.Length,
                highestRating = highestRating,
                lowestRating = lowestRating,
            }
        );
    }

    /// <summary>
    /// Benchmarks the performance difference between modes and broadcasts results
    /// </summary>
    public static void BenchmarkModes(Match testMatch, GameData data, int iterations = 100)
    {
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

        // Broadcast results
        MatchResultsEvent.BroadcastBenchmark(
            new BenchmarkResult
            {
                iterations = iterations,
                simpleModeTotalTime = simpleTime,
                simpleModeAvgTime = simpleTime / iterations,
                advancedModeTotalTime = advancedTime,
                advancedModeAvgTime = advancedTime / iterations,
                speedMultiplier = advancedTime / simpleTime,
            }
        );
    }

    /// <summary>
    /// Creates a MatchResult object from a completed match
    /// </summary>
    private static MatchResult CreateMatchResult(
        Match match,
        GameData data,
        MatchSimulationMode mode,
        float simTime
    )
    {
        var result = new MatchResult
        {
            match = match,
            rating = match.rating,
            finishType = match.finishType.ToString(),
            simulationMode = mode,
            simulationTime = simTime,
            wasTitleMatch = match.titleMatch,
            participantNames = new List<string>(),
            events = new List<MatchEvent>(),
        };

        // Get winner name
        if (data.wrestlers.TryGetValue(match.winnerId, out var winner))
        {
            result.winnerName = winner.name;
        }
        else
        {
            result.winnerName = "Unknown";
        }

        // Get participant names
        foreach (var participantId in match.participants)
        {
            if (data.wrestlers.TryGetValue(participantId, out var wrestler))
            {
                result.participantNames.Add(wrestler.name);

                // Check for injuries
                if (wrestler.injured && wrestler.recoveryWeeksRemaining > 0)
                {
                    result.events.Add(
                        new MatchEvent
                        {
                            eventType = "Injury",
                            description = $"Suffered a {wrestler.injuryType}",
                            wrestlerName = wrestler.name,
                        }
                    );
                }
            }
        }

        // Get title name if applicable
        if (match.titleMatch && match.titleId.HasValue)
        {
            if (data.titles.TryGetValue(match.titleId.Value, out var title))
            {
                result.titleName = title.name;
            }
            else
            {
                result.titleName = "Championship";
            }
        }

        return result;
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
            participants = new List<Guid>(original.participants),
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
                "Fast simulation without phases. Best for bulk processing, off-screen matches, and season simulations. "
                    + "Reduced injury chance and minimal logging for performance.",

            MatchSimulationMode.Advanced =>
                "Full phase-by-phase simulation with detailed logging. Includes Opening, Mid Phase, Climax, and Aftermath. "
                    + "Best for matches the player is watching or important story matches.",

            _ => "Unknown mode",
        };
    }
}

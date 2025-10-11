using System;
using System.Collections.Generic;

/// <summary>
/// Event system for broadcasting match simulation results to UI
/// </summary>
public static class MatchResultsEvent
{
    // Single match result event
    public static event Action<MatchResult> OnMatchCompleted;
    
    // Bulk simulation progress event
    public static event Action<BulkSimulationProgress> OnBulkProgress;
    
    // Bulk simulation completed event
    public static event Action<BulkSimulationSummary> OnBulkCompleted;
    
    // Benchmark results event
    public static event Action<BenchmarkResult> OnBenchmarkCompleted;

    /// <summary>
    /// Broadcast a single match result
    /// </summary>
    public static void BroadcastMatchResult(MatchResult result)
    {
        OnMatchCompleted?.Invoke(result);
    }

    /// <summary>
    /// Broadcast bulk simulation progress
    /// </summary>
    public static void BroadcastBulkProgress(BulkSimulationProgress progress)
    {
        OnBulkProgress?.Invoke(progress);
    }

    /// <summary>
    /// Broadcast bulk simulation completion
    /// </summary>
    public static void BroadcastBulkComplete(BulkSimulationSummary summary)
    {
        OnBulkCompleted?.Invoke(summary);
    }

    /// <summary>
    /// Broadcast benchmark results
    /// </summary>
    public static void BroadcastBenchmark(BenchmarkResult result)
    {
        OnBenchmarkCompleted?.Invoke(result);
    }
}

/// <summary>
/// Contains all relevant information about a completed match
/// </summary>
[Serializable]
public class MatchResult
{
    public Match match;
    public string winnerName;
    public int rating;
    public string finishType;
    public MatchSimulationMode simulationMode;
    public float simulationTime; // in seconds
    public List<string> participantNames;
    public bool wasTitleMatch;
    public string titleName;
    
    // Additional context
    public List<MatchEvent> events; // Injuries, referee bumps, etc.
}

/// <summary>
/// Represents events that occurred during a match
/// </summary>
[Serializable]
public class MatchEvent
{
    public string eventType; // "Injury", "RefereeBump", "NearFall", etc.
    public string description;
    public string wrestlerName;
}

/// <summary>
/// Progress update for bulk simulations
/// </summary>
[Serializable]
public class BulkSimulationProgress
{
    public int completedMatches;
    public int totalMatches;
    public float percentComplete;
    public float estimatedTimeRemaining;
    public MatchResult lastCompletedMatch;
}

/// <summary>
/// Summary of a completed bulk simulation
/// </summary>
[Serializable]
public class BulkSimulationSummary
{
    public int totalMatches;
    public float totalTime;
    public float averageTimePerMatch;
    public MatchSimulationMode mode;
    public List<MatchResult> results;
    
    // Statistics
    public int totalInjuries;
    public float averageRating;
    public int highestRating;
    public int lowestRating;
}

/// <summary>
/// Results from a simulation mode benchmark
/// </summary>
[Serializable]
public class BenchmarkResult
{
    public int iterations;
    public float simpleModeTotalTime;
    public float simpleModeAvgTime;
    public float advancedModeTotalTime;
    public float advancedModeAvgTime;
    public float speedMultiplier; // How much faster Simple is than Advanced
}

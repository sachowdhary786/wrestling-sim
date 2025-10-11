using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension methods for MatchSimulator to broadcast results to UI
/// </summary>
public static class MatchSimulatorExtensions
{
    /// <summary>
    /// Simulates a match and broadcasts the result to UI listeners
    /// </summary>
    public static Match SimulateAndBroadcast(
        Match booking, 
        GameData data, 
        MatchSimulationMode mode = MatchSimulationMode.Advanced
    )
    {
        var startTime = Time.realtimeSinceStartup;
        
        // Run simulation
        var result = MatchSimulator.Simulate(booking, data, mode);
        
        var simTime = Time.realtimeSinceStartup - startTime;
        
        // Create and broadcast result
        var matchResult = CreateMatchResult(result, data, mode, simTime);
        MatchResultsEvent.BroadcastMatchResult(matchResult);
        
        return result;
    }

    /// <summary>
    /// Creates a MatchResult object from a completed match
    /// </summary>
    private static MatchResult CreateMatchResult(Match match, GameData data, MatchSimulationMode mode, float simTime)
    {
        var result = new MatchResult
        {
            match = match,
            rating = match.rating,
            finishType = match.finishType ?? "Pinfall",
            simulationMode = mode,
            simulationTime = simTime,
            wasTitleMatch = match.titleMatch,
            participantNames = new List<string>(),
            events = new List<MatchEvent>()
        };

        // Get winner name
        var winner = data.wrestlers.Find(w => w.id.ToString() == match.winnerId);
        result.winnerName = winner != null ? winner.name : "Unknown";

        // Get participant names and check for events
        foreach (var participantId in match.participants)
        {
            var wrestler = data.wrestlers.Find(w => w.id.ToString() == participantId);
            if (wrestler != null)
            {
                result.participantNames.Add(wrestler.name);
                
                // Check for injuries that occurred during this match
                if (wrestler.injured && wrestler.recoveryWeeksRemaining > 0)
                {
                    result.events.Add(new MatchEvent
                    {
                        eventType = "Injury",
                        description = $"Suffered a {wrestler.injuryType}",
                        wrestlerName = wrestler.name
                    });
                }
            }
        }

        // Get title name if applicable
        if (match.titleMatch && !string.IsNullOrEmpty(match.titleId))
        {
            var title = data.titles?.Find(t => t.id.ToString() == match.titleId);
            result.titleName = title != null ? title.name : "Championship";
        }

        // Check for referee events
        if (match.referee != null)
        {
            // You can extend this to track referee bumps, knockouts, etc.
            // For now, just basic info
        }

        return result;
    }
}

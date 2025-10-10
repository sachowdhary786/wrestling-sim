using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main orchestrator for match simulation.
/// Coordinates the different phases and systems to simulate a complete wrestling match.
/// </summary>
public static class MatchSimulator
{
    // Match-type modifiers for different styles
    private static readonly Dictionary<
        string,
        (float tech, float brawl, float psych, float aerial)
    > matchTypeWeights = new()
    {
        { "Singles", (1.0f, 1.0f, 1.0f, 1.0f) },
        { "Tag", (0.9f, 1.0f, 1.1f, 1.0f) },
        { "Hardcore", (0.6f, 1.4f, 0.9f, 0.8f) },
        { "Cage", (0.8f, 1.3f, 1.0f, 0.9f) },
        { "Aerial", (0.7f, 0.8f, 1.0f, 1.3f) },
    };

    /// <summary>
    /// Simulates a complete wrestling match from start to finish
    /// </summary>
    /// <param name="booking">The match to simulate</param>
    /// <param name="data">Game data</param>
    /// <param name="mode">Simulation mode (Simple for fast bulk sims, Advanced for detailed phase-by-phase)</param>
    public static Match Simulate(Match booking, GameData data, MatchSimulationMode mode = MatchSimulationMode.Advanced)
    {
        if (booking.participants.Count < 2)
        {
            Debug.LogWarning("Match has less than 2 participants!");
            return booking;
        }

        // Route to appropriate simulation mode
        return mode == MatchSimulationMode.Simple
            ? SimulateSimple(booking, data)
            : SimulateAdvanced(booking, data);
    }

    /// <summary>
    /// Advanced simulation with full phase-by-phase detail and logging
    /// </summary>
    private static Match SimulateAdvanced(Match booking, GameData data)
    {
        // Fetch wrestlers
        List<Wrestler> wrestlers = GetWrestlersFromBooking(booking, data);
        if (wrestlers.Count < 2)
        {
            Debug.LogWarning("Could not find enough wrestlers for match!");
            return booking;
        }

        // Assign referee if not already assigned
        if (booking.referee == null)
        {
            RefereeManager.AssignReferee(booking, data);
        }

        // Get match type weights
        var weights = matchTypeWeights.ContainsKey(booking.matchType)
            ? matchTypeWeights[booking.matchType]
            : matchTypeWeights["Singles"];

        // Initialize match state
        MatchState state = new MatchState(wrestlers, booking, data, weights);

        // === PHASE 1: Opening (Feeling Out Process) ===
        MatchPhaseSimulator.SimulateOpeningPhase(state);

        // === PHASE 2: Mid Phase (Momentum Swings) ===
        MatchPhaseSimulator.SimulateMidPhase(state);

        // === PHASE 3: Climax (Finish Sequence) ===
        Wrestler winner = MatchPhaseSimulator.SimulateClimaxPhase(state);
        booking.winnerId = winner.id.ToString();

        // === Calculate Final Rating ===
        booking.rating = MatchPerformanceCalculator.CalculateMatchRating(state);

        // === PHASE 4: Aftermath ===
        MatchPhaseSimulator.SimulateAftermath(state, winner);

        return booking;
    }

    /// <summary>
    /// Simple simulation for fast bulk processing (no phases, minimal logging)
    /// </summary>
    private static Match SimulateSimple(Match booking, GameData data)
    {
        // Fetch wrestlers
        List<Wrestler> wrestlers = GetWrestlersFromBooking(booking, data);
        if (wrestlers.Count < 2)
        {
            Debug.LogWarning("Could not find enough wrestlers for match!");
            return booking;
        }

        // Assign referee if not already assigned
        if (booking.referee == null)
        {
            RefereeManager.AssignReferee(booking, data);
        }

        // Get match type weights
        var weights = matchTypeWeights.ContainsKey(booking.matchType)
            ? matchTypeWeights[booking.matchType]
            : matchTypeWeights["Singles"];

        // Use SimpleMatchSimulator for fast calculation
        return SimpleMatchSimulator.Simulate(booking, wrestlers, data, weights);
    }

    private static List<Wrestler> GetWrestlersFromBooking(Match booking, GameData data)
    {
        List<Wrestler> wrestlers = new List<Wrestler>();
        foreach (string id in booking.participants)
        {
            var wrestler = data.wrestlers.Find(x => x.id.ToString() == id);
            if (wrestler != null)
                wrestlers.Add(wrestler);
        }
        return wrestlers;
    }
}

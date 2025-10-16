using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ShowSimulator
{
    /// <summary>
    /// Simulates an entire show with all matches
    /// </summary>
    /// <param name="show">The show to simulate</param>
    /// <param name="data">Game data</param>
    /// <param name="mode">Simulation mode (Simple for fast sims, Advanced for detailed)</param>
    public static Show SimulateShow(
        Show show,
        GameData data,
        MatchSimulationMode mode = MatchSimulationMode.Advanced
    )
    {
        List<float> ratings = new List<float>();

        for (int i = 0; i < show.matches.Count; i++)
        {
            // Simulate each match with specified mode
            show.matches[i] = MatchSimulator.Simulate(show.matches[i], data, mode);
            ratings.Add(show.matches[i].rating);

            // Handle title changes
            if (show.matches[i].titleMatch)
                TitleManager.CheckTitleChange(show.matches[i], data);

            // Update popularity/stamina
            StatManager.UpdateAfterMatch(show.matches[i], data);
        }

        // --- Calculate show average rating ---
        show.averageRating = Mathf.RoundToInt(ratings.Average());
        data.shows.Add(show);

        if (mode == MatchSimulationMode.Advanced)
        {
            Debug.Log($"Show {show.name} completed with average rating {show.averageRating}");
        }

        return show;
    }
}

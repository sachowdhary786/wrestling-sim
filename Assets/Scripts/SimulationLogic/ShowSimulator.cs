using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ShowSimulator
{
    public static Show SimulateShow(Show show, GameData data)
    {
        List<int> ratings = new List<int>();

        for (int i = 0; i < show.matches.Count; i++)
        {
            show.matches[i] = MatchSimulator.Simulate(show.matches[i], data);
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

        Debug.Log($"Show {show.name} completed with average rating {show.averageRating}");
        return show;
    }
}

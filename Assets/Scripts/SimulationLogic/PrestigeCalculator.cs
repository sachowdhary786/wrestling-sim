using UnityEngine;

/// <summary>
/// Calculates and updates company prestige based on show performance and roster strength.
/// </summary>
public static class PrestigeCalculator
{
    /// <summary>
    /// Updates a company's prestige after a show.
    /// </summary>
    public static void UpdatePrestige(Company company, Show show)
    {
        float totalRating = 0;
        foreach (var match in show.matches)
        {
            totalRating += match.rating;
        }
        float averageRating = totalRating / show.matches.Count;

        // Prestige change is based on the quality of the show relative to the company's current prestige
        float prestigeChange = (averageRating - company.prestige) * 0.1f;

        // Bonus for having a very high-rated match (a "Match of the Year" contender)
        if (show.matches.Exists(m => m.rating >= 95))
        {
            prestigeChange += 2;
        }

        // Cap the change to prevent wild swings
        prestigeChange = Mathf.Clamp(prestigeChange, -5, 5);

        company.prestige = Mathf.Clamp(company.prestige + (int)prestigeChange, 0, 100);
        Debug.Log($"[Prestige] {company.name}'s show had an average rating of {averageRating:F1}. Prestige changed by {(int)prestigeChange}. New Prestige: {company.prestige}");
    }
}

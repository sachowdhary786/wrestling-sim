using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fast match simulation for bulk processing.
/// Skips phases and detailed logging for performance.
/// Use for simulating off-screen matches, tournaments, or season simulations.
/// </summary>
public static class SimpleMatchSimulator
{
    /// <summary>
    /// Simulates a match quickly without phase-by-phase detail
    /// </summary>
    public static Match Simulate(
        Match booking,
        List<Wrestler> wrestlers,
        GameData data,
        (float tech, float brawl, float psych, float aerial) weights
    )
    {
        // Calculate performance scores for all wrestlers
        Dictionary<Wrestler, float> scores = CalculatePerformanceScores(wrestlers, booking, data, weights);

        // Determine winner
        Wrestler winner = DetermineWinner(scores);
        booking.winnerId = winner.id.ToString();

        // Calculate match rating (with referee influence)
        booking.rating = CalculateRating(scores, wrestlers, data, booking);

        // Determine finish type
        string finishType = DetermineFinishType(booking.matchType);
        
        // Apply referee influence on finish
        finishType = RefereeManager.ApplyRefereeInfluence(finishType, booking.referee, booking);
        booking.finishType = finishType;

        // Quick injury check (lower chance than advanced mode)
        CheckForInjuries(wrestlers, booking.matchType, data);

        return booking;
    }

    private static Dictionary<Wrestler, float> CalculatePerformanceScores(
        List<Wrestler> wrestlers,
        Match match,
        GameData data,
        (float tech, float brawl, float psych, float aerial) weights
    )
    {
        Dictionary<Wrestler, float> scores = new Dictionary<Wrestler, float>();

        foreach (var wrestler in wrestlers)
        {
            // Base performance calculation
            float performance = MatchPerformanceCalculator.CalculateBasePerformance(wrestler, weights, match);

            // Apply bonuses
            performance = MatchPerformanceCalculator.ApplyTraitBonuses(wrestler, match, data, performance);
            performance += MatchPerformanceCalculator.GetFeudHeatBonus(wrestler, data);

            // Add some randomness
            performance += UnityEngine.Random.Range(-5f, 5f);

            scores[wrestler] = performance;
        }

        // Apply chemistry (simplified)
        ApplyChemistryModifiers(scores, wrestlers);

        return scores;
    }

    private static void ApplyChemistryModifiers(Dictionary<Wrestler, float> scores, List<Wrestler> wrestlers)
    {
        for (int i = 0; i < wrestlers.Count; i++)
        {
            for (int j = i + 1; j < wrestlers.Count; j++)
            {
                var a = wrestlers[i];
                var b = wrestlers[j];

                int chemistry = 0;
                if (a.friends.Contains(b.id.ToString()))
                    chemistry += 3; // Smaller bonus than advanced mode
                if (a.rivals.Contains(b.id.ToString()))
                    chemistry -= 3;

                if (chemistry != 0)
                {
                    scores[a] += chemistry;
                    scores[b] += chemistry;
                }
            }
        }
    }

    private static Wrestler DetermineWinner(Dictionary<Wrestler, float> scores)
    {
        Wrestler winner = null;
        float highScore = float.MinValue;

        foreach (var kvp in scores)
        {
            float adjusted = kvp.Value + UnityEngine.Random.Range(-8f, 8f);
            if (adjusted > highScore)
            {
                highScore = adjusted;
                winner = kvp.Key;
            }
        }

        return winner;
    }

    private static int CalculateRating(Dictionary<Wrestler, float> scores, List<Wrestler> wrestlers, GameData data, Match match)
    {
        float avgPerformance = 0;
        foreach (float val in scores.Values)
            avgPerformance += val;
        avgPerformance /= wrestlers.Count;

        // Add tag bonus
        int tagBonus = MatchPerformanceCalculator.GetTagChemistryBonus(wrestlers, data);
        avgPerformance += tagBonus;

        // Referee influence
        float refereeBonus = RefereeManager.GetRefereeRatingModifier(match.referee, match);
        avgPerformance += refereeBonus;

        // Simplified rating calculation
        float psychBonus = MatchPerformanceCalculator.AverageStat(wrestlers, w => w.psychology) * 0.15f;
        float popularityBonus = MatchPerformanceCalculator.AverageStat(wrestlers, w => w.popularity) * 0.08f;
        float randomFactor = UnityEngine.Random.Range(-8, 8);

        int rating = Mathf.Clamp(
            Mathf.RoundToInt(avgPerformance * 0.6f + psychBonus + popularityBonus + randomFactor),
            0,
            100
        );

        return rating;
    }

    private static string DetermineFinishType(string matchType)
    {
        // Simplified finish type determination
        string[] finishes = { "Pinfall", "Submission", "Knockout", "Count Out", "DQ" };
        float[] weights = { 65f, 20f, 8f, 4f, 3f };

        // Adjust for match type
        switch (matchType)
        {
            case "Hardcore":
            case "NoDisqualification":
            case "StreetFight":
                weights[2] += 15f; // More knockouts
                weights[4] = 0f;   // No DQs
                break;
            case "Submission":
            case "IQuitMatch":
                weights[1] += 40f; // Much more submissions
                break;
            case "LastManStanding":
                weights[2] += 30f; // Mostly knockouts
                weights[0] = 10f;  // Fewer pinfalls
                break;
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

    private static void CheckForInjuries(List<Wrestler> wrestlers, string matchType, GameData data)
    {
        foreach (var wrestler in wrestlers)
        {
            if (wrestler.injured)
                continue;

            // Reduced injury chance compared to advanced mode (50% of normal rate)
            float baseChance = 0.5f;

            // Match type modifiers (reduced)
            baseChance += matchType switch
            {
                "Hardcore" => 7f,
                "LadderMatch" => 10f,
                "SteelCage" => 5f,
                "TLC" => 12f,
                "HellInACell" => 8f,
                _ => 1f
            };

            // Toughness reduces chance
            baseChance *= (100 - wrestler.toughness) / 100f;

            // Roll for injury
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll < baseChance)
            {
                // Apply injury (simplified - always minor in simple mode)
                wrestler.injured = true;
                wrestler.injurySeverity = 1; // Always minor
                wrestler.injuryType = "minor injury";
                wrestler.recoveryWeeksRemaining = UnityEngine.Random.Range(1, 3);

                // Small stat penalty
                wrestler.stamina = Mathf.Max(0, wrestler.stamina - 3);
            }
        }
    }
}

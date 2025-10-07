using System;
using System.Collections.Generic;
using UnityEngine;

public static class MatchSimulator
{
    // Match-type modifiers
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

    public static Match Simulate(Match booking, GameData data)
    {
        if (booking.participants.Count < 2)
        {
            Debug.LogWarning("Match has less than 2 participants!");
            return booking;
        }

        // Fetch wrestlers
        List<Wrestler> wrestlers = new List<Wrestler>();
        foreach (string id in booking.participants)
        {
            var w = data.wrestlers.Find(x => x.id == id);
            if (w != null)
                wrestlers.Add(w);
        }

        var weights = matchTypeWeights.ContainsKey(booking.matchType)
            ? matchTypeWeights[booking.matchType]
            : matchTypeWeights["Singles"];

        // Calculate base performance
        Dictionary<Wrestler, float> scores = new Dictionary<Wrestler, float>();
        foreach (var w in wrestlers)
        {
            float hometownBonus =
                (booking.location.Contains(w.hometown, StringComparison.OrdinalIgnoreCase))
                    ? 1.05f
                    : 1.0f;
            float formFactor = 1.0f + UnityEngine.Random.Range(-0.05f, 0.05f);

            // Weighted average of skill attributes
            float performance =
                (
                    w.technical * weights.tech
                    + w.brawling * weights.brawl
                    + w.psychology * weights.psych
                    + w.aerial * weights.aerial
                ) / 4.0f;

            performance *= hometownBonus * formFactor;

            performance = ApplyTraitBonuses(w, booking, data, performance);

            performance = GetFeudHeatBonus(w, data);

            scores[w] = performance;
        }

        // Chemistry boost / penalty
        for (int i = 0; i < wrestlers.Count; i++)
        {
            for (int j = i + 1; j < wrestlers.Count; j++)
            {
                var a = wrestlers[i];
                var b = wrestlers[j];

                int chemistry = 0;
                if (a.friends.Contains(b.id))
                    chemistry += 5;
                if (a.rivals.Contains(b.id))
                    chemistry -= 5;

                if (chemistry != 0)
                {
                    scores[a] += chemistry;
                    scores[b] += chemistry;
                }
            }
        }

        // Pick winner based on performance (with randomness)
        Wrestler winner = wrestlers[0];
        float highScore = 0;

        foreach (var kvp in scores)
        {
            float adjusted = kvp.Value + UnityEngine.Random.Range(-10f, 10f);
            if (adjusted > highScore)
            {
                highScore = adjusted;
                winner = kvp.Key;
            }
        }

        booking.winnerId = winner.id;

        // --- Match rating ---
        float avgPerformance = 0;
        foreach (float val in scores.Values)
            avgPerformance += val;

        avgPerformance /= wrestlers.Count;

        int tagBonus = GetTagChemistryBonus(wrestlers, data);
        avgPerformance += tagBonus;

        // Match quality depends on chemistry and psychology
        float psychBonus = AverageStat(wrestlers, w => w.psychology) * 0.2f;
        float popularityBonus = AverageStat(wrestlers, w => w.popularity) * 0.1f;
        float randomFactor = UnityEngine.Random.Range(-10, 10);

        booking.rating = Mathf.Clamp(
            Mathf.RoundToInt(avgPerformance * 0.6f + psychBonus + popularityBonus + randomFactor),
            0,
            100
        );

        return booking;
    }

    private static float AverageStat(List<Wrestler> wrestlers, Func<Wrestler, int> selector)
    {
        float total = 0;
        foreach (var w in wrestlers)
            total += selector(w);
        return total / wrestlers.Count;
    }

    private static int GetTagChemistryBonus(List<Wrestler> wrestlers, GameData data)
    {
        int bonus = 0;
        foreach (var team in data.tagTeams)
        {
            int count = 0;
            foreach (var m in team.members)
                if (wrestlers.Exists(w => w.id == m))
                    count++;

            if (count >= 2)
                bonus += team.chemistry; // Both members present
        }
        return bonus;
    }

    private static float ApplyTraitBonuses(
        Wrestler w,
        Match match,
        GameData data,
        float basePerformance
    )
    {
        foreach (var tId in w.traits)
        {
            var trait = data.traits.Find(t => t.id == tId);
            if (trait == null)
                continue;

            switch (trait.effect)
            {
                case TraitEffect.CrowdFavourite:
                    if (match.location.Contains(w.hometown, StringComparison.OrdinalIgnoreCase))
                        basePerformance *= 1.05f;
                    break;

                case TraitEffect.HardcoreSpecialist:
                    if (match.matchType == "Hardcore")
                        basePerformance += 10f;
                    break;

                case TraitEffect.SubmissionExpert:
                    basePerformance += UnityEngine.Random.Range(0, 10f);
                    break;

                case TraitEffect.BigMatchPerformer:
                    if (match.titleMatch)
                        basePerformance *= 1.05f;
                    break;

                case TraitEffect.LazyWorker:
                    basePerformance *= UnityEngine.Random.value > 0.7f ? 0.9f : 1f;
                    break;

                case TraitEffect.ChemistryMaster:
                    basePerformance += 5f;
                    break;
            }
        }

        return basePerformance;
    }

    private static int GetFeudHeatBonus(List<Wrestler> wrestlers, GameData data)
    {
        int maxHeat = 0;
        foreach (var feud in data.feuds)
        {
            if (!feud.active)
                continue;

            int matchCount = 0;
            foreach (var p in feud.participants)
                if (wrestlers.Exists(w => w.id == p))
                    matchCount++;

            if (matchCount >= 2)
                maxHeat = Mathf.Max(maxHeat, feud.heat);
        }
        return maxHeat / 10; // e.g. 80 heat = +8 bonus
    }
}

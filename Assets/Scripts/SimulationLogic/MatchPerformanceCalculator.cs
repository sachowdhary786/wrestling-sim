using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all performance and rating calculations for matches
/// </summary>
public static class MatchPerformanceCalculator
{
    public static float CalculateBasePerformance(
        Wrestler wrestler,
        WrestlerStats tempStats,
        (float tech, float brawl, float psych, float aerial) weights,
        Match match
    )
    {
        float hometownBonus = match.location.Contains(wrestler.hometown, StringComparison.OrdinalIgnoreCase)
            ? 1.05f
            : 1.0f;
        float formFactor = 1.0f + UnityEngine.Random.Range(-0.05f, 0.05f);

        // Weighted average of skill attributes using temporary, morale-adjusted stats
        float performance =
            (
                tempStats.Technical * weights.tech
                + tempStats.Brawling * weights.brawl
                + tempStats.Psychology * weights.psych
                + tempStats.Aerial * weights.aerial
            ) / 4.0f;

        performance *= hometownBonus * formFactor;

        return performance;
    }

    public static float ApplyTraitBonuses(
        Wrestler wrestler,
        Match match,
        GameData data,
        float basePerformance
    )
    {
        foreach (var traitId in wrestler.traits)
        {
            var trait = data.traits.Find(t => t.id == traitId);
            if (trait == null)
                continue;

            switch (trait.effect)
            {
                case TraitEffect.CrowdFavourite:
                    if (match.location.Contains(wrestler.hometown, StringComparison.OrdinalIgnoreCase))
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

    public static int GetFeudHeatBonus(Wrestler wrestler, GameData data)
    {
        int maxHeat = 0;
        foreach (var feud in data.feuds)
        {
            if (!feud.active)
                continue;

            bool wrestlerInFeud = feud.participants.Contains(wrestler.id.ToString());
            if (wrestlerInFeud)
            {
                maxHeat = Mathf.Max(maxHeat, feud.heat);
            }
        }
        return maxHeat / 10; // e.g. 80 heat = +8 bonus
    }

    public static void ApplyChemistryModifiers(MatchState state)
    {
        for (int i = 0; i < state.wrestlers.Count; i++)
        {
            for (int j = i + 1; j < state.wrestlers.Count; j++)
            {
                var a = state.wrestlers[i];
                var b = state.wrestlers[j];

                int chemistry = 0;
                if (a.friends.Contains(b.id))
                    chemistry += 5;
                if (a.rivals.Contains(b.id))
                    chemistry -= 5;

                if (chemistry != 0)
                {
                    state.scores[a] += chemistry;
                    state.scores[b] += chemistry;
                    Debug.Log($"  Chemistry: {a.name} & {b.name} ({(chemistry > 0 ? "positive" : "negative")})");
                }
            }
        }
    }

    public static int GetTagChemistryBonus(List<Wrestler> wrestlers, GameData data)
    {
        int bonus = 0;
        foreach (var team in data.teams)
        {
            int count = 0;
            foreach (var memberId in team.members)
            {
                if (wrestlers.Exists(w => w.id == memberId))
                    count++;
            }

            if (count >= 2)
                bonus += team.teamwork;
        }
        return bonus;
    }

    public static int CalculateMatchRating(MatchState state)
    {
        float roadAgentBonus = 0;
        if (state.Booking.roadAgentId.HasValue)
        {
            var roadAgent = state.Data.wrestlers.FirstOrDefault(w => w.id == state.Booking.roadAgentId.Value);
            var roadAgentStaffInfo = state.Company.corporateStaff.FirstOrDefault(s => s.staffId == state.Booking.roadAgentId.Value);
            if (roadAgent != null && roadAgentStaffInfo != null)
            {
                // Agent's psychology influence contributes to the match story quality
                roadAgentBonus = roadAgentStaffInfo.psychologyInfluence / 10f; // Adds up to 10 points
                Debug.Log($"  Road Agent {roadAgent.name} bonus: +{roadAgentBonus:F1}");
            }
        }

        float managerBonus = 0;
        foreach (var entry in state.Booking.managers)
        {
            var manager = state.Data.wrestlers.FirstOrDefault(w => w.id == entry.Value);
            if (manager != null)
            {
                // Manager's charisma and mic skill contribute to the match hype
                managerBonus += (manager.charisma + manager.micSkill) / 20f; // Each manager adds up to 10 points
            }
        }

        float avgPerformance = 0;
        foreach (float val in state.scores.Values)
            avgPerformance += val;
        avgPerformance /= state.wrestlers.Count;

        int tagBonus = GetTagChemistryBonus(state.wrestlers, state.data);
        avgPerformance += tagBonus;

        float psychBonus = AverageStat(state.wrestlerStats.Values.ToList(), s => s.Psychology) * 0.2f;
        float popularityBonus = AverageStat(state.wrestlers, w => w.popularity) * 0.1f;
        float randomFactor = UnityEngine.Random.Range(-10, 10);

        // Referee influence on rating
        float refereeBonus = RefereeManager.GetRefereeRatingModifier(state.match.referee, state.match);
        avgPerformance += refereeBonus;

        if (state.match.referee != null && refereeBonus != 0)
        {
            Debug.Log($"  Referee {state.match.referee.name} modifier: {refereeBonus:+0.0;-0.0}");
        }

        int rating = Mathf.Clamp(
            Mathf.RoundToInt(avgPerformance * 0.6f + psychBonus + popularityBonus + randomFactor + managerBonus + roadAgentBonus + state.BookingModifier),
            0,
            100
        );

        Debug.Log($"  Final Match Rating: {rating}/100");
        return rating;
    }

    public static float AverageStat(List<WrestlerStats> stats, Func<WrestlerStats, int> selector)
    {
        float total = 0;
        foreach (var s in stats)
            total += selector(s);
        return total / stats.Count;
    }

    public static float AverageStat(List<Wrestler> wrestlers, Func<Wrestler, int> selector)
    {
        float total = 0;
        foreach (var w in wrestlers)
            total += selector(w);
        return total / wrestlers.Count;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles special referee events and incidents during matches
/// </summary>
public static class RefereeEventSystem
{
    /// <summary>
    /// Possible referee events during a match
    /// </summary>
    public enum RefereeEvent
    {
        None,
        RefereeBump,          // Referee accidentally knocked down
        RefereeKnockedOut,    // Referee knocked unconscious
        SlowCount,            // Referee counts slowly
        FastCount,            // Referee counts quickly (corruption)
        MissedCall,           // Referee misses an obvious rule break
        WrongCall,            // Referee makes incorrect decision
        RefusesDQ,            // Referee refuses to DQ despite rules
        EarlyStop,            // Referee stops match prematurely
        ArgumentWithWrestler, // Referee argues with wrestler
        EjectedFromMatch,     // Referee ejects manager/wrestler
        ReplacementNeeded     // Referee injured, needs replacement
    }

    /// <summary>
    /// Data for a referee event
    /// </summary>
    [Serializable]
    public class RefereeEventData
    {
        public RefereeEvent eventType;
        public string description;
        public float impactOnRating; // Can be positive or negative
        public bool requiresReplacement;
        public int matchPhase; // 1=Opening, 2=Mid, 3=Climax

        public RefereeEventData(RefereeEvent type, string desc, float impact, int phase = 2)
        {
            eventType = type;
            description = desc;
            impactOnRating = impact;
            matchPhase = phase;
            requiresReplacement = type == RefereeEvent.RefereeKnockedOut || type == RefereeEvent.ReplacementNeeded;
        }
    }

    /// <summary>
    /// Checks for potential referee events during a match phase
    /// </summary>
    public static RefereeEventData CheckForEvent(Referee referee, Match match, int phase, MatchState state = null)
    {
        if (referee == null)
            return null;

        float eventChance = CalculateEventChance(referee, match, phase);
        float roll = UnityEngine.Random.Range(0f, 100f);

        if (roll < eventChance)
        {
            return GenerateEvent(referee, match, phase, state);
        }

        return null;
    }

    /// <summary>
    /// Applies the effects of a referee event
    /// </summary>
    public static void ApplyEvent(RefereeEventData eventData, Referee referee, Match match, MatchState state = null)
    {
        if (eventData == null)
            return;

        Debug.Log($"🎬 REFEREE EVENT: {eventData.description}");

        // Apply rating impact
        if (state != null)
        {
            foreach (var wrestler in state.wrestlers)
            {
                state.scores[wrestler] += eventData.impactOnRating;
            }
        }

        // Handle specific events
        switch (eventData.eventType)
        {
            case RefereeEvent.RefereeBump:
                HandleRefereeBump(referee, match);
                break;

            case RefereeEvent.RefereeKnockedOut:
                HandleRefereeKnockout(referee, match);
                break;

            case RefereeEvent.FastCount:
                HandleFastCount(referee, match);
                break;

            case RefereeEvent.SlowCount:
                HandleSlowCount(referee, match);
                break;

            case RefereeEvent.MissedCall:
                HandleMissedCall(referee, match);
                break;

            case RefereeEvent.ArgumentWithWrestler:
                HandleArgument(referee, match, state);
                break;
        }
    }

    /// <summary>
    /// Gets all possible events for a referee
    /// </summary>
    public static List<RefereeEventData> GetPossibleEvents(Referee referee, Match match)
    {
        List<RefereeEventData> events = new List<RefereeEventData>();

        // Referee Bump - can happen to anyone
        events.Add(new RefereeEventData(
            RefereeEvent.RefereeBump,
            $"{referee.name} gets bumped by a wrestler!",
            -2f,
            2
        ));

        // Knockout - rare but dramatic
        events.Add(new RefereeEventData(
            RefereeEvent.RefereeKnockedOut,
            $"{referee.name} is knocked out cold!",
            -5f,
            3
        ));

        // Corruption-based events
        if (referee.corruption > 50)
        {
            events.Add(new RefereeEventData(
                RefereeEvent.FastCount,
                $"{referee.name} counts suspiciously fast!",
                -3f,
                3
            ));

            events.Add(new RefereeEventData(
                RefereeEvent.MissedCall,
                $"{referee.name} conveniently misses an obvious rule break!",
                -2f,
                2
            ));
        }

        // Low consistency events
        if (referee.consistency < 50)
        {
            events.Add(new RefereeEventData(
                RefereeEvent.WrongCall,
                $"{referee.name} makes a questionable call!",
                -3f,
                2
            ));

            events.Add(new RefereeEventData(
                RefereeEvent.SlowCount,
                $"{referee.name} counts very slowly...",
                -1f,
                3
            ));
        }

        // High strictness events
        if (referee.strictness > 70)
        {
            events.Add(new RefereeEventData(
                RefereeEvent.ArgumentWithWrestler,
                $"{referee.name} argues with a wrestler about the rules!",
                0f,
                2
            ));

            events.Add(new RefereeEventData(
                RefereeEvent.EjectedFromMatch,
                $"{referee.name} ejects someone from ringside!",
                1f,
                2
            ));
        }

        return events;
    }

    // Private helper methods

    private static float CalculateEventChance(Referee referee, Match match, int phase)
    {
        float baseChance = 5f; // 5% base chance

        // Phase modifiers
        if (phase == 3) // Climax
            baseChance *= 1.5f; // More likely during finish

        // Referee factors
        if (referee.consistency < 50)
            baseChance += (50 - referee.consistency) * 0.1f;

        if (referee.corruption > 60)
            baseChance += (referee.corruption - 60) * 0.1f;

        if (referee.fatigue > 70)
            baseChance += (referee.fatigue - 70) * 0.2f;

        // Match type modifiers
        if (IsHighRiskMatch(match.matchType))
            baseChance *= 2f;

        return Mathf.Min(baseChance, 30f); // Cap at 30%
    }

    private static RefereeEventData GenerateEvent(Referee referee, Match match, int phase, MatchState state)
    {
        var possibleEvents = GetPossibleEvents(referee, match);
        
        // Filter by phase
        possibleEvents = possibleEvents.FindAll(e => e.matchPhase == phase || e.matchPhase == 0);

        if (possibleEvents.Count == 0)
            return null;

        // Weight events by referee stats
        List<float> weights = new List<float>();
        foreach (var evt in possibleEvents)
        {
            float weight = 1f;

            // Corruption events more likely for corrupt refs
            if ((evt.eventType == RefereeEvent.FastCount || evt.eventType == RefereeEvent.MissedCall) 
                && referee.corruption > 60)
                weight = referee.corruption / 50f;

            // Consistency events more likely for inconsistent refs
            if ((evt.eventType == RefereeEvent.WrongCall || evt.eventType == RefereeEvent.SlowCount)
                && referee.consistency < 50)
                weight = (100 - referee.consistency) / 50f;

            weights.Add(weight);
        }

        // Weighted random selection
        float totalWeight = 0;
        foreach (float w in weights)
            totalWeight += w;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0;

        for (int i = 0; i < possibleEvents.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return possibleEvents[i];
        }

        return possibleEvents[0];
    }

    private static void HandleRefereeBump(Referee referee, Match match)
    {
        // Referee temporarily down, creates opportunity for shenanigans
        referee.stats.timesBumped++;
        
        // Small chance of injury from bump
        if (UnityEngine.Random.value < 0.05f) // 5%
        {
            referee.isInjured = true;
            referee.injuryWeeksRemaining = 1;
            Debug.Log($"⚠️ {referee.name} injured from the bump!");
        }
    }

    private static void HandleRefereeKnockout(Referee referee, Match match)
    {
        // Referee knocked out, needs replacement
        referee.stats.timesKnockedOut++;
        
        // Always causes injury
        referee.isInjured = true;
        referee.injuryWeeksRemaining = UnityEngine.Random.Range(1, 4);
        
        Debug.Log($"⚠️ {referee.name} is knocked out! Replacement referee needed!");
        Debug.Log($"⚠️ {referee.name} will be out for {referee.injuryWeeksRemaining} weeks.");
    }

    private static void HandleFastCount(Referee referee, Match match)
    {
        // Corrupt referee helps a wrestler
        referee.stats.controversies++;
        Debug.Log($"⚠️ Controversy! {referee.name}'s fast count raises questions!");
    }

    private static void HandleSlowCount(Referee referee, Match match)
    {
        // Inconsistent referee counts slowly
        Debug.Log($"🤔 {referee.name} seems to be counting in slow motion...");
    }

    private static void HandleMissedCall(Referee referee, Match match)
    {
        // Referee misses an obvious rule break
        referee.stats.controversies++;
        Debug.Log($"⚠️ How did {referee.name} miss that?!");
    }

    private static void HandleArgument(Referee referee, Match match, MatchState state)
    {
        // Referee argues with wrestler
        if (state != null && state.wrestlers.Count > 0)
        {
            var wrestler = state.wrestlers[UnityEngine.Random.Range(0, state.wrestlers.Count)];
            Debug.Log($"🗣️ {referee.name} and {wrestler.name} are having words!");
        }
    }

    private static bool IsHighRiskMatch(string matchType)
    {
        return matchType switch
        {
            "Hardcore" => true,
            "NoDisqualification" => true,
            "StreetFight" => true,
            "LadderMatch" => true,
            "TLC" => true,
            "HellInACell" => true,
            "Cage" => true,
            _ => false
        };
    }
}

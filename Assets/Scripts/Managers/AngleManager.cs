using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles the triggering and execution of post-match angles and storylines.
/// </summary>
public static class AngleManager
{
    /// <summary>
    /// Determines if a post-match angle should occur and, if so, generates and applies it.
    /// </summary>
    public static void TryTriggerPostMatchAngle(MatchState state, Wrestler winner, GameData gameData)
    {
        // Base chance for an angle to occur.
        float angleChance = 15f;

        // Increase chance based on match context.
        if (state.match.titleMatch) angleChance += 10f;
        if (state.match.finishType == FinishType.ControversialFinish) angleChance += 25f;
        if (state.match.finishType == FinishType.DQ) angleChance += 20f;

        // High match rating can lead to respect angles.
        if (state.match.rating >= 85) angleChance += 10f;

        // If a random roll fails, no angle occurs.
        if (UnityEngine.Random.Range(0f, 100f) > angleChance)
        {
            return;
        }

        Debug.Log("[AngleManager] A post-match angle is triggered!");
        GenerateAndApplyAngle(state, winner, gameData);
    }

    private static void GenerateAndApplyAngle(MatchState state, Wrestler winner, GameData gameData)
    {
        var losers = state.wrestlers.Where(w => w.id != winner.id).ToList();

        // High-rated match respect angle
        if (state.match.rating >= 90 && state.match.finishType == FinishType.Pinfall && losers.Count > 0)
        {
            if (UnityEngine.Random.value < 0.4f) // 40% chance
            {
                Debug.Log($"[AngleManager] What a match! {winner.name} and {losers[0].name} are showing each other respect!");
                // Future: Could lead to a new tag team or alliance.
                return;
            }
        }

        // Turn angle in tag matches
        if (state.match.matchType.Contains("Tag") && losers.Count > 0)
        {
            var partner = state.wrestlers.FirstOrDefault(w => winner.friends.Contains(w.id));
            if (partner != null && UnityEngine.Random.value < 0.2f) // 20% chance of turn
            {
                Debug.Log($"[AngleManager] Unbelievable! {winner.name} has turned on their partner, {partner.name}!");
                StartNewFeud(winner, partner, gameData);
                // Future: Implement alignment change (Heel/Face turn).
                return;
            }
        }

        // Sore loser attack (default angle)
        if (losers.Count > 0)
        {
            Wrestler attacker = losers[UnityEngine.Random.Range(0, losers.Count)];
            if (!attacker.friends.Contains(winner.id))
            {
                Debug.Log($"[AngleManager] {attacker.name} is not taking the loss well! They are attacking {winner.name}!");
                StartNewFeud(attacker, winner, gameData);
                return;
            }
        }

        // Post-match promo by a charismatic winner
        if (winner.charisma > 80 && UnityEngine.Random.value < 0.3f)
        {
            Debug.Log($"[AngleManager] {winner.name} gets on the mic to address the crowd after their big win!");
            // Future: Could lead to calling out a champion or another rival.
        }
    }

    private static void StartNewFeud(Wrestler wrestler1, Wrestler wrestler2, GameData gameData)
    {
        // Check if a feud between these participants already exists.
        if (gameData.feuds.Any(f => f.participants.Contains(wrestler1.id) && f.participants.Contains(wrestler2.id)))
        {
            Debug.Log($"[AngleManager] Feud between {wrestler1.name} and {wrestler2.name} already exists. Escalating instead.");
            // In the future, we could increase the feud's heat here.
            return;
        }

        var newFeud = new Feud
        {
            id = System.Guid.NewGuid(),
            participants = new List<System.Guid> { wrestler1.id, wrestler2.id },
            heat = 40, // Starting heat
            durationWeeks = 0,
            active = true
        };

        gameData.feuds.Add(newFeud);
        Debug.Log($"[AngleManager] A new feud has begun between {wrestler1.name} and {wrestler2.name}!");
    }
}

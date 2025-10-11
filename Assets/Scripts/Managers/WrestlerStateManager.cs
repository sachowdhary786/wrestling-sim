using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages wrestler states: fatigue, morale, popularity, and momentum
/// </summary>
public static class WrestlerStateManager
{
    // ========================================
    // FATIGUE MANAGEMENT
    // ========================================
    
    /// <summary>
    /// Updates wrestler fatigue after a match
    /// </summary>
    public static void ApplyMatchFatigue(Wrestler wrestler, Match match, GameData data)
    {
        if (wrestler == null) return;
        
        int fatigueGain = CalculateMatchFatigue(wrestler, match);
        wrestler.fatigue = Mathf.Clamp(wrestler.fatigue + fatigueGain, 0, 100);
        wrestler.matchesThisWeek++;
        wrestler.matchesThisMonth++;
        wrestler.daysRestSinceLastMatch = 0;
        
        if (fatigueGain > 15)
        {
            Debug.Log($"[STATE] {wrestler.name} is getting fatigued ({wrestler.fatigue}/100)");
        }
    }
    
    private static int CalculateMatchFatigue(Wrestler wrestler, Match match)
    {
        int baseFatigue = 15;
        
        // Match type modifiers
        baseFatigue += match.matchType switch
        {
            "Hardcore" => 20,
            "LadderMatch" => 25,
            "TLC" => 30,
            "HellInACell" => 25,
            "IronMan" => 35,
            _ => 0
        };
        
        // Main event adds fatigue
        if (match.titleMatch)
            baseFatigue += 5;
        
        // Stamina reduces fatigue
        baseFatigue -= (wrestler.stamina / 10);
        
        // Already fatigued wrestlers gain more
        if (wrestler.fatigue > 60)
            baseFatigue += 10;
        
        return Mathf.Max(baseFatigue, 5); // Minimum 5 fatigue
    }
    
    /// <summary>
    /// Recovers wrestler fatigue (call daily/weekly)
    /// </summary>
    public static void RecoverFatigue(Wrestler wrestler, int days = 1)
    {
        if (wrestler == null) return;
        
        int recovery = CalculateFatigueRecovery(wrestler, days);
        wrestler.fatigue = Mathf.Clamp(wrestler.fatigue - recovery, 0, 100);
        wrestler.daysRestSinceLastMatch += days;
    }
    
    private static int CalculateFatigueRecovery(Wrestler wrestler, int days)
    {
        int baseRecovery = 10 * days; // 10 per day
        
        // High stamina recovers faster
        baseRecovery += (wrestler.stamina / 20) * days;
        
        // Injured wrestlers recover slower
        if (wrestler.injured)
            baseRecovery /= 2;
        
        return baseRecovery;
    }

    // ========================================
    // MORALE MANAGEMENT
    // ========================================
    
    /// <summary>
    /// Updates morale based on match result and booking
    /// </summary>
    public static void UpdateMoraleAfterMatch(Wrestler wrestler, Match match, bool won, GameData data)
    {
        if (wrestler == null) return;
        
        int moraleChange = 0;
        
        // Winning boosts morale
        if (won)
        {
            moraleChange += match.titleMatch ? 10 : 5;
            
            // Winning main events is big
            if (match.rating > 80)
                moraleChange += 3;
        }
        else
        {
            // Losing hurts, but not as much
            moraleChange -= match.titleMatch ? 5 : 2;
            
            // Poor rating hurts more
            if (match.rating < 50)
                moraleChange -= 3;
        }
        
        // High fatigue reduces morale
        if (wrestler.fatigue > 70)
            moraleChange -= 2;
        
        // Apply change
        wrestler.morale = Mathf.Clamp(wrestler.morale + moraleChange, 0, 100);
        
        if (moraleChange != 0)
        {
            string direction = moraleChange > 0 ? "improved" : "decreased";
            Debug.Log($"[STATE] {wrestler.name}'s morale {direction} to {wrestler.morale}/100");
        }
    }
    
    /// <summary>
    /// Daily morale drift (call regularly)
    /// </summary>
    public static void UpdateDailyMorale(Wrestler wrestler)
    {
        if (wrestler == null) return;
        
        // Morale trends toward 70 (default "content" state)
        int targetMorale = 70;
        int difference = targetMorale - wrestler.morale;
        int change = Mathf.RoundToInt(difference * 0.05f); // 5% drift per day
        
        wrestler.morale = Mathf.Clamp(wrestler.morale + change, 0, 100);
    }

    // ========================================
    // MOMENTUM MANAGEMENT
    // ========================================
    
    /// <summary>
    /// Updates momentum based on booking strength
    /// </summary>
    public static void UpdateMomentum(Wrestler wrestler, Match match, bool won, GameData data)
    {
        if (wrestler == null) return;
        
        int momentumChange = 0;
        
        // Winning builds momentum
        if (won)
        {
            if (match.titleMatch)
                momentumChange += 20; // Big boost for title wins
            else if (match.rating > 80)
                momentumChange += 15; // Great match
            else if (match.rating > 70)
                momentumChange += 10;
            else
                momentumChange += 5;
        }
        else
        {
            // Losing costs momentum
            if (match.titleMatch)
                momentumChange -= 10;
            else
                momentumChange -= 5;
        }
        
        // Clean finishes matter more
        if (match.finishType == "Pinfall" || match.finishType == "Submission")
        {
            momentumChange = Mathf.RoundToInt(momentumChange * 1.2f);
        }
        
        // Apply change
        wrestler.momentum = Mathf.Clamp(wrestler.momentum + momentumChange, -100, 100);
        
        if (Mathf.Abs(momentumChange) > 0)
        {
            string status = wrestler.momentum > 50 ? "hot" : wrestler.momentum < -50 ? "cold" : "neutral";
            Debug.Log($"[STATE] {wrestler.name}'s momentum: {wrestler.momentum}/100 ({status})");
        }
    }
    
    /// <summary>
    /// Momentum decays over time if not used
    /// </summary>
    public static void DecayMomentum(Wrestler wrestler, int days = 7)
    {
        if (wrestler == null) return;
        
        // Momentum decays toward 0
        int decayAmount = Mathf.RoundToInt(wrestler.momentum * 0.1f * days); // 10% per week
        wrestler.momentum = Mathf.Clamp(wrestler.momentum - Mathf.Sign(wrestler.momentum) * Mathf.Abs(decayAmount), -100, 100);
    }

    // ========================================
    // POPULARITY MANAGEMENT
    // ========================================
    
    /// <summary>
    /// Updates popularity based on match performance
    /// </summary>
    public static void UpdatePopularity(Wrestler wrestler, Match match, GameData data)
    {
        if (wrestler == null) return;
        
        int popularityChange = 0;
        
        // High-rated matches boost popularity
        if (match.rating >= 90)
            popularityChange += 3;
        else if (match.rating >= 80)
            popularityChange += 2;
        else if (match.rating >= 70)
            popularityChange += 1;
        else if (match.rating < 50)
            popularityChange -= 1;
        
        // Title matches get more exposure
        if (match.titleMatch)
            popularityChange += 1;
        
        // Charismatic wrestlers gain popularity faster
        if (wrestler.charisma > 80)
            popularityChange = Mathf.RoundToInt(popularityChange * 1.5f);
        
        wrestler.popularity = Mathf.Clamp(wrestler.popularity + popularityChange, 0, 100);
        
        if (popularityChange > 0)
        {
            Debug.Log($"[STATE] {wrestler.name}'s popularity increased to {wrestler.popularity}/100");
        }
    }

    // ========================================
    // BATCH UPDATES
    // ========================================
    
    /// <summary>
    /// Updates all wrestler states after a match
    /// </summary>
    public static void UpdateWrestlerAfterMatch(Wrestler wrestler, Match match, bool won, GameData data)
    {
        ApplyMatchFatigue(wrestler, match, data);
        UpdateMoraleAfterMatch(wrestler, match, won, data);
        UpdateMomentum(wrestler, match, won, data);
        UpdatePopularity(wrestler, match, data);
    }
    
    /// <summary>
    /// Weekly reset for all wrestlers
    /// </summary>
    public static void WeeklyReset(GameData data)
    {
        Debug.Log("[STATE] Running weekly wrestler state updates...");
        
        foreach (var wrestler in data.wrestlers)
        {
            // Reset weekly counters
            wrestler.matchesThisWeek = 0;
            
            // Recover fatigue
            RecoverFatigue(wrestler, 7);
            
            // Update morale
            UpdateDailyMorale(wrestler);
            
            // Decay momentum
            if (wrestler.matchesThisWeek == 0)
            {
                DecayMomentum(wrestler, 7);
            }
            
            // Increase days rest
            wrestler.daysRestSinceLastMatch += 7;
        }
        
        Debug.Log("[STATE] Weekly reset complete");
    }
    
    /// <summary>
    /// Monthly reset for all wrestlers
    /// </summary>
    public static void MonthlyReset(GameData data)
    {
        Debug.Log("[STATE] Running monthly wrestler state updates...");
        
        foreach (var wrestler in data.wrestlers)
        {
            wrestler.matchesThisMonth = 0;
        }
        
        Debug.Log("[STATE] Monthly reset complete");
    }

    // ========================================
    // QUERY METHODS
    // ========================================
    
    /// <summary>
    /// Gets wrestler's overall condition rating
    /// </summary>
    public static int GetConditionRating(Wrestler wrestler)
    {
        if (wrestler == null) return 0;
        
        // Average of inverse fatigue, morale, and normalized momentum
        int inverseFatigue = 100 - wrestler.fatigue;
        int normalizedMomentum = (wrestler.momentum + 100) / 2; // Convert -100/100 to 0-100
        
        return (inverseFatigue + wrestler.morale + normalizedMomentum) / 3;
    }
    
    /// <summary>
    /// Checks if wrestler should be rested
    /// </summary>
    public static bool ShouldRest(Wrestler wrestler)
    {
        if (wrestler == null) return true;
        
        return wrestler.fatigue > 80 
            || wrestler.morale < 30 
            || wrestler.matchesThisWeek >= 3;
    }
    
    /// <summary>
    /// Gets wrestler's push level based on momentum and popularity
    /// </summary>
    public static string GetPushLevel(Wrestler wrestler)
    {
        if (wrestler == null) return "None";
        
        int pushScore = wrestler.popularity + (wrestler.momentum / 2);
        
        if (pushScore >= 120) return "Mega Push";
        if (pushScore >= 100) return "Strong Push";
        if (pushScore >= 80) return "Mid Push";
        if (pushScore >= 60) return "Neutral";
        if (pushScore >= 40) return "Cooling Off";
        return "Buried";
    }
    
    /// <summary>
    /// Formats wrestler state for debugging
    /// </summary>
    public static string GetStateReport(Wrestler wrestler)
    {
        if (wrestler == null) return "No wrestler";
        
        return $"{wrestler.name}: " +
               $"Fatigue {wrestler.fatigue}/100, " +
               $"Morale {wrestler.morale}/100, " +
               $"Popularity {wrestler.popularity}/100, " +
               $"Momentum {wrestler.momentum:+0;-0}, " +
               $"Matches: {wrestler.matchesThisWeek}/week, " +
               $"Rest: {wrestler.daysRestSinceLastMatch} days, " +
               $"Push: {GetPushLevel(wrestler)}";
    }
}

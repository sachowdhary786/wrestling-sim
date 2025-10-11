using UnityEngine;

/// <summary>
/// Manages all wrestler morale logic, including updates, stat effects, and event hooks.
/// </summary>
public static class MoraleManager
{
    private const float HIGH_MORALE_THRESHOLD = 80f;
    private const float LOW_MORALE_THRESHOLD = 30f;
    private const float STAT_MODIFIER_HIGH = 1.05f; // 5% boost
    private const float STAT_MODIFIER_LOW = 0.95f;  // 5% penalty

    /// <summary>
    /// Updates a wrestler's morale based on the outcome of a match.
    /// </summary>
    public static void UpdateMoraleAfterMatch(Wrestler wrestler, Match match, bool won)
    {
        int moraleChange = 0;

        if (won)
        {
            moraleChange += (match.isMainEvent || match.titleMatch) ? 10 : 5;
        }
        else
        {
            // Losing clean hurts more
            if (match.finishType == FinishType.Clean)
            {
                moraleChange -= 10;
            }
            else
            {
                moraleChange -= 5; // Protected loss
            }

            // Being squashed is demoralizing
            if (match.matchAim == MatchAim.Squash)
            {
                moraleChange -= 15;
            }
        }

        wrestler.morale = Mathf.Clamp(wrestler.morale + moraleChange, 0, 100);
        Debug.Log($"[Morale] {wrestler.name}'s morale changed by {moraleChange}. New morale: {wrestler.morale}");
    }

    /// <summary>
    /// Applies a weekly decay and handles morale changes from lack of booking.
    /// </summary>
    public static void ProcessWeeklyMoraleChanges(Wrestler wrestler, bool wasBooked)
    {
        int moraleChange = -1; // Base weekly decay

        if (!wasBooked)
        {
            moraleChange -= 2; // Penalty for not being used
        }

        wrestler.morale = Mathf.Clamp(wrestler.morale + moraleChange, 0, 100);
    }

    /// <summary>
    /// Gets a temporary, morale-adjusted copy of a wrestler's stats for use in a single match.
    /// This does not permanently alter the wrestler's base stats.
    /// </summary>
    public static WrestlerStats GetMoraleAdjustedStats(Wrestler wrestler)
    {
        var stats = new WrestlerStats(wrestler); // Create a copy of current stats

        if (wrestler.morale >= HIGH_MORALE_THRESHOLD)
        {
            stats.Technical = Mathf.RoundToInt(stats.Technical * STAT_MODIFIER_HIGH);
            stats.Brawling = Mathf.RoundToInt(stats.Brawling * STAT_MODIFIER_HIGH);
            stats.Psychology = Mathf.RoundToInt(stats.Psychology * STAT_MODIFIER_HIGH);
        }
        else if (wrestler.morale <= LOW_MORALE_THRESHOLD)
        {
            stats.Technical = Mathf.RoundToInt(stats.Technical * STAT_MODIFIER_LOW);
            stats.Brawling = Mathf.RoundToInt(stats.Brawling * STAT_MODIFIER_LOW);
            stats.Psychology = Mathf.RoundToInt(stats.Psychology * STAT_MODIFIER_LOW);
        }

        return stats;
    }

    /// <summary>
    /// Hook for future systems. Applies a one-time morale bonus or penalty from an event.
    /// </summary>
    /// <param name="wrestler">The wrestler to affect.</param>
    /// <param name="amount">The amount to change morale by (can be negative).</param>
    /// <param name="reason">The reason for the change, for logging.</param>
    public static void ApplyEventBonus(Wrestler wrestler, int amount, string reason)
    {
        wrestler.morale = Mathf.Clamp(wrestler.morale + amount, 0, 100);
        Debug.Log($"[Morale Event] {wrestler.name}'s morale changed by {amount} due to: {reason}. New morale: {wrestler.morale}");
    }
}

/// <summary>
/// A temporary struct to hold a wrestler's stats for a single match simulation.
/// </summary>
public struct WrestlerStats
{
    public int Technical;
    public int Brawling;
    public int Aerial;
    public int Psychology;
    public int Charisma;
    public int Stamina;
    public int Toughness;

    public WrestlerStats(Wrestler w)
    {
        Technical = w.technical;
        Brawling = w.brawling;
        Aerial = w.aerial;
        Psychology = w.psychology;
        Charisma = w.charisma;
        Stamina = w.stamina;
        Toughness = w.toughness;
    }
}

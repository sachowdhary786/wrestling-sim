using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages referee scheduling and workload distribution
/// </summary>
public static class RefereeScheduler
{
    /// <summary>
    /// Assigns referees to all matches in a show
    /// </summary>
    public static void AssignRefereesToShow(Show show, GameData data)
    {
        if (data.referees == null || data.referees.Count == 0)
        {
            Debug.LogWarning("No referees available!");
            return;
        }

        // Get available referees
        var availableRefs = data.referees.Values
            .Where(r => RefereeCareerManager.CanWorkMatch(r))
            .ToList();

        if (availableRefs.Count == 0)
        {
            Debug.LogWarning("No available referees! Using injured/fatigued refs...");
            availableRefs = data.referees.Values.Where(r => r.isActive && !r.isInjured).ToList();
        }

        // Sort matches by importance
        var sortedMatches = show.matches
            .OrderByDescending(m => GetMatchImportance(m))
            .ToList();

        // Assign refs, trying to balance workload
        Dictionary<Referee, int> refWorkload = new Dictionary<Referee, int>();
        foreach (var r in availableRefs)
            refWorkload[r] = r.matchesThisWeek;

        foreach (var match in sortedMatches)
        {
            if (match.referee != null)
                continue; // Already assigned

            // Find best available ref for this match
            var bestRef = FindBestReferee(match, availableRefs, refWorkload, data);
            
            if (bestRef != null)
            {
                match.referee = bestRef;
                refWorkload[bestRef]++;
                Debug.Log($"Assigned {bestRef.name} to {match.matchType} match");
            }
            else
            {
                Debug.LogWarning($"Could not assign referee to {match.matchType} match!");
            }
        }
    }

    /// <summary>
    /// Finds the best referee for a specific match
    /// </summary>
    public static Referee FindBestReferee(Match match, List<Referee> availableRefs, Dictionary<Referee, int> workload, GameData data)
    {
        if (availableRefs.Count == 0)
            return null;

        // Score each referee
        Dictionary<Referee, float> scores = new Dictionary<Referee, float>();

        foreach (var referee in availableRefs)
        {
            float score = CalculateRefereeMatchScore(referee, match, workload[referee]);
            scores[referee] = score;
        }

        // Return highest scoring ref
        return scores.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// Calculates how suitable a referee is for a match
    /// </summary>
    private static float CalculateRefereeMatchScore(Referee referee, Match match, int currentWorkload)
    {
        float score = 50f; // Base score

        // Experience match importance
        if (match.titleMatch)
        {
            if (referee.isMainEventRef)
                score += 30f;
            else if (referee.experience >= 70)
                score += 15f;
            else
                score -= 10f; // Penalize inexperienced refs for title matches
        }

        // Match type suitability
        if (IsHardcoreMatch(match.matchType))
        {
            if (referee.isHardcoreSpecialist)
                score += 25f;
            else
                score -= 5f;
        }

        // Quality rating
        score += referee.GetQualityRating() * 0.3f;

        // Workload penalty (prefer less worked refs)
        score -= currentWorkload * 5f;

        // Fatigue penalty
        score -= referee.fatigue * 0.2f;

        // Effectiveness
        score *= RefereeCareerManager.GetEffectiveness(referee);

        // Recent form
        string form = referee.stats.GetCurrentForm();
        score += form switch
        {
            "Excellent" => 10f,
            "Good" => 5f,
            "Average" => 0f,
            "Poor" => -5f,
            "Struggling" => -10f,
            _ => 0f
        };

        return score;
    }

    /// <summary>
    /// Gets the importance level of a match
    /// </summary>
    private static int GetMatchImportance(Match match)
    {
        int importance = 0;

        if (match.titleMatch)
            importance += 100;

        // Match type importance
        importance += match.matchType switch
        {
            "Singles" => 10,
            "Tag" => 15,
            "Hardcore" => 20,
            "Cage" => 25,
            "LadderMatch" => 30,
            "TLC" => 35,
            "HellInACell" => 40,
            _ => 10
        };

        return importance;
    }

    /// <summary>
    /// Generates a referee schedule report
    /// </summary>
    public static string GenerateScheduleReport(GameData data)
    {
        var report = "=== REFEREE SCHEDULE REPORT ===\n\n";

        var activeRefs = data.referees.Values.Where(r => r.isActive).OrderBy(r => r.name).ToList();

        foreach (var referee in activeRefs)
        {
            report += $"{referee.name}\n";
            report += $"  Status: {GetRefereeStatus(referee)}\n";
            report += $"  Matches This Week: {referee.matchesThisWeek}/5\n";
            report += $"  Fatigue: {referee.fatigue}/100\n";
            report += $"  Form: {referee.stats.GetCurrentForm()}\n";
            report += $"  Availability: {(RefereeCareerManager.CanWorkMatch(referee) ? "Available" : "Unavailable")}\n";
            report += "\n";
        }

        return report;
    }

    /// <summary>
    /// Balances referee workload across a season
    /// </summary>
    public static void BalanceWorkload(List<Show> upcomingShows, GameData data)
    {
        // Pre-assign referees to shows to balance workload
        var availableRefs = data.referees.Values.Where(r => r.isActive && !r.isInjured).ToList();
        
        if (availableRefs.Count == 0)
            return;

        Dictionary<Referee, int> plannedWorkload = new Dictionary<Referee, int>();
        foreach (var r in availableRefs)
            plannedWorkload[r] = 0;

        foreach (var show in upcomingShows)
        {
            // Assign main event ref to title matches
            var titleMatches = show.matches.Where(m => m.titleMatch).ToList();
            foreach (var match in titleMatches)
            {
                var mainEventRefs = availableRefs
                    .Where(r => r.isMainEventRef)
                    .OrderBy(r => plannedWorkload[r])
                    .ToList();

                if (mainEventRefs.Count > 0)
                {
                    match.referee = mainEventRefs[0];
                    plannedWorkload[mainEventRefs[0]]++;
                }
            }

            // Assign remaining matches
            var remainingMatches = show.matches.Where(m => m.referee == null).ToList();
            foreach (var match in remainingMatches)
            {
                var leastWorkedRef = availableRefs
                    .OrderBy(r => plannedWorkload[r])
                    .First();

                match.referee = leastWorkedRef;
                plannedWorkload[leastWorkedRef]++;
            }
        }

        Debug.Log($"Balanced workload across {upcomingShows.Count} shows for {availableRefs.Count} referees");
    }

    /// <summary>
    /// Finds a replacement referee mid-match
    /// </summary>
    public static Referee FindReplacementReferee(Match match, Referee originalRef, GameData data)
    {
        var availableRefs = data.referees.Values
            .Where(r => r.isActive && !r.isInjured && r != originalRef)
            .OrderByDescending(r => r.experience)
            .ToList();

        if (availableRefs.Count == 0)
        {
            Debug.LogWarning("No replacement referee available!");
            return null;
        }

        var replacement = availableRefs[0];
        Debug.Log($"🔄 {replacement.name} is replacing {originalRef.name} as referee!");
        return replacement;
    }

    private static string GetRefereeStatus(Referee referee)
    {
        if (referee.isInjured)
            return $"INJURED ({referee.injuryWeeksRemaining} weeks)";
        if (!referee.isActive)
            return "INACTIVE";
        if (referee.fatigue > 80)
            return "EXHAUSTED";
        if (referee.fatigue > 60)
            return "FATIGUED";
        return "ACTIVE";
    }

    private static bool IsHardcoreMatch(string matchType)
    {
        return matchType switch
        {
            "Hardcore" => true,
            "NoDisqualification" => true,
            "StreetFight" => true,
            "FallsCountAnywhere" => true,
            "LastManStanding" => true,
            "TLC" => true,
            "LadderMatch" => true,
            "HellInACell" => true,
            _ => false
        };
    }
}

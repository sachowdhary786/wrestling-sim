using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Utility methods for referee operations and queries
/// </summary>
public static class RefereeUtilities
{
    /// <summary>
    /// Gets a comprehensive report for a referee
    /// </summary>
    public static string GetRefereeReport(Referee referee)
    {
        var report = $"=== {referee.name} - Referee Profile ===\n\n";
        
        // Basic Info
        report += "BASIC INFORMATION:\n";
        report += $"Age: {referee.age}\n";
        report += $"Years Experience: {referee.yearsExperience}\n";
        report += $"Status: {(referee.isActive ? "Active" : "Inactive")}\n";
        if (referee.isInjured)
            report += $"Injury: {referee.injuryWeeksRemaining} weeks remaining\n";
        report += "\n";
        
        // Stats
        report += "ATTRIBUTES:\n";
        report += $"Experience: {referee.experience}/100\n";
        report += $"Consistency: {referee.consistency}/100\n";
        report += $"Strictness: {referee.strictness}/100\n";
        report += $"Corruption: {referee.corruption}/100\n";
        report += $"Quality Rating: {referee.GetQualityRating()}/100\n";
        report += "\n";
        
        // Specializations
        report += "SPECIALIZATIONS:\n";
        if (referee.isMainEventRef)
            report += "- Main Event Referee\n";
        if (referee.isHardcoreSpecialist)
            report += "- Hardcore Specialist\n";
        if (referee.isFavoredByCompany)
            report += "- Company Favorite\n";
        if (!referee.isMainEventRef && !referee.isHardcoreSpecialist)
            report += "- None\n";
        report += "\n";
        
        // Career Stats
        report += "CAREER STATISTICS:\n";
        report += $"Total Matches: {referee.stats.totalMatches}\n";
        report += $"Title Matches: {referee.stats.titleMatches}\n";
        report += $"Hardcore Matches: {referee.stats.hardcoreMatches}\n";
        report += $"Average Rating: {referee.stats.averageMatchRating:F1}/100\n";
        report += $"Highest Rated: {referee.stats.highestRatedMatch}/100\n";
        report += $"Perfect Matches: {referee.stats.perfectMatches}\n";
        report += $"Reputation: {referee.stats.reputation}/100\n";
        report += $"Current Form: {referee.stats.GetCurrentForm()}\n";
        report += "\n";
        
        // Finish Types
        report += "FINISH TYPE BREAKDOWN:\n";
        report += $"Pinfalls: {referee.stats.pinfalls}\n";
        report += $"Submissions: {referee.stats.submissions}\n";
        report += $"Knockouts: {referee.stats.knockouts}\n";
        report += $"DQs: {referee.stats.disqualifications}\n";
        report += $"Count Outs: {referee.stats.countOuts}\n";
        report += $"Controversial: {referee.stats.controversialFinishes}\n";
        report += $"Botched: {referee.stats.botchedFinishes}\n";
        report += "\n";
        
        // Incidents
        report += "INCIDENTS:\n";
        report += $"Times Bumped: {referee.stats.timesBumped}\n";
        report += $"Times Knocked Out: {referee.stats.timesKnockedOut}\n";
        report += $"Controversies: {referee.stats.controversies}\n";
        report += "\n";
        
        // Current Status
        report += "CURRENT STATUS:\n";
        report += $"Fatigue: {referee.fatigue}/100\n";
        report += $"Matches This Week: {referee.matchesThisWeek}/5\n";
        report += $"Consecutive Weeks: {referee.consecutiveWeeks}\n";
        report += $"Effectiveness: {RefereeCareerManager.GetEffectiveness(referee) * 100:F0}%\n";
        report += $"Can Work: {(RefereeCareerManager.CanWorkMatch(referee) ? "Yes" : "No")}\n";
        report += "\n";
        
        // Achievements
        if (referee.stats.achievements.Count > 0)
        {
            report += "ACHIEVEMENTS:\n";
            foreach (var achievement in referee.stats.achievements)
            {
                report += $"🏆 {achievement}\n";
            }
        }
        
        return report;
    }

    /// <summary>
    /// Gets top referees by various criteria
    /// </summary>
    public static List<Referee> GetTopReferees(GameData data, string criteria = "quality", int count = 5)
    {
        var refs = data.referees.Values.Where(r => r.isActive).ToList();
        
        return criteria.ToLower() switch
        {
            "quality" => refs.OrderByDescending(r => r.GetQualityRating()).Take(count).ToList(),
            "experience" => refs.OrderByDescending(r => r.experience).Take(count).ToList(),
            "matches" => refs.OrderByDescending(r => r.stats.totalMatches).Take(count).ToList(),
            "rating" => refs.OrderByDescending(r => r.stats.averageMatchRating).Take(count).ToList(),
            "reputation" => refs.OrderByDescending(r => r.stats.reputation).Take(count).ToList(),
            "controversial" => refs.OrderByDescending(r => r.stats.controversies).Take(count).ToList(),
            _ => refs.OrderByDescending(r => r.GetQualityRating()).Take(count).ToList()
        };
    }

    /// <summary>
    /// Compares two referees
    /// </summary>
    public static string CompareReferees(Referee ref1, Referee ref2)
    {
        var report = $"=== {ref1.name} vs {ref2.name} ===\n\n";
        
        report += $"Quality: {ref1.GetQualityRating()} vs {ref2.GetQualityRating()}\n";
        report += $"Experience: {ref1.experience} vs {ref2.experience}\n";
        report += $"Consistency: {ref1.consistency} vs {ref2.consistency}\n";
        report += $"Corruption: {ref1.corruption} vs {ref2.corruption}\n";
        report += $"Total Matches: {ref1.stats.totalMatches} vs {ref2.stats.totalMatches}\n";
        report += $"Avg Rating: {ref1.stats.averageMatchRating:F1} vs {ref2.stats.averageMatchRating:F1}\n";
        report += $"Reputation: {ref1.stats.reputation} vs {ref2.stats.reputation}\n";
        report += $"Controversies: {ref1.stats.controversies} vs {ref2.stats.controversies}\n";
        
        return report;
    }

    /// <summary>
    /// Gets all referees suitable for a specific match type
    /// </summary>
    public static List<Referee> GetSuitableReferees(GameData data, string matchType, bool titleMatch = false)
    {
        var suitable = data.referees.Values
            .Where(r => r.isActive && !r.isInjured && r.IsSuitableFor(matchType))
            .ToList();
        
        if (titleMatch)
        {
            suitable = suitable.Where(r => r.experience >= 70).ToList();
        }
        
        return suitable.OrderByDescending(r => r.GetQualityRating()).ToList();
    }

    /// <summary>
    /// Generates a referee power rankings list
    /// </summary>
    public static string GeneratePowerRankings(GameData data)
    {
        var report = "=== REFEREE POWER RANKINGS ===\n\n";
        
        var refs = data.referees.Values
            .Where(r => r.isActive && r.stats.totalMatches >= 10)
            .OrderByDescending(r => CalculatePowerRanking(r))
            .ToList();
        
        for (int i = 0; i < refs.Count && i < 10; i++)
        {
            var r = refs[i];
            float ranking = CalculatePowerRanking(r);
            report += $"{i + 1}. {r.name} - {ranking:F1} points\n";
            report += $"   Quality: {r.GetQualityRating()}, Form: {r.stats.GetCurrentForm()}, Rep: {r.stats.reputation}\n";
        }
        
        return report;
    }

    /// <summary>
    /// Calculates a power ranking score for a referee
    /// </summary>
    private static float CalculatePowerRanking(Referee referee)
    {
        float score = 0;
        
        // Quality rating (0-100)
        score += referee.GetQualityRating();
        
        // Reputation (0-100)
        score += referee.stats.reputation * 0.5f;
        
        // Recent form bonus
        score += referee.stats.GetCurrentForm() switch
        {
            "Excellent" => 20f,
            "Good" => 10f,
            "Average" => 0f,
            "Poor" => -10f,
            "Struggling" => -20f,
            _ => 0f
        };
        
        // Experience bonus
        if (referee.isMainEventRef)
            score += 15f;
        if (referee.isHardcoreSpecialist)
            score += 10f;
        
        // Controversy penalty
        score -= referee.stats.controversies * 0.5f;
        
        // Fatigue penalty
        score -= referee.fatigue * 0.1f;
        
        return score;
    }

    /// <summary>
    /// Gets referee statistics summary for all refs
    /// </summary>
    public static string GetLeagueStatistics(GameData data)
    {
        var report = "=== REFEREE LEAGUE STATISTICS ===\n\n";
        
        var activeRefs = data.referees.Values.Where(r => r.isActive).ToList();
        
        if (activeRefs.Count == 0)
        {
            return report + "No active referees.\n";
        }
        
        // Aggregate stats
        int totalMatches = activeRefs.Sum(r => r.stats.totalMatches);
        float avgRating = activeRefs.Average(r => r.stats.averageMatchRating);
        int totalControversies = activeRefs.Sum(r => r.stats.controversies);
        int totalKnockouts = activeRefs.Sum(r => r.stats.timesKnockedOut);
        
        report += $"Active Referees: {activeRefs.Count}\n";
        report += $"Total Matches Officiated: {totalMatches}\n";
        report += $"League Average Rating: {avgRating:F1}/100\n";
        report += $"Total Controversies: {totalControversies}\n";
        report += $"Total Knockouts: {totalKnockouts}\n";
        report += "\n";
        
        // Leaders
        report += "CATEGORY LEADERS:\n";
        report += $"Most Matches: {activeRefs.OrderByDescending(r => r.stats.totalMatches).First().name} ({activeRefs.Max(r => r.stats.totalMatches)})\n";
        report += $"Highest Rating: {activeRefs.OrderByDescending(r => r.stats.averageMatchRating).First().name} ({activeRefs.Max(r => r.stats.averageMatchRating):F1})\n";
        report += $"Most Experienced: {activeRefs.OrderByDescending(r => r.experience).First().name} ({activeRefs.Max(r => r.experience)})\n";
        report += $"Most Consistent: {activeRefs.OrderByDescending(r => r.consistency).First().name} ({activeRefs.Max(r => r.consistency)})\n";
        report += $"Most Controversial: {activeRefs.OrderByDescending(r => r.stats.controversies).First().name} ({activeRefs.Max(r => r.stats.controversies)})\n";
        
        return report;
    }

    /// <summary>
    /// Finds referees that need training
    /// </summary>
    public static List<Referee> GetRefereesNeedingTraining(GameData data)
    {
        return data.referees.Values
            .Where(r => r.isActive && 
                   (r.experience < 60 || 
                    r.consistency < 60 || 
                    r.corruption > 40 ||
                    r.stats.GetCurrentForm() == "Poor" || 
                    r.stats.GetCurrentForm() == "Struggling"))
            .OrderBy(r => r.GetQualityRating())
            .ToList();
    }

    /// <summary>
    /// Finds referees ready for promotion
    /// </summary>
    public static List<Referee> GetRefereesReadyForPromotion(GameData data)
    {
        return data.referees.Values
            .Where(r => r.isActive && 
                   !r.isMainEventRef && 
                   r.experience >= 70 && 
                   r.stats.totalMatches >= 50 &&
                   r.stats.averageMatchRating >= 70)
            .OrderByDescending(r => r.GetQualityRating())
            .ToList();
    }

    /// <summary>
    /// Simulates a week for all referees
    /// </summary>
    public static void AdvanceAllRefereesWeek(GameData data)
    {
        foreach (var referee in data.referees.Values)
        {
            RefereeCareerManager.AdvanceWeek(referee);
        }
        
        Debug.Log($"Advanced week for {data.referees.Count} referees");
    }
}

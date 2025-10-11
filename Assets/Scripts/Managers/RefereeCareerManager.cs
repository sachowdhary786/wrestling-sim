using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages referee career progression, fatigue, injuries, and development
/// </summary>
public static class RefereeCareerManager
{
    // Constants
    private const int MAX_MATCHES_PER_WEEK = 5;
    private const int FATIGUE_PER_MATCH = 15;
    private const int FATIGUE_RECOVERY_PER_WEEK = 20;
    private const float INJURY_CHANCE_BASE = 0.5f; // 0.5% base chance
    private const float INJURY_CHANCE_PER_FATIGUE = 0.02f; // +0.02% per fatigue point

    /// <summary>
    /// Advances a referee's career by one week
    /// </summary>
    public static void AdvanceWeek(Referee referee)
    {
        if (!referee.isActive)
            return;

        // Handle injury recovery
        if (referee.isInjured)
        {
            referee.injuryWeeksRemaining--;
            if (referee.injuryWeeksRemaining <= 0)
            {
                referee.isInjured = false;
                referee.injuryWeeksRemaining = 0;
                Debug.Log($"Referee {referee.name} has recovered from injury!");
            }
            return; // Skip rest if injured
        }

        // Recover fatigue
        referee.fatigue = Mathf.Max(0, referee.fatigue - FATIGUE_RECOVERY_PER_WEEK);

        // Track consecutive weeks
        if (referee.matchesThisWeek > 0)
            referee.consecutiveWeeks++;
        else
            referee.consecutiveWeeks = 0;

        // Reset weekly counter
        referee.matchesThisWeek = 0;

        // Age progression (once per year / 52 weeks)
        if (referee.stats.totalMatches % 52 == 0 && referee.stats.totalMatches > 0)
        {
            referee.age++;
            referee.yearsExperience++;
            
            // Natural skill changes with age
            ApplyAgingEffects(referee);
        }

        // Skill development based on performance
        DevelopSkills(referee);
    }

    /// <summary>
    /// Records a match officiated by a referee
    /// </summary>
    public static void RecordMatch(Referee referee, Match match, bool wasKnockedOut = false, bool wasBumped = false)
    {
        if (referee == null)
            return;

        // Update statistics
        referee.stats.RecordMatch(match, wasKnockedOut, wasBumped);

        // Increase fatigue
        referee.fatigue = Mathf.Min(100, referee.fatigue + FATIGUE_PER_MATCH);
        referee.matchesThisWeek++;

        // Check for injury
        CheckForInjury(referee, match.matchType, wasKnockedOut, wasBumped);

        // Check for achievements
        CheckAchievements(referee);
    }

    /// <summary>
    /// Checks if a referee can work another match this week
    /// </summary>
    public static bool CanWorkMatch(Referee referee)
    {
        if (!referee.isActive || referee.isInjured)
            return false;

        if (referee.matchesThisWeek >= MAX_MATCHES_PER_WEEK)
            return false;

        // High fatigue reduces availability
        if (referee.fatigue > 80)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < (referee.fatigue - 80) * 2) // Up to 40% chance to decline
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the referee's current effectiveness (reduced by fatigue)
    /// </summary>
    public static float GetEffectiveness(Referee referee)
    {
        if (referee.fatigue == 0)
            return 1.0f;

        // Fatigue reduces effectiveness by up to 30%
        float reduction = (referee.fatigue / 100f) * 0.3f;
        return 1.0f - reduction;
    }

    /// <summary>
    /// Trains a referee to improve their skills
    /// </summary>
    public static void TrainReferee(Referee referee, string focusArea, int weeks = 1)
    {
        if (!referee.isActive || referee.isInjured)
            return;

        int improvement = weeks * 2; // 2 points per week of training

        switch (focusArea.ToLower())
        {
            case "strictness":
                referee.strictness = Mathf.Min(100, referee.strictness + improvement);
                Debug.Log($"{referee.name} trained in rule enforcement (+{improvement} strictness)");
                break;

            case "consistency":
                referee.consistency = Mathf.Min(100, referee.consistency + improvement);
                Debug.Log($"{referee.name} trained in consistency (+{improvement} consistency)");
                break;

            case "experience":
                referee.experience = Mathf.Min(100, referee.experience + improvement);
                Debug.Log($"{referee.name} gained experience (+{improvement} experience)");
                break;

            case "corruption_reduction":
                referee.corruption = Mathf.Max(0, referee.corruption - improvement);
                Debug.Log($"{referee.name} improved integrity (-{improvement} corruption)");
                break;
        }
    }

    /// <summary>
    /// Promotes a referee to main event status
    /// </summary>
    public static bool PromoteToMainEvent(Referee referee)
    {
        if (referee.experience < 70)
        {
            Debug.LogWarning($"{referee.name} needs 70+ experience for main event promotion (current: {referee.experience})");
            return false;
        }

        if (referee.stats.totalMatches < 50)
        {
            Debug.LogWarning($"{referee.name} needs 50+ matches for main event promotion (current: {referee.stats.totalMatches})");
            return false;
        }

        referee.isMainEventRef = true;
        referee.stats.achievements.Add("Promoted to Main Event Referee");
        Debug.Log($"🎉 {referee.name} promoted to Main Event Referee!");
        return true;
    }

    /// <summary>
    /// Assigns hardcore specialist status
    /// </summary>
    public static bool AssignHardcoreSpecialist(Referee referee)
    {
        if (referee.stats.hardcoreMatches < 20)
        {
            Debug.LogWarning($"{referee.name} needs 20+ hardcore matches (current: {referee.stats.hardcoreMatches})");
            return false;
        }

        referee.isHardcoreSpecialist = true;
        referee.stats.achievements.Add("Became Hardcore Specialist");
        Debug.Log($"🎉 {referee.name} is now a Hardcore Specialist!");
        return true;
    }

    /// <summary>
    /// Suspends a referee for controversial behavior
    /// </summary>
    public static void SuspendReferee(Referee referee, int weeks, string reason)
    {
        referee.isActive = false;
        referee.injuryWeeksRemaining = weeks; // Reuse this field for suspension
        referee.stats.controversies++;
        Debug.Log($"⚠️ {referee.name} suspended for {weeks} weeks. Reason: {reason}");
    }

    /// <summary>
    /// Retires a referee
    /// </summary>
    public static void RetireReferee(Referee referee, string reason = "Career End")
    {
        referee.isActive = false;
        referee.stats.achievements.Add($"Retired: {reason}");
        Debug.Log($"👋 {referee.name} has retired. {referee.stats.totalMatches} career matches officiated.");
    }

    // Private helper methods

    private static void CheckForInjury(Referee referee, string matchType, bool wasKnockedOut, bool wasBumped)
    {
        float injuryChance = INJURY_CHANCE_BASE;

        // Fatigue increases injury risk
        injuryChance += referee.fatigue * INJURY_CHANCE_PER_FATIGUE;

        // Hardcore matches are riskier
        if (IsHardcoreMatch(matchType))
            injuryChance *= 3f;

        // Being knocked out or bumped greatly increases risk
        if (wasKnockedOut)
            injuryChance *= 10f;
        else if (wasBumped)
            injuryChance *= 5f;

        // Age factor
        if (referee.age > 50)
            injuryChance *= 1.5f;

        // Roll for injury
        float roll = Random.Range(0f, 100f);
        if (roll < injuryChance)
        {
            int severity = DetermineInjurySeverity(injuryChance);
            referee.isInjured = true;
            referee.injuryWeeksRemaining = severity switch
            {
                1 => Random.Range(1, 3),   // Minor: 1-2 weeks
                2 => Random.Range(3, 6),   // Moderate: 3-5 weeks
                3 => Random.Range(6, 12),  // Severe: 6-11 weeks
                _ => 1
            };

            string injuryType = severity switch
            {
                1 => "minor injury",
                2 => "moderate injury",
                3 => "serious injury",
                _ => "injury"
            };

            Debug.Log($"⚠️ Referee {referee.name} suffered a {injuryType}! Out for {referee.injuryWeeksRemaining} weeks.");
        }
    }

    private static int DetermineInjurySeverity(float injuryChance)
    {
        if (injuryChance < 5) return 1;   // Minor
        if (injuryChance < 15) return 2;  // Moderate
        return 3;                         // Severe
    }

    private static void ApplyAgingEffects(Referee referee)
    {
        if (referee.age < 40)
        {
            // Prime years - slight improvements
            if (referee.experience < 95)
                referee.experience++;
        }
        else if (referee.age >= 50)
        {
            // Decline phase
            if (Random.value < 0.3f) // 30% chance per year
            {
                referee.consistency = Mathf.Max(0, referee.consistency - 1);
                Debug.Log($"{referee.name} is showing signs of age (consistency -1)");
            }
        }

        // Consider retirement
        if (referee.age >= 60 && Random.value < 0.2f) // 20% chance per year after 60
        {
            RetireReferee(referee, "Age");
        }
    }

    private static void DevelopSkills(Referee referee)
    {
        // Young refs develop faster
        float developmentRate = referee.age < 35 ? 0.02f : 0.01f;

        // Good performance accelerates development
        if (referee.stats.GetCurrentForm() == "Excellent")
            developmentRate *= 2f;

        // Random skill improvement
        if (Random.value < developmentRate)
        {
            // Choose random stat to improve
            int stat = Random.Range(0, 3);
            switch (stat)
            {
                case 0:
                    if (referee.experience < 100)
                        referee.experience++;
                    break;
                case 1:
                    if (referee.consistency < 100)
                        referee.consistency++;
                    break;
                case 2:
                    if (referee.corruption > 0 && Random.value < 0.5f)
                        referee.corruption--;
                    break;
            }
        }
    }

    private static void CheckAchievements(Referee referee)
    {
        var stats = referee.stats;
        var achievements = stats.achievements;

        // Milestone achievements
        if (stats.totalMatches == 100 && !achievements.Contains("100 Matches"))
        {
            achievements.Add("100 Matches");
            Debug.Log($"🏆 {referee.name} reached 100 career matches!");
        }

        if (stats.totalMatches == 500 && !achievements.Contains("500 Matches"))
        {
            achievements.Add("500 Matches");
            Debug.Log($"🏆 {referee.name} reached 500 career matches!");
        }

        if (stats.totalMatches == 1000 && !achievements.Contains("1000 Matches"))
        {
            achievements.Add("1000 Matches - Legend");
            Debug.Log($"🏆 {referee.name} is a legendary referee with 1000 matches!");
        }

        // Quality achievements
        if (stats.perfectMatches >= 50 && !achievements.Contains("50 Perfect Matches"))
        {
            achievements.Add("50 Perfect Matches");
            Debug.Log($"🏆 {referee.name} has officiated 50 perfect matches!");
        }

        if (stats.averageMatchRating >= 80 && stats.totalMatches >= 50 && !achievements.Contains("Elite Referee"))
        {
            achievements.Add("Elite Referee");
            Debug.Log($"🏆 {referee.name} is an Elite Referee (80+ average rating)!");
        }

        // Specialty achievements
        if (stats.titleMatches >= 100 && !achievements.Contains("Title Match Specialist"))
        {
            achievements.Add("Title Match Specialist");
            Debug.Log($"🏆 {referee.name} is a Title Match Specialist!");
        }

        // Controversial achievements
        if (stats.controversialFinishes >= 25 && !achievements.Contains("Controversial Figure"))
        {
            achievements.Add("Controversial Figure");
            Debug.Log($"⚠️ {referee.name} has become a controversial figure...");
        }
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

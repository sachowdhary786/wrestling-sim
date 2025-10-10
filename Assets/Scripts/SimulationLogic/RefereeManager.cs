using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages referee assignment and their influence on match outcomes
/// </summary>
public static class RefereeManager
{
    /// <summary>
    /// Assigns an appropriate referee to a match
    /// </summary>
    public static void AssignReferee(Match match, GameData data)
    {
        if (data.referees == null || data.referees.Count == 0)
        {
            Debug.LogWarning("No referees available in GameData!");
            return;
        }

        // Filter suitable referees
        var suitableRefs = data.referees
            .Where(r => r.isActive && r.IsSuitableFor(match.matchType))
            .ToList();

        if (suitableRefs.Count == 0)
        {
            Debug.LogWarning($"No suitable referees for {match.matchType} match!");
            match.referee = data.referees.First(r => r.isActive);
            return;
        }

        // Prioritize based on match importance
        if (match.titleMatch)
        {
            // Title matches get experienced refs
            var experiencedRefs = suitableRefs
                .Where(r => r.experience >= 70)
                .OrderByDescending(r => r.GetQualityRating())
                .ToList();

            match.referee = experiencedRefs.Count > 0
                ? experiencedRefs.First()
                : suitableRefs.OrderByDescending(r => r.experience).First();
        }
        else
        {
            // Regular matches - random suitable ref
            match.referee = suitableRefs[Random.Range(0, suitableRefs.Count)];
        }
    }

    /// <summary>
    /// Calculates the referee's influence on match rating
    /// </summary>
    public static float GetRefereeRatingModifier(Referee referee, Match match)
    {
        if (referee == null)
            return 0f;

        float modifier = 0f;

        // Experience bonus (up to +5)
        modifier += (referee.experience / 100f) * 5f;

        // Consistency bonus (up to +3)
        modifier += (referee.consistency / 100f) * 3f;

        // Corruption penalty (up to -4)
        modifier -= (referee.corruption / 100f) * 4f;

        // Main event ref bonus for title matches
        if (match.titleMatch && referee.isMainEventRef)
            modifier += 2f;

        // Hardcore specialist bonus
        if (referee.isHardcoreSpecialist && IsHardcoreMatch(match.matchType))
            modifier += 3f;

        return modifier;
    }

    /// <summary>
    /// Determines if the referee influences the finish type
    /// </summary>
    public static string ApplyRefereeInfluence(string baseFinishType, Referee referee, Match match)
    {
        if (referee == null)
            return baseFinishType;

        // High strictness increases DQ/Count Out chance
        if (referee.strictness > 70)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < (referee.strictness - 70) * 0.5f) // Up to 15% chance
            {
                return Random.value > 0.5f ? "DQ" : "Count Out";
            }
        }

        // High corruption can change finish types (screwjobs, fast counts)
        if (referee.corruption > 60)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < (referee.corruption - 60) * 0.3f) // Up to 12% chance
            {
                Debug.Log($"⚠️ Referee {referee.name} makes a controversial call!");
                
                // Corrupt finish - favor company-backed wrestlers or create controversy
                if (referee.isFavoredByCompany)
                {
                    return "Controversial Finish";
                }
            }
        }

        // Low consistency can cause botched finishes
        if (referee.consistency < 40)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < (40 - referee.consistency) * 0.2f) // Up to 8% chance
            {
                Debug.Log($"⚠️ Referee {referee.name} botches the finish!");
                return "Botched Finish";
            }
        }

        return baseFinishType;
    }

    /// <summary>
    /// Applies referee influence during match phases (Advanced mode only)
    /// </summary>
    public static void ApplyPhaseInfluence(MatchState state, string phase)
    {
        if (state.match.referee == null)
            return;

        var referee = state.match.referee;

        switch (phase)
        {
            case "Opening":
                // Experienced refs help match flow
                if (referee.experience > 70)
                {
                    foreach (var wrestler in state.wrestlers)
                    {
                        state.scores[wrestler] += referee.experience * 0.02f; // Small bonus
                    }
                    Debug.Log($"  Referee {referee.name}'s experience improves match flow");
                }
                break;

            case "MidPhase":
                // Consistent refs maintain match quality
                if (referee.consistency > 70)
                {
                    // Reduce randomness in momentum swings
                    Debug.Log($"  Referee {referee.name} maintains consistent pacing");
                }
                break;

            case "Climax":
                // This is where referee influence matters most
                if (referee.corruption > 70)
                {
                    Debug.Log($"  Referee {referee.name} may influence the outcome...");
                }
                break;
        }
    }

    /// <summary>
    /// Gets a description of the referee's style
    /// </summary>
    public static string GetRefereeStyle(Referee referee)
    {
        if (referee == null)
            return "No referee assigned";

        List<string> traits = new List<string>();

        if (referee.strictness > 70)
            traits.Add("Strict");
        else if (referee.strictness < 30)
            traits.Add("Lenient");

        if (referee.corruption > 60)
            traits.Add("Corrupt");
        else if (referee.corruption < 20)
            traits.Add("Fair");

        if (referee.experience > 80)
            traits.Add("Veteran");
        else if (referee.experience < 30)
            traits.Add("Rookie");

        if (referee.consistency > 80)
            traits.Add("Consistent");
        else if (referee.consistency < 40)
            traits.Add("Unpredictable");

        if (referee.isMainEventRef)
            traits.Add("Main Event");
        if (referee.isHardcoreSpecialist)
            traits.Add("Hardcore Specialist");

        return traits.Count > 0 ? string.Join(", ", traits) : "Standard Referee";
    }

    /// <summary>
    /// Creates a default referee pool for testing
    /// </summary>
    public static List<Referee> CreateDefaultReferees()
    {
        return new List<Referee>
        {
            new Referee("Earl Hebner", strictness: 60, corruption: 40, experience: 95)
            {
                id = "ref_earl",
                consistency = 85,
                isMainEventRef = true
            },
            new Referee("Mike Chioda", strictness: 70, corruption: 20, experience: 90)
            {
                id = "ref_mike",
                consistency = 90,
                isMainEventRef = true
            },
            new Referee("Charles Robinson", strictness: 50, corruption: 15, experience: 85)
            {
                id = "ref_charles",
                consistency = 80,
                isMainEventRef = true
            },
            new Referee("Nick Patrick", strictness: 40, corruption: 70, experience: 75)
            {
                id = "ref_nick",
                consistency = 60,
                isFavoredByCompany = true
            },
            new Referee("Tommy Young", strictness: 80, corruption: 10, experience: 80)
            {
                id = "ref_tommy",
                consistency = 85
            },
            new Referee("Bryce Remsburg", strictness: 30, corruption: 5, experience: 70)
            {
                id = "ref_bryce",
                consistency = 75,
                isHardcoreSpecialist = true
            },
            new Referee("Rookie Ref", strictness: 60, corruption: 20, experience: 25)
            {
                id = "ref_rookie",
                consistency = 40
            }
        };
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
            _ => false
        };
    }
}

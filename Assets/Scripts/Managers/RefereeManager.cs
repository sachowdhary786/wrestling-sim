using System;
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
        var suitableRefs = data.referees.Values
            .Where(r => r.isActive && r.IsSuitableFor(match.matchType))
            .ToList();

        if (suitableRefs.Count == 0)
        {
            Debug.LogWarning($"No suitable referees for {match.matchType} match!");
            match.referee = data.referees.Values.First(r => r.isActive);
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

            match.referee =
                experiencedRefs.Count > 0
                    ? experiencedRefs.First()
                    : suitableRefs.OrderByDescending(r => r.experience).First();
        }
        else
        {
            // Regular matches - random suitable ref
            match.referee = suitableRefs[UnityEngine.Random.Range(0, suitableRefs.Count)];
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
    public static FinishType ApplyRefereeInfluence(
        FinishType baseFinishType,
        Referee referee,
        Match match
    )
    {
        if (referee == null)
            return baseFinishType;

        // High strictness increases DQ/Count Out chance
        if (referee.strictness > 70 && UnityEngine.Random.Range(0f, 100f) < (referee.strictness - 70) * 0.5f)
        {
            Debug.Log($"⚠️ Referee {referee.name}'s strictness leads to a DQ/Count Out!");
            return UnityEngine.Random.value > 0.5f ? FinishType.DQ : FinishType.CountOut;
        }

        // High corruption can lead to screwjobs or fast counts
        if (referee.corruption > 60 && UnityEngine.Random.Range(0f, 100f) < (referee.corruption - 60) * 0.3f)
        {
            Debug.Log($"⚠️ Referee {referee.name} makes a controversial call!");
            if (referee.isFavoredByCompany)
            {
                return FinishType.ControversialFinish; // e.g., Fast Count
            }
            else
            {
                // Turn a clean finish into a controversial one
                if(baseFinishType == FinishType.Pinfall || baseFinishType == FinishType.Submission)
                {
                    return FinishType.ControversialFinish;
                }
            }
        }

        // Low consistency can cause botched finishes or missed calls
        if (referee.consistency < 40 && UnityEngine.Random.Range(0f, 100f) < (40 - referee.consistency) * 0.2f)
        {
            Debug.Log($"⚠️ Referee {referee.name} botches the call!");
            // 50% chance of a full botched finish, 50% chance of a near fall that should have been a finish
            if(UnityEngine.Random.value > 0.5f)
            {
                return FinishType.BotchedFinish;
            }
            else
            {
                Debug.Log("  ...but it only gets a 2-count!");
                // This doesn't change the finish type, but adds a story element (handled in logging)
            }
        }

        // Lenient refs might allow a non-hardcore match to become one
        if (referee.strictness < 30 && !IsHardcoreMatch(match.matchType) && UnityEngine.Random.Range(0f, 100f) < 10f)
        {
            Debug.Log($"⚠️ Referee {referee.name} is letting them fight! This is turning into a brawl!");
            // This could be a trigger for a future storyline event
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
    static List<Referee> CreateDefaultReferees()
    {
        return new List<Referee>
        {
            new Referee("Earl Hebner", strictness: 60, corruption: 40, experience: 95)
            {
                id = Guid.NewGuid(),
                consistency = 85,
                isMainEventRef = true,
            },
            new Referee("Mike Chioda", strictness: 70, corruption: 20, experience: 90)
            {
                id = Guid.NewGuid(),
                consistency = 90,
                isMainEventRef = true,
            },
            new Referee("Charles Robinson", strictness: 50, corruption: 15, experience: 85)
            {
                id = Guid.NewGuid(),
                consistency = 80,
                isMainEventRef = true,
            },
            new Referee("Nick Patrick", strictness: 40, corruption: 70, experience: 75)
            {
                id = Guid.NewGuid(),
                consistency = 60,
                isFavoredByCompany = true,
            },
            new Referee("Tommy Young", strictness: 80, corruption: 10, experience: 80)
            {
                id = Guid.NewGuid(),
                consistency = 85,
            },
            new Referee("Bryce Remsburg", strictness: 30, corruption: 5, experience: 70)
            {
                id = Guid.NewGuid(),
                consistency = 75,
                isHardcoreSpecialist = true,
            },
            new Referee("Rookie Ref", strictness: 60, corruption: 20, experience: 25)
            {
                id = Guid.NewGuid(),
                consistency = 40,
            },
        };
    }

    public static bool IsHardcoreMatch(string matchType)
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
            _ => false,
        };
    }
}

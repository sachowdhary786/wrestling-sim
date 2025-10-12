using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles injury simulation and management during matches
/// </summary>
public static class MatchInjurySystem
{
    public static void CheckForInjuries(
        List<Wrestler> wrestlers,
        Match match,
        string matchType,
        float fatigue,
        GameData data
    )
    {
        foreach (var wrestler in wrestlers)
        {
            if (wrestler.injured)
                continue; // Already injured

            float injuryChance = CalculateInjuryChance(wrestler, matchType, fatigue);

            // Roll for injury
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll < injuryChance)
            {
                ApplyInjury(wrestler, injuryChance, data);
            }
        }
    }

    private static float CalculateInjuryChance(Wrestler wrestler, string matchType, float fatigue)
    {
        float chance = 1f; // base 1%

        // Fatigue multiplier
        if (fatigue > 0.5f)
            chance += (fatigue - 0.5f) * 30f;

        // Match type risk factor
        chance += matchType switch
        {
            "Hardcore" => 15f,
            "Ladder" => 20f,
            "Cage" => 10f,
            "TLC" => 25f,
            _ => 2f,
        };

        // Toughness reduces injury chance
        chance *= (100 - wrestler.toughness) / 100f;

        // Low stamina increases risk
        if (wrestler.stamina < 30)
            chance *= 1.5f;

        return Mathf.Clamp(chance, 0f, 90f);
    }

    private static void ApplyInjury(Wrestler wrestler, float injuryChance, GameData data)
    {
        int severity = DetermineInjurySeverity(injuryChance);
        wrestler.injuryType = GetRandomInjuryType(severity);
        wrestler.injured = true;
        wrestler.injurySeverity = severity;
        int recoveryWeeks = GetRecoveryWeeks(severity);

        // Apply bonus from company doctor, if available
        var company = data.companies.FirstOrDefault(c => c.id == wrestler.contract?.companyId);
        if (company != null)
        {
            var doctor = company.corporateStaff.FirstOrDefault(s => s.role == StaffRole.Doctor);
            if (doctor != null)
            {
                float reduction = recoveryWeeks * (doctor.injuryRecoveryBonus / 100f);
                recoveryWeeks -= Mathf.RoundToInt(reduction);
                var doctorInfo = data.wrestlers.First(w => w.id == doctor.staffId);
                Debug.Log(
                    $"[Injury] {doctorInfo.name} has reduced {wrestler.name}'s recovery time!"
                );
            }
        }

        wrestler.recoveryWeeksRemaining = recoveryWeeks;

        Debug.Log(
            $"⚠️ {wrestler.name} suffers a {wrestler.injuryType}! Estimated recovery: {wrestler.recoveryWeeksRemaining} weeks."
        );

        ApplyInjuryStatPenalty(wrestler, severity);
    }

    private static int GetRecoveryWeeks(int severity)
    {
        return severity switch
        {
            1 => Random.Range(1, 5), // 1-4 weeks for minor
            2 => Random.Range(4, 13), // 1-3 months for moderate
            3 => Random.Range(13, 53), // 3-12 months for major
            _ => 0,
        };
    }

    private static int DetermineInjurySeverity(float injuryChance)
    {
        if (injuryChance < 10)
            return 1; // Minor
        if (injuryChance < 25)
            return 2; // Moderate
        return 3; // Major
    }

    private static string GetRandomInjuryType(int severity)
    {
        string[] minor = { "sprained wrist", "bruised ribs", "twisted ankle" };
        string[] moderate = { "shoulder strain", "concussion", "knee sprain" };
        string[] major = { "broken arm", "torn ACL", "neck injury" };

        return severity switch
        {
            1 => minor[UnityEngine.Random.Range(0, minor.Length)],
            2 => moderate[UnityEngine.Random.Range(0, moderate.Length)],
            3 => major[UnityEngine.Random.Range(0, major.Length)],
            _ => "unknown injury",
        };
    }

    private static void ApplyInjuryStatPenalty(Wrestler wrestler, int severity)
    {
        float penalty = severity switch
        {
            1 => 0.95f,
            2 => 0.85f,
            3 => 0.70f,
            _ => 1f,
        };

        wrestler.technical = Mathf.RoundToInt(wrestler.technical * penalty);
        wrestler.brawling = Mathf.RoundToInt(wrestler.brawling * penalty);
        wrestler.aerial = Mathf.RoundToInt(wrestler.aerial * penalty);
        wrestler.stamina = Mathf.RoundToInt(wrestler.stamina * penalty);
    }
}

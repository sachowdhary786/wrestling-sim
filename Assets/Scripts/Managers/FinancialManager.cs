using UnityEngine;
using System.Linq;

/// <summary>
/// Handles the monthly financial cycle for all companies.
/// </summary>
public static class FinancialManager
{

    /// <summary>
    /// Processes monthly finances for major promotions (TV deals, salaries).
    /// </summary>
    public static void ProcessMonthlyFinances(GameData gameData)
    {
        Debug.Log("[Finances] Processing monthly finances for major promotions...");
        var majorPromotions = gameData.companies.Where(c => c.tier == CompanyTier.Major);

        foreach (var company in majorPromotions)
        {
            // Standard monthly income and expenses for major companies
            // ... (existing logic) ...
        }
    }

    /// <summary>
    /// Processes show-by-show finances for indie promotions (gate, merch, appearance fees).
    /// </summary>
    public static void ProcessIndieShowFinances(Show show, Company company, GameData gameData)
    {
        if (company.tier == CompanyTier.Major) return;

        float totalGate = 0;
        float totalMerch = 0;
        float totalAppearanceFees = 0;

        var participants = show.matches.SelectMany(m => m.participants).Distinct();
        foreach (var wrestlerId in participants)
        {
            var wrestler = gameData.wrestlers.First(w => w.id == wrestlerId);

            // Calculate effective local popularity with crowd fatigue
            int fatigue = wrestler.crowdFatigue.ContainsKey(show.location) ? wrestler.crowdFatigue[show.location] : 0;
            int effectivePopularity = Mathf.Max(0, wrestler.GetPopularity(show.location) - fatigue);

            // Gate income is driven by local popularity
            totalGate += effectivePopularity * 100; // $100 per point of effective local popularity

            // Merch income is driven by charisma
            totalMerch += wrestler.charisma * 10; // $10 per point of charisma

            // Appearance fees are based on overall popularity
            totalAppearanceFees += wrestler.popularity * 50; // $50 per point of overall popularity

            // Increase crowd fatigue for this location
            wrestler.crowdFatigue[show.location] = fatigue + 10;
        }


        float totalIncome = totalGate + totalMerch;
        company.finances += totalIncome - totalAppearanceFees;

        Debug.Log($"[Indie Finances] {company.name} Show: Gate=${totalGate}, Merch=${totalMerch}, Fees=${totalAppearanceFees}. Net: ${totalIncome - totalAppearanceFees:C}");
    }

    private static void DecayCrowdFatigue(Wrestler wrestler, string activeLocation)
    {
        var locations = wrestler.crowdFatigue.Keys.ToList();
        foreach (var location in locations)
        {
            if (location != activeLocation)
            {
                wrestler.crowdFatigue[location] = Mathf.Max(0, wrestler.crowdFatigue[location] - 5);
            }
        }
    }
}

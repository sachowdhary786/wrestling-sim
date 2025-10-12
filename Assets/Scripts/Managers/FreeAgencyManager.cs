using System.Linq;
using UnityEngine;

public static class FreeAgencyManager
{
    /// <summary>
    /// AI companies evaluate and attempt to sign available free agents.
    /// </summary>
    public static void ProcessFreeAgency(GameData gameData)
    {
        Debug.Log("[Free Agency] Processing free agency period...");

        var freeAgents = gameData.wrestlers.Where(w => w.contract == null && !w.isRetired).ToList();
        if (freeAgents.Count == 0)
        {
            Debug.Log("[Free Agency] No free agents available.");
            return;
        }

        foreach (var company in gameData.companies.Where(c => c.companyType == CompanyType.AI))
        {
            // AI companies will try to sign one person if they are below their roster cap
            if (company.roster.Count < company.rosterCap)
            {
                // Find the best available free agent the company can afford
                var target = FindBestFitFreeAgent(company, freeAgents, gameData);
                if (target != null)
                {
                    SignWrestler(company, target, gameData);
                    // Remove the signed wrestler from the available pool for this cycle
                    freeAgents.Remove(target);
                }
            }
        }
    }

    private static Wrestler FindBestFitFreeAgent(
        Company company,
        System.Collections.Generic.List<Wrestler> freeAgents,
        GameData gameData
    )
    {
        // Filter out wrestlers who are too popular for an indie promotion
        if (company.tier == CompanyTier.Indie)
        {
            freeAgents = freeAgents.Where(w => w.popularity < 65).ToList();
        }

        var affordableAgents = freeAgents.Where(w => (500 * w.popularity) < company.finances);
        if (!affordableAgents.Any())
            return null;

        // AI finds the best wrestler based on its philosophy
        switch (company.bookingPhilosophy)
        {
            case BookingPhilosophy.StarPower:
                return affordableAgents.OrderByDescending(w => w.popularity).FirstOrDefault();
            case BookingPhilosophy.WorkRate:
                return affordableAgents
                    .OrderByDescending(w => w.technical + w.brawling + w.psychology)
                    .FirstOrDefault();
            case BookingPhilosophy.Balanced:
            default:
                // Balanced approach considers a mix of popularity and in-ring skill
                return affordableAgents
                    .OrderByDescending(w => w.popularity + (w.technical / 2))
                    .FirstOrDefault();
        }
    }

    private static void SignWrestler(Company company, Wrestler wrestler, GameData gameData)
    {
        // Create a new contract
        int salary = 500 * wrestler.popularity;
        int duration = 12; // Default 12 months
        wrestler.contract = new Contract(company.id, salary, duration);

        // Add wrestler to roster and deduct finances
        company.roster.Add(wrestler.id);
        company.finances -= salary; // Simplified: just deduct one month for now

        Debug.Log($"[Free Agency] {company.name} has signed {wrestler.name}!");
    }
}

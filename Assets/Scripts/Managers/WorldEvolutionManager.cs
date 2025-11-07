using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class WorldEvolutionManager
{
    public static void ProcessWeeklyDecay(GameData gameData)
    {
        foreach (var wrestler in gameData.wrestlers.Values)
        {
            var locations = wrestler.crowdFatigue.Keys.ToList();
            foreach (var location in locations)
            {
                // Decay fatigue in all locations each week
                wrestler.crowdFatigue[location] = Mathf.Max(0, wrestler.crowdFatigue[location] - 5);
            }
        }
    }

    /// <summary>
    /// Processes yearly events like retirements and new wrestler generation.
    /// </summary>
    public static void ProcessYearlyEvolution(GameData gameData)
    {
        Debug.Log("[World Evolution] Processing yearly world changes...");
        ProcessRetirements(gameData);
        GenerateNewWrestlers(gameData, 5); // Generate 5 new wrestlers each year
        AssignTrainers(gameData);
    }

    private static void ProcessRetirements(GameData gameData)
    {
        var potentialRetirees = gameData.wrestlers.Values.Where(w => !w.isRetired && w.age >= 38).ToList();
        foreach (var wrestler in potentialRetirees)
        {
            float retirementChance = (wrestler.age - 37) * 0.05f; // 5% chance at 38, 10% at 39...
            if (Random.value < retirementChance)
            {
                wrestler.isRetired = true;

                // Adjust salary to a non-active staff rate if they are under contract
                if (wrestler.contract != null)
                {
                    if (gameData.companies.TryGetValue(wrestler.contract.companyId, out var company))
                    {
                        // Adjust salary first, as it applies to both tiers
                        int previousSalary = wrestler.contract.monthlySalary;
                        wrestler.contract.monthlySalary = (int)(previousSalary * 0.25f);

                        if (company.tier == CompanyTier.Major)
                        {
                            // Assign a new role. 25% Agent, 25% Trainer, 20% Doctor, 20% Scout, 10% Head Booker
                            float roleValue = Random.value;
                            StaffRole newRole;
                            if (roleValue < 0.25f) newRole = StaffRole.RoadAgent;
                            else if (roleValue < 0.5f) newRole = StaffRole.Trainer;
                            else if (roleValue < 0.7f) newRole = StaffRole.Doctor;
                            else if (roleValue < 0.9f) newRole = StaffRole.Scout;
                            else newRole = StaffRole.HeadBooker;
                            var staffMember = new CorporateStaff(wrestler.id, newRole);
                            company.corporateStaff.Add(staffMember);
                            Debug.Log($"[Retirement] {wrestler.name} has retired and become a {newRole} at {company.name}. Salary adjusted to {wrestler.contract.monthlySalary:C}.");
                        }
                        else // Indie Promotion
                        {
                            // Transition to a Manager role
                            wrestler.isManager = true;
                            Debug.Log($"[Retirement] {wrestler.name} has retired from in-ring competition and become a Manager at {company.name}. Salary adjusted to {wrestler.contract.monthlySalary:C}.");
                        }
                    }
                }
                else
                {
                    Debug.Log($"[Retirement] {wrestler.name} has retired from in-ring competition at age {wrestler.age}.");
                }
            }
        }
    }

    private static void GenerateNewWrestlers(GameData gameData, int count)
    {
        // Find the best scout in the world to influence the quality of the new generation
        var bestScout = gameData.companies.Values
            .Where(c => c.tier == CompanyTier.Major)
            .SelectMany(c => c.corporateStaff)
            .Where(s => s.role == StaffRole.Scout)
            .OrderByDescending(s => s.talentDiscovery)
            .FirstOrDefault();

        float qualityBonus = 0;
        if (bestScout != null)
        {
            qualityBonus = bestScout.talentDiscovery / 10f; // Best scout can add up to 10 points to base stats
            var scoutInfo = gameData.wrestlers[bestScout.staffId];
            Debug.Log($"[Scouting] The new generation of wrestlers is influenced by the keen eye of {scoutInfo.name}.");
        }

        for (int i = 0; i < count; i++)
        {
            var newWrestler = new Wrestler
            {
                name = GetRandomName(),
                hometown = GetRandomHometown(),
                age = Random.Range(18, 24),
                popularity = Random.Range(20, 45) + (int)qualityBonus,
                charisma = Random.Range(30, 70) + (int)qualityBonus,
                micSkill = Random.Range(20, 60) + (int)qualityBonus,
                psychology = Random.Range(20, 60) + (int)qualityBonus,
                technical = Random.Range(30, 75) + (int)qualityBonus,
                brawling = Random.Range(30, 75) + (int)qualityBonus,
                aerial = Random.Range(30, 75) + (int)qualityBonus,
                stamina = Random.Range(40, 80),
                toughness = Random.Range(40, 80),
                alignment = (Alignment)Random.Range(0, 3)
            };
            gameData.wrestlers.Add(newWrestler.id, newWrestler);
            Debug.Log($"[Newgen] A new wrestler, {newWrestler.name} from {newWrestler.hometown}, has entered the world!");
        }
    }

    private static string GetRandomName()
    {
        string[] firstNames = { "Ace", "Blade", "Jax", "Rex", "Spike", "Vortex", "Rocco", "Blaze", "Cruz", "Zane" };
        string[] lastNames = { "Steel", "Maverick", "Rage", "Storm", "Viper", "Savage", "Fury", "Blade", "Hunter", "Cross" };
        return firstNames[Random.Range(0, firstNames.Length)] + " " + lastNames[Random.Range(0, lastNames.Length)];
    }

    private static string GetRandomHometown()
    {
        string[] towns = { "Detroit, MI", "Las Vegas, NV", "Chicago, IL", "Philadelphia, PA", "Atlanta, GA", "Mexico City, MX", "Toronto, ON, Canada", "Tokyo, Japan" };
        return towns[Random.Range(0, towns.Length)];
    }

    private static void AssignTrainers(GameData gameData)
    {
        foreach (var company in gameData.companies.Values.Where(c => c.tier == CompanyTier.Major))
        {
            var trainers = company.corporateStaff.Where(s => s.role == StaffRole.Trainer).ToList();
            if (!trainers.Any()) continue;

            var trainees = gameData.wrestlers.Values.Where(w => w.contract?.companyId == company.id && w.age < 25 && !w.trainerId.HasValue && !w.isRetired).ToList();

            foreach (var trainee in trainees)
            {
                var trainer = trainers[Random.Range(0, trainers.Count)];
                trainee.trainerId = trainer.staffId;
                var trainerInfo = gameData.wrestlers[trainer.staffId];
                Debug.Log($"[Training] {trainerInfo.name} is now training {trainee.name}.");
            }
        }
    }

    public static void ProcessMonthlyTraining(GameData gameData)
    {
        foreach (var wrestler in gameData.wrestlers.Values.Where(w => w.trainerId.HasValue && !w.isRetired))
        {
            var trainerStaffInfo = gameData.companies.Values
                .SelectMany(c => c.corporateStaff)
                .FirstOrDefault(s => s.staffId == wrestler.trainerId.Value);

            if (trainerStaffInfo == null) continue;

            // The chance to improve is based on the trainer's skill
            float improvementChance = trainerStaffInfo.skillImprovement / 100f;

            if (Random.value < improvementChance)
            {
                // Select a random in-ring skill to improve
                int skillToImprove = Random.Range(0, 5);
                switch (skillToImprove)
                {
                    case 0: wrestler.technical = Mathf.Min(100, wrestler.technical + 1); break;
                    case 1: wrestler.brawling = Mathf.Min(100, wrestler.brawling + 1); break;
                    case 2: wrestler.aerial = Mathf.Min(100, wrestler.aerial + 1); break;
                    case 3: wrestler.psychology = Mathf.Min(100, wrestler.psychology + 1); break;
                    case 4: wrestler.stamina = Mathf.Min(100, wrestler.stamina + 1); break;
                }
                var trainerInfo = gameData.wrestlers[trainerStaffInfo.staffId];
                Debug.Log($"[Training] {wrestler.name} has improved under the guidance of {trainerInfo.name}!");
            }
        }
    }
}

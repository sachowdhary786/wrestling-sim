using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages all companies in the wrestling world, runs the weekly simulation loop, and handles global rankings.
/// </summary>
public class LeagueManager : MonoBehaviour
{
    public List<Company> companies;
    public GameData gameData;
    private int _weekCounter = 0;

    private void Awake()
    {
        // Load all data from JSON files
        gameData = DataLoader.LoadGameData();
        if (gameData != null)
        {
            companies = gameData.companies;
            Debug.Log("[LeagueManager] Game data loaded successfully.");
            UpdateLeagueRankings(); // Show initial rankings
        }
    /// Simulates one week for the entire league.
    /// </summary>
    public void SimulateWeek()
    {
        Debug.Log($"--- Simulating Week ---");

        foreach (var company in companies.Where(c => c.companyType == CompanyType.AI))
        {
            // For AI companies, book and simulate their weekly show
            Debug.Log($"--- Simulating {company.name}'s Week ---");

            float bookingModifier = 0f;
            if (company.delegateBooking && company.tier == CompanyTier.Major)
            {
                var headBooker = company.corporateStaff.FirstOrDefault(s => s.role == StaffRole.HeadBooker);
                if (headBooker != null)
                {
                    // Booker's acuity provides a bonus or penalty to the whole show
                    bookingModifier = (headBooker.bookingAcuity - 50) / 10f; // -5 to +5 modifier
                    var bookerInfo = gameData.wrestlers.First(w => w.id == headBooker.staffId);
                    Debug.Log($"[Booking] Show booked by {bookerInfo.name}. Modifier: {bookingModifier:F1}");
                }
            }

            // 1. Book a show using the BookingManager
            var show = BookingManager.BookShow(company, gameData);

            // 2. Simulate the show using Simple Mode
            foreach (var match in show.matches)
            {
                MatchSimulator.Simulate(match, gameData, MatchSimulationMode.Simple, bookingModifier);
            }

            // 3. Process finances for this show
            FinancialManager.ProcessIndieShowFinances(show, company, gameData);

            // 4. Update company prestige based on the show
            PrestigeCalculator.UpdatePrestige(company, show);
        }

        // Process free agency signings
        FreeAgencyManager.ProcessFreeAgency(gameData);

        // Update league rankings
        UpdateLeagueRankings();

        // Process weekly decay for all wrestlers
        WorldEvolutionManager.ProcessWeeklyDecay(gameData);

        // Process yearly events
        _weekCounter++;
        // Process monthly events
        if (_weekCounter % 4 == 0)
        {
            FinancialManager.ProcessMonthlyFinances(gameData);
        WorldEvolutionManager.ProcessMonthlyTraining(gameData);
        }

        // Process yearly events
        if (_weekCounter % 52 == 0)
        {
            WorldEvolutionManager.ProcessYearlyEvolution(gameData);
        }
    }

    private void UpdateLeagueRankings()
    {
        companies = companies.OrderByDescending(c => c.prestige).ToList();
        Debug.Log("--- League Rankings Updated ---");
        for (int i = 0; i < companies.Count; i++)
        {
            Debug.Log($"{i + 1}. {companies[i].name} (Prestige: {companies[i].prestige})");
        }
    }

}

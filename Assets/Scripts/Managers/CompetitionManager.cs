using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages competition between player's company and AI rival company
/// Tracks ratings war, talent raids, and determines winner
/// </summary>
public class CompetitionManager
{
    public string playerCompanyId = "player_company";
    public string playerCompanyName = "Player Wrestling Entertainment";

    public RivalCompanyAI rivalAI;

    // Competition Tracking
    public int currentWeek = 1;
    public int playerWins = 0;
    public int rivalWins = 0;
    public int draws = 0;

    public List<WeekResult> weeklyResults = new List<WeekResult>();

    // Rosters
    public List<Wrestler> playerRoster = new List<Wrestler>();
    public List<Wrestler> rivalRoster = new List<Wrestler>();
    public List<Wrestler> freeAgents = new List<Wrestler>();

    // Show History
    public List<Show> playerShows = new List<Show>();
    public List<Show> rivalShows = new List<Show>();

    public CompetitionManager(
        GameData gameData,
        string rivalName = "Rival Wrestling Federation",
        BookingPhilosophy rivalPhilosophy = BookingPhilosophy.Balanced
    )
    {
        // Initialize rival AI
        rivalAI = new RivalCompanyAI(rivalName, "rival_company", gameData, rivalPhilosophy);

        Debug.Log($"[COMPETITION] {playerCompanyName} vs {rivalName} - RATINGS WAR BEGINS!");
    }

    /// <summary>
    /// Splits roster between player and rival
    /// </summary>
    public void DivideRoster(GameData gameData, float playerSplit = 0.5f)
    {
        var allWrestlers = gameData.wrestlers.Where(w => w.active).ToList();
        int playerCount = Mathf.FloorToInt(allWrestlers.Count * playerSplit);

        // Sort by popularity and alternate assignment
        var sorted = allWrestlers.OrderByDescending(w => w.popularity).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            if (i < playerCount)
            {
                sorted[i].companyId = playerCompanyId;
                playerRoster.Add(sorted[i]);
            }
            else
            {
                sorted[i].companyId = rivalAI.companyId;
                rivalRoster.Add(sorted[i]);
            }
        }

        Debug.Log(
            $"[COMPETITION] Roster divided: Player {playerRoster.Count} vs Rival {rivalRoster.Count}"
        );
    }

    /// <summary>
    /// Runs a head-to-head week where both companies compete
    /// </summary>
    public WeekResult RunCompetitiveWeek(Show playerShow, GameData gameData, bool sameNight = true)
    {
        Debug.Log($"\n{'=', -60}");
        Debug.Log($"WEEK {currentWeek} - RATINGS WAR!");
        Debug.Log($"{'=', -60}");

        // Player books and runs their show
        Debug.Log($"\n[PLAYER] {playerShow.showName} airs...");
        float playerRating = SimulatePlayerShow(playerShow, gameData);
        playerShows.Add(playerShow);

        // AI rival books and runs their show
        Show rivalShow;
        if (sameNight)
        {
            // AI can see player's booking and counter-program
            rivalShow = rivalAI.BookAndRunShow(
                $"{rivalAI.companyName} Monday Night",
                currentWeek,
                playerShow
            );
        }
        else
        {
            // Different nights, no counter-programming
            rivalShow = rivalAI.BookAndRunShow(
                $"{rivalAI.companyName} Friday Night",
                currentWeek,
                null
            );
        }
        rivalShows.Add(rivalShow);

        // Determine winner
        WeekResult result = new WeekResult
        {
            week = currentWeek,
            playerRating = playerRating,
            rivalRating = rivalShow.averageRating,
            playerShow = playerShow,
            rivalShow = rivalShow,
        };

        float margin = Mathf.Abs(playerRating - rivalShow.averageRating);

        if (playerRating > rivalShow.averageRating + 2) // Player wins by 2+ points
        {
            result.winner = "Player";
            playerWins++;
            Debug.Log(
                $"\n🏆 PLAYER WINS THE WEEK! ({playerRating:F1} vs {rivalShow.averageRating:F1})"
            );
        }
        else if (rivalShow.averageRating > playerRating + 2) // Rival wins by 2+ points
        {
            result.winner = "Rival";
            rivalWins++;
            rivalAI.wins++;
            Debug.Log(
                $"\n💀 RIVAL WINS THE WEEK! ({rivalShow.averageRating:F1} vs {playerRating:F1})"
            );
        }
        else // Too close to call
        {
            result.winner = "Draw";
            draws++;
            Debug.Log($"\n🤝 IT'S A DRAW! ({playerRating:F1} vs {rivalShow.averageRating:F1})");
        }

        weeklyResults.Add(result);

        // Show standings
        Debug.Log(
            $"\n📊 STANDINGS: Player {playerWins}W-{rivalWins}L-{draws}D | Rival {rivalWins}W-{playerWins}L-{draws}D"
        );

        // Weekly maintenance
        WrestlerStateManager.WeeklyReset(gameData);

        // AI adjusts strategy based on performance
        rivalAI.AdjustStrategy();

        // Talent movement (every 4 weeks)
        if (currentWeek % 4 == 0)
        {
            HandleTalentMovement(gameData);
        }

        // AI creates feuds
        rivalAI.TryCreateFeud();

        currentWeek++;

        return result;
    }

    /// <summary>
    /// Simulates player's show (you'd call your own simulation here)
    /// </summary>
    private float SimulatePlayerShow(Show show, GameData gameData)
    {
        if (show.matches == null || show.matches.Count == 0)
        {
            Debug.LogWarning("[PLAYER] No matches booked!");
            return 0f;
        }

        int totalRating = 0;

        foreach (var match in show.matches)
        {
            MatchSimulator.Simulate(match, gameData, MatchSimulationMode.Advanced);
            totalRating += match.rating;

            Debug.Log($"  {GetMatchDescription(match, gameData)} - Rating: {match.rating}/100");

            // Update wrestler states
            foreach (var participantId in match.participants)
            {
                var wrestler = gameData.wrestlers.Find(w => w.id.ToString() == participantId);
                if (wrestler == null)
                    continue;

                bool won = participantId == match.winnerId;
                WrestlerStateManager.UpdateWrestlerAfterMatch(wrestler, match, won, gameData);
            }
        }

        show.averageRating = totalRating / show.matches.Count;
        Debug.Log($"[PLAYER] Show Average: {show.averageRating}/100");

        return show.averageRating;
    }

    /// <summary>
    /// Handles talent raids and free agency
    /// </summary>
    private void HandleTalentMovement(GameData gameData)
    {
        Debug.Log("\n[TALENT MOVEMENT] Contract negotiations phase...");

        // Check for unhappy wrestlers who might jump ship
        CheckForDefections(gameData);

        // AI tries to sign talent
        rivalAI.TrySignTalent(freeAgents, 500000f);

        // Update rosters
        UpdateRosters(gameData);
    }

    /// <summary>
    /// Checks if wrestlers want to leave their company
    /// </summary>
    private void CheckForDefections(GameData gameData)
    {
        // Player's roster - check for unhappy wrestlers
        foreach (var wrestler in playerRoster.ToList())
        {
            if (wrestler.morale < 30 && wrestler.momentum < -30)
            {
                // Very unhappy, might leave
                if (Random.value < 0.3f) // 30% chance
                {
                    Debug.Log($"⚠️ {wrestler.name} is unhappy and becomes a free agent!");
                    wrestler.companyId = "free_agent";
                    playerRoster.Remove(wrestler);
                    freeAgents.Add(wrestler);
                }
            }
        }

        // Rival's roster - AI's wrestlers can also leave
        foreach (var wrestler in rivalRoster.ToList())
        {
            if (wrestler.morale < 30 && wrestler.momentum < -30)
            {
                if (Random.value < 0.3f)
                {
                    Debug.Log(
                        $"💰 {wrestler.name} leaves {rivalAI.companyName} and becomes available!"
                    );
                    wrestler.companyId = "free_agent";
                    rivalRoster.Remove(wrestler);
                    freeAgents.Add(wrestler);
                }
            }
        }
    }

    /// <summary>
    /// Updates roster lists based on company assignments
    /// </summary>
    private void UpdateRosters(GameData gameData)
    {
        playerRoster = gameData.wrestlers.Where(w => w.companyId == playerCompanyId).ToList();
        rivalRoster = gameData.wrestlers.Where(w => w.companyId == rivalAI.companyId).ToList();
        freeAgents = gameData
            .wrestlers.Where(w => w.companyId == "free_agent" || string.IsNullOrEmpty(w.companyId))
            .ToList();

        Debug.Log(
            $"[ROSTERS] Player: {playerRoster.Count} | Rival: {rivalRoster.Count} | Free Agents: {freeAgents.Count}"
        );
    }

    /// <summary>
    /// Generates monthly competition report
    /// </summary>
    public void GenerateMonthlyReport()
    {
        Debug.Log("\n" + new string('=', 60));
        Debug.Log("MONTHLY COMPETITION REPORT");
        Debug.Log(new string('=', 60));

        // Last 4 weeks
        var lastFourWeeks = weeklyResults.TakeLast(4).ToList();

        Debug.Log("\nLast 4 Weeks:");
        foreach (var week in lastFourWeeks)
        {
            string winner =
                week.winner == "Player" ? "🏆 YOU"
                : week.winner == "Rival" ? "💀 THEM"
                : "🤝 DRAW";
            Debug.Log(
                $"  Week {week.week}: {winner} ({week.playerRating:F1} vs {week.rivalRating:F1})"
            );
        }

        // Overall standings
        Debug.Log($"\nOverall Record:");
        Debug.Log($"  You: {playerWins}W - {rivalWins}L - {draws}D");
        Debug.Log($"  {rivalAI.companyName}: {rivalWins}W - {playerWins}L - {draws}D");

        // Average ratings
        float playerAvg = playerShows.Average(s => s.averageRating);
        float rivalAvg = rivalShows.Average(s => s.averageRating);

        Debug.Log($"\nAverage Show Ratings:");
        Debug.Log($"  You: {playerAvg:F1}/100");
        Debug.Log($"  Rival: {rivalAvg:F1}/100");

        // Top performers
        Debug.Log($"\nYour Top Performer:");
        var topPlayer = playerRoster.OrderByDescending(w => w.popularity).FirstOrDefault();
        if (topPlayer != null)
        {
            Debug.Log($"  {topPlayer.name} - Popularity: {topPlayer.popularity}/100");
        }

        Debug.Log($"\nRival's Top Performer:");
        var topRival = rivalRoster.OrderByDescending(w => w.popularity).FirstOrDefault();
        if (topRival != null)
        {
            Debug.Log($"  {topRival.name} - Popularity: {topRival.popularity}/100");
        }

        // AI status
        Debug.Log($"\nRival AI Status:");
        Debug.Log(rivalAI.GetStatusSummary());

        Debug.Log(new string('=', 60) + "\n");
    }

    /// <summary>
    /// Generates season finale report
    /// </summary>
    public void GenerateSeasonReport()
    {
        Debug.Log("\n" + new string('*', 60));
        Debug.Log("🏁 SEASON FINALE - RATINGS WAR CONCLUDED");
        Debug.log(new string('*', 60));

        // Determine overall winner
        string overallWinner;
        if (playerWins > rivalWins)
        {
            overallWinner = "🎉🎉🎉 YOU WIN THE RATINGS WAR! 🎉🎉🎉";
        }
        else if (rivalWins > playerWins)
        {
            overallWinner = $"💀💀💀 {rivalAI.companyName.ToUpper()} WINS! 💀💀💀";
        }
        else
        {
            overallWinner = "🤝 RATINGS WAR ENDS IN A TIE!";
        }

        Debug.Log($"\n{overallWinner}\n");

        // Final stats
        Debug.Log($"Final Record:");
        Debug.Log($"  {playerCompanyName}: {playerWins}W - {rivalWins}L - {draws}D");
        Debug.Log($"  {rivalAI.companyName}: {rivalWins}W - {playerWins}L - {draws}D");

        Debug.Log($"\nTotal Shows:");
        Debug.Log($"  You ran: {playerShows.Count} shows");
        Debug.Log($"  Rival ran: {rivalShows.Count} shows");

        Debug.Log($"\nAverage Ratings:");
        Debug.Log($"  You: {playerShows.Average(s => s.averageRating):F1}/100");
        Debug.Log($"  Rival: {rivalShows.Average(s => s.averageRating):F1}/100");

        // Longest win streaks
        int playerStreak = CalculateLongestWinStreak(true);
        int rivalStreak = CalculateLongestWinStreak(false);

        Debug.Log($"\nLongest Win Streaks:");
        Debug.Log($"  You: {playerStreak} weeks");
        Debug.Log($"  Rival: {rivalStreak} weeks");

        Debug.Log("\n" + new string('*', 60) + "\n");
    }

    /// <summary>
    /// Calculates longest consecutive win streak
    /// </summary>
    private int CalculateLongestWinStreak(bool forPlayer)
    {
        int maxStreak = 0;
        int currentStreak = 0;
        string targetWinner = forPlayer ? "Player" : "Rival";

        foreach (var result in weeklyResults)
        {
            if (result.winner == targetWinner)
            {
                currentStreak++;
                maxStreak = Mathf.Max(maxStreak, currentStreak);
            }
            else
            {
                currentStreak = 0;
            }
        }

        return maxStreak;
    }

    /// <summary>
    /// Helper to get match description
    /// </summary>
    private string GetMatchDescription(Match match, GameData gameData)
    {
        var wrestlers = match
            .participants.Select(id => gameData.wrestlers.Find(w => w.id.ToString() == id))
            .Where(w => w != null)
            .Select(w => w.name)
            .ToList();

        string desc = string.Join(" vs ", wrestlers);
        if (match.titleMatch)
            desc += " (TITLE)";

        return desc;
    }
}

/// <summary>
/// Results from a week's competition
/// </summary>
public class WeekResult
{
    public int week;
    public float playerRating;
    public float rivalRating;
    public string winner; // "Player", "Rival", or "Draw"
    public Show playerShow;
    public Show rivalShow;
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Example implementation showing how to use BookingManager and WrestlerStateManager
/// for a full season simulation
/// </summary>
public class SeasonSimulatorExample : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int weeksPerSeason = 52;
    [SerializeField] private int matchesPerShow = 6;
    [SerializeField] private bool autoSimulate = false;
    
    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private bool showStateReports = true;
    
    private GameData gameData;
    private int currentWeek = 1;
    private List<Show> completedShows = new List<Show>();

    void Start()
    {
        InitializeGameData();
    }

    void Update()
    {
        // Press SPACE to simulate one week
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SimulateWeek();
        }
        
        // Press R to generate state report
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateStateReport();
        }
    }

    /// <summary>
    /// Initialize game data with wrestlers, titles, feuds, etc.
    /// </summary>
    private void InitializeGameData()
    {
        gameData = new GameData();
        
        // Load from JSON or create test data
        LoadOrCreateWrestlers();
        LoadOrCreateTitles();
        LoadOrCreateFeuds();
        LoadOrCreateReferees();
        
        Debug.Log($"[SEASON] Initialized with {gameData.wrestlers.Count} wrestlers, " +
                  $"{gameData.titles.Count} titles, {gameData.feuds.Count} feuds");
    }

    /// <summary>
    /// Simulates one week of shows
    /// </summary>
    public void SimulateWeek()
    {
        Debug.Log($"==================== WEEK {currentWeek} ====================");
        
        // Book and simulate Monday show
        Show mondayShow = CreateShow("Monday Night Wrestling", "WWE Arena");
        mondayShow = BookingManager.BookShow(mondayShow, gameData, matchesPerShow);
        SimulateShow(mondayShow);
        completedShows.Add(mondayShow);
        
        // Book and simulate Friday show
        Show fridayShow = CreateShow("Friday Fight Night", "Smackdown Arena");
        fridayShow = BookingManager.BookShow(fridayShow, gameData, matchesPerShow - 1);
        SimulateShow(fridayShow);
        completedShows.Add(fridayShow);
        
        // Weekly maintenance
        WrestlerStateManager.WeeklyReset(gameData);
        UpdateFeudHeat();
        CheckInjuryRecovery();
        
        // Monthly maintenance
        if (currentWeek % 4 == 0)
        {
            WrestlerStateManager.MonthlyReset(gameData);
            GenerateMonthlyReport();
        }
        
        // Show top performers
        if (showStateReports)
        {
            ShowTopPerformers();
        }
        
        currentWeek++;
        
        // Check season end
        if (currentWeek > weeksPerSeason)
        {
            Debug.Log("=== SEASON COMPLETE ===");
            GenerateSeasonSummary();
        }
    }

    /// <summary>
    /// Simulates all matches in a show
    /// </summary>
    private void SimulateShow(Show show)
    {
        Debug.Log($"\n--- {show.showName} ---");
        int totalRating = 0;
        
        for (int i = 0; i < show.matches.Count; i++)
        {
            var match = show.matches[i];
            
            // Determine match position
            string position = i == 0 ? "OPENER" : 
                            i == show.matches.Count - 1 ? "MAIN EVENT" : 
                            "MIDCARD";
            
            // Simulate match
            MatchSimulator.Simulate(match, gameData, MatchSimulationMode.Advanced);
            totalRating += match.rating;
            
            // Get participant names
            var wrestlers = GetMatchWrestlers(match);
            var winner = gameData.wrestlers.Find(w => w.id.ToString() == match.winnerId);
            
            if (verboseLogging)
            {
                string participants = string.Join(" vs ", wrestlers.Select(w => w.name));
                Debug.Log($"  [{position}] {participants}");
                Debug.Log($"    Winner: {winner?.name ?? "Unknown"} via {match.finishType}");
                Debug.Log($"    Rating: {match.rating}/100");
            }
            
            // Update wrestler states
            foreach (var wrestler in wrestlers)
            {
                bool won = wrestler.id.ToString() == match.winnerId;
                WrestlerStateManager.UpdateWrestlerAfterMatch(wrestler, match, won, gameData);
                
                if (verboseLogging && wrestler.injured)
                {
                    Debug.Log($"    ⚠️ {wrestler.name} injured! ({wrestler.injuryType})");
                }
            }
        }
        
        show.averageRating = totalRating / show.matches.Count;
        Debug.Log($"\n{show.showName} Average Rating: {show.averageRating}/100\n");
    }

    /// <summary>
    /// Creates a new show
    /// </summary>
    private Show CreateShow(string name, string location)
    {
        return new Show
        {
            id = $"show_{currentWeek}_{name.Replace(" ", "_")}",
            showName = name,
            date = $"Week {currentWeek}",
            companyId = "default",
            matches = new List<Match>()
        };
    }

    /// <summary>
    /// Gets wrestlers participating in a match
    /// </summary>
    private List<Wrestler> GetMatchWrestlers(Match match)
    {
        return match.participants
            .Select(id => gameData.wrestlers.Find(w => w.id.ToString() == id))
            .Where(w => w != null)
            .ToList();
    }

    /// <summary>
    /// Shows top performing wrestlers
    /// </summary>
    private void ShowTopPerformers()
    {
        Debug.Log("\n=== TOP 5 WRESTLERS ===");
        
        var topWrestlers = gameData.wrestlers
            .Where(w => w.active && !w.injured)
            .OrderByDescending(w => w.popularity + (w.momentum / 2))
            .Take(5);
        
        int rank = 1;
        foreach (var wrestler in topWrestlers)
        {
            Debug.Log($"{rank}. {WrestlerStateManager.GetStateReport(wrestler)}");
            rank++;
        }
    }

    /// <summary>
    /// Updates feud heat over time
    /// </summary>
    private void UpdateFeudHeat()
    {
        foreach (var feud in gameData.feuds)
        {
            if (!feud.active) continue;
            
            // Feuds decay heat slowly if not used
            feud.heat = Mathf.Max(0, feud.heat - 2);
            feud.durationWeeks++;
            
            // End feuds that have gone too long or lost heat
            if (feud.durationWeeks > 12 || feud.heat < 20)
            {
                feud.active = false;
                Debug.Log($"[FEUD] Ended feud (ID: {feud.id}) after {feud.durationWeeks} weeks");
            }
        }
    }

    /// <summary>
    /// Checks and updates injured wrestlers
    /// </summary>
    private void CheckInjuryRecovery()
    {
        foreach (var wrestler in gameData.wrestlers)
        {
            if (!wrestler.injured) continue;
            
            wrestler.recoveryWeeksRemaining--;
            
            if (wrestler.recoveryWeeksRemaining <= 0)
            {
                wrestler.injured = false;
                wrestler.injurySeverity = 0;
                wrestler.injuryType = null;
                wrestler.morale += 10; // Happy to be back
                
                Debug.Log($"[INJURY] {wrestler.name} has returned from injury!");
            }
        }
    }

    /// <summary>
    /// Generates a monthly summary report
    /// </summary>
    private void GenerateMonthlyReport()
    {
        Debug.Log("\n========== MONTHLY REPORT ==========");
        
        // Top rated shows
        var topShows = completedShows
            .Where(s => s.averageRating > 0)
            .OrderByDescending(s => s.averageRating)
            .Take(3);
        
        Debug.Log("Top Shows:");
        foreach (var show in topShows)
        {
            Debug.Log($"  {show.showName} - {show.averageRating}/100");
        }
        
        // Most popular wrestlers
        var topPopular = gameData.wrestlers
            .OrderByDescending(w => w.popularity)
            .Take(3);
        
        Debug.Log("\nMost Popular:");
        foreach (var wrestler in topPopular)
        {
            Debug.Log($"  {wrestler.name} - {wrestler.popularity}/100");
        }
        
        // Hottest feuds
        var hotFeuds = gameData.feuds
            .Where(f => f.active)
            .OrderByDescending(f => f.heat)
            .Take(3);
        
        Debug.Log("\nHottest Feuds:");
        foreach (var feud in hotFeuds)
        {
            var participants = string.Join(" vs ", 
                feud.participants.Select(id => 
                    gameData.wrestlers.Find(w => w.id.ToString() == id)?.name ?? "Unknown"
                ));
            Debug.Log($"  {participants} - Heat: {feud.heat}/100");
        }
        
        Debug.Log("===================================\n");
    }

    /// <summary>
    /// Generates season summary
    /// </summary>
    private void GenerateSeasonSummary()
    {
        Debug.Log("\n********** SEASON SUMMARY **********");
        
        float avgShowRating = completedShows.Average(s => s.averageRating);
        Debug.Log($"Total Shows: {completedShows.Count}");
        Debug.Log($"Average Show Rating: {avgShowRating:F1}/100");
        
        // Wrestler of the year
        var wrestlerOfYear = gameData.wrestlers
            .OrderByDescending(w => w.popularity + w.momentum)
            .First();
        
        Debug.Log($"\nWrestler of the Year: {wrestlerOfYear.name}");
        Debug.Log(WrestlerStateManager.GetStateReport(wrestlerOfYear));
        
        Debug.Log("\n***********************************\n");
    }

    /// <summary>
    /// Generates detailed state report for all wrestlers
    /// </summary>
    [ContextMenu("Generate State Report")]
    public void GenerateStateReport()
    {
        Debug.Log("\n========== FULL ROSTER STATE REPORT ==========");
        
        var sortedWrestlers = gameData.wrestlers
            .OrderByDescending(w => w.popularity)
            .ToList();
        
        foreach (var wrestler in sortedWrestlers)
        {
            Debug.Log(WrestlerStateManager.GetStateReport(wrestler));
        }
        
        Debug.Log("==============================================\n");
    }

    // ========================================
    // TEST DATA CREATION
    // ========================================
    
    private void LoadOrCreateWrestlers()
    {
        // TODO: Load from JSON file
        // For now, create test wrestlers
        
        gameData.wrestlers = new List<Wrestler>
        {
            CreateTestWrestler(1, "John Cena", 95, 90, Alignment.Face),
            CreateTestWrestler(2, "Randy Orton", 90, 85, Alignment.Heel),
            CreateTestWrestler(3, "AJ Styles", 85, 80, Alignment.Face),
            CreateTestWrestler(4, "Seth Rollins", 85, 75, Alignment.Neutral),
            CreateTestWrestler(5, "Roman Reigns", 90, 95, Alignment.Face),
            CreateTestWrestler(6, "Drew McIntyre", 80, 70, Alignment.Face),
            CreateTestWrestler(7, "Bobby Lashley", 75, 85, Alignment.Heel),
            CreateTestWrestler(8, "Finn Balor", 80, 75, Alignment.Face),
            CreateTestWrestler(9, "Samoa Joe", 75, 80, Alignment.Heel),
            CreateTestWrestler(10, "Kevin Owens", 80, 70, Alignment.Heel),
        };
    }
    
    private Wrestler CreateTestWrestler(int id, string name, int popularity, int overall, Alignment alignment)
    {
        return new Wrestler
        {
            id = id,
            name = name,
            popularity = popularity,
            morale = 70,
            fatigue = 0,
            momentum = 0,
            alignment = alignment,
            technical = overall + Random.Range(-10, 10),
            brawling = overall + Random.Range(-10, 10),
            aerial = overall + Random.Range(-15, 5),
            psychology = overall + Random.Range(-5, 15),
            charisma = popularity,
            stamina = 75,
            toughness = 70,
            active = true,
            injured = false
        };
    }
    
    private void LoadOrCreateTitles()
    {
        gameData.titles = new List<Title>
        {
            new Title
            {
                id = 1,
                name = "World Heavyweight Championship",
                currentChampion = "1" // John Cena
            },
            new Title
            {
                id = 2,
                name = "Intercontinental Championship",
                currentChampion = "3" // AJ Styles
            }
        };
    }
    
    private void LoadOrCreateFeuds()
    {
        gameData.feuds = new List<Feud>
        {
            new Feud
            {
                id = "feud_001",
                participants = new List<string> { "1", "2" }, // Cena vs Orton
                heat = 85,
                durationWeeks = 2,
                active = true
            },
            new Feud
            {
                id = "feud_002",
                participants = new List<string> { "5", "7" }, // Reigns vs Lashley
                heat = 70,
                durationWeeks = 1,
                active = true
            }
        };
    }
    
    private void LoadOrCreateReferees()
    {
        gameData.referees = RefereeManager.CreateDefaultReferees();
    }
}

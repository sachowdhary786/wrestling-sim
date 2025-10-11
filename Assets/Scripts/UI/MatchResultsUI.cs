using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying match simulation results
/// Subscribe to MatchResultsEvent to receive updates
/// </summary>
public class MatchResultsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button closeButton;

    [Header("Bulk Simulation UI")]
    [SerializeField] private GameObject bulkProgressPanel;
    [SerializeField] private TextMeshProUGUI bulkProgressText;
    [SerializeField] private Slider bulkProgressBar;
    [SerializeField] private TextMeshProUGUI bulkSummaryText;

    [Header("Benchmark UI")]
    [SerializeField] private GameObject benchmarkPanel;
    [SerializeField] private TextMeshProUGUI benchmarkText;

    [Header("Settings")]
    [SerializeField] private bool autoShowResults = true;
    [SerializeField] private bool showInjuryAlerts = true;
    [SerializeField] private bool showRatingStars = true;

    private void OnEnable()
    {
        // Subscribe to events
        MatchResultsEvent.OnMatchCompleted += HandleMatchCompleted;
        MatchResultsEvent.OnBulkProgress += HandleBulkProgress;
        MatchResultsEvent.OnBulkCompleted += HandleBulkCompleted;
        MatchResultsEvent.OnBenchmarkCompleted += HandleBenchmarkCompleted;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        MatchResultsEvent.OnMatchCompleted -= HandleMatchCompleted;
        MatchResultsEvent.OnBulkProgress -= HandleBulkProgress;
        MatchResultsEvent.OnBulkCompleted -= HandleBulkCompleted;
        MatchResultsEvent.OnBenchmarkCompleted -= HandleBenchmarkCompleted;
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideAllPanels);
        }

        HideAllPanels();
    }

    // ========================================
    // EVENT HANDLERS
    // ========================================

    private void HandleMatchCompleted(MatchResult result)
    {
        if (autoShowResults && resultsPanel != null)
        {
            ShowMatchResult(result);
        }

        // Log to console as fallback
        Debug.Log(FormatMatchResultLog(result));
    }

    private void HandleBulkProgress(BulkSimulationProgress progress)
    {
        if (bulkProgressPanel != null)
        {
            bulkProgressPanel.SetActive(true);
            
            if (bulkProgressBar != null)
            {
                bulkProgressBar.value = progress.percentComplete / 100f;
            }

            if (bulkProgressText != null)
            {
                bulkProgressText.text = $"Simulating Matches: {progress.completedMatches}/{progress.totalMatches}\n" +
                                       $"{progress.percentComplete:F1}% Complete\n" +
                                       $"Est. Time Remaining: {progress.estimatedTimeRemaining:F1}s";
            }
        }
    }

    private void HandleBulkCompleted(BulkSimulationSummary summary)
    {
        if (bulkProgressPanel != null)
        {
            bulkProgressPanel.SetActive(false);
        }

        if (bulkSummaryText != null && resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            bulkSummaryText.text = FormatBulkSummary(summary);
        }

        Debug.Log(FormatBulkSummaryLog(summary));
    }

    private void HandleBenchmarkCompleted(BenchmarkResult result)
    {
        if (benchmarkPanel != null)
        {
            benchmarkPanel.SetActive(true);
            
            if (benchmarkText != null)
            {
                benchmarkText.text = FormatBenchmarkResult(result);
            }
        }

        Debug.Log(FormatBenchmarkLog(result));
    }

    // ========================================
    // DISPLAY METHODS
    // ========================================

    public void ShowMatchResult(MatchResult result)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        if (resultsText != null)
        {
            resultsText.text = FormatMatchResult(result);
        }
    }

    public void HideAllPanels()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (bulkProgressPanel != null) bulkProgressPanel.SetActive(false);
        if (benchmarkPanel != null) benchmarkPanel.SetActive(false);
    }

    // ========================================
    // FORMATTING METHODS
    // ========================================

    private string FormatMatchResult(MatchResult result)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=== MATCH RESULT ===");
        sb.AppendLine();
        
        // Participants
        sb.AppendLine("<b>Match:</b>");
        sb.AppendLine(string.Join(" vs ", result.participantNames));
        sb.AppendLine();
        
        // Winner
        sb.AppendLine($"<b>Winner:</b> <color=green>{result.winnerName}</color>");
        sb.AppendLine($"<b>Finish:</b> {result.finishType}");
        sb.AppendLine();
        
        // Rating
        string stars = showRatingStars ? GetRatingStars(result.rating) : "";
        sb.AppendLine($"<b>Rating:</b> {result.rating}/100 {stars}");
        sb.AppendLine();
        
        // Title
        if (result.wasTitleMatch)
        {
            sb.AppendLine($"<color=yellow>★ {result.titleName} Title Match ★</color>");
            sb.AppendLine();
        }
        
        // Events
        if (result.events != null && result.events.Count > 0)
        {
            sb.AppendLine("<b>Match Events:</b>");
            foreach (var evt in result.events)
            {
                string icon = evt.eventType == "Injury" ? "⚠️" : "•";
                sb.AppendLine($"{icon} {evt.wrestlerName}: {evt.description}");
            }
            sb.AppendLine();
        }
        
        // Technical info
        sb.AppendLine($"<size=10><i>Mode: {result.simulationMode} | Time: {result.simulationTime:F4}s</i></size>");
        
        return sb.ToString();
    }

    private string FormatBulkSummary(BulkSimulationSummary summary)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=== BULK SIMULATION COMPLETE ===");
        sb.AppendLine();
        sb.AppendLine($"<b>Total Matches:</b> {summary.totalMatches}");
        sb.AppendLine($"<b>Total Time:</b> {summary.totalTime:F2}s");
        sb.AppendLine($"<b>Avg Time/Match:</b> {summary.averageTimePerMatch:F4}s");
        sb.AppendLine($"<b>Mode:</b> {summary.mode}");
        sb.AppendLine();
        sb.AppendLine("<b>Statistics:</b>");
        sb.AppendLine($"• Average Rating: {summary.averageRating}/100");
        sb.AppendLine($"• Highest Rating: {summary.highestRating}/100");
        sb.AppendLine($"• Lowest Rating: {summary.lowestRating}/100");
        sb.AppendLine($"• Total Injuries: {summary.totalInjuries}");
        
        return sb.ToString();
    }

    private string FormatBenchmarkResult(BenchmarkResult result)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=== BENCHMARK RESULTS ===");
        sb.AppendLine();
        sb.AppendLine($"<b>Iterations:</b> {result.iterations}");
        sb.AppendLine();
        sb.AppendLine("<b>Simple Mode:</b>");
        sb.AppendLine($"  Total: {result.simpleModeTotalTime:F3}s");
        sb.AppendLine($"  Average: {result.simpleModeAvgTime:F5}s/match");
        sb.AppendLine();
        sb.AppendLine("<b>Advanced Mode:</b>");
        sb.AppendLine($"  Total: {result.advancedModeTotalTime:F3}s");
        sb.AppendLine($"  Average: {result.advancedModeAvgTime:F5}s/match");
        sb.AppendLine();
        sb.AppendLine($"<color=green><b>Speed Improvement: {result.speedMultiplier:F2}x</b></color>");
        sb.AppendLine("(Simple mode is faster)");
        
        return sb.ToString();
    }

    // ========================================
    // LOG FORMATTING (for Debug.Log fallback)
    // ========================================

    private string FormatMatchResultLog(MatchResult result)
    {
        return $"[MATCH RESULT] {string.Join(" vs ", result.participantNames)} | " +
               $"Winner: {result.winnerName} ({result.finishType}) | " +
               $"Rating: {result.rating}/100 | " +
               $"Mode: {result.simulationMode} | " +
               $"Time: {result.simulationTime:F4}s";
    }

    private string FormatBulkSummaryLog(BulkSimulationSummary summary)
    {
        return $"[BULK SIMULATION] {summary.totalMatches} matches completed in {summary.totalTime:F2}s " +
               $"({summary.averageTimePerMatch:F4}s/match) | " +
               $"Avg Rating: {summary.averageRating} | " +
               $"Injuries: {summary.totalInjuries} | " +
               $"Mode: {summary.mode}";
    }

    private string FormatBenchmarkLog(BenchmarkResult result)
    {
        return $"[BENCHMARK] {result.iterations} iterations | " +
               $"Simple: {result.simpleModeAvgTime:F5}s/match | " +
               $"Advanced: {result.advancedModeAvgTime:F5}s/match | " +
               $"Speed: {result.speedMultiplier:F2}x";
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private string GetRatingStars(int rating)
    {
        if (rating >= 90) return "★★★★★";
        if (rating >= 80) return "★★★★☆";
        if (rating >= 70) return "★★★☆☆";
        if (rating >= 60) return "★★☆☆☆";
        if (rating >= 50) return "★☆☆☆☆";
        return "☆☆☆☆☆";
    }

#if UNITY_EDITOR
    [ContextMenu("Test Single Match Event")]
    private void TestSingleMatchEvent()
    {
        var testResult = new MatchResult
        {
            winnerName = "John Cena",
            rating = 85,
            finishType = "Pinfall",
            simulationMode = MatchSimulationMode.Advanced,
            simulationTime = 0.0234f,
            participantNames = new System.Collections.Generic.List<string> { "John Cena", "Randy Orton" },
            wasTitleMatch = true,
            titleName = "WWE Championship",
            events = new System.Collections.Generic.List<MatchEvent>()
        };
        
        MatchResultsEvent.BroadcastMatchResult(testResult);
    }

    [ContextMenu("Test Bulk Complete Event")]
    private void TestBulkCompleteEvent()
    {
        var testSummary = new BulkSimulationSummary
        {
            totalMatches = 50,
            totalTime = 2.5f,
            averageTimePerMatch = 0.05f,
            mode = MatchSimulationMode.Simple,
            averageRating = 72,
            highestRating = 95,
            lowestRating = 45,
            totalInjuries = 3
        };
        
        MatchResultsEvent.BroadcastBulkComplete(testSummary);
    }
#endif
}

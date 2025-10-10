using System;
using System.Collections.Generic;

/// <summary>
/// Tracks detailed statistics for a referee's career
/// </summary>
[Serializable]
public class RefereeStats
{
    public string refereeId;
    
    // Match Statistics
    public int totalMatches;
    public int titleMatches;
    public int mainEventMatches;
    public int hardcoreMatches;
    
    // Finish Type Statistics
    public int pinfalls;
    public int submissions;
    public int knockouts;
    public int disqualifications;
    public int countOuts;
    public int controversialFinishes;
    public int botchedFinishes;
    
    // Quality Metrics
    public float averageMatchRating;
    public int highestRatedMatch;
    public int lowestRatedMatch;
    
    // Incidents
    public int timesKnockedOut;
    public int timesBumped;
    public int controversies;
    public int perfectMatches; // No incidents, high rating
    
    // Reputation
    public int reputation; // 0-100, builds over time
    public List<string> achievements = new List<string>();
    
    // Recent Performance (last 10 matches)
    public List<int> recentMatchRatings = new List<int>();
    
    public RefereeStats(string refereeId)
    {
        this.refereeId = refereeId;
        this.reputation = 50; // Start neutral
    }

    /// <summary>
    /// Records a match officiated by this referee
    /// </summary>
    public void RecordMatch(Match match, bool wasKnockedOut = false, bool wasBumped = false)
    {
        totalMatches++;
        
        if (match.titleMatch)
            titleMatches++;
        
        // Track match type
        if (IsHardcoreMatch(match.matchType))
            hardcoreMatches++;
        
        // Track finish type
        switch (match.finishType)
        {
            case "Pinfall":
                pinfalls++;
                break;
            case "Submission":
                submissions++;
                break;
            case "Knockout":
                knockouts++;
                break;
            case "DQ":
                disqualifications++;
                break;
            case "Count Out":
                countOuts++;
                break;
            case "Controversial Finish":
                controversialFinishes++;
                controversies++;
                break;
            case "Botched Finish":
                botchedFinishes++;
                controversies++;
                break;
        }
        
        // Track rating
        UpdateAverageRating(match.rating);
        recentMatchRatings.Add(match.rating);
        if (recentMatchRatings.Count > 10)
            recentMatchRatings.RemoveAt(0);
        
        if (match.rating > highestRatedMatch)
            highestRatedMatch = match.rating;
        
        if (lowestRatedMatch == 0 || match.rating < lowestRatedMatch)
            lowestRatedMatch = match.rating;
        
        // Track incidents
        if (wasKnockedOut)
            timesKnockedOut++;
        if (wasBumped)
            timesBumped++;
        
        // Check for perfect match
        if (match.rating >= 85 && !wasKnockedOut && !wasBumped && 
            match.finishType != "Controversial Finish" && match.finishType != "Botched Finish")
        {
            perfectMatches++;
        }
        
        // Update reputation
        UpdateReputation(match.rating, wasKnockedOut, wasBumped);
    }

    private void UpdateAverageRating(int newRating)
    {
        averageMatchRating = ((averageMatchRating * (totalMatches - 1)) + newRating) / totalMatches;
    }

    private void UpdateReputation(int matchRating, bool wasKnockedOut, bool wasBumped)
    {
        // Good performance increases reputation
        if (matchRating >= 80)
            reputation = Math.Min(100, reputation + 1);
        else if (matchRating >= 70)
            reputation = Math.Min(100, reputation + 0);
        else if (matchRating < 50)
            reputation = Math.Max(0, reputation - 1);
        
        // Incidents decrease reputation
        if (wasKnockedOut)
            reputation = Math.Max(0, reputation - 2);
        if (wasBumped)
            reputation = Math.Max(0, reputation - 1);
    }

    /// <summary>
    /// Gets the referee's current form (based on recent matches)
    /// </summary>
    public string GetCurrentForm()
    {
        if (recentMatchRatings.Count < 3)
            return "Unknown";
        
        float recentAvg = 0;
        foreach (int rating in recentMatchRatings)
            recentAvg += rating;
        recentAvg /= recentMatchRatings.Count;
        
        if (recentAvg >= 80) return "Excellent";
        if (recentAvg >= 70) return "Good";
        if (recentAvg >= 60) return "Average";
        if (recentAvg >= 50) return "Poor";
        return "Struggling";
    }

    /// <summary>
    /// Gets the referee's specialty based on match history
    /// </summary>
    public string GetSpecialty()
    {
        if (totalMatches < 10)
            return "Developing";
        
        float hardcorePercent = (float)hardcoreMatches / totalMatches;
        float titlePercent = (float)titleMatches / totalMatches;
        
        if (hardcorePercent > 0.4f)
            return "Hardcore Specialist";
        if (titlePercent > 0.3f)
            return "Main Event Specialist";
        
        return "All-Rounder";
    }

    private bool IsHardcoreMatch(string matchType)
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

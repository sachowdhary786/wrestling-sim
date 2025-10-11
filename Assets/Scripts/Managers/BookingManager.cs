using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AI-driven booking manager that creates match cards for shows
/// Considers feuds, wrestler states, and card balance
/// </summary>
public static class BookingManager
{
    /// <summary>
    /// Books a complete show with AI logic
    /// </summary>
    public static Show BookShow(
        Show show, 
        GameData data, 
        int targetMatchCount = 6,
        BookingPriority priority = BookingPriority.Balanced
    )
    {
        Debug.Log($"[BOOKING] Starting booking for {show.showName}...");
        
        // Initialize match list
        show.matches = new List<Match>();
        
        // Get available wrestlers
        var availableWrestlers = GetAvailableWrestlers(data);
        
        if (availableWrestlers.Count < 2)
        {
            Debug.LogWarning("[BOOKING] Not enough available wrestlers!");
            return show;
        }
        
        // Track used wrestlers to avoid overuse
        HashSet<string> usedWrestlers = new HashSet<string>();
        
        // 1. MAIN EVENT - Prioritize active feuds
        var mainEvent = BookMainEvent(data, availableWrestlers, usedWrestlers, show);
        if (mainEvent != null)
        {
            show.matches.Add(mainEvent);
            Debug.Log($"[BOOKING] Main Event booked: {GetMatchDescription(mainEvent, data)}");
        }
        
        // 2. SEMI-MAIN / TITLE MATCHES
        var semiMain = BookSemiMain(data, availableWrestlers, usedWrestlers, show);
        if (semiMain != null && show.matches.Count < targetMatchCount)
        {
            show.matches.Insert(0, semiMain); // Add before main event
            Debug.Log($"[BOOKING] Semi-Main booked: {GetMatchDescription(semiMain, data)}");
        }
        
        // 3. MIDCARD MATCHES
        int midcardCount = targetMatchCount - show.matches.Count - 1; // Leave room for opener
        for (int i = 0; i < midcardCount; i++)
        {
            var midcard = BookMidcardMatch(data, availableWrestlers, usedWrestlers, show);
            if (midcard != null)
            {
                show.matches.Insert(0, midcard); // Add to beginning (will be in correct order)
                Debug.Log($"[BOOKING] Midcard {i+1} booked: {GetMatchDescription(midcard, data)}");
            }
        }
        
        // 4. OPENER - High energy match
        var opener = BookOpenerMatch(data, availableWrestlers, usedWrestlers, show);
        if (opener != null)
        {
            show.matches.Insert(0, opener);
            Debug.Log($"[BOOKING] Opener booked: {GetMatchDescription(opener, data)}");
        }
        
        Debug.Log($"[BOOKING] Show booked with {show.matches.Count} matches");
        return show;
    }

    // ========================================
    // MAIN EVENT BOOKING
    // ========================================
    
    private static Match BookMainEvent(GameData data, List<Wrestler> available, HashSet<string> used, Show show)
    {
        // Priority 1: Active feuds with highest heat
        var feudMatch = BookFeudMatch(data, available, used, show, minHeat: 70);
        if (feudMatch != null)
        {
            feudMatch.titleMatch = ShouldBeAtelMatch(feudMatch, data, true);
            return feudMatch;
        }
        
        // Priority 2: Title match with top talent
        var titleMatch = BookTitleMatch(data, available, used, show, isMainEvent: true);
        if (titleMatch != null)
            return titleMatch;
        
        // Priority 3: Top two popular wrestlers
        var topMatch = BookTopTalentMatch(data, available, used, show);
        return topMatch;
    }

    // ========================================
    // SEMI-MAIN BOOKING
    // ========================================
    
    private static Match BookSemiMain(GameData data, List<Wrestler> available, HashSet<string> used, Show show)
    {
        // Look for mid-tier feuds or title matches
        var feudMatch = BookFeudMatch(data, available, used, show, minHeat: 50);
        if (feudMatch != null)
        {
            feudMatch.titleMatch = ShouldBeTitleMatch(feudMatch, data, false);
            return feudMatch;
        }
        
        // Or book a title match
        var titleMatch = BookTitleMatch(data, available, used, show, isMainEvent: false);
        if (titleMatch != null)
            return titleMatch;
        
        return null;
    }

    // ========================================
    // MIDCARD BOOKING
    // ========================================
    
    private static Match BookMidcardMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show)
    {
        // Check for any active feuds first
        var feudMatch = BookFeudMatch(data, available, used, show, minHeat: 30);
        if (feudMatch != null)
            return feudMatch;
        
        // Book based on alignment conflicts
        var alignmentMatch = BookAlignmentMatch(data, available, used, show);
        if (alignmentMatch != null)
            return alignmentMatch;
        
        // Random midcard match
        return BookRandomMatch(data, available, used, show, MatchSlot.Midcard);
    }

    // ========================================
    // OPENER BOOKING
    // ========================================
    
    private static Match BookOpenerMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show)
    {
        // Openers should be high-energy wrestlers
        var highEnergyWrestlers = available
            .Where(w => !used.Contains(w.id.ToString()))
            .Where(w => w.aerial > 70 || w.charisma > 75) // High flyers or charismatic
            .Where(w => w.fatigue < 50)
            .OrderByDescending(w => w.aerial + w.charisma)
            .Take(4)
            .ToList();
        
        if (highEnergyWrestlers.Count >= 2)
        {
            var match = CreateMatch(highEnergyWrestlers.Take(2).ToList(), data, show);
            match.matchType = "Singles";
            MarkWrestlersAsUsed(match, used);
            return match;
        }
        
        return BookRandomMatch(data, available, used, show, MatchSlot.Opener);
    }

    // ========================================
    // SPECIALIZED BOOKING METHODS
    // ========================================
    
    private static Match BookFeudMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show, int minHeat = 0)
    {
        if (data.feuds == null || data.feuds.Count == 0)
            return null;
        
        // Find best active feud with available wrestlers
        var bestFeud = data.feuds
            .Where(f => f.active && f.heat >= minHeat)
            .OrderByDescending(f => f.heat)
            .FirstOrDefault(f => HasAvailableParticipants(f, available, used, 2));
        
        if (bestFeud == null)
            return null;
        
        // Get feud participants who are available
        var feudWrestlers = GetFeudWrestlers(bestFeud, data, available, used);
        if (feudWrestlers.Count < 2)
            return null;
        
        var match = CreateMatch(feudWrestlers.Take(2).ToList(), data, show);
        match.matchType = DetermineMatchType(feudWrestlers, bestFeud);
        
        MarkWrestlersAsUsed(match, used);
        return match;
    }
    
    private static Match BookTitleMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show, bool isMainEvent)
    {
        if (data.titles == null || data.titles.Count == 0)
            return null;
        
        // Find a title and champion
        var title = data.titles.FirstOrDefault(t => !string.IsNullOrEmpty(t.currentChampion));
        if (title == null)
            return null;
        
        var champion = data.wrestlers.Find(w => w.id.ToString() == title.currentChampion);
        if (champion == null || used.Contains(champion.id.ToString()) || !IsAvailable(champion))
            return null;
        
        // Find best challenger
        var challenger = available
            .Where(w => !used.Contains(w.id.ToString()))
            .Where(w => w.id != champion.id)
            .Where(w => w.popularity > 60 || w.momentum > 30)
            .OrderByDescending(w => w.popularity + w.momentum)
            .FirstOrDefault();
        
        if (challenger == null)
            return null;
        
        var match = CreateMatch(new List<Wrestler> { champion, challenger }, data, show);
        match.titleMatch = true;
        match.titleId = title.id.ToString();
        match.matchType = "Singles";
        
        MarkWrestlersAsUsed(match, used);
        return match;
    }
    
    private static Match BookTopTalentMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show)
    {
        var topWrestlers = available
            .Where(w => !used.Contains(w.id.ToString()))
            .OrderByDescending(w => w.popularity + (w.momentum / 2))
            .Take(2)
            .ToList();
        
        if (topWrestlers.Count < 2)
            return null;
        
        var match = CreateMatch(topWrestlers, data, show);
        match.matchType = "Singles";
        MarkWrestlersAsUsed(match, used);
        return match;
    }
    
    private static Match BookAlignmentMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show)
    {
        // Try to book Face vs Heel
        var faces = available.Where(w => !used.Contains(w.id.ToString()) && w.alignment == Alignment.Face).ToList();
        var heels = available.Where(w => !used.Contains(w.id.ToString()) && w.alignment == Alignment.Heel).ToList();
        
        if (faces.Count > 0 && heels.Count > 0)
        {
            var face = faces.OrderByDescending(w => w.popularity).First();
            var heel = heels.OrderByDescending(w => w.popularity).First();
            
            var match = CreateMatch(new List<Wrestler> { face, heel }, data, show);
            match.matchType = "Singles";
            MarkWrestlersAsUsed(match, used);
            return match;
        }
        
        return null;
    }
    
    private static Match BookRandomMatch(GameData data, List<Wrestler> available, HashSet<string> used, Show show, MatchSlot slot)
    {
        var unusedWrestlers = available
            .Where(w => !used.Contains(w.id.ToString()))
            .OrderBy(w => w.fatigue) // Prefer less fatigued
            .ThenByDescending(w => w.morale)
            .Take(4)
            .ToList();
        
        if (unusedWrestlers.Count < 2)
            return null;
        
        // Randomly pick 2
        var selected = unusedWrestlers.OrderBy(x => UnityEngine.Random.value).Take(2).ToList();
        
        var match = CreateMatch(selected, data, show);
        match.matchType = "Singles";
        MarkWrestlersAsUsed(match, used);
        return match;
    }

    // ========================================
    // HELPER METHODS
    // ========================================
    
    private static List<Wrestler> GetAvailableWrestlers(GameData data)
    {
        return data.wrestlers
            .Where(w => IsAvailable(w))
            .ToList();
    }
    
    private static bool IsAvailable(Wrestler w)
    {
        return w.active 
            && !w.injured 
            && w.fatigue < 90 // Too fatigued
            && w.morale > 20; // Too demoralized
    }
    
    private static bool HasAvailableParticipants(Feud feud, List<Wrestler> available, HashSet<string> used, int minCount)
    {
        int availableCount = feud.participants
            .Count(id => available.Any(w => w.id.ToString() == id && !used.Contains(id)));
        
        return availableCount >= minCount;
    }
    
    private static List<Wrestler> GetFeudWrestlers(Feud feud, GameData data, List<Wrestler> available, HashSet<string> used)
    {
        return feud.participants
            .Select(id => data.wrestlers.Find(w => w.id.ToString() == id))
            .Where(w => w != null && available.Contains(w) && !used.Contains(w.id.ToString()))
            .ToList();
    }
    
    private static Match CreateMatch(List<Wrestler> wrestlers, GameData data, Show show)
    {
        var match = new Match
        {
            id = UnityEngine.Random.Range(1000, 9999),
            participants = wrestlers.Select(w => w.id.ToString()).ToList(),
            location = show.showName,
            matchType = "Singles",
            titleMatch = false
        };
        
        return match;
    }
    
    private static void MarkWrestlersAsUsed(Match match, HashSet<string> used)
    {
        foreach (var participantId in match.participants)
        {
            used.Add(participantId);
        }
    }
    
    private static bool ShouldBeTitleMatch(Match match, GameData data, bool isMainEvent)
    {
        // Randomly decide if match should be for a title (if available)
        if (isMainEvent)
            return UnityEngine.Random.value > 0.3f; // 70% chance for main event
        else
            return UnityEngine.Random.value > 0.6f; // 40% chance otherwise
    }
    
    private static string DetermineMatchType(List<Wrestler> wrestlers, Feud feud)
    {
        // Higher heat = more likely to be gimmick match
        if (feud.heat > 80 && UnityEngine.Random.value > 0.7f)
        {
            string[] gimmickMatches = { "Hardcore", "Cage", "NoDisqualification", "StreetFight" };
            return gimmickMatches[UnityEngine.Random.Range(0, gimmickMatches.Length)];
        }
        
        return "Singles";
    }
    
    private static string GetMatchDescription(Match match, GameData data)
    {
        var wrestlers = match.participants
            .Select(id => data.wrestlers.Find(w => w.id.ToString() == id))
            .Where(w => w != null)
            .Select(w => w.name)
            .ToList();
        
        string desc = string.Join(" vs ", wrestlers);
        if (match.titleMatch)
            desc += " (Title Match)";
        
        return desc;
    }
}

/// <summary>
/// Booking priorities for AI decision-making
/// </summary>
public enum BookingPriority
{
    Balanced,        // Mix of everything
    Storyline,       // Prioritize feuds and angles
    StarPower,       // Feature top stars
    WorkRate,        // Feature best in-ring workers
    Development     // Give newer talent opportunities
}

/// <summary>
/// Match position on the card
/// </summary>
public enum MatchSlot
{
    Opener,
    EarlyMidcard,
    Midcard,
    LateMidcard,
    SemiMain,
    MainEvent
}

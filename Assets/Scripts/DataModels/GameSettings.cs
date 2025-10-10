using UnityEngine;

/// <summary>
/// Centralized game configuration stored as a ScriptableObject.
/// Create via: Assets > Create > Wrestling Sim > Game Settings
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "Wrestling Sim/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("=== Simulation Mode ===")]
    [Tooltip("Default simulation mode for matches")]
    public MatchSimulationMode defaultSimulationMode = MatchSimulationMode.Advanced;
    
    [Tooltip("Use Simple mode for matches with combined rating below this threshold")]
    [Range(0, 200)]
    public int autoSimpleThreshold = 100;

    // ========================================
    // MATCH SIMULATION SETTINGS
    // ========================================
    [Header("=== Match Simulation ===")]
    [Tooltip("Randomness applied to performance calculations")]
    [Range(0f, 20f)]
    public float performanceRandomness = 10f;
    
    [Tooltip("Randomness for winner determination in simple mode")]
    [Range(0f, 20f)]
    public float simpleWinnerRandomness = 8f;
    
    [Tooltip("Randomness applied to match ratings")]
    [Range(0, 15)]
    public int ratingRandomness = 10;

    [Header("Performance Bonuses")]
    [Tooltip("Hometown performance multiplier")]
    [Range(1f, 1.2f)]
    public float hometownBonus = 1.05f;
    
    [Tooltip("Form factor randomness range")]
    [Range(0f, 0.2f)]
    public float formVariance = 0.05f;

    [Header("Chemistry System")]
    [Tooltip("Score bonus for friends in advanced mode")]
    [Range(0, 10)]
    public int friendsChemistryBonus = 5;
    
    [Tooltip("Score penalty for rivals in advanced mode")]
    [Range(0, 10)]
    public int rivalsChemistryPenalty = 5;
    
    [Tooltip("Chemistry bonus in simple mode (scaled down)")]
    [Range(0, 5)]
    public int simpleChemistryBonus = 3;

    [Header("Match Phase Settings")]
    [Tooltip("Number of momentum shifts in mid-phase")]
    [Range(1, 10)]
    public int minMomentumShifts = 2;
    
    [Range(1, 10)]
    public int maxMomentumShifts = 5;
    
    [Tooltip("Chance for near fall during mid-phase")]
    [Range(0f, 1f)]
    public float nearFallChance = 0.5f;
    
    [Tooltip("Chance for post-match angle")]
    [Range(0f, 1f)]
    public float postMatchAngleChance = 0.15f;

    // ========================================
    // INJURY SYSTEM SETTINGS
    // ========================================
    [Header("=== Injury System ===")]
    [Tooltip("Base injury chance percentage")]
    [Range(0f, 10f)]
    public float baseInjuryChance = 1f;
    
    [Tooltip("Enable injuries during matches")]
    public bool injuriesEnabled = true;
    
    [Tooltip("Simple mode injury rate multiplier")]
    [Range(0f, 1f)]
    public float simpleInjuryMultiplier = 0.5f;

    [Header("Injury Severity Thresholds")]
    [Tooltip("Max chance for minor injuries (severity 1)")]
    [Range(0f, 50f)]
    public float minorInjuryThreshold = 10f;
    
    [Tooltip("Max chance for moderate injuries (severity 2)")]
    [Range(0f, 50f)]
    public float moderateInjuryThreshold = 25f;

    [Header("Match Type Risk Factors")]
    public float hardcoreRisk = 15f;
    public float ladderRisk = 20f;
    public float cageRisk = 10f;
    public float tlcRisk = 25f;
    public float standardRisk = 2f;

    [Header("Recovery Time (Weeks)")]
    public Vector2Int minorRecovery = new Vector2Int(1, 3);
    public Vector2Int moderateRecovery = new Vector2Int(3, 6);
    public Vector2Int majorRecovery = new Vector2Int(6, 12);

    [Header("Injury Stat Penalties (Multipliers)")]
    [Range(0.5f, 1f)]
    public float minorStatPenalty = 0.95f;
    
    [Range(0.5f, 1f)]
    public float moderateStatPenalty = 0.85f;
    
    [Range(0.5f, 1f)]
    public float majorStatPenalty = 0.70f;

    // ========================================
    // RATING CALCULATION SETTINGS
    // ========================================
    [Header("=== Match Rating Calculation ===")]
    [Tooltip("Weight of performance in rating calculation")]
    [Range(0f, 1f)]
    public float performanceWeight = 0.6f;
    
    [Tooltip("Weight of psychology stat")]
    [Range(0f, 0.5f)]
    public float psychologyWeight = 0.2f;
    
    [Tooltip("Weight of psychology in simple mode")]
    [Range(0f, 0.5f)]
    public float simplePsychologyWeight = 0.15f;
    
    [Tooltip("Weight of popularity stat")]
    [Range(0f, 0.5f)]
    public float popularityWeight = 0.1f;
    
    [Tooltip("Weight of popularity in simple mode")]
    [Range(0f, 0.5f)]
    public float simplePopularityWeight = 0.08f;
    
    [Tooltip("Feud heat bonus divisor (heat / divisor = bonus)")]
    [Range(5, 20)]
    public int feudHeatDivisor = 10;

    // ========================================
    // TRAIT BONUSES
    // ========================================
    [Header("=== Trait Effects ===")]
    [Tooltip("Crowd Favourite hometown multiplier")]
    [Range(1f, 1.2f)]
    public float crowdFavouriteBonus = 1.05f;
    
    [Tooltip("Hardcore Specialist bonus")]
    [Range(0f, 20f)]
    public float hardcoreSpecialistBonus = 10f;
    
    [Tooltip("Submission Expert max random bonus")]
    [Range(0f, 20f)]
    public float submissionExpertMaxBonus = 10f;
    
    [Tooltip("Big Match Performer multiplier")]
    [Range(1f, 1.2f)]
    public float bigMatchPerformerBonus = 1.05f;
    
    [Tooltip("Lazy Worker penalty multiplier")]
    [Range(0.7f, 1f)]
    public float lazyWorkerPenalty = 0.9f;
    
    [Tooltip("Chance Lazy Worker actually shows up")]
    [Range(0f, 1f)]
    public float lazyWorkerChance = 0.7f;
    
    [Tooltip("Chemistry Master flat bonus")]
    [Range(0f, 10f)]
    public float chemistryMasterBonus = 5f;

    // ========================================
    // REFEREE SETTINGS
    // ========================================
    [Header("=== Referee System ===")]
    [Tooltip("Enable referee events (bumps, knockouts)")]
    public bool refereeEventsEnabled = true;
    
    [Tooltip("Chance for referee event per phase")]
    [Range(0f, 0.5f)]
    public float refereeEventChance = 0.1f;
    
    [Tooltip("Max referee rating modifier (+ or -)")]
    [Range(0f, 10f)]
    public float maxRefereeRatingModifier = 5f;

    // ========================================
    // FINISH TYPES
    // ========================================
    [Header("=== Finish Type Weights ===")]
    [Tooltip("Default finish type probabilities (should sum to 100)")]
    public FinishWeights standardFinishWeights = new FinishWeights
    {
        pinfall = 60f,
        submission = 20f,
        knockout = 10f,
        countOut = 5f,
        disqualification = 5f
    };
    
    public FinishWeights simpleFinishWeights = new FinishWeights
    {
        pinfall = 65f,
        submission = 20f,
        knockout = 8f,
        countOut = 4f,
        disqualification = 3f
    };

    // ========================================
    // DEBUG & LOGGING
    // ========================================
    [Header("=== Debug & Logging ===")]
    [Tooltip("Enable detailed match phase logging")]
    public bool verboseMatchLogging = true;
    
    [Tooltip("Enable injury logging")]
    public bool logInjuries = true;
    
    [Tooltip("Enable referee event logging")]
    public bool logRefereeEvents = true;
    
    [Tooltip("Enable performance calculation logging")]
    public bool logPerformance = false;

    // ========================================
    // HELPER METHODS
    // ========================================
    
    /// <summary>
    /// Get injury risk for specific match type
    /// </summary>
    public float GetMatchTypeRisk(string matchType)
    {
        return matchType switch
        {
            "Hardcore" => hardcoreRisk,
            "Ladder" or "LadderMatch" => ladderRisk,
            "Cage" or "SteelCage" => cageRisk,
            "TLC" => tlcRisk,
            _ => standardRisk
        };
    }
    
    /// <summary>
    /// Get recovery time range for injury severity
    /// </summary>
    public Vector2Int GetRecoveryRange(int severity)
    {
        return severity switch
        {
            1 => minorRecovery,
            2 => moderateRecovery,
            3 => majorRecovery,
            _ => new Vector2Int(0, 0)
        };
    }
    
    /// <summary>
    /// Get stat penalty multiplier for injury severity
    /// </summary>
    public float GetInjuryStatPenalty(int severity)
    {
        return severity switch
        {
            1 => minorStatPenalty,
            2 => moderateStatPenalty,
            3 => majorStatPenalty,
            _ => 1f
        };
    }
    
    /// <summary>
    /// Determine injury severity based on injury chance
    /// </summary>
    public int DetermineInjurySeverity(float injuryChance)
    {
        if (injuryChance < minorInjuryThreshold) return 1;
        if (injuryChance < moderateInjuryThreshold) return 2;
        return 3;
    }
}

/// <summary>
/// Finish type probability weights
/// </summary>
[System.Serializable]
public struct FinishWeights
{
    [Range(0f, 100f)] public float pinfall;
    [Range(0f, 100f)] public float submission;
    [Range(0f, 100f)] public float knockout;
    [Range(0f, 100f)] public float countOut;
    [Range(0f, 100f)] public float disqualification;
    
    public float Total => pinfall + submission + knockout + countOut + disqualification;
}

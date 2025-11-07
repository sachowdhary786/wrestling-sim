using UnityEngine;

/// <summary>
/// Singleton manager for accessing game settings throughout the application.
/// Automatically loads settings from Resources folder.
/// </summary>
public class GameSettingsManager : MonoBehaviour
{
    private static GameSettingsManager _instance;
    public static GameSettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameSettingsManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSettingsManager");
                    _instance = go.AddComponent<GameSettingsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private GameSettings _settings;
    
    /// <summary>
    /// Current game settings. Loads from Resources if not already loaded.
    /// </summary>
    public static GameSettings Settings
    {
        get
        {
            if (Instance._settings == null)
            {
                Instance.LoadSettings();
            }
            return Instance._settings;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    /// <summary>
    /// Loads settings from Resources/GameSettings
    /// </summary>
    private void LoadSettings()
    {
        _settings = Resources.Load<GameSettings>("GameSettings");
        
        if (_settings == null)
        {
            Debug.LogWarning("GameSettings not found in Resources folder! Creating default settings...");
            _settings = CreateDefaultSettings();
        }
        else
        {
            Debug.Log($"[GameSettings] Loaded successfully. Default mode: {_settings.defaultSimulationMode}");
        }
    }

    /// <summary>
    /// Creates default settings as fallback (should create actual asset in Resources)
    /// </summary>
    private GameSettings CreateDefaultSettings()
    {
        GameSettings defaultSettings = ScriptableObject.CreateInstance<GameSettings>();
        Debug.LogError("Using temporary default settings. Please create a GameSettings asset in Resources folder!");
        return defaultSettings;
    }

    /// <summary>
    /// Reloads settings from disk (useful for testing)
    /// </summary>
    public void ReloadSettings()
    {
        _settings = null;
        LoadSettings();
    }

    /// <summary>
    /// Static accessor for cleaner code
    /// </summary>
    public static GameSettings Get() => Settings;

#if UNITY_EDITOR
    [ContextMenu("Reload Settings")]
    private void ReloadSettingsEditor()
    {
        ReloadSettings();
    }

    [ContextMenu("Print Current Settings")]
    private void PrintSettings()
    {
        if (_settings != null)
        {
            Debug.Log($"=== GAME SETTINGS ===");
            Debug.Log($"Default Simulation Mode: {_settings.defaultSimulationMode}");
            Debug.Log($"Injuries Enabled: {_settings.injuriesEnabled}");
            Debug.Log($"Base Injury Chance: {_settings.baseInjuryChance}%");
            Debug.Log($"Hometown Bonus: {_settings.hometownBonus}x");
            Debug.Log($"Performance Randomness: ±{_settings.performanceRandomness}");
        }
        else
        {
            Debug.LogWarning("No settings loaded!");
        }
    }
#endif
}

using UnityEngine;

/// <summary>
/// Singleton that manages all game settings via PlayerPrefs.
/// Accessible from anywhere via GameSettings.Instance.
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    // --- Setting Keys ---
    private const string KEY_SENSITIVITY = "Settings_Sensitivity";
    private const string KEY_MASTER_VOLUME = "Settings_MasterVolume";
    private const string KEY_MUSIC_VOLUME = "Settings_MusicVolume";
    private const string KEY_SFX_VOLUME = "Settings_SFXVolume";
    private const string KEY_FULLSCREEN = "Settings_Fullscreen";
    private const string KEY_QUALITY = "Settings_Quality";
    private const string KEY_FOV = "Settings_FOV";
    private const string KEY_VIEW_BOB = "Settings_ViewBob";

    // --- Default Values ---
    public const float DEFAULT_SENSITIVITY = 15f;
    public const float DEFAULT_MASTER_VOLUME = 1f;
    public const float DEFAULT_MUSIC_VOLUME = 0.8f;
    public const float DEFAULT_SFX_VOLUME = 1f;
    public const float DEFAULT_FOV = 60f;

    // --- Current Values (readable from other scripts) ---
    public float Sensitivity { get; private set; }
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public bool Fullscreen { get; private set; }
    public int QualityLevel { get; private set; }
    public float FieldOfView { get; private set; }
    public bool ViewBobEnabled { get; private set; }

    // Events fired when settings change (UI and gameplay scripts can subscribe)
    public System.Action OnSettingsChanged;

    void Awake()
    {
        // Singleton pattern: persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplySettings();
    }

    public void LoadSettings()
    {
        Sensitivity = PlayerPrefs.GetFloat(KEY_SENSITIVITY, DEFAULT_SENSITIVITY);
        MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, DEFAULT_MASTER_VOLUME);
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, DEFAULT_MUSIC_VOLUME);
        SFXVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, DEFAULT_SFX_VOLUME);
        Fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        QualityLevel = PlayerPrefs.GetInt(KEY_QUALITY, QualitySettings.GetQualityLevel());
        FieldOfView = PlayerPrefs.GetFloat(KEY_FOV, DEFAULT_FOV);
        ViewBobEnabled = PlayerPrefs.GetInt(KEY_VIEW_BOB, 1) == 1;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, Sensitivity);
        PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, MasterVolume);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, MusicVolume);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, SFXVolume);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, Fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KEY_QUALITY, QualityLevel);
        PlayerPrefs.SetFloat(KEY_FOV, FieldOfView);
        PlayerPrefs.SetInt(KEY_VIEW_BOB, ViewBobEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ApplySettings()
    {
        AudioListener.volume = MasterVolume;
        Screen.fullScreen = Fullscreen;
        QualitySettings.SetQualityLevel(QualityLevel);

        // Apply FOV to main camera if available
        if (Camera.main != null)
        {
            Camera.main.fieldOfView = FieldOfView;
        }

        OnSettingsChanged?.Invoke();
    }

    // --- Setters (call from UI) ---

    public void SetSensitivity(float value)
    {
        Sensitivity = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        AudioListener.volume = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetFullscreen(bool value)
    {
        Fullscreen = value;
        Screen.fullScreen = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetQualityLevel(int value)
    {
        QualityLevel = value;
        QualitySettings.SetQualityLevel(value);
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetFieldOfView(float value)
    {
        FieldOfView = value;
        if (Camera.main != null) Camera.main.fieldOfView = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetViewBobEnabled(bool value)
    {
        ViewBobEnabled = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void ResetToDefaults()
    {
        Sensitivity = DEFAULT_SENSITIVITY;
        MasterVolume = DEFAULT_MASTER_VOLUME;
        MusicVolume = DEFAULT_MUSIC_VOLUME;
        SFXVolume = DEFAULT_SFX_VOLUME;
        Fullscreen = true;
        QualityLevel = QualitySettings.names.Length - 1; // Highest quality
        FieldOfView = DEFAULT_FOV;
        ViewBobEnabled = true;

        SaveSettings();
        ApplySettings();
    }
}

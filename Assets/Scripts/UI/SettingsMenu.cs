using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime UI controller for the Settings panel.
/// Binds sliders, toggles, and dropdowns to GameSettings.
/// Can be used from both the Main Menu and the Pause Menu.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("UI References (auto-wired by SettingsMenuBuilder)")]
    public Slider sensitivitySlider;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider fovSlider;
    public Toggle fullscreenToggle;
    public Toggle viewBobToggle;
    public TMP_Dropdown qualityDropdown;

    [Header("Value Labels")]
    public TextMeshProUGUI sensitivityValueText;
    public TextMeshProUGUI masterVolumeValueText;
    public TextMeshProUGUI musicVolumeValueText;
    public TextMeshProUGUI sfxVolumeValueText;
    public TextMeshProUGUI fovValueText;

    [Header("Panel")]
    public GameObject settingsPanel;

    private bool initialized = false;

    void Awake()
    {
        EnsureInitialized();
    }

    void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        // Ensure GameSettings singleton exists
        if (GameSettings.Instance == null)
        {
            GameObject go = new GameObject("GameSettings");
            go.AddComponent<GameSettings>();
        }

        InitializeUI();
        BindListeners();
    }

    void InitializeUI()
    {
        GameSettings gs = GameSettings.Instance;
        if (gs == null) return;

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 1f;
            sensitivitySlider.maxValue = 50f;
            sensitivitySlider.SetValueWithoutNotify(gs.Sensitivity);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.SetValueWithoutNotify(gs.MasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.SetValueWithoutNotify(gs.MusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.SetValueWithoutNotify(gs.SFXVolume);
        }

        if (fovSlider != null)
        {
            fovSlider.minValue = 60f;
            fovSlider.maxValue = 120f;
            fovSlider.SetValueWithoutNotify(gs.FieldOfView);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(gs.Fullscreen);
        }

        if (viewBobToggle != null)
        {
            viewBobToggle.SetIsOnWithoutNotify(gs.ViewBobEnabled);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.SetValueWithoutNotify(gs.QualityLevel);
        }

        UpdateValueLabels();
    }

    void BindListeners()
    {
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(v => { GameSettings.Instance?.SetSensitivity(v); UpdateValueLabels(); });

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(v => { GameSettings.Instance?.SetMasterVolume(v); UpdateValueLabels(); });

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(v => { GameSettings.Instance?.SetMusicVolume(v); UpdateValueLabels(); });

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(v => { GameSettings.Instance?.SetSFXVolume(v); UpdateValueLabels(); });

        if (fovSlider != null)
            fovSlider.onValueChanged.AddListener(v => { GameSettings.Instance?.SetFieldOfView(v); UpdateValueLabels(); });

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(v => GameSettings.Instance?.SetFullscreen(v));

        if (viewBobToggle != null)
            viewBobToggle.onValueChanged.AddListener(v => GameSettings.Instance?.SetViewBobEnabled(v));

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(v => GameSettings.Instance?.SetQualityLevel(v));
    }

    void UpdateValueLabels()
    {
        if (sensitivityValueText != null && sensitivitySlider != null)
            sensitivityValueText.text = sensitivitySlider.value.ToString("F1");

        if (masterVolumeValueText != null && masterVolumeSlider != null)
            masterVolumeValueText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";

        if (musicVolumeValueText != null && musicVolumeSlider != null)
            musicVolumeValueText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";

        if (sfxVolumeValueText != null && sfxVolumeSlider != null)
            sfxVolumeValueText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";

        if (fovValueText != null && fovSlider != null)
            fovValueText.text = Mathf.RoundToInt(fovSlider.value) + "°";
    }

    public void ResetDefaults()
    {
        GameSettings.Instance?.ResetToDefaults();
        InitializeUI();
    }

    public void Open()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        EnsureInitialized();
        InitializeUI(); // Refresh values
    }

    public void Close()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}

using UnityEngine;

/// <summary>
/// Fades in title screen music and provides spectrum data for the visualizer and beat shake.
/// Attach to a GameObject with an AudioSource in the MainMenu scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TitleScreenMusic : MonoBehaviour
{
    public static TitleScreenMusic Instance { get; private set; }

    [Header("Fade In")]
    [SerializeField] private float fadeDuration = 4.85f;
    [SerializeField] private float targetVolume = 0.8f;

    [Header("Spectrum")]
    [SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;

    [Header("Beat Detection")]
    [SerializeField] private float beatThreshold = 1.3f;
    [SerializeField] private float beatCooldown = 0.2f;
    [SerializeField] private float energyMinimum = 0.0005f;

    private AudioSource audioSource;
    private float fadeTimer;
    private float[] spectrumData = new float[2048]; // 2048 for ~21Hz per bin at 44100Hz

    // Beat detection — spectral flux on sub-bass/kick range
    private float[] prevSpectrum = new float[2048];
    private float fluxHistory;
    private float lastBeatTime = -1f;

    public float[] SpectrumData => spectrumData;
    public bool IsBeat { get; private set; }

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.loop = true;

        // Force values (serialized scene values override code defaults)
        fadeDuration = 4.85f;
        beatThreshold = 1.3f;
        beatCooldown = 0.18f;
        energyMinimum = 0.0005f;
    }

    void Start()
    {
        audioSource.Play();

        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged += ApplyMusicVolume;
    }

    private float EffectiveVolume => targetVolume * (GameSettings.Instance != null ? GameSettings.Instance.MusicVolume : 1f);

    private void ApplyMusicVolume()
    {
        // Only override if fade is complete
        if (fadeTimer >= fadeDuration)
            audioSource.volume = EffectiveVolume;
    }

    void Update()
    {
        // Fade in (scaled by music volume setting)
        if (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, EffectiveVolume, fadeTimer / fadeDuration);
        }

        // Get spectrum (1024 samples for better frequency resolution)
        audioSource.GetSpectrumData(spectrumData, 0, fftWindow);

        DetectBeat();
    }

    private void DetectBeat()
    {
        // Spectral flux on kick/bass range only
        // At 2048 samples / 44100Hz, each bin ≈ 21.5Hz
        // Bins 1-7 ≈ 21-150Hz = kick drum / sub-bass territory
        float flux = 0f;
        for (int i = 1; i <= 7; i++)
        {
            // Only count positive changes (onset = energy appearing, not fading)
            float diff = spectrumData[i] - prevSpectrum[i];
            if (diff > 0f)
                flux += diff;
        }

        // Also check snare range: bins 7-14 ≈ 150-300Hz
        float snareFlux = 0f;
        for (int i = 7; i <= 14; i++)
        {
            float diff = spectrumData[i] - prevSpectrum[i];
            if (diff > 0f)
                snareFlux += diff;
        }

        // Combine with kick weighted heavier
        float totalFlux = flux * 1.5f + snareFlux * 0.5f;

        // Compare to running average
        bool spike = totalFlux > fluxHistory * beatThreshold && totalFlux > energyMinimum;
        bool cooledDown = (Time.time - lastBeatTime) > beatCooldown;

        IsBeat = spike && cooledDown;

        if (IsBeat)
        {
            lastBeatTime = Time.time;
            Debug.Log($"[Beat] flux={totalFlux:F5} avg={fluxHistory:F5} kick={flux:F5} snare={snareFlux:F5}");
        }

        // Slow-moving average so spikes stay detectable
        fluxHistory = Mathf.Lerp(fluxHistory, totalFlux, Time.deltaTime * 1.5f);

        // Save current frame for next comparison
        System.Array.Copy(spectrumData, prevSpectrum, spectrumData.Length);
    }

    void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged -= ApplyMusicVolume;
        if (Instance == this) Instance = null;
    }
}

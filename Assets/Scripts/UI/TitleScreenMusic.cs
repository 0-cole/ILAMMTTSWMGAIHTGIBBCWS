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
    private float[] spectrumData = new float[1024];

    // Beat detection
    private float energyHistory;
    private float lastBeatTime = -1f;

    public float[] SpectrumData => spectrumData;
    public bool IsBeat { get; private set; }

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.loop = true;
    }

    void Start()
    {
        audioSource.Play();
    }

    void Update()
    {
        // Fade in
        if (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, fadeTimer / fadeDuration);
        }

        // Get spectrum (1024 samples for better frequency resolution)
        audioSource.GetSpectrumData(spectrumData, 0, fftWindow);

        DetectBeat();
    }

    private void DetectBeat()
    {
        // Sum bass frequencies (bins 1-32 ≈ 20-600Hz at 44100/1024)
        float currentEnergy = 0f;
        for (int i = 1; i < 32; i++)
        {
            currentEnergy += spectrumData[i] * spectrumData[i];
        }
        currentEnergy = Mathf.Sqrt(currentEnergy);

        // Beat = energy spike above running average, with cooldown to prevent rapid re-triggers
        bool spike = currentEnergy > energyHistory * beatThreshold && currentEnergy > energyMinimum;
        bool cooledDown = (Time.time - lastBeatTime) > beatCooldown;

        IsBeat = spike && cooledDown;

        if (IsBeat)
        {
            lastBeatTime = Time.time;
            Debug.Log($"[Beat] energy={currentEnergy:F4} avg={energyHistory:F4} ratio={currentEnergy / Mathf.Max(0.0001f, energyHistory):F2}");
        }

        // Smooth running average — slow attack so beats stay detectable as spikes
        float smoothSpeed = currentEnergy > energyHistory ? 2f : 0.5f;
        energyHistory = Mathf.Lerp(energyHistory, currentEnergy, Time.deltaTime * smoothSpeed);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

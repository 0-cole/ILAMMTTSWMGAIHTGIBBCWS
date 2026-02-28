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
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private float targetVolume = 0.8f;

    [Header("Spectrum")]
    [SerializeField] private FFTWindow fftWindow = FFTWindow.Blackman;

    private AudioSource audioSource;
    private float fadeTimer;
    private float[] spectrumData = new float[256];

    // Beat detection
    private float[] bandEnergies = new float[8];
    private float[] bandHistory = new float[8];
    private float beatThreshold = 1.5f;

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

        // Get spectrum
        audioSource.GetSpectrumData(spectrumData, 0, fftWindow);

        // Simple beat detection using low-frequency energy
        DetectBeat();
    }

    private void DetectBeat()
    {
        // Sum low-frequency bands (bass)
        float currentEnergy = 0f;
        for (int i = 0; i < 16; i++)
        {
            currentEnergy += spectrumData[i] * spectrumData[i];
        }
        currentEnergy = Mathf.Sqrt(currentEnergy);

        // Compare to running average
        float avg = bandHistory[0];
        IsBeat = currentEnergy > avg * beatThreshold && currentEnergy > 0.005f;

        // Update history with smoothing
        bandHistory[0] = Mathf.Lerp(bandHistory[0], currentEnergy, Time.deltaTime * 5f);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

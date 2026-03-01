using UnityEngine;

/// <summary>
/// Manages level audio: crossfades between ambience and combat music.
/// Combat music pauses (not stops) on EndCombat so it resumes from the same position.
/// Singleton — one per level scene.
/// </summary>
public class LevelMusicManager : MonoBehaviour
{
    public static LevelMusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource combatSource;

    [Header("Ambience")]
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private float ambienceVolume = 0.5f;

    [Header("Crossfade")]
    [SerializeField] private float combatFadeInDuration = 0.5f;
    [SerializeField] private float combatFadeOutDuration = 1.5f;
    [SerializeField] private float ambienceFadeDuration = 1.5f;

    public AudioSource AmbienceSource => ambienceSource;
    public AudioSource CombatSource => combatSource;

    private float combatBaseVolume = 0.7f;
    private bool inCombat = false;
    private bool combatEverStarted = false;

    // Fade state
    private float combatCurrentVol = 0f;
    private float combatTargetVol = 0f;
    private float combatFadeSpeed = 0f;

    private float ambienceCurrentVol = 0f;
    private float ambienceTargetVol = 0f;
    private float ambienceFadeSpeed = 0f;

    private float MusicMultiplier => GameSettings.Instance != null ? GameSettings.Instance.MusicVolume : 1f;

    void Awake()
    {
        Instance = this;

        if (ambienceSource == null)
            ambienceSource = gameObject.AddComponent<AudioSource>();
        if (combatSource == null)
            combatSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Start ambient music
        if (ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.loop = true;
            ambienceSource.ignoreListenerPause = false;
            ambienceCurrentVol = ambienceVolume;
            ambienceTargetVol = ambienceVolume;
            ambienceSource.volume = ambienceVolume * MusicMultiplier;
            ambienceSource.Play();
        }

        if (combatSource != null)
        {
            combatSource.loop = true;
            combatSource.ignoreListenerPause = false;
            combatSource.volume = 0f;
        }

        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged += ApplyMusicVolume;
    }

    void Update()
    {
        // Smoothly fade combat volume
        if (!Mathf.Approximately(combatCurrentVol, combatTargetVol))
        {
            combatCurrentVol = Mathf.MoveTowards(combatCurrentVol, combatTargetVol, combatFadeSpeed * Time.unscaledDeltaTime);
            if (combatSource != null)
                combatSource.volume = combatCurrentVol * MusicMultiplier;

            // When combat fades to zero, pause the source to preserve playback position
            if (combatCurrentVol <= 0f && !inCombat && combatSource != null && combatSource.isPlaying)
            {
                combatSource.Pause();
            }
        }

        // Smoothly fade ambience volume
        if (!Mathf.Approximately(ambienceCurrentVol, ambienceTargetVol))
        {
            ambienceCurrentVol = Mathf.MoveTowards(ambienceCurrentVol, ambienceTargetVol, ambienceFadeSpeed * Time.unscaledDeltaTime);
            if (ambienceSource != null)
                ambienceSource.volume = ambienceCurrentVol * MusicMultiplier;
        }
    }

    /// <summary>
    /// Start combat music. First call plays from beginning; subsequent calls resume from where it was paused.
    /// Ambience fades out simultaneously.
    /// </summary>
    public void StartCombat(AudioClip clip = null, float volume = 0.7f)
    {
        inCombat = true;

        if (clip != null)
            combatBaseVolume = volume;

        // First time: assign clip and play from start
        // Subsequent: just unpause from where we left off
        if (combatSource != null)
        {
            if (!combatEverStarted || (clip != null && combatSource.clip != clip))
            {
                combatSource.clip = clip;
                combatSource.volume = 0f;
                combatSource.loop = true;
                combatSource.Play();
                combatEverStarted = true;
            }
            else
            {
                combatSource.UnPause();
            }
        }

        // Fade in combat
        combatCurrentVol = combatSource != null ? combatSource.volume / Mathf.Max(MusicMultiplier, 0.001f) : 0f;
        combatTargetVol = combatBaseVolume;
        combatFadeSpeed = combatBaseVolume / Mathf.Max(combatFadeInDuration, 0.01f);

        // Fade out ambience
        ambienceTargetVol = 0f;
        ambienceFadeSpeed = ambienceVolume / Mathf.Max(combatFadeInDuration, 0.01f);
    }

    /// <summary>
    /// End combat: fade out combat music over 1.5s (then pause), fade ambience back in.
    /// </summary>
    public void EndCombat()
    {
        inCombat = false;

        // Fade out combat
        combatTargetVol = 0f;
        combatFadeSpeed = combatBaseVolume / Mathf.Max(combatFadeOutDuration, 0.01f);

        // Fade ambience back in
        if (ambienceSource != null && !ambienceSource.isPlaying && ambienceClip != null)
        {
            ambienceSource.Play();
        }
        ambienceTargetVol = ambienceVolume;
        ambienceFadeSpeed = ambienceVolume / Mathf.Max(ambienceFadeDuration, 0.01f);
    }

    /// <summary>
    /// Immediately stop ambience (used by EncounterIntro during typing sequence).
    /// </summary>
    public void StopAmbience()
    {
        if (ambienceSource != null)
        {
            ambienceSource.Stop();
            ambienceCurrentVol = 0f;
            ambienceTargetVol = 0f;
        }
    }

    /// <summary>
    /// Legacy — still works but prefer StartCombat() for crossfade behavior.
    /// </summary>
    public void PlayCombatMusic(AudioClip clip, float volume = 0.7f)
    {
        StartCombat(clip, volume);
    }

    private void ApplyMusicVolume()
    {
        if (ambienceSource != null)
            ambienceSource.volume = ambienceCurrentVol * MusicMultiplier;
        if (combatSource != null && (combatSource.isPlaying || inCombat))
            combatSource.volume = combatCurrentVol * MusicMultiplier;
    }

    void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged -= ApplyMusicVolume;
        if (Instance == this) Instance = null;
    }
}

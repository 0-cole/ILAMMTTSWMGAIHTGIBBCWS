using UnityEngine;

/// <summary>
/// Manages level audio sources. Singleton -- one per level scene.
/// Music plays continuously regardless of encounters.
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

    public AudioSource AmbienceSource => ambienceSource;
    public AudioSource CombatSource => combatSource;

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
        if (ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.loop = true;
            ambienceSource.volume = ambienceVolume * MusicMultiplier;
            ambienceSource.Play();
        }

        if (combatSource != null)
        {
            combatSource.loop = true;
            combatSource.volume = 0f;
        }

        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged += ApplyMusicVolume;
    }

    /// <summary>
    /// Play combat music directly on the combat source.
    /// </summary>
    public void PlayCombatMusic(AudioClip clip, float volume = 0.7f)
    {
        if (combatSource != null && clip != null)
        {
            combatSource.clip = clip;
            combatSource.volume = volume * MusicMultiplier;
            combatSource.loop = true;
            combatSource.Play();
        }
    }

    /// <summary>
    /// Stop ambience (e.g. during encounter intro typing).
    /// </summary>
    public void StopAmbience()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();
    }

    private void ApplyMusicVolume()
    {
        if (ambienceSource != null && ambienceSource.isPlaying)
            ambienceSource.volume = ambienceVolume * MusicMultiplier;
        if (combatSource != null && combatSource.isPlaying)
            combatSource.volume = combatSource.volume; // preserve current volume ratio
    }

    void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged -= ApplyMusicVolume;
        if (Instance == this) Instance = null;
    }
}

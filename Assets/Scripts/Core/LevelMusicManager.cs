using UnityEngine;

/// <summary>
/// Manages level audio: ambient music and combat music transitions.
/// Singleton — one per level scene.
/// 
/// Setup:
/// 1. Create a "LevelMusic" GameObject in your level
/// 2. Add two AudioSource components (ambience + combat)
/// 3. Add this script and assign references
/// 4. Assign ambient clip — it plays on Start
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

    private float combatBaseVolume = 0.7f;

    void Awake()
    {
        Instance = this;

        if (ambienceSource == null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
        }
        if (combatSource == null)
        {
            combatSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private float MusicMultiplier => GameSettings.Instance != null ? GameSettings.Instance.MusicVolume : 1f;

    void Start()
    {
        // Start ambient music
        if (ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.volume = ambienceVolume * MusicMultiplier;
            ambienceSource.loop = true;
            ambienceSource.ignoreListenerPause = false;
            ambienceSource.Play();
        }
        if (combatSource != null)
        {
            combatSource.ignoreListenerPause = false;
        }

        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged += ApplyMusicVolume;
    }

    private void ApplyMusicVolume()
    {
        if (ambienceSource != null && ambienceSource.isPlaying)
            ambienceSource.volume = ambienceVolume * MusicMultiplier;
        if (combatSource != null && combatSource.isPlaying)
            combatSource.volume = combatBaseVolume * MusicMultiplier;
    }

    public void StopAmbience()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();
    }

    public void PlayCombatMusic(AudioClip clip, float volume = 0.7f)
    {
        if (combatSource != null && clip != null)
        {
            combatBaseVolume = volume;
            combatSource.clip = clip;
            combatSource.volume = volume * MusicMultiplier;
            combatSource.loop = true;
            combatSource.Play();
        }
    }

    void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged -= ApplyMusicVolume;
        if (Instance == this) Instance = null;
    }
}

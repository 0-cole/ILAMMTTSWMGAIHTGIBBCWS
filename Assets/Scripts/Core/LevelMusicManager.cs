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

    void Start()
    {
        // Start ambient music
        if (ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }
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
            combatSource.clip = clip;
            combatSource.volume = volume;
            combatSource.loop = true;
            combatSource.Play();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

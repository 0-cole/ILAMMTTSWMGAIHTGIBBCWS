using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// ULTRAKILL-style encounter intro. Freezes player/enemies, types out text, then starts combat music.
/// 
/// Setup:
/// 1. Create a Canvas (Screen Space - Overlay) with a child TMP_Text centered on screen
/// 2. Add this script to the Canvas
/// 3. Assign the textDisplay reference
/// 4. The SpawnTrigger calls PlayIntro() when encounterIntro is assigned
/// </summary>
public class EncounterIntro : MonoBehaviour
{
    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI textDisplay;

    [Header("Text Content")]
    [SerializeField] private string topLine = "REVENGE : FIRST";
    [SerializeField] private string bottomLine = "COVENANT";

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float pauseBetweenLines = 0.3f;
    [SerializeField] private float holdDuration = 1.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip combatMusic;
    [SerializeField] private float combatMusicVolume = 0.7f;
    [SerializeField] private AudioClip typeSlamClip;
    [SerializeField] private float typeSlamVolume = 0.15f;
    [Tooltip("Optional: directly assign the ambience AudioSource to stop. If empty, uses LevelMusicManager.")]
    [SerializeField] private AudioSource directAmbienceSource;

    [Header("Style")]
    [SerializeField] private Color textColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float fontSize = 72f;

    private AudioSource ambienceSource;
    private AudioSource musicSource;
    private AudioSource slamSource;
    private bool isPlaying = false;

    public bool IsPlaying => isPlaying;

    void Awake()
    {
        // Hide text until intro plays
        if (textDisplay != null)
            textDisplay.gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this to start the encounter intro sequence.
    /// ambienceSource is the current ambient audio to fade out.
    /// musicSource is the AudioSource to play combat music on.
    /// onComplete is called when the intro finishes.
    /// </summary>
    public void PlayIntro(AudioSource ambience, AudioSource music, System.Action onComplete = null)
    {
        if (isPlaying) return;
        ambienceSource = ambience;
        musicSource = music;

        if (typeSlamClip != null && slamSource == null)
        {
            slamSource = gameObject.AddComponent<AudioSource>();
            slamSource.playOnAwake = false;
            slamSource.clip = typeSlamClip;
            slamSource.volume = typeSlamVolume;
        }

        StartCoroutine(IntroSequence(onComplete));
    }

    private IEnumerator IntroSequence(System.Action onComplete)
    {
        isPlaying = true;

        // Freeze everything — disable player movement and all enemy AI
        var playerMove = FindFirstObjectByType<DoomMovement>();
        var playerLook = FindFirstObjectByType<MouseLook>();
        var viewBob = FindFirstObjectByType<ViewBob>();
        if (playerMove != null) playerMove.enabled = false;
        if (playerLook != null) playerLook.enabled = false;
        if (viewBob != null) viewBob.enabled = false;

        // Freeze all enemies
        var glonks = FindObjectsByType<GlonkEnemy>(FindObjectsSortMode.None);
        var billboards = FindObjectsByType<BillboardShooter>(FindObjectsSortMode.None);
        foreach (var g in glonks) g.enabled = false;
        foreach (var b in billboards) b.enabled = false;

        // Stop ambience immediately
        // Use direct reference first, then passed parameter, then LevelMusicManager fallback
        AudioSource ambToStop = directAmbienceSource != null ? directAmbienceSource : ambienceSource;
        if (ambToStop != null)
        {
            ambToStop.Stop();
        }
        else if (LevelMusicManager.Instance != null)
        {
            LevelMusicManager.Instance.StopAmbience();
        }

        // Setup text
        if (textDisplay != null)
        {
            textDisplay.gameObject.SetActive(true);
            textDisplay.text = "";
            textDisplay.color = textColor;
            textDisplay.fontSize = fontSize;
            textDisplay.alignment = TextAlignmentOptions.Center;
            textDisplay.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        }

        // Type out top line
        yield return StartCoroutine(TypeText(topLine));

        yield return new WaitForSecondsRealtime(pauseBetweenLines);

        // Type out bottom line below
        if (textDisplay != null)
            textDisplay.text += "\n";
        yield return StartCoroutine(TypeText(bottomLine));

        // Hold for a moment
        yield return new WaitForSecondsRealtime(holdDuration);

        // Fade out text
        float fadeTimer = 0f;
        Color startColor = textDisplay != null ? textDisplay.color : textColor;
        while (fadeTimer < fadeOutDuration)
        {
            fadeTimer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);
            if (textDisplay != null)
                textDisplay.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (textDisplay != null)
            textDisplay.gameObject.SetActive(false);

        // Start combat music directly
        if (musicSource != null && combatMusic != null)
        {
            musicSource.clip = combatMusic;
            musicSource.volume = combatMusicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Unfreeze everything
        if (playerMove != null) playerMove.enabled = true;
        if (playerLook != null) playerLook.enabled = true;
        if (viewBob != null) viewBob.enabled = true;
        foreach (var g in glonks) g.enabled = true;
        foreach (var b in billboards) b.enabled = true;

        isPlaying = false;
        onComplete?.Invoke();
    }

    private IEnumerator TypeText(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (textDisplay != null)
                textDisplay.text += text[i];

            if (text[i] != ' ' && slamSource != null)
            {
                slamSource.pitch = Random.Range(0.9f, 1.1f);
                slamSource.PlayOneShot(typeSlamClip, typeSlamVolume);
            }

            yield return new WaitForSecondsRealtime(typeSpeed);
        }
    }
}

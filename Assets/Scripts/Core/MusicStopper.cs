using UnityEngine;

/// <summary>
/// Trigger zone that fades out combat music and fades in ambience.
/// Place on an empty with a trigger collider at the end of the level.
/// </summary>
public class MusicStopper : MonoBehaviour
{
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float ambienceFadeInDuration = 2f;

    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        var mgr = LevelMusicManager.Instance;
        if (mgr == null) return;

        // Fade out combat music then fade in ambience
        StartCoroutine(FadeOutCombatThenAmbience(mgr));
    }

    private System.Collections.IEnumerator FadeOutCombatThenAmbience(LevelMusicManager mgr)
    {
        AudioSource combat = mgr.CombatSource;
        if (combat != null && combat.isPlaying)
        {
            float startVol = combat.volume;
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                combat.volume = Mathf.Lerp(startVol, 0f, t / fadeOutDuration);
                yield return null;
            }
            combat.Stop();
            combat.volume = 0f;
        }

        mgr.FadeInAmbience(ambienceFadeInDuration);
    }
}

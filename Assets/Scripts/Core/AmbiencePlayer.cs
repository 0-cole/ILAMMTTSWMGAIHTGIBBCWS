using UnityEngine;

/// <summary>
/// Place on an empty with a Box Collider (isTrigger).
/// When the player enters, fades in ambience via LevelMusicManager, then self-destructs.
/// </summary>
public class AmbiencePlayer : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (LevelMusicManager.Instance != null)
            LevelMusicManager.Instance.FadeInAmbience(fadeDuration);

        Destroy(gameObject);
    }
}

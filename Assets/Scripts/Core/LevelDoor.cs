using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDoor : MonoBehaviour
{
    [Tooltip("Name of the scene to load (must be added to Build Settings)")]
    public string nextLevelName;

    [Tooltip("Optional fade duration before loading")]
    public float fadeDuration = 1f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(LoadNextLevel());
    }

    private System.Collections.IEnumerator LoadNextLevel()
    {
        // Simple fade-to-black using a full-screen UI overlay if one exists
        CanvasGroup fade = GetComponentInChildren<CanvasGroup>();
        if (fade != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fade.alpha = t / fadeDuration;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        SceneManager.LoadScene(nextLevelName);
    }
}

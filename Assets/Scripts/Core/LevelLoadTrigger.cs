using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple trigger that loads a scene when the player enters.
/// Attach to the quad at the bottom of the exit chute.
/// </summary>
public class LevelLoadTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName = "level2";

    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        SceneManager.LoadScene(sceneName);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger at end-of-level chutes. On enter:
/// 1. Disables player input (movement + mouse look)
/// 2. Smoothly forces camera to look straight down
/// 3. Loads the next level after a delay (when player reaches bottom)
/// Place on an empty with a trigger collider spanning the chute.
/// </summary>
public class LevelEnder : MonoBehaviour
{
    [Header("Next Level")]
    [SerializeField] private string nextLevelName = "level2";

    [Header("Camera")]
    [Tooltip("How fast the camera rotates to look down (degrees/sec)")]
    [SerializeField] private float cameraRotateSpeed = 90f;

    [Header("Timing")]
    [Tooltip("Seconds after trigger before loading next level")]
    [SerializeField] private float loadDelay = 4f;

    private bool triggered;
    private Transform cameraTransform;
    private Transform playerBody;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        // Disable player controls
        var movement = other.GetComponent<DoomMovement>();
        var mouseLook = other.GetComponentInChildren<MouseLook>();
        var viewBob = other.GetComponentInChildren<ViewBob>();

        if (movement != null) movement.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;
        if (viewBob != null) viewBob.enabled = false;

        cameraTransform = other.GetComponentInChildren<Camera>()?.transform;
        playerBody = other.transform;

        StartCoroutine(EndLevelSequence());
    }

    private System.Collections.IEnumerator EndLevelSequence()
    {
        // Smoothly rotate camera to look straight down
        if (cameraTransform != null)
        {
            // Target: local X rotation = 90 (looking down), keep current Y/Z
            Quaternion startLocal = cameraTransform.localRotation;
            Quaternion targetLocal = Quaternion.Euler(90f, 0f, 0f);

            float angle = Quaternion.Angle(startLocal, targetLocal);
            float duration = angle / cameraRotateSpeed;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                cameraTransform.localRotation = Quaternion.Slerp(startLocal, targetLocal, t / duration);
                yield return null;
            }
            cameraTransform.localRotation = targetLocal;
        }

        // Wait for player to fall through chute
        float elapsed = 0f;
        float remaining = loadDelay - (cameraTransform != null ? Quaternion.Angle(Quaternion.identity, Quaternion.Euler(90f, 0f, 0f)) / cameraRotateSpeed : 0f);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        SceneManager.LoadScene(nextLevelName);
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger at end-of-level chutes. On enter:
/// 1. Disables player input (movement + mouse look)
/// 2. Smoothly centers the player on the chute and forces camera to look straight down
/// 3. Plays wind/fall audio while falling
/// 4. Applies gravity so the player keeps falling through the chute
/// The actual scene load is handled by a separate LevelLoadTrigger on a quad at the bottom.
/// </summary>
public class LevelEnder : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("How fast the camera rotates to look down (degrees/sec)")]
    [SerializeField] private float cameraRotateSpeed = 90f;


    [Header("Fall Physics")]
    [SerializeField] private float fallGravity = 35f;
    [SerializeField] private float maxFallSpeed = 50f;

    [Header("Audio")]
    [SerializeField] private AudioClip windLoop;
    [SerializeField] private float windVolume = 0.6f;

    private bool triggered;
    private Transform cameraTransform;
    private Transform playerBody;
    private CharacterController controller;
    private AudioSource audioSource;
    private float fallSpeed;

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

        controller = other.GetComponent<CharacterController>();
        cameraTransform = other.GetComponentInChildren<Camera>()?.transform;
        playerBody = other.transform;

        // Wind audio
        if (windLoop != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = windLoop;
            audioSource.loop = true;
            audioSource.volume = windVolume;
            audioSource.spatialBlend = 0f;
            audioSource.Play();
        }

        StartCoroutine(CameraLookDown());
    }

    void Update()
    {
        if (!triggered || controller == null) return;

        // Apply gravity
        fallSpeed += fallGravity * Time.deltaTime;
        fallSpeed = Mathf.Min(fallSpeed, maxFallSpeed);

        // Move: gravity only
        controller.Move(Vector3.down * fallSpeed * Time.deltaTime);

        // Scale wind volume with speed
        if (audioSource != null)
            audioSource.volume = Mathf.Lerp(windVolume * 0.3f, windVolume, fallSpeed / maxFallSpeed);
    }

    private IEnumerator CameraLookDown()
    {
        if (cameraTransform == null) yield break;

        Quaternion startLocal = cameraTransform.localRotation;
        Quaternion targetLocal = Quaternion.Euler(0f, 0f, 0f);

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
}

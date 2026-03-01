using UnityEngine;
using System.Collections;

/// <summary>
/// ULTRAKILL-style level intro: player drops from above with wind SFX,
/// lands with a crash + camera shake + freeze frame, then gameplay begins.
///
/// Setup:
/// 1. Create an empty GameObject in your level, add this script
/// 2. Position it where the player should LAND (it teleports the player up from here)
/// 3. Assign wind loop and crash SFX in Inspector
/// 4. (Optional) Assign a chute prefab — it follows the player during the fall so it looks infinite
/// </summary>
public class LevelIntro : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private float spawnHeight = 50f;
    [SerializeField] private float fallGravity = 35f;
    [SerializeField] private float maxFallSpeed = 50f;

    [Header("Landing")]
    [SerializeField] private float landingShakeDuration = 0.35f;
    [SerializeField] private float landingShakeMagnitude = 0.5f;
    [SerializeField] private float landingFreezeDuration = 0.12f;

    [Header("Audio")]
    [SerializeField] private AudioClip windLoop;
    [SerializeField] private float windVolume = 0.6f;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private float crashVolume = 0.9f;

    [Header("Chute (Optional)")]
    [Tooltip("A tall tube prefab (4 walls + torches). It follows the player during the fall.")]
    [SerializeField] private GameObject chutePrefab;
    [SerializeField] private Vector3 chuteOffset = Vector3.zero;

    [Header("Visual")]
    [SerializeField] private float fadeFromBlackDuration = 0.5f;

    private DoomMovement playerMove;
    private MouseLook playerLook;
    private CharacterController controller;
    private AudioSource audioSource;
    private GameObject chuteInstance;
    private float fallSpeed = 0f;
    private bool falling = true;
    private bool landed = false;

    void Start()
    {
        playerMove = FindFirstObjectByType<DoomMovement>();
        if (playerMove == null)
        {
            Debug.LogWarning("[LevelIntro] No DoomMovement found — skipping intro.");
            Destroy(gameObject);
            return;
        }

        controller = playerMove.GetComponent<CharacterController>();
        playerLook = playerMove.GetComponentInChildren<MouseLook>();
        if (playerLook == null)
            playerLook = FindFirstObjectByType<MouseLook>();

        // Disable player control during drop
        playerMove.enabled = false;
        if (playerLook != null) playerLook.enabled = false;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Teleport player above landing zone
        controller.enabled = false;
        playerMove.transform.position += Vector3.up * spawnHeight;
        controller.enabled = true;

        // Spawn chute around player
        if (chutePrefab != null)
        {
            chuteInstance = Instantiate(chutePrefab, playerMove.transform.position + chuteOffset, Quaternion.identity);
        }

        // Wind audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        if (windLoop != null)
        {
            audioSource.clip = windLoop;
            audioSource.loop = true;
            audioSource.volume = windVolume;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (!falling || landed) return;

        // Accelerate downward
        fallSpeed += fallGravity * Time.deltaTime;
        fallSpeed = Mathf.Min(fallSpeed, maxFallSpeed);

        // Move player down
        controller.Move(Vector3.down * fallSpeed * Time.deltaTime);

        // Keep chute centered on player
        if (chuteInstance != null)
            chuteInstance.transform.position = playerMove.transform.position + chuteOffset;

        // Wind volume increases with speed
        if (audioSource != null && windLoop != null)
            audioSource.volume = Mathf.Lerp(windVolume * 0.3f, windVolume, fallSpeed / maxFallSpeed);

        // Landing detection
        if (controller.isGrounded)
        {
            StartCoroutine(LandingSequence());
        }
    }

    private IEnumerator LandingSequence()
    {
        falling = false;
        landed = true;

        // Stop wind
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Crash sound
        if (crashSound != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(crashSound, crashVolume);
        }

        // Camera shake
        CameraShake shake = FindFirstObjectByType<CameraShake>();
        if (shake != null)
            shake.Shake(landingShakeDuration, landingShakeMagnitude);

        // Destroy chute
        if (chuteInstance != null)
            Destroy(chuteInstance);

        // Brief freeze frame on impact
        if (landingFreezeDuration > 0f)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(landingFreezeDuration);
            Time.timeScale = 1f;
        }

        // Re-enable player control
        playerMove.enabled = true;
        if (playerLook != null) playerLook.enabled = true;

        // Wait for crash SFX to finish, then clean up
        if (crashSound != null)
            yield return new WaitForSeconds(crashSound.length);

        Destroy(gameObject);
    }
}

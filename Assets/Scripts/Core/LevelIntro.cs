using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ULTRAKILL-style level intro: player drops from above through an infinite-scrolling
/// chute with wind SFX, lands with a crash + camera shake + freeze frame, then gameplay begins.
///
/// Setup:
/// 1. Create an empty GameObject in your level, add this script
/// 2. Position it where the player should LAND (it teleports the player up from here)
/// 3. Assign wind loop and crash SFX in Inspector
/// 4. (Optional) Assign a chute prefab — multiple copies stack vertically and recycle
///    to create an infinite falling shaft effect
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
    [Tooltip("A tube segment prefab (4 walls). Multiple copies stack and recycle to look infinite.")]
    [SerializeField] private GameObject chutePrefab;
    [Tooltip("XZ offset so the chute centers on the player (adjust if prefab pivot isn't centered).")]
    [SerializeField] private Vector3 chuteOffset = Vector3.zero;
    [Tooltip("Height of one chute segment. Must match the actual prefab height.")]
    [SerializeField] private float chuteSegmentHeight = 10f;
    [Tooltip("How many segments to spawn (3-5 is enough to fill the view).")]
    [SerializeField] private int chuteSegmentCount = 4;

    [Header("Hole Seal")]
    [Tooltip("Optional prefab to spawn over the hole after landing (e.g. a ceiling slab matching the room).")]
    [SerializeField] private GameObject ceilingCapPrefab;
    [Tooltip("Drag a Transform here to mark where the ceiling cap spawns. If empty, spawns at this object's position.")]
    [SerializeField] private Transform ceilingCapSpawnPoint;

    [Header("Void Cap")]
    [Tooltip("Size of the auto-generated black cap at the top of the chute (X=width, Y=depth). Set to 0 to disable.")]
    [SerializeField] private Vector2 voidCapSize = new Vector2(6f, 6f);
    [Tooltip("How far above the player to place the void cap")]
    [SerializeField] private float voidCapHeight = 15f;

    private DoomMovement playerMove;
    private MouseLook playerLook;
    private CharacterController controller;
    private AudioSource audioSource;
    private List<GameObject> chuteSegments = new List<GameObject>();
    private GameObject voidCap;
    private float fallSpeed = 0f;
    private bool falling = true;
    private bool landed = false;
    private float landingY;

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

        // Remember where the player should land
        landingY = playerMove.transform.position.y;

        // Teleport player above landing zone
        controller.enabled = false;
        playerMove.transform.position += Vector3.up * spawnHeight;
        controller.enabled = true;

        // Spawn chute segments stacked vertically around the player
        if (chutePrefab != null && chuteSegmentCount > 0)
        {
            float playerY = playerMove.transform.position.y;
            Vector3 playerXZ = new Vector3(playerMove.transform.position.x, 0f, playerMove.transform.position.z);

            // Place segments so the player starts inside one of the upper segments
            float topY = playerY + chuteSegmentHeight;
            for (int i = 0; i < chuteSegmentCount; i++)
            {
                float segY = topY - (i * chuteSegmentHeight);
                Vector3 pos = playerXZ + chuteOffset + Vector3.up * segY;
                GameObject seg = Instantiate(chutePrefab, pos, Quaternion.identity);
                seg.name = $"ChuteSegment_{i}";
                // Disable colliders so CharacterController falls through
                foreach (var col in seg.GetComponentsInChildren<Collider>())
                    col.enabled = false;
                chuteSegments.Add(seg);
            }
        }

        // Create a black cap above the chute so looking up shows infinite darkness
        if (voidCapSize.x > 0 && voidCapSize.y > 0)
        {
            voidCap = GameObject.CreatePrimitive(PrimitiveType.Quad);
            voidCap.name = "VoidCap";
            Destroy(voidCap.GetComponent<Collider>());
            voidCap.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // face downward
            voidCap.transform.localScale = new Vector3(voidCapSize.x, voidCapSize.y, 1f);
            var renderer = voidCap.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = Color.black;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
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

        // Recycle chute segments: when the player falls below a segment, move it to the bottom
        RecycleChuteSegments();

        // Keep void cap above player
        if (voidCap != null)
            voidCap.transform.position = playerMove.transform.position + Vector3.up * voidCapHeight;

        // Wind volume increases with speed
        if (audioSource != null && windLoop != null)
            audioSource.volume = Mathf.Lerp(windVolume * 0.3f, windVolume, fallSpeed / maxFallSpeed);

        // Landing detection — grounded check OR passed below landing height
        if (controller.isGrounded || playerMove.transform.position.y <= landingY)
        {
            StartCoroutine(LandingSequence());
        }
    }

    /// <summary>
    /// Infinite scroll: when the player falls past the top segment, move it below the bottom one.
    /// </summary>
    private void RecycleChuteSegments()
    {
        if (chuteSegments.Count < 2) return;

        float playerY = playerMove.transform.position.y;

        // Find the highest and lowest segment
        int highestIdx = 0;
        int lowestIdx = 0;
        for (int i = 1; i < chuteSegments.Count; i++)
        {
            if (chuteSegments[i].transform.position.y > chuteSegments[highestIdx].transform.position.y)
                highestIdx = i;
            if (chuteSegments[i].transform.position.y < chuteSegments[lowestIdx].transform.position.y)
                lowestIdx = i;
        }

        float highestY = chuteSegments[highestIdx].transform.position.y;
        float lowestY = chuteSegments[lowestIdx].transform.position.y;

        // If player is more than one segment below the highest, recycle it to below the lowest
        if (playerY < highestY - chuteSegmentHeight)
        {
            Vector3 pos = chuteSegments[lowestIdx].transform.position;
            pos.y -= chuteSegmentHeight;
            chuteSegments[highestIdx].transform.position = pos;
        }
    }

    private IEnumerator LandingSequence()
    {
        falling = false;
        landed = true;

        // Snap player to landing position
        controller.enabled = false;
        var pos = playerMove.transform.position;
        playerMove.transform.position = new Vector3(pos.x, landingY, pos.z);
        controller.enabled = true;

        // Stop wind
        if (audioSource != null)
            audioSource.Stop();

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

        // Destroy all chute segments and void cap
        foreach (var seg in chuteSegments)
        {
            if (seg != null) Destroy(seg);
        }
        chuteSegments.Clear();
        if (voidCap != null)
            Destroy(voidCap);

        // Seal the hole with a ceiling cap
        if (ceilingCapPrefab != null)
        {
            Vector3 capPos = ceilingCapSpawnPoint != null ? ceilingCapSpawnPoint.position : transform.position;
            Quaternion capRot = ceilingCapSpawnPoint != null ? ceilingCapSpawnPoint.rotation : Quaternion.identity;
            Instantiate(ceilingCapPrefab, capPos, capRot);
        }

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

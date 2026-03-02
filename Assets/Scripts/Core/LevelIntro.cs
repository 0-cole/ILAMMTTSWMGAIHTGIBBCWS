using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ULTRAKILL-style level intro: player drops from above through a chute
/// with wind SFX, lands with crash + camera shake + freeze frame.
/// 
/// Chute segments spawn centered on the player using child transform
/// averaging (no renderer bounds needed). New segments spawn as the
/// player falls past half the segment height.
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
    [Tooltip("A tube segment prefab (4 walls). Spawns centered on the player as they fall.")]
    [SerializeField] private GameObject chutePrefab;
    [Tooltip("Manual XYZ tweak added AFTER auto-centering. Use to nudge the chute if it's slightly off.")]
    [SerializeField] private Vector3 chuteOffset = Vector3.zero;
    [Tooltip("Height of the walls in the chute prefab (check ProBuilder Object Size Y).")]
    [SerializeField] private float wallHeight = 4.17f;
    [Tooltip("Stop spawning segments this many meters above the landing point.")]
    [SerializeField] private float stopSpawningAboveLanding = 10f;

    [Header("Hole Seal")]
    [SerializeField] private GameObject ceilingCapPrefab;
    [SerializeField] private Transform ceilingCapSpawnPoint;

    [Header("Void Cap")]
    [SerializeField] private Vector2 voidCapSize = new Vector2(6f, 6f);
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
    private float lastSpawnY;
    private bool doneSpawning = false;

    void Start()
    {
        playerMove = FindFirstObjectByType<DoomMovement>();
        if (playerMove == null)
        {
            Debug.LogWarning("[LevelIntro] No DoomMovement found -- skipping intro.");
            Destroy(gameObject);
            return;
        }

        controller = playerMove.GetComponent<CharacterController>();
        playerLook = playerMove.GetComponentInChildren<MouseLook>();
        if (playerLook == null)
            playerLook = FindFirstObjectByType<MouseLook>();

        playerMove.enabled = false;
        if (playerLook != null) playerLook.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        landingY = playerMove.transform.position.y;

        controller.enabled = false;
        playerMove.transform.position += Vector3.up * spawnHeight;
        controller.enabled = true;

        // Spawn first chute centered on player
        if (chutePrefab != null)
        {
            SpawnChuteCentered();
            lastSpawnY = playerMove.transform.position.y;
        }

        // Void cap
        if (voidCapSize.x > 0 && voidCapSize.y > 0)
        {
            voidCap = GameObject.CreatePrimitive(PrimitiveType.Quad);
            voidCap.name = "VoidCap";
            Destroy(voidCap.GetComponent<Collider>());
            voidCap.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            voidCap.transform.localScale = new Vector3(voidCapSize.x, voidCapSize.y, 1f);
            var rend = voidCap.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Unlit/Color"));
            rend.material.color = Color.black;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

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

        fallSpeed += fallGravity * Time.deltaTime;
        fallSpeed = Mathf.Min(fallSpeed, maxFallSpeed);
        controller.Move(Vector3.down * fallSpeed * Time.deltaTime);

        float playerY = playerMove.transform.position.y;

        // Spawn new segment when player has fallen half the wall height past last spawn
        if (chutePrefab != null && !doneSpawning)
        {
            if (playerY < lastSpawnY - wallHeight * 0.5f)
            {
                if (playerY < landingY + stopSpawningAboveLanding)
                {
                    doneSpawning = true;
                }
                else
                {
                    SpawnChuteCentered();
                    lastSpawnY = playerY;
                }
            }
        }

        if (voidCap != null)
            voidCap.transform.position = playerMove.transform.position + Vector3.up * voidCapHeight;

        if (audioSource != null && windLoop != null)
            audioSource.volume = Mathf.Lerp(windVolume * 0.3f, windVolume, fallSpeed / maxFallSpeed);

        if (controller.isGrounded || playerY <= landingY)
            StartCoroutine(LandingSequence());
    }

    /// <summary>
    /// Spawns the chute prefab centered on the player by averaging
    /// direct children positions, then applies the manual chuteOffset tweak.
    /// </summary>
    private void SpawnChuteCentered()
    {
        Vector3 playerPos = playerMove.transform.position;

        // Instantiate at origin first
        GameObject seg = Instantiate(chutePrefab, Vector3.zero, Quaternion.identity);

        // Average direct children positions to find the visual center
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (Transform child in seg.transform)
        {
            sum += child.position;
            count++;
        }

        Vector3 childCenter = count > 0 ? sum / count : Vector3.zero;
        // Shift so child center aligns with player, then apply manual offset
        seg.transform.position = playerPos - childCenter + chuteOffset;

        seg.name = "ChuteSegment_" + chuteSegments.Count;

        // Disable all colliders so player falls through
        foreach (var col in seg.GetComponentsInChildren<Collider>())
            col.enabled = false;

        chuteSegments.Add(seg);
    }

    private IEnumerator LandingSequence()
    {
        falling = false;
        landed = true;

        controller.enabled = false;
        var pos = playerMove.transform.position;
        playerMove.transform.position = new Vector3(pos.x, landingY, pos.z);
        controller.enabled = true;

        if (audioSource != null) audioSource.Stop();

        if (crashSound != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(crashSound, crashVolume);
        }

        CameraShake shake = FindFirstObjectByType<CameraShake>();
        if (shake != null)
            shake.Shake(landingShakeDuration, landingShakeMagnitude);

        foreach (var seg in chuteSegments)
            if (seg != null) Destroy(seg);
        chuteSegments.Clear();

        if (voidCap != null) Destroy(voidCap);

        if (ceilingCapPrefab != null)
        {
            Vector3 capPos = ceilingCapSpawnPoint != null ? ceilingCapSpawnPoint.position : transform.position;
            Quaternion capRot = ceilingCapSpawnPoint != null ? ceilingCapSpawnPoint.rotation : Quaternion.identity;
            Instantiate(ceilingCapPrefab, capPos, capRot);
        }

        if (landingFreezeDuration > 0f)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(landingFreezeDuration);
            Time.timeScale = 1f;
        }

        playerMove.enabled = true;
        if (playerLook != null) playerLook.enabled = true;

        if (crashSound != null)
            yield return new WaitForSeconds(crashSound.length);

        Destroy(gameObject);
    }
}

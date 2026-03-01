using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ULTRAKILL-style level intro: player drops from above through a continuously-spawning
/// chute with wind SFX, lands with a crash + camera shake + freeze frame, then gameplay begins.
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
    [Tooltip("A tube segment prefab (4 walls). Clones spawn continuously as the player falls.")]
    [SerializeField] private GameObject chutePrefab;
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
    private float lowestSegmentBottomY = float.MaxValue;
    private float segmentHeight = 0f;
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

        // Spawn first chute segment centered on the player
        if (chutePrefab != null)
            SpawnSegmentCenteredAt(playerMove.transform.position);

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

        fallSpeed += fallGravity * Time.deltaTime;
        fallSpeed = Mathf.Min(fallSpeed, maxFallSpeed);
        controller.Move(Vector3.down * fallSpeed * Time.deltaTime);

        // Spawn new segments when the player is getting close to the bottom of existing ones
        if (chutePrefab != null && !doneSpawning)
        {
            float playerY = playerMove.transform.position.y;
            // When within one segment height of the bottom, spawn a new one below
            while (playerY < lowestSegmentBottomY + segmentHeight)
            {
                float nextCenterY = lowestSegmentBottomY - segmentHeight * 0.5f;
                if (nextCenterY < landingY + stopSpawningAboveLanding)
                {
                    doneSpawning = true;
                    break;
                }
                Vector3 target = new Vector3(playerMove.transform.position.x, nextCenterY, playerMove.transform.position.z);
                SpawnSegmentCenteredAt(target);
            }
        }

        if (voidCap != null)
            voidCap.transform.position = playerMove.transform.position + Vector3.up * voidCapHeight;

        if (audioSource != null && windLoop != null)
            audioSource.volume = Mathf.Lerp(windVolume * 0.3f, windVolume, fallSpeed / maxFallSpeed);

        if (controller.isGrounded || playerMove.transform.position.y <= landingY)
            StartCoroutine(LandingSequence());
    }

    /// <summary>
    /// Spawns a chute segment, then measures its actual renderer bounds and
    /// shifts it so the visual center ends up at the target position.
    /// No manual offset or bounds pre-calculation needed.
    /// </summary>
    private void SpawnSegmentCenteredAt(Vector3 target)
    {
        GameObject seg = Instantiate(chutePrefab, target, Quaternion.identity);

        // Measure where the visual center actually ended up
        var renderers = seg.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            // Shift so visual center matches target
            Vector3 shift = target - bounds.center;
            seg.transform.position += shift;

            // Track segment height from first segment
            if (segmentHeight < 0.1f)
            {
                segmentHeight = bounds.size.y;
                Debug.Log("[LevelIntro] Measured segment height: " + segmentHeight);
            }

            // Track the bottom edge of the lowest segment
            float thisBottom = target.y - segmentHeight * 0.5f;
            if (thisBottom < lowestSegmentBottomY)
                lowestSegmentBottomY = thisBottom;
        }

        seg.name = "ChuteSegment_" + chuteSegments.Count;
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

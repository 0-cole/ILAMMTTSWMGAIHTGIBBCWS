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
    [Tooltip("Height of one chute segment in world units.")]
    [SerializeField] private float chuteSegmentHeight = 10f;
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
    private float nextSpawnY;
    private bool doneSpawning = false;
    private Vector3 prefabBoundsCenter;

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

        // Measure the prefab once to know its visual center
        if (chutePrefab != null)
        {
            MeasurePrefab();
            // Spawn the first segment centered on the player
            float playerY = playerMove.transform.position.y;
            SpawnSegmentAtVisualY(playerY);
            nextSpawnY = playerY - chuteSegmentHeight;
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

        // Continuously spawn new segments below as the player falls
        if (chutePrefab != null && !doneSpawning)
        {
            float playerY = playerMove.transform.position.y;
            // When the player is within half a segment of needing the next one, spawn it
            while (playerY < nextSpawnY + chuteSegmentHeight * 0.5f)
            {
                if (nextSpawnY < landingY + stopSpawningAboveLanding)
                {
                    doneSpawning = true;
                    break;
                }
                SpawnSegmentAtVisualY(nextSpawnY);
                nextSpawnY -= chuteSegmentHeight;
            }
        }

        if (voidCap != null)
            voidCap.transform.position = playerMove.transform.position + Vector3.up * voidCapHeight;

        if (audioSource != null && windLoop != null)
            audioSource.volume = Mathf.Lerp(windVolume * 0.3f, windVolume, fallSpeed / maxFallSpeed);

        if (controller.isGrounded || playerMove.transform.position.y <= landingY)
            StartCoroutine(LandingSequence());
    }

    private void MeasurePrefab()
    {
        GameObject temp = Instantiate(chutePrefab, Vector3.zero, Quaternion.identity);
        var renderers = temp.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            prefabBoundsCenter = bounds.center;
            float measuredHeight = bounds.size.y;
            if (measuredHeight > 0.1f && Mathf.Abs(chuteSegmentHeight - 10f) < 0.01f)
            {
                chuteSegmentHeight = measuredHeight;
                Debug.Log("[LevelIntro] Auto-detected segment height: " + chuteSegmentHeight);
            }
            Debug.Log("[LevelIntro] Prefab center: " + prefabBoundsCenter + " height: " + bounds.size.y);
        }
        Destroy(temp);
    }

    /// <summary>
    /// Spawns a chute segment so its visual center is at the given Y,
    /// and centered on the player's XZ.
    /// </summary>
    private void SpawnSegmentAtVisualY(float visualY)
    {
        Vector3 rootPos = new Vector3(
            playerMove.transform.position.x - prefabBoundsCenter.x,
            visualY - prefabBoundsCenter.y,
            playerMove.transform.position.z - prefabBoundsCenter.z
        );
        GameObject seg = Instantiate(chutePrefab, rootPos, Quaternion.identity);
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

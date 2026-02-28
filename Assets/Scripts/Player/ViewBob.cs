using UnityEngine;

/// <summary>
/// Classic DOOM-style weapon bob. Attach to Main Camera.
/// Bobs the weapon (wand) up/down and side-to-side when the player walks.
/// Also compensates for FOV changes so the weapon stays in the same screen position.
/// Reads enable/disable from GameSettings.
/// </summary>
public class ViewBob : MonoBehaviour
{
    [Header("Weapon Bob Settings")]
    [Tooltip("The weapon/wand transform to bob. Auto-found if left empty.")]
    [SerializeField] private Transform weaponTransform;

    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitudeY = 0.06f;
    [SerializeField] private float bobAmplitudeX = 0.04f;
    [SerializeField] private float bobSmoothing = 10f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Idle Sway (subtle breathing motion)")]
    [SerializeField] private float idleSwayAmount = 0.003f;
    [SerializeField] private float idleSwaySpeed = 1.5f;

    [Header("FOV Compensation")]
    [Tooltip("FOV at which the weapon was originally positioned.")]
    [SerializeField] private float referenceFOV = 60f;
    [Tooltip("How much the weapon scale compensates for FOV changes. 0 = no scaling, 1 = full compensation.")]
    [Range(0f, 1f)]
    [SerializeField] private float fovScaleStrength = 0.4f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;

    private Vector3 weaponOriginalPosition; // The position set in the editor at referenceFOV
    private Vector3 weaponOriginalScale;    // The scale set in the editor at referenceFOV
    private float bobTimer;
    private bool viewBobEnabled = true;
    private bool initialized;
    private Camera cam;

    // Smooth bob offset to avoid jitter
    private Vector3 currentBobOffset;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (initialized) return;

        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();

        // Auto-find weapon
        if (weaponTransform == null)
        {
            Transform found = transform.Find("Wand");
            if (found == null) found = transform.Find("Weapon");
            if (found == null && transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.Contains("Punch") || child.name.Contains("Canvas")) continue;
                    found = child;
                    break;
                }
            }
            weaponTransform = found;
        }

        if (weaponTransform != null)
        {
            weaponOriginalPosition = weaponTransform.localPosition;
            weaponOriginalScale = weaponTransform.localScale;
            Debug.Log($"[ViewBob] Attached to weapon: {weaponTransform.name}");
        }
        else
        {
            Debug.LogWarning("[ViewBob] No weapon transform found! Assign it in the Inspector.");
        }

        if (GameSettings.Instance != null)
        {
            viewBobEnabled = GameSettings.Instance.ViewBobEnabled;
            GameSettings.Instance.OnSettingsChanged += OnSettingsChanged;
        }

        initialized = true;
    }

    void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSettingsChanged -= OnSettingsChanged;
    }

    void OnSettingsChanged()
    {
        if (GameSettings.Instance != null)
            viewBobEnabled = GameSettings.Instance.ViewBobEnabled;

        if (!viewBobEnabled)
            bobTimer = 0f;
    }

    void LateUpdate()
    {
        if (!initialized) Initialize();
        if (PauseManager.IsGamePaused) return;
        if (characterController == null || weaponTransform == null) return;

        // --- 1. Calculate FOV-compensated base position ---
        Vector3 basePos = weaponOriginalPosition;
        if (cam != null && referenceFOV > 0f)
        {
            float fovRatio = referenceFOV / cam.fieldOfView;
            // Only scale X and Y to keep weapon at same screen edge;
            // keep Z (depth) the same so it doesn't clip behind near plane
            basePos = new Vector3(
                weaponOriginalPosition.x * fovRatio,
                weaponOriginalPosition.y * fovRatio,
                weaponOriginalPosition.z
            );
        }

        // --- 2. Calculate bob offset ---
        Vector3 targetBobOffset = Vector3.zero;

        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        float speed = horizontalVelocity.magnitude;
        bool isMoving = speed > 0.5f && characterController.isGrounded;

        if (isMoving && viewBobEnabled)
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float freqMul = isSprinting ? sprintMultiplier : 1f;
            float ampMul = isSprinting ? sprintMultiplier : 1f;
            float speedFactor = Mathf.Clamp01(speed / 8f);
            ampMul *= speedFactor;

            bobTimer += Time.deltaTime * bobFrequency * freqMul;

            float bobY = Mathf.Sin(bobTimer) * bobAmplitudeY * ampMul;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitudeX * ampMul;
            targetBobOffset = new Vector3(bobX, bobY, 0f);
        }
        else if (viewBobEnabled)
        {
            float idleY = Mathf.Sin(Time.time * idleSwaySpeed) * idleSwayAmount;
            float idleX = Mathf.Cos(Time.time * idleSwaySpeed * 0.7f) * idleSwayAmount * 0.5f;
            targetBobOffset = new Vector3(idleX, idleY, 0f);
            bobTimer = 0f;
        }
        else
        {
            bobTimer = 0f;
        }

        // --- 3. Smooth the bob offset ---
        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, bobSmoothing * Time.deltaTime);

        // --- 4. Apply: base position + bob offset ---
        weaponTransform.localPosition = basePos + currentBobOffset;

        // --- 5. FOV-based scale compensation ---
        // Partially scale the weapon so it doesn't shrink too much at high FOV
        if (cam != null && referenceFOV > 0f)
        {
            float rawScale = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad)
                           / Mathf.Tan(referenceFOV * 0.5f * Mathf.Deg2Rad);
            float fovScale = Mathf.Lerp(1f, rawScale, fovScaleStrength);
            weaponTransform.localScale = weaponOriginalScale * fovScale;
        }
    }
}

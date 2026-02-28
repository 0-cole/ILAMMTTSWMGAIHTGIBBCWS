using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image weaponText; 
    [SerializeField] private TMPro.TextMeshProUGUI manaText; // New: Percentage text
    [SerializeField] private Image manaBarFill;
    [SerializeField] private Image panelBackground;

    [Header("Weapon Sprites")]
    [SerializeField] private Sprite fireballIconSprite;
    [SerializeField] private Sprite lightningIconSprite;
    [SerializeField] private Sprite fireballTextSprite;
    [SerializeField] private Sprite lightningTextSprite;

    [Header("Animation Settings")]
    [SerializeField] private float iconSpinSpeed = 90f; 
    [SerializeField] private float fillSmoothSpeed = 10f;
    [SerializeField] private float colorSmoothSpeed = 5f; // New: Smooth color transition
    [SerializeField] private float flipDuration = 0.2f; // New: Flip animation speed

    [Header("Color Coding")]
    [SerializeField] private Color fullColor = new Color(0f, 1f, 1f, 1f); 
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private float lowThreshold = 0.3f;
    [SerializeField] private float mediumThreshold = 0.6f;

    private float targetFill = 1f;
    private int lastWeaponType = -1; 
    private Coroutine currentFlipRoutine;

    [Header("3D Icon Settings")]
    [SerializeField] private Transform modelSpawnPoint; // Where to spawn the 3D book in HUD
    [SerializeField] private float modelSpinSpeed = 90f;
    [SerializeField] private float modelScale = 100f; // Scale up for UI
    
    private GameObject currentModelObject;

    void Start()
    {
        // weaponController should be assigned in Inspector for best performance
        // Fallback to Find if not assigned (but log warning)
        if (weaponController == null)
        {
            weaponController = FindFirstObjectByType<WeaponController>();
            if (weaponController == null)
            {
                Debug.LogError("[WeaponDisplay] WeaponController not found! Please assign in Inspector.");
            }
            else
            {
                Debug.LogWarning("[WeaponDisplay] WeaponController found via search. Assign in Inspector for better performance.");
            }
        }

        // Set initial transparency
        if (panelBackground != null)
        {
            CanvasGroup group = panelBackground.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 0.85f;
            }
        }
    }

    void Update()
    {
        if (weaponController == null) return;

        if (weaponController == null || weaponController.weapons.Count == 0) return;

        int currentType = weaponController.weapons[weaponController.currentWeaponIndex].weaponTypeIndex;

        if (currentType != lastWeaponType)
        {
            if (currentFlipRoutine != null) StopCoroutine(currentFlipRoutine);
            currentFlipRoutine = StartCoroutine(FlipWeaponText(currentType));
            
            lastWeaponType = currentType;
        }

        // Spin the weapon icon (2D or 3D)
        if (currentModelObject != null)
        {
            currentModelObject.transform.Rotate(Vector3.up, modelSpinSpeed * Time.deltaTime);
        }
        else if (weaponIcon != null)
        {
            weaponIcon.transform.Rotate(Vector3.forward, iconSpinSpeed * Time.deltaTime);
        }

        // Update mana bar
        if (manaBarFill != null)
        {
            targetFill = weaponController.currentMana / weaponController.maxMana;
            manaBarFill.fillAmount = Mathf.Lerp(manaBarFill.fillAmount, targetFill, fillSmoothSpeed * Time.deltaTime);

            // Color coding logic
            Color targetColor = fullColor;
            if (targetFill <= lowThreshold) targetColor = lowColor;
            else if (targetFill <= mediumThreshold) targetColor = mediumColor;

            // Smooth color transition
            manaBarFill.color = Color.Lerp(manaBarFill.color, targetColor, colorSmoothSpeed * Time.deltaTime);

            // Update Text
            if (manaText != null)
            {
                manaText.text = $"{Mathf.CeilToInt(targetFill * 100)}%";
                manaText.color = manaBarFill.color; // Match the bar color
            }
        }
    }

    System.Collections.IEnumerator FlipWeaponText(int type)
    {
        // 1. Rotate to 90 degrees (Invisible)
        float elapsed = 0f;
        Quaternion startRot = weaponText.transform.localRotation;
        Quaternion midRot = Quaternion.Euler(90f, 0f, 0f); // Rotate around X axis

        while (elapsed < flipDuration / 2f)
        {
            elapsed += Time.deltaTime;
            weaponText.transform.localRotation = Quaternion.Lerp(startRot, midRot, elapsed / (flipDuration / 2f));
            yield return null;
        }
        weaponText.transform.localRotation = midRot;

        // 2. Change Sprite / Model
        if (weaponController != null && weaponController.weapons.Count > type)
        {
            // Update 2D Text
            if (type == 0) // Fireball
            {
                if (weaponText != null) weaponText.sprite = fireballTextSprite;
            }
            else // Lightning
            {
                if (weaponText != null) weaponText.sprite = lightningTextSprite;
            }

            // Update Icon (2D or 3D)
            GameObject prefab = weaponController.weapons[weaponController.currentWeaponIndex].modelPrefab;
            
            // Clean up old model
            if (currentModelObject != null) Destroy(currentModelObject);

            if (prefab != null && modelSpawnPoint != null)
            {
                // Use 3D Model
                if (weaponIcon != null) weaponIcon.enabled = false; // Hide 2D icon

                currentModelObject = Instantiate(prefab, modelSpawnPoint);
                currentModelObject.transform.localPosition = Vector3.zero;
                currentModelObject.transform.localRotation = Quaternion.identity;
                currentModelObject.transform.localScale = Vector3.one * modelScale;
                SetLayerRecursively(currentModelObject, LayerMask.NameToLayer("UI"));
            }
            else
            {
                // Use 2D Sprite Fallback
                if (weaponIcon != null) 
                {
                    weaponIcon.enabled = true;
                    if (type == 0) weaponIcon.sprite = fireballIconSprite;
                    else weaponIcon.sprite = lightningIconSprite; 
                }
            }
        }

        // 3. Rotate from -90 to 0 (Flip in)
        elapsed = 0f;
        Quaternion enterRot = Quaternion.Euler(-90f, 0f, 0f);
        Quaternion finalRot = Quaternion.identity;

        while (elapsed < flipDuration / 2f)
        {
            elapsed += Time.deltaTime;
            weaponText.transform.localRotation = Quaternion.Lerp(enterRot, finalRot, elapsed / (flipDuration / 2f));
            yield return null;
        }
        weaponText.transform.localRotation = finalRot;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (newLayer < 0) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

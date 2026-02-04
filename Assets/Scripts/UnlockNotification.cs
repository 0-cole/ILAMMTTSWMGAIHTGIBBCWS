using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockNotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup notificationGroup;
    [SerializeField] private Image unlockIcon;
    [SerializeField] private TextMeshProUGUI unlockText;
    [SerializeField] private Image flashOverlay;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 5f;
    
    private float timer;
    private bool isShowing;

    public static UnlockNotification Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (notificationGroup != null) notificationGroup.alpha = 0f;
        if (flashOverlay != null) 
        {
            Color c = flashOverlay.color;
            c.a = 0f;
            flashOverlay.color = c;
        }
    }

    [Header("3D Settings")]
    [SerializeField] private Transform modelSpawnPoint; // Where to spawn the 3D book
    [SerializeField] private float modelSpinSpeed = 90f;
    [SerializeField] private float modelScale = 100f; // Scale up for UI
    
    private GameObject currentModelObject;

    public void ShowUnlock(string weaponName, Sprite icon = null, GameObject modelPrefab = null, string subtitle = "")
    {
        if (notificationGroup == null) return;

        string mainText = $"UNLOCKED\n<color=yellow>{weaponName.ToUpper()}</color>";
        if (!string.IsNullOrEmpty(subtitle))
        {
            mainText += $"\n<size=50%>{subtitle}</size>";
        }
        unlockText.text = mainText;
        
        // Handle 2D Icon
        if (unlockIcon != null)
        {
            unlockIcon.gameObject.SetActive(icon != null && modelPrefab == null);
            unlockIcon.sprite = icon;
        }

        // Handle 3D Model
        if (currentModelObject != null) Destroy(currentModelObject);
        
        if (modelPrefab != null && modelSpawnPoint != null)
        {
            currentModelObject = Instantiate(modelPrefab, modelSpawnPoint);
            currentModelObject.transform.localPosition = Vector3.zero;
            currentModelObject.transform.localRotation = Quaternion.identity;
            
            // Set scale (might need adjustment based on the asset)
            currentModelObject.transform.localScale = Vector3.one * modelScale; 
            
            // Ensure it renders on top of UI if using Screen Space Camera, 
            // otherwise for Overlay it might look weird without a special shader, 
            // but we'll assume standard setup for now.
            SetLayerRecursively(currentModelObject, LayerMask.NameToLayer("UI")); 
        }

        isShowing = true;
        timer = displayDuration;
        notificationGroup.alpha = 1f;

        // Flash Effect
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0.8f;
            flashOverlay.color = c;
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (newLayer < 0) return; // Invalid layer
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void Update()
    {
        // specific rotation logic
        if (currentModelObject != null)
        {
            currentModelObject.transform.Rotate(Vector3.up, modelSpinSpeed * Time.deltaTime);
        }

        // Fade out flash
        if (flashOverlay != null && flashOverlay.color.a > 0)
        {
            Color c = flashOverlay.color;
            c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 10f);
            flashOverlay.color = c;
        }

        // Handle Notification Display
        if (isShowing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isShowing = false;
            }
        }
        else if (notificationGroup != null && notificationGroup.alpha > 0)
        {
            notificationGroup.alpha -= Time.deltaTime * fadeSpeed;
        }
    }
}

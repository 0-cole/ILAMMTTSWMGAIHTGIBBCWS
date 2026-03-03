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

    [Header("Timing")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float slideSpeed = 8f;
    
    [Header("Slide Animation")]
    [SerializeField] private float slideDistance = 80f; // Pixels to slide in from
    
    [Header("3D Settings")]
    [SerializeField] private Transform modelSpawnPoint;
    [SerializeField] private float modelSpinSpeed = 90f;
    [SerializeField] private float modelScale = 100f;
    
    private float timer;
    private bool isShowing;
    private GameObject currentModelObject;
    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private Vector2 hiddenPosition;

    public static UnlockNotification Instance;

    void Awake()
    {
        Instance = this;
        rectTransform = notificationGroup?.GetComponent<RectTransform>();
        if (notificationGroup != null) notificationGroup.alpha = 0f;
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

        // Store positions for slide animation
        if (rectTransform != null)
        {
            targetPosition = rectTransform.anchoredPosition;
            hiddenPosition = targetPosition + new Vector2(0, slideDistance);
            rectTransform.anchoredPosition = hiddenPosition;
        }
    }

    public void ShowUnlock(string weaponName, Sprite icon = null, GameObject modelPrefab = null, string subtitle = "")
    {
        if (notificationGroup == null) return;

        // Build styled text
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
        CleanupModel();
        
        if (modelPrefab != null && modelSpawnPoint != null)
        {
            currentModelObject = Instantiate(modelPrefab, modelSpawnPoint);
            currentModelObject.transform.localPosition = Vector3.zero;
            currentModelObject.transform.localRotation = Quaternion.identity;
            currentModelObject.transform.localScale = Vector3.one * modelScale; 
            SetLayerRecursively(currentModelObject, LayerMask.NameToLayer("UI")); 
        }

        // Start showing
        isShowing = true;
        timer = displayDuration;
        notificationGroup.alpha = 1f;

        // Slide in from above
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = hiddenPosition;
        }

        // Flash Effect
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0.8f;
            flashOverlay.color = c;
        }
    }

    void CleanupModel()
    {
        if (currentModelObject != null)
        {
            Destroy(currentModelObject);
            currentModelObject = null;
        }
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

    void Update()
    {
        // Spin 3D model
        if (currentModelObject != null)
        {
            currentModelObject.transform.Rotate(Vector3.up, modelSpinSpeed * Time.deltaTime);
        }

        // Fade out flash overlay
        if (flashOverlay != null && flashOverlay.color.a > 0)
        {
            Color c = flashOverlay.color;
            c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 10f);
            if (c.a < 0.01f) c.a = 0f;
            flashOverlay.color = c;
        }

        // Slide animation
        if (rectTransform != null)
        {
            Vector2 goal = isShowing ? targetPosition : hiddenPosition;
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition, goal, Time.deltaTime * slideSpeed);
        }

        // Handle notification timer
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

            // Cleanup once fully faded
            if (notificationGroup.alpha <= 0.01f)
            {
                notificationGroup.alpha = 0f;
                CleanupModel();
                // Snap to hidden position so it's fully offscreen
                if (rectTransform != null)
                    rectTransform.anchoredPosition = hiddenPosition;
            }
        }
    }
}

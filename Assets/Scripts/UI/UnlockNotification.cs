using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UnlockNotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup notificationGroup;
    [SerializeField] private Image unlockIcon;
    [SerializeField] private TextMeshProUGUI unlockText;
    [SerializeField] private Image flashOverlay;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float slideSpeed = 8f;
    
    [Header("Slide Animation")]
    [SerializeField] private float slideDistance = 80f;
    
    [Header("3D Settings")]
    [SerializeField] private Transform modelSpawnPoint;
    [SerializeField] private float modelSpinSpeed = 90f;
    [SerializeField] private float modelScale = 100f;
    
    private bool isShowing;
    public bool IsShowing => isShowing;
    private GameObject currentModelObject;
    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private Vector2 hiddenPosition;
    private Coroutine activeRoutine;

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

        // Cancel any existing notification
        if (activeRoutine != null) StopCoroutine(activeRoutine);

        string mainText = $"UNLOCKED\n<color=yellow>{weaponName.ToUpper()}</color>";
        if (!string.IsNullOrEmpty(subtitle))
            mainText += $"\n<size=50%>{subtitle}</size>";
        unlockText.text = mainText;
        
        if (unlockIcon != null)
        {
            unlockIcon.gameObject.SetActive(icon != null && modelPrefab == null);
            unlockIcon.sprite = icon;
        }

        CleanupModel();
        
        if (modelPrefab != null && modelSpawnPoint != null)
        {
            currentModelObject = Instantiate(modelPrefab, modelSpawnPoint);
            currentModelObject.transform.localPosition = Vector3.zero;
            currentModelObject.transform.localRotation = Quaternion.identity;
            currentModelObject.transform.localScale = Vector3.one * modelScale; 
            SetLayerRecursively(currentModelObject, LayerMask.NameToLayer("UI")); 
        }

        // Flash
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0.8f;
            flashOverlay.color = c;
        }

        activeRoutine = StartCoroutine(NotificationLifecycle());
    }

    private IEnumerator NotificationLifecycle()
    {
        isShowing = true;
        notificationGroup.alpha = 1f;

        // Slide in from hidden position
        if (rectTransform != null)
            rectTransform.anchoredPosition = hiddenPosition;

        // Force minimum durations in case serialized values are bad
        float showTime = Mathf.Max(displayDuration, 1f);
        float fadeTime = Mathf.Max(fadeOutDuration, 0.3f);

        // Display phase — slide in + hold
        float elapsed = 0f;
        while (elapsed < showTime)
        {
            elapsed += Time.deltaTime;

            // Slide toward visible position
            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.Lerp(
                    rectTransform.anchoredPosition, targetPosition, Time.deltaTime * slideSpeed);

            // Spin model
            if (currentModelObject != null)
                currentModelObject.transform.Rotate(Vector3.up, modelSpinSpeed * Time.deltaTime);

            // Fade flash overlay
            if (flashOverlay != null && flashOverlay.color.a > 0)
            {
                Color c = flashOverlay.color;
                c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 10f);
                flashOverlay.color = c;
            }

            yield return null;
        }

        // Fade out phase
        float startAlpha = notificationGroup.alpha;
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            notificationGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            // Slide toward hidden
            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.Lerp(
                    rectTransform.anchoredPosition, hiddenPosition, Time.deltaTime * slideSpeed);

            // Keep spinning model
            if (currentModelObject != null)
                currentModelObject.transform.Rotate(Vector3.up, modelSpinSpeed * Time.deltaTime);

            yield return null;
        }

        // Fully hidden
        notificationGroup.alpha = 0f;
        if (rectTransform != null)
            rectTransform.anchoredPosition = hiddenPosition;
        CleanupModel();
        isShowing = false;
        activeRoutine = null;
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
            SetLayerRecursively(child.gameObject, newLayer);
    }
}

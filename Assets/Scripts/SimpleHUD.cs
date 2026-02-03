using UnityEngine;
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthPanelBackground;
    [SerializeField] private Image damageOverlay; // Optional: full-screen red flash

    [Header("Animation Settings")]
    [SerializeField] private float fillSmoothSpeed = 10f;
    [SerializeField] private float colorSmoothSpeed = 5f; // New: Smooth color transition
    [SerializeField] private float overlayFadeSpeed = 2f;

    [Header("Color Coding")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float damagedThreshold = 0.6f;
    [SerializeField] private float criticalThreshold = 0.3f;

    private float targetFill = 1f;
    private float overlayAlpha = 0f;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.GetHealthPercent() * 100f, 100f);
        }

        // Set initial transparency
        if (healthPanelBackground != null)
        {
            CanvasGroup group = healthPanelBackground.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 0.85f;
            }
        }

        // Hide damage overlay initially
        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }
    }

    void Update()
    {
        // Smooth health bar animation
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, fillSmoothSpeed * Time.deltaTime);

            // Color coding logic
            Color targetColor = healthyColor;
            if (targetFill <= criticalThreshold) targetColor = criticalColor;
            else if (targetFill <= damagedThreshold) targetColor = damagedColor;

            // Smooth color transition
            healthBarFill.color = Color.Lerp(healthBarFill.color, targetColor, colorSmoothSpeed * Time.deltaTime);
        }

        // Fade out damage overlay
        if (damageOverlay != null && overlayAlpha > 0f)
        {
            overlayAlpha -= overlayFadeSpeed * Time.deltaTime;
            overlayAlpha = Mathf.Max(overlayAlpha, 0f);
            
            Color c = damageOverlay.color;
            c.a = overlayAlpha;
            damageOverlay.color = c;
        }
    }

    void UpdateHealth(float current, float max)
    {
        float previousFill = targetFill;
        targetFill = current / max;

        // Flash red when damaged
        if (targetFill < previousFill && damageOverlay != null)
        {
            overlayAlpha = 0.3f; // Flash intensity
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealth;
        }
    }
}

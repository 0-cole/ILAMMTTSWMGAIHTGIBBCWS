using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GlonkEnemy glonk;

    [Header("Behavior")]
    [SerializeField] private bool billboard = true; // Always face camera
    [SerializeField] private float smoothSpeed = 10f;

    private float targetFill = 1f;
    private Camera mainCamera;

    void Start()
    {
        // Auto-find references if missing
        if (glonk == null) glonk = GetComponentInParent<GlonkEnemy>();
        
        // If still null, try finding in children (case dependent)
        if (glonk == null) glonk = GetComponentInChildren<GlonkEnemy>();

        if (glonk != null)
        {
            // Subscribe to health events
            glonk.OnHealthChanged += UpdateHealth;
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        // Smoothly animate the bar
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, smoothSpeed * Time.deltaTime);
        }

        // Face the camera
        if (billboard && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    void UpdateHealth(float current, float max)
    {
        targetFill = current / max;
    }

    void OnDestroy()
    {
        if (glonk != null)
        {
            glonk.OnHealthChanged -= UpdateHealth;
        }
    }
}

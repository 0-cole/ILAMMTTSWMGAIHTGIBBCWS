using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GlonkEnemy glonk;
    [SerializeField] private BillboardShooter billboardShooter;

    [Header("Behavior")]
    [SerializeField] private bool billboard = true; // Always face camera
    [SerializeField] private float smoothSpeed = 10f;

    private float targetFill = 1f;
    private Camera mainCamera;

    void Start()
    {
        // Clear serialized references that may point to prefab instances
        // and re-discover from the actual runtime parent
        glonk = GetComponentInParent<GlonkEnemy>();
        billboardShooter = GetComponentInParent<BillboardShooter>();

        if (glonk != null)
        {
            glonk.OnHealthChanged += UpdateHealth;
        }
        else if (billboardShooter != null)
        {
            billboardShooter.OnHealthChanged += UpdateHealth;
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
        if (billboardShooter != null)
        {
            billboardShooter.OnHealthChanged -= UpdateHealth;
        }
    }
}

using UnityEngine;

/// <summary>
/// Shakes the title text forward (Z-axis scale pulse) on music beats.
/// Attach to the title text RectTransform. Does NOT affect menu buttons.
/// 
/// The "forward" shake is achieved by pulsing the scale — making the title
/// briefly punch outward toward the camera on each beat.
/// </summary>
public class TitleBeatShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDecay = 8f;
    [SerializeField] private float scalePunch = 1.12f;

    [Header("Optional Position Shake")]
    [SerializeField] private bool positionShake = true;
    [SerializeField] private float posShakeAmount = 5f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private float currentShake;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition3D;
            originalScale = rectTransform.localScale;
        }
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Check for beat
        if (TitleScreenMusic.Instance != null && TitleScreenMusic.Instance.IsBeat)
        {
            currentShake = 1f;
        }

        // Apply shake
        if (currentShake > 0.01f)
        {
            currentShake = Mathf.Lerp(currentShake, 0f, Time.deltaTime * shakeDecay);

            // Scale punch (forward feel)
            float s = Mathf.Lerp(1f, scalePunch, currentShake);
            rectTransform.localScale = originalScale * s;

            // Optional subtle position shake
            if (positionShake)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-1f, 1f) * posShakeAmount * currentShake,
                    Random.Range(-1f, 1f) * posShakeAmount * currentShake * 0.5f,
                    0f
                );
                rectTransform.anchoredPosition3D = originalPosition + offset;
            }
        }
        else
        {
            rectTransform.localScale = originalScale;
            rectTransform.anchoredPosition3D = originalPosition;
        }
    }
}

using UnityEngine;

/// <summary>
/// Subtle scale pulse on the title image on music beats.
/// Attach to the title Image RectTransform.
/// </summary>
public class TitleBeatShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDecay = 10f;
    [SerializeField] private float scalePunch = 1.03f;

    private Vector3 originalScale;
    private float currentShake;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;

            // Stretch to fill screen
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }

    void Update()
    {
        if (rectTransform == null) return;

        if (TitleScreenMusic.Instance != null && TitleScreenMusic.Instance.IsBeat)
        {
            currentShake = 1f;
        }

        if (currentShake > 0.001f)
        {
            currentShake = Mathf.Lerp(currentShake, 0f, Time.deltaTime * shakeDecay);
            float s = Mathf.Lerp(1f, scalePunch, currentShake);
            rectTransform.localScale = originalScale * s;
        }
        else
        {
            rectTransform.localScale = originalScale;
        }
    }
}

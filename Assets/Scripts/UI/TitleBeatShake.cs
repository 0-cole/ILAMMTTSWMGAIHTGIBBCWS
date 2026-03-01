using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flashes the title screen white on music beats with a smooth fade out.
/// Attach to the title Image. Creates an overlay Image for the flash effect.
/// Also ensures the title image stretches to fill the screen.
/// </summary>
public class TitleBeatShake : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashFadeSpeed = 3f;
    [SerializeField] private float flashIntensity = 0.6f;
    [SerializeField] private Color flashColor = Color.white;

    private Image flashOverlay;
    private float currentFlash;

    void Start()
    {
        // Ensure title image stretches to fill screen
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Create flash overlay as a sibling on top
        GameObject flashObj = new GameObject("BeatFlashOverlay");
        flashObj.transform.SetParent(transform.parent, false);
        flashObj.transform.SetAsLastSibling();

        RectTransform flashRT = flashObj.AddComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = Vector2.zero;
        flashRT.offsetMax = Vector2.zero;

        flashOverlay = flashObj.AddComponent<Image>();
        flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        flashOverlay.raycastTarget = false;
    }

    void Update()
    {
        if (flashOverlay == null) return;

        // Check for beat
        if (TitleScreenMusic.Instance != null && TitleScreenMusic.Instance.IsBeat)
        {
            currentFlash = flashIntensity;
        }

        // Fade out flash
        if (currentFlash > 0.001f)
        {
            currentFlash = Mathf.Lerp(currentFlash, 0f, Time.deltaTime * flashFadeSpeed);
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, currentFlash);
        }
        else
        {
            currentFlash = 0f;
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        }
    }
}

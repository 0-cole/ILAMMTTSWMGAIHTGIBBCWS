using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stretches the title image to fill the screen. Attach to the title Image.
/// </summary>
public class TitleBeatShake : MonoBehaviour
{
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Disable Preserve Aspect so the image truly fills the screen
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.preserveAspect = false;
            img.type = Image.Type.Simple;
        }

        // Set camera background to black to hide any edge gaps
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }
    }
}

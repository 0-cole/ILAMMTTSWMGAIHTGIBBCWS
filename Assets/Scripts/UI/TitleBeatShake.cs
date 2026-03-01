using UnityEngine;

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
    }
}

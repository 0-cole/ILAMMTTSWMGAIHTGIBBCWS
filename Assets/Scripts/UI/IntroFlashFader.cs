using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-destroying flash fade. Created by IntroSplash at runtime.
/// </summary>
public class IntroFlashFader : MonoBehaviour
{
    private Image img;
    private Color color;
    private float duration;
    private float timer;

    public void Init(Color flashColor, float fadeDuration)
    {
        color = flashColor;
        duration = fadeDuration;
        img = GetComponent<Image>();
    }

    void Update()
    {
        if (img == null) return;

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / duration);
        img.color = new Color(color.r, color.g, color.b, alpha);

        if (timer >= duration)
            Destroy(gameObject);
    }
}

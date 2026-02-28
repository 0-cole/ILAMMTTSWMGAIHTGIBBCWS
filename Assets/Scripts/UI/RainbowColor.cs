using UnityEngine;
using UnityEngine.UI;

public class RainbowColor : MonoBehaviour
{
    public float speed = 0.25f; // Slower, smoother speed
    private Button btn;
    private Image img;

    void Start()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();

        // We must reset the base image color to white!
        // Otherwise, Unity's Button ColorTint mode multiplies our rainbow
        // with the button's default base color (red), making it look dark and muddy.
        if (img != null)
        {
            img.color = Color.white;
        }
    }

    void Update()
    {
        float h = Mathf.Repeat(Time.time * speed, 1f);
        Color rainbowColor = Color.HSVToRGB(h, 0.75f, 1f); // Slightly softer rainbow

        // Update the Button's color block so it transitions cleanly and handles hover/press states
        if (btn != null)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = rainbowColor;
            cb.selectedColor = rainbowColor;
            
            // Generate darker/brighter shifted colors for hover and press
            cb.highlightedColor = Color.HSVToRGB(h, 0.5f, 1f); 
            cb.pressedColor = Color.HSVToRGB(h, 1f, 0.8f);
            
            btn.colors = cb;
        }
        else if (img != null)
        {
            // Fallback if there is no Button component
            img.color = rainbowColor;
        }
    }
}

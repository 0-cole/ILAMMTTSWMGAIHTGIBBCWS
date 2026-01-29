using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Dot Reticle Settings")]
    [SerializeField] private float dotSize = 8f;
    [SerializeField] private Color dotColor = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent white
    [SerializeField] private bool useOutline = true;
    [SerializeField] private Color outlineColor = new Color(0f, 0f, 0f, 0.3f);
    [SerializeField] private float outlineSize = 2f;

    private Image dotImage;
    private Image outlineImage;

    void Start()
    {
        CreateDotReticle();
    }

    void CreateDotReticle()
    {
        // Create outline first (behind the dot)
        if (useOutline)
        {
            GameObject outlineObj = new GameObject("ReticleOutline");
            outlineObj.transform.SetParent(transform);
            
            outlineImage = outlineObj.AddComponent<Image>();
            outlineImage.color = outlineColor;
            
            RectTransform outlineRect = outlineObj.GetComponent<RectTransform>();
            outlineRect.anchorMin = new Vector2(0.5f, 0.5f);
            outlineRect.anchorMax = new Vector2(0.5f, 0.5f);
            outlineRect.anchoredPosition = Vector2.zero;
            outlineRect.sizeDelta = new Vector2(dotSize + outlineSize, dotSize + outlineSize);
            
            // Make it circular
            outlineImage.sprite = CreateCircleSprite();
        }

        // Create the dot
        GameObject dotObj = new GameObject("ReticleDot");
        dotObj.transform.SetParent(transform);
        
        dotImage = dotObj.AddComponent<Image>();
        dotImage.color = dotColor;
        
        RectTransform dotRect = dotObj.GetComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0.5f, 0.5f);
        dotRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotRect.anchoredPosition = Vector2.zero;
        dotRect.sizeDelta = new Vector2(dotSize, dotSize);
        
        // Make it circular
        dotImage.sprite = CreateCircleSprite();
    }

    Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Bilinear;
        
        float center = resolution / 2f;
        float radius = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (dist < radius - 1)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (dist < radius)
                {
                    // Anti-aliased edge
                    float alpha = 1f - (dist - (radius - 1));
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    public void SetColor(Color color)
    {
        dotColor = color;
        if (dotImage != null)
        {
            dotImage.color = color;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (dotImage != null)
        {
            Color c = dotImage.color;
            c.a = alpha;
            dotImage.color = c;
        }
    }

    public void SetSize(float size)
    {
        dotSize = size;
        if (dotImage != null)
        {
            dotImage.rectTransform.sizeDelta = new Vector2(size, size);
        }
        if (outlineImage != null)
        {
            outlineImage.rectTransform.sizeDelta = new Vector2(size + outlineSize, size + outlineSize);
        }
    }
}

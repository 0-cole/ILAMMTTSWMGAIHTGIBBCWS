using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Upside-down audio spectrum visualizer bars at the top of the screen.
/// Creates bars as UI Images that hang down from the top.
/// 
/// Setup:
/// 1. Create an empty GameObject as child of your Canvas
/// 2. Add this script
/// 3. Add a HorizontalLayoutGroup (optional, or let script position them)
/// 4. Bars are created at runtime
/// </summary>
public class AudioVisualizer : MonoBehaviour
{
    [Header("Bar Settings")]
    [SerializeField] private int barCount = 48;
    [SerializeField] private float barWidth = 8f;
    [SerializeField] private float barSpacing = 2f;
    [SerializeField] private float maxBarHeight = 150f;
    [SerializeField] private float heightMultiplier = 5000f;
    [SerializeField] private float lerpSpeed = 15f;

    [Header("Appearance")]
    [SerializeField] private Color barColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Gradient barGradient;
    [SerializeField] private bool useGradient = false;

    private RectTransform[] bars;
    private Image[] barImages;
    private float[] barHeights;

    void Start()
    {
        CreateBars();
    }

    void CreateBars()
    {
        bars = new RectTransform[barCount];
        barImages = new Image[barCount];
        barHeights = new float[barCount];

        float totalWidth = barCount * (barWidth + barSpacing);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < barCount; i++)
        {
            GameObject barObj = new GameObject($"Bar_{i}");
            barObj.transform.SetParent(transform, false);

            RectTransform rect = barObj.AddComponent<RectTransform>();
            // Anchor to top-center
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f); // Pivot at top so bars grow downward

            float x = startX + i * (barWidth + barSpacing) + barWidth / 2f;
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(barWidth, 0f);

            Image img = barObj.AddComponent<Image>();
            if (useGradient && barGradient != null)
            {
                img.color = barGradient.Evaluate((float)i / barCount);
            }
            else
            {
                img.color = barColor;
            }

            bars[i] = rect;
            barImages[i] = img;
        }
    }

    void Update()
    {
        if (TitleScreenMusic.Instance == null) return;

        float[] spectrum = TitleScreenMusic.Instance.SpectrumData;
        if (spectrum == null || spectrum.Length == 0) return;

        // Map spectrum bins to bars (logarithmic distribution)
        for (int i = 0; i < barCount; i++)
        {
            // Logarithmic mapping — more bars for low frequencies
            float t = (float)i / barCount;
            int startBin = Mathf.FloorToInt(Mathf.Pow(t, 2f) * (spectrum.Length / 2));
            int endBin = Mathf.FloorToInt(Mathf.Pow((float)(i + 1) / barCount, 2f) * (spectrum.Length / 2));
            endBin = Mathf.Max(endBin, startBin + 1);

            float sum = 0f;
            for (int b = startBin; b < endBin && b < spectrum.Length; b++)
            {
                sum += spectrum[b];
            }
            float avg = sum / (endBin - startBin);

            float targetHeight = Mathf.Clamp(avg * heightMultiplier, 0f, maxBarHeight);
            barHeights[i] = Mathf.Lerp(barHeights[i], targetHeight, Time.deltaTime * lerpSpeed);

            if (bars[i] != null)
            {
                bars[i].sizeDelta = new Vector2(barWidth, barHeights[i]);
            }
        }
    }
}

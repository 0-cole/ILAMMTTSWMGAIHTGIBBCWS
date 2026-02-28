using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays a pre-made visualizer video wrapped into a circle using polar coordinates.
/// Place as a square RawImage in the top-right corner of your Canvas.
/// 
/// Setup:
/// 1. Create a square RawImage (e.g. 250x250) anchored top-right
/// 2. Add this script to it
/// 3. Assign the VideoClip in Inspector
/// 4. No need to flip Y — the shader handles the mapping
/// </summary>
[RequireComponent(typeof(RawImage))]
public class VideoVisualizer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoClip visualizerClip;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 4f;

    [Header("Circle")]
    [SerializeField] private float innerRadius = 0.15f;
    [SerializeField] private float outerRadius = 0.48f;

    private RawImage rawImage;
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private Material circularMat;
    private float fadeTimer;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent
        rawImage.raycastTarget = false;

        // Circular visualizer shader (polar warp + additive blend)
        var circShader = Shader.Find("UI/CircularVisualizer");
        if (circShader != null)
        {
            circularMat = new Material(circShader);
            circularMat.SetFloat("_InnerRadius", innerRadius);
            circularMat.SetFloat("_OuterRadius", outerRadius);
            rawImage.material = circularMat;
        }

        // Render above the IntroSplash canvas (sort order 999)
        var canvasOverride = gameObject.AddComponent<Canvas>();
        canvasOverride.overrideSorting = true;
        canvasOverride.sortingOrder = 1000;

        // Create render texture
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();

        // Setup video player — reuse existing or create new
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = gameObject.AddComponent<VideoPlayer>();

        videoPlayer.clip = visualizerClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = false;

        // Fully mute video audio — music is handled by TitleScreenMusic
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        for (ushort i = 0; i < 16; i++)
            videoPlayer.SetDirectAudioMute(i, true);

        videoPlayer.Play();
        rawImage.texture = renderTexture;
    }

    void Update()
    {
        // Fade in synced with music
        if (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
            rawImage.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (circularMat != null)
            Destroy(circularMat);
    }
}

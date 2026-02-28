using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays a pre-made visualizer video on a RawImage at the top of the screen.
/// Renders above the intro splash so it's visible during the intro sequence.
/// 
/// Setup:
/// 1. Create a RawImage in your Canvas, anchored to the top, stretched width
/// 2. Add this script to it
/// 3. Assign the VideoClip in Inspector (Assets/Video/TitleVisualizer.mp4)
/// 4. Flip Y scale to -1 on the RectTransform if your video isn't already upside down
/// </summary>
[RequireComponent(typeof(RawImage))]
public class VideoVisualizer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoClip visualizerClip;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 5.5f;

    private RawImage rawImage;
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private float fadeTimer;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent
        rawImage.raycastTarget = false; // Don't block button clicks

        // Use additive blending so video black = transparent (bars show through)
        var additiveShader = Shader.Find("UI/Additive");
        if (additiveShader != null)
            rawImage.material = new Material(additiveShader);

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
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// ULTRAKILL-style intro splash sequence.
/// Shows team logo → game title → then reveals main menu.
/// 
/// Setup:
/// 1. Create a Canvas (Screen Space - Overlay, sort order 999)
/// 2. Add this script to it
/// 3. Create child panels for each splash screen (black background + centered text/image)
/// 4. Assign references in Inspector
/// 5. Assign your mainMenuPanel so it gets enabled after the intro
/// </summary>
public class IntroSplash : MonoBehaviour
{
    [Header("Splash Screens (in order)")]
    [Tooltip("Each panel is shown in sequence, then hidden")]
    public GameObject[] splashPanels;

    [Header("Main Menu")]
    [Tooltip("The main menu panel to reveal after intro")]
    public GameObject mainMenuPanel;

    [Header("Timing")]
    [Tooltip("Total duration of the entire intro sequence (overrides individual timings)")]
    public float totalIntroDuration = 4.85f;
    public float fadeInDuration = 0.4f;
    public float fadeOutDuration = 0.3f;
    public float delayBetweenSplashes = 0.2f;

    [Header("Options")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    [Header("Flash on Reveal")]
    public float flashDuration = 0.8f;
    public Color flashColor = Color.white;

    /// <summary>
    /// Set this to true before loading the MainMenu scene to skip the intro.
    /// PauseManager.ReturnToMainMenu sets this automatically.
    /// </summary>
    public static bool SkipIntro = false;

    private bool skipped = false;

    void Start()
    {
        if (SkipIntro)
        {
            SkipIntro = false;
            skipped = true;

            // Hide all splash panels
            foreach (var panel in splashPanels)
            {
                if (panel != null) panel.SetActive(false);
            }

            // Show main menu immediately
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            Destroy(gameObject);
            return;
        }

        // Hide main menu until intro is done
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Hide all splash panels initially
        foreach (var panel in splashPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        StartCoroutine(PlayIntro());
    }

    void Update()
    {
        if (allowSkip && (Input.GetKeyDown(skipKey) || Input.GetMouseButtonDown(0)))
        {
            skipped = true;
        }
    }

    private IEnumerator PlayIntro()
    {
        // Calculate hold duration per panel to hit totalIntroDuration
        int panelCount = 0;
        foreach (var p in splashPanels) { if (p != null) panelCount++; }
        float fixedTimePerPanel = fadeInDuration + fadeOutDuration;
        float totalFixedTime = panelCount * fixedTimePerPanel + Mathf.Max(0, panelCount - 1) * delayBetweenSplashes;
        float holdDuration = Mathf.Max(0.1f, (totalIntroDuration - totalFixedTime) / Mathf.Max(1, panelCount));

        foreach (var panel in splashPanels)
        {
            if (panel == null) continue;
            if (skipped) break;

            panel.SetActive(true);

            // Fade text in using panel's own CanvasGroup
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            yield return StartCoroutine(Fade(cg, 0f, 1f, fadeInDuration));
            if (skipped) break;

            float held = 0f;
            while (held < holdDuration && !skipped)
            {
                held += Time.deltaTime;
                yield return null;
            }
            if (skipped) break;

            yield return StartCoroutine(Fade(cg, 1f, 0f, fadeOutDuration));

            panel.SetActive(false);

            if (!skipped)
                yield return new WaitForSeconds(delayBetweenSplashes);
        }

        // Show main menu
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // White flash on reveal — spawn it on the target canvas so it survives this object's destruction
        if (mainMenuPanel != null)
        {
            Canvas targetCanvas = mainMenuPanel.GetComponentInParent<Canvas>();
            if (targetCanvas != null)
            {
                GameObject flashObj = new GameObject("IntroFlash");
                flashObj.transform.SetParent(targetCanvas.transform, false);
                flashObj.transform.SetAsLastSibling();

                RectTransform flashRT = flashObj.AddComponent<RectTransform>();
                flashRT.anchorMin = Vector2.zero;
                flashRT.anchorMax = Vector2.one;
                flashRT.offsetMin = Vector2.zero;
                flashRT.offsetMax = Vector2.zero;

                Image flashImg = flashObj.AddComponent<Image>();
                flashImg.color = flashColor;
                flashImg.raycastTarget = false;

                // Use a self-destroying fader component
                IntroFlashFader fader = flashObj.AddComponent<IntroFlashFader>();
                fader.Init(flashColor, flashDuration);
            }
        }

        Destroy(gameObject);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        group.alpha = from;
        while (t < duration && !skipped)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}

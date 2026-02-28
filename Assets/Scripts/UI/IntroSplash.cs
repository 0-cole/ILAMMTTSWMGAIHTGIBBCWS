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
    public float fadeInDuration = 0.5f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.5f;
    public float delayBetweenSplashes = 0.3f;

    [Header("Options")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    private bool skipped = false;

    void Start()
    {
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

        // Show main menu and destroy the entire intro overlay immediately
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

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

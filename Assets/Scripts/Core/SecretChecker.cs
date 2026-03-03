using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SecretChecker : MonoBehaviour
{
    [Header("Tracked Pickups")]
    [SerializeField] private List<BoostPickup> trackedPickups = new List<BoostPickup>();

    [Header("Notification UI")]
    [SerializeField] private CanvasGroup notificationGroup;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private Image unlockIcon;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 3f;

    void Awake()
    {
        // Don't touch notificationGroup here — UnlockNotification manages its own alpha.
        // SecretChecker only enables/disables its own elements when a boost fires.
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0f;
            flashOverlay.color = c;
        }
    }

    void OnEnable()
    {
        BoostPickup.OnBoostCollected += HandleBoostCollected;
    }

    void OnDisable()
    {
        BoostPickup.OnBoostCollected -= HandleBoostCollected;
    }

    void HandleBoostCollected(string statName, float duration)
    {
        StartCoroutine(ShowNotification(statName, duration));
    }

    IEnumerator ShowNotification(string statName, float duration)
    {
        // Flash the screen
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0.8f;
            flashOverlay.color = c;
        }

        // Hide the old unlock icon so it doesn't bleed through
        if (unlockIcon != null) unlockIcon.gameObject.SetActive(false);

        // Show notification panel via alpha
        if (notificationGroup != null)
            notificationGroup.alpha = 1f;

        if (notificationText != null)
            notificationText.text = $"{statName.ToUpper()} BOOSTED FOR {Mathf.RoundToInt(duration)} SECONDS!";

        // Fade out the flash quickly
        float flashDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            if (flashOverlay != null)
            {
                Color c = flashOverlay.color;
                c.a = Mathf.Lerp(0.8f, 0f, elapsed / flashDuration);
                flashOverlay.color = c;
            }
            yield return null;
        }
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0f;
            flashOverlay.color = c;
        }

        // Hold the notification for a few seconds
        yield return new WaitForSeconds(Mathf.Max(displayDuration, 1f));

        // Just kill it — no fade, just hide via alpha
        if (notificationGroup != null)
            notificationGroup.alpha = 0f;
    }
}

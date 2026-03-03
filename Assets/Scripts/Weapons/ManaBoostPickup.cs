using UnityEngine;
using System.Collections;

public class ManaBoostPickup : MonoBehaviour
{
    [Header("Notification")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject modelPrefab;

    private bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        collected = true;

        var wc = other.GetComponentInChildren<WeaponController>();
        if (wc == null) wc = FindFirstObjectByType<WeaponController>();
        if (wc != null)
            wc.ActivateManaBoost();

        if (UnlockNotification.Instance != null)
            UnlockNotification.Instance.ShowUnlock("MANA BOOSTED!", icon, modelPrefab, "Regen Doubled for 20s!");

        // Hide visuals and collider, wait for notification to finish, then destroy
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        StartCoroutine(DestroyAfterNotification());
    }

    private IEnumerator DestroyAfterNotification()
    {
        // Wait until the notification finishes fading out
        while (UnlockNotification.Instance != null &&
               UnlockNotification.Instance.IsShowing)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}

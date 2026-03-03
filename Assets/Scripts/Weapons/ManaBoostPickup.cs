using UnityEngine;

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

        Destroy(transform.root.gameObject);
    }
}

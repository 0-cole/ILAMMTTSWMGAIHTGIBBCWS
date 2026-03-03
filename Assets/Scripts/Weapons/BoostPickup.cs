using UnityEngine;
using System;

public class BoostPickup : MonoBehaviour
{
    [Header("Boost Settings")]
    public string statName = "MANA";
    public float boostDuration = 20f;

    public static event Action<string, float> OnBoostCollected;

    private bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        collected = true;

        // Apply the boost effect based on stat type
        if (statName.Equals("MANA", StringComparison.OrdinalIgnoreCase))
        {
            var wc = other.GetComponentInChildren<WeaponController>();
            if (wc == null) wc = FindFirstObjectByType<WeaponController>();
            if (wc != null) wc.ActivateManaBoost();
        }

        // Notify any listeners (SecretChecker)
        OnBoostCollected?.Invoke(statName, boostDuration);
    }
}

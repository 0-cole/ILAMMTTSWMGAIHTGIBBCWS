using UnityEngine;

/// <summary>
/// Blocks the player from passing until lightning has been picked up.
/// Place on an empty with a collider (NOT a trigger) so it acts as a wall.
/// Self-destructs once the player has the Lightning weapon unlocked.
/// </summary>
public class LightningPickupDetector : MonoBehaviour
{
    void Update()
    {
        var wc = FindFirstObjectByType<WeaponController>();
        if (wc == null) return;

        foreach (var weapon in wc.weapons)
        {
            if (weapon.weaponTypeIndex == 1 && weapon.isUnlocked)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}

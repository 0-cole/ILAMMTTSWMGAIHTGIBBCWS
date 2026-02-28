/// <summary>
/// Common interface for all damageable entities (enemies, destructibles, etc.)
/// Implement this to make any entity targetable by weapons and homing fireballs.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
    float CurrentHealth { get; }
    bool IsDead { get; }
    UnityEngine.Transform transform { get; } // Already provided by MonoBehaviour
}

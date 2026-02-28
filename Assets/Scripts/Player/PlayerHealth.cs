using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.5f; // Invincibility frames
    private float lastDamageTime;

    // Events for UI updates
    public System.Action<float, float> OnHealthChanged;

    private CameraShake cameraShake;

    void Start()
    {
        currentHealth = maxHealth;

        // Auto-find CameraShake on main camera
        if (Camera.main != null)
            cameraShake = Camera.main.GetComponent<CameraShake>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        // Invincibility frames check
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Screen shake on damage
        if (cameraShake != null)
            cameraShake.Shake();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Public getter for UI
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}

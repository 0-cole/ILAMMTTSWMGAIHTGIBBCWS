using UnityEngine;

public class GlonkEnemy : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        // Optional: Pulse red or something to show damage
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Spawning a death effect if assigned
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Goodbye, Glonk
        Destroy(gameObject);
    }
}

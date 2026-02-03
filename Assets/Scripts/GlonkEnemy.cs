using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GlonkEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("AI Settings")]
    [SerializeField] private float damageOnContact = 10f;
    [SerializeField] private float updatePathInterval = 0.2f;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;

    [Header("Pickups")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject manaPickupPrefab;
    [Range(0f, 1f)] [SerializeField] private float healthDropChance = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float manaDropChance = 0.5f;
    [SerializeField] private float pickupSpawnForce = 3f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextPathUpdate;

    public System.Action<float, float> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            Debug.Log($"[Glonk] Found Player: {playerObj.name}");
        }
        else
        {
            Debug.LogError("[Glonk] Could NOT find object with tag 'Player'! Please tag your player object.");
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[Glonk] Agent is NOT on the NavMesh! Is it baked? Is the floor blue?");
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (playerTransform != null && Time.time >= nextPathUpdate)
        {
            if (agent.isOnNavMesh)
            {
                nextPathUpdate = Time.time + updatePathInterval;
                agent.SetDestination(playerTransform.position);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.velocity = Vector3.zero;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Drop pickups (DOOM-style)
        SpawnPickup(healthPickupPrefab, healthDropChance);
        SpawnPickup(manaPickupPrefab, manaDropChance);

        Destroy(gameObject);
    }

    void SpawnPickup(GameObject pickupPrefab, float dropChance)
    {
        if (pickupPrefab == null) return;
        if (Random.value > dropChance) return; // Random chance

        GameObject pickup = Instantiate(pickupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        
        // Add slight upward force for visual pop
        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDirection * pickupSpawnForce, ForceMode.Impulse);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnContact);
            }
        }
    }
}

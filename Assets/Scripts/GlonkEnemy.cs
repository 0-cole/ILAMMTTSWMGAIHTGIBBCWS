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
    private PlayerHealth cachedPlayerHealth; // Cache to avoid GetComponent every attack
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
            cachedPlayerHealth = playerObj.GetComponent<PlayerHealth>(); // Cache the component
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

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stunDuration = 1.0f; // How long to freeze after attacking
    private float nextAttackTime;
    private float stunEndTime; // When the stun wears off

    void Update()
    {
        if (playerTransform != null)
        {
            // Check if stunned (freeze after attack)
            if (Time.time < stunEndTime)
            {
                // Stop the agent while stunned
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                return; // Skip all movement/attack logic
            }
            else
            {
                // Resume movement if stun ended
                if (agent.isOnNavMesh && agent.isStopped)
                {
                    agent.isStopped = false;
                }
            }

            // AI Movement
            if (Time.time >= nextPathUpdate)
            {
                if (agent.isOnNavMesh)
                {
                    nextPathUpdate = Time.time + updatePathInterval;
                    agent.SetDestination(playerTransform.position);
                }
            }

            // Attack Logic (Distance Check because Collision is unreliable with NavMesh)
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                if (cachedPlayerHealth != null)
                {
                    cachedPlayerHealth.TakeDamage(damageOnContact);
                    nextAttackTime = Time.time + 1.0f; // 1 second cooldown between hits
                    stunEndTime = Time.time + stunDuration; // Stun after attacking!
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"[Glonk {gameObject.name}] TakeDamage called: {amount} damage. Current HP: {currentHealth}");
        
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

    private bool isDead = false;

    void Die()
    {
        if (isDead) return;
        isDead = true;

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

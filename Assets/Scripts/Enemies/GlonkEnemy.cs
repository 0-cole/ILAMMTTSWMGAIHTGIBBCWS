using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GlonkEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    
    [Header("AI Settings")]
    [SerializeField] private float damageOnContact = 10f;
    [SerializeField] private float updatePathInterval = 0.2f;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip deathGrunt;

    [Header("Pickups")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject manaPickupPrefab;
    [Range(0f, 1f)] [SerializeField] private float healthDropChance = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float manaDropChance = 0.5f;
    [SerializeField] private float pickupSpawnForce = 3f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerHealth cachedPlayerHealth;
    private float nextPathUpdate;
    private bool canSeePlayer;

    // --- NavMesh Fallback ---
    // If no NavMesh is baked on the current map, the enemy enters fallback mode:
    // stands still, faces the player, and can still attack at melee range.
    private bool navMeshAvailable = true;

    [Header("Line of Sight")]
    [SerializeField] private float losCheckInterval = 0.15f;
    [SerializeField] private LayerMask losBlockingLayers; // Assign to walls/environment only
    private float nextLosCheck;

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

        // --- NavMesh Availability Check ---
        // Use SamplePosition to verify there's actually a baked NavMesh nearby
        NavMeshHit navHit;
        if (!NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            // No NavMesh found within 5 units — this map isn't baked
            navMeshAvailable = false;
            agent.enabled = false;
            Debug.LogWarning($"[Glonk] No NavMesh found near {gameObject.name}! Entering fallback mode (stand & face). " +
                             "Bake the NavMesh for this scene: Window → AI → Navigation → Bake.");
        }
        else if (!agent.isOnNavMesh)
        {
            // NavMesh exists but agent isn't on it — try warping to nearest valid point
            if (agent.Warp(navHit.position))
            {
                Debug.LogWarning($"[Glonk] Agent wasn't on NavMesh, warped to nearest valid point.");
            }
            else
            {
                navMeshAvailable = false;
                agent.enabled = false;
                Debug.LogWarning($"[Glonk] Agent could not warp to NavMesh. Entering fallback mode.");
            }
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stunDuration = 1.0f; // How long to freeze after attacking
    private float nextAttackTime;
    private float stunEndTime; // When the stun wears off

    void Update()
    {
        if (playerTransform == null) return;

        // Periodic LOS check
        if (Time.time >= nextLosCheck)
        {
            nextLosCheck = Time.time + losCheckInterval;
            canSeePlayer = CheckLineOfSight();
        }

        // ---- FALLBACK MODE (no NavMesh) ----
        if (!navMeshAvailable)
        {
            FallbackUpdate();
            return;
        }

        // ---- NORMAL MODE (NavMesh available) ----

        // Check if stunned (freeze after attack)
        if (Time.time < stunEndTime)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            return;
        }
        else
        {
            if (agent.isOnNavMesh && agent.isStopped) agent.isStopped = false;
        }

        // Only chase & attack if we can SEE the player
        if (canSeePlayer)
        {
            // AI Movement
            if (Time.time >= nextPathUpdate)
            {
                if (agent.isOnNavMesh)
                {
                    nextPathUpdate = Time.time + updatePathInterval;
                    agent.SetDestination(playerTransform.position);
                }
            }

            // Attack Logic
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                if (cachedPlayerHealth != null)
                {
                    cachedPlayerHealth.TakeDamage(damageOnContact);
                    nextAttackTime = Time.time + 1.0f;
                    stunEndTime = Time.time + stunDuration;
                }
            }
        }
        else
        {
            // Lost sight - stop moving
            if (agent.isOnNavMesh) agent.isStopped = true;
        }
    }

    /// <summary>
    /// Fallback behavior when no NavMesh is available:
    /// Face the player and attack if they get close enough.
    /// </summary>
    void FallbackUpdate()
    {
        // Check if stunned
        if (Time.time < stunEndTime) return;

        if (canSeePlayer)
        {
            // Face the player (Y-axis rotation only)
            Vector3 lookDir = playerTransform.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 5f
                );
            }

            // Attack if close enough
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                if (cachedPlayerHealth != null)
                {
                    cachedPlayerHealth.TakeDamage(damageOnContact);
                    nextAttackTime = Time.time + 1.0f;
                    stunEndTime = Time.time + stunDuration;
                }
            }
        }
    }

    bool CheckLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = playerTransform.position + Vector3.up * 0.5f;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, losBlockingLayers))
        {
            // Something is blocking the view
            return false;
        }
        // Nothing blocked it - we can see the player
        return true;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"[Glonk {gameObject.name}] TakeDamage called: {amount} damage. Current HP: {currentHealth}");
        
        currentHealth -= amount;
        
        if (navMeshAvailable && agent.enabled && agent.isOnNavMesh)
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
}

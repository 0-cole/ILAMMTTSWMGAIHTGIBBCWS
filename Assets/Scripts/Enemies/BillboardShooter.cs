using UnityEngine;

public class BillboardShooter : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    [Header("Shooting")]
    [SerializeField] private GameObject fireballPrefab; // Reuse WickedFireball prefab
    [SerializeField] private float fireRate = 2f; // Seconds between shots
    [SerializeField] private float fireballSpeed = 10f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float spawnOffset = 1.2f; // How far in front to spawn the fireball

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip deathGrunt;
    [SerializeField] private AudioClip attackGrunt;
    [SerializeField] private AudioClip spawnSound;

    [Header("Pickups")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject manaPickupPrefab;
    [Range(0f, 1f)] [SerializeField] private float healthDropChance = 0.4f;
    [Range(0f, 1f)] [SerializeField] private float manaDropChance = 0.6f;
    [SerializeField] private float pickupSpawnForce = 3f;

    private Transform playerTransform;
    private float nextFireTime;
    private bool isDead;
    private Camera mainCamera;

    public System.Action<float, float> OnHealthChanged;

    /// <summary>
    /// Public initializer so spawners can set dependencies without reflection.
    /// </summary>
    public void Initialize(GameObject fireball, GameObject healthPickup, GameObject manaPickup)
    {
        fireballPrefab = fireball;
        healthPickupPrefab = healthPickup;
        manaPickupPrefab = manaPickup;
    }

    void Start()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;

        if (spawnSound != null) AudioSource.PlayClipAtPoint(spawnSound, transform.position);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void LateUpdate()
    {
        if (isDead) return;

        // Billboard: Always face the camera
        if (mainCamera != null)
        {
            transform.LookAt(
                transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }

        // Shooting logic
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange && Time.time >= nextFireTime)
        {
            ShootAtPlayer();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootAtPlayer()
    {
        if (fireballPrefab == null) return;

        if (attackGrunt != null) AudioSource.PlayClipAtPoint(attackGrunt, transform.position);

        // Aim at the player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        
        // Aim slightly upward toward player center mass
        Vector3 targetPoint = playerTransform.position + Vector3.up * 0.5f;
        direction = (targetPoint - transform.position).normalized;

        Vector3 spawnPosition = transform.position + direction * spawnOffset;

        GameObject fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.LookRotation(direction));

        // If using WickedFireball, it handles its own movement.
        // If using a simple Rigidbody projectile instead:
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * fireballSpeed;
        }

        // Ignore collision between this enemy and the fireball
        Collider shooterCollider = GetComponent<Collider>();
        Collider fireballCollider = fireball.GetComponent<Collider>();
        if (shooterCollider != null && fireballCollider != null)
        {
            Physics.IgnoreCollision(shooterCollider, fireballCollider);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathGrunt != null) AudioSource.PlayClipAtPoint(deathGrunt, transform.position);

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Drop pickups
        SpawnPickup(healthPickupPrefab, healthDropChance);
        SpawnPickup(manaPickupPrefab, manaDropChance);

        Destroy(gameObject);
    }

    void SpawnPickup(GameObject pickupPrefab, float dropChance)
    {
        if (pickupPrefab == null) return;
        if (Random.value > dropChance) return;

        GameObject pickup = Instantiate(pickupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDirection * pickupSpawnForce, ForceMode.Impulse);
        }
    }
}

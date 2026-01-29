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

        Destroy(gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Debug.Log("Glonk hit player!");
        }
    }
}

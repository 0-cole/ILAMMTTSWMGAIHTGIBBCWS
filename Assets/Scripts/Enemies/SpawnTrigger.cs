using UnityEngine;
using System.Collections;

/// <summary>
/// Place this on a trigger collider. When the player enters for the first time,
/// it spawns all EnemySpawnPoints with a matching spawnGroupId, then destroys itself.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SpawnTrigger : MonoBehaviour
{
    [Header("Spawn Group")]
    [Tooltip("Matches EnemySpawnPoint.spawnGroupId")]
    public int spawnGroupId = 0;

    [Header("Timing")]
    [Tooltip("Delay between each enemy spawn in the group")]
    public float delayBetweenSpawns = 0.3f;

    [Header("Visual")]
    [Tooltip("Optional: destroy after this delay to let particles finish")]
    public float selfDestroyDelay = 0.5f;

    private bool triggered = false;

    void Start()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        // Find all spawn points in the scene with matching group id
        EnemySpawnPoint[] allPoints = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);

        int spawnCount = 0;
        foreach (var point in allPoints)
        {
            if (point.spawnGroupId == spawnGroupId && !point.hasSpawned)
            {
                point.SpawnEnemy();
                spawnCount++;
                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        Debug.Log($"[SpawnTrigger] Group {spawnGroupId}: Spawned {spawnCount} enemies.");

        // Wait a moment then destroy the trigger
        yield return new WaitForSeconds(selfDestroyDelay);
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"Spawn Trigger (Group {spawnGroupId})");
#endif
    }
}

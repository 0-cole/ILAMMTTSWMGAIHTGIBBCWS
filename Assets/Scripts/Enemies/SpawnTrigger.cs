using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Place this on a trigger collider. When the player enters for the first time,
/// it spawns enemies wave by wave. Each wave waits for all enemies to die before the next spawns.
/// Optionally plays an ULTRAKILL-style encounter intro before spawning.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SpawnTrigger : MonoBehaviour
{
    [Header("Spawn Group")]
    [Tooltip("Matches EnemySpawnPoint.spawnGroupId")]
    public float spawnGroupId = 0;

    [Header("Timing")]
    [Tooltip("Delay between each enemy spawn in a wave")]
    public float delayBetweenSpawns = 0.3f;
    [Tooltip("Delay after a wave is cleared before the next wave spawns")]
    public float delayBetweenWaves = 1.0f;

    [Header("Visual")]
    [Tooltip("Optional: destroy after this delay to let particles finish")]
    public float selfDestroyDelay = 0.5f;

    [Header("Encounter Intro")]
    [Tooltip("Optional: assign an EncounterIntro to play a cinematic text intro before spawning")]
    public EncounterIntro encounterIntro;

    private bool triggered = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Mark all other SpawnTriggers with the same group ID as triggered so they don't fire
        foreach (var st in FindObjectsByType<SpawnTrigger>(FindObjectsSortMode.None))
        {
            if (st != this && Mathf.Approximately(st.spawnGroupId, spawnGroupId))
            {
                st.triggered = true;
                Destroy(st.gameObject);
            }
        }

        if (encounterIntro != null)
        {
            AudioSource ambience = LevelMusicManager.Instance != null ? LevelMusicManager.Instance.AmbienceSource : null;
            AudioSource combat = LevelMusicManager.Instance != null ? LevelMusicManager.Instance.CombatSource : null;

            encounterIntro.PlayIntro(ambience, combat, () =>
            {
                StartCoroutine(WaveSequence());
            });
        }
        else
        {
            StartCoroutine(WaveSequence());
        }
    }

    private IEnumerator WaveSequence()
    {
        EnemySpawnPoint[] allPoints = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);

        // Get all matching points grouped by wave
        var waveGroups = allPoints
            .Where(p => Mathf.Approximately(p.spawnGroupId, spawnGroupId) && !p.hasSpawned)
            .GroupBy(p => p.waveNumber)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var wave in waveGroups)
        {
            List<GameObject> waveEnemies = new List<GameObject>();

            // Spawn all enemies in this wave
            foreach (var point in wave)
            {
                GameObject enemy = point.SpawnEnemy();
                if (enemy != null)
                    waveEnemies.Add(enemy);
                yield return new WaitForSeconds(delayBetweenSpawns);
            }

            Debug.Log($"[SpawnTrigger] Group {spawnGroupId}, Wave {wave.Key}: Spawned {waveEnemies.Count} enemies.");

            // Wait for all enemies in this wave to die
            while (waveEnemies.Any(e => e != null))
            {
                yield return new WaitForSeconds(0.25f);
            }

            Debug.Log($"[SpawnTrigger] Group {spawnGroupId}, Wave {wave.Key}: Cleared!");

            // Brief pause before next wave
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        Debug.Log($"[SpawnTrigger] Group {spawnGroupId}: All waves complete!");

        yield return new WaitForSeconds(selfDestroyDelay);
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = encounterIntro != null ? new Color(1f, 0.5f, 0f, 0.3f) : new Color(0f, 1f, 0f, 0.3f);
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
        string label = encounterIntro != null
            ? $"Spawn Trigger (Group {spawnGroupId}) [ENCOUNTER]"
            : $"Spawn Trigger (Group {spawnGroupId})";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
#endif
    }
}

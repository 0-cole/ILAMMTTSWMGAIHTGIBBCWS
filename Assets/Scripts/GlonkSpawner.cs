using UnityEngine;
using System.Collections;

public class GlonkSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject glonkPrefab;
    public float respawnDelay = 3.0f;
    public bool spawnOnStart = true;

    private GameObject currentGlonk;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnGlonk();
        }
    }

    void Update()
    {
        // Standard Respawn Logic
        if (currentGlonk == null && !IsInvoking("SpawnGlonk"))
        {
            Invoke("SpawnGlonk", respawnDelay);
        }

        // DEBUG: Summon the Horde (Press T)
        if (Input.GetKeyDown(KeyCode.T))
        {
            SummonHorde(30);
        }
    }

    void SpawnGlonk()
    {
        if (glonkPrefab != null)
        {
            currentGlonk = Instantiate(glonkPrefab, transform.position, transform.rotation);
        }
    }

    void SummonHorde(int count)
    {
        if (glonkPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            // Random offset so they don't fuse immediately (though physics explosions are funny)
            Vector3 randomOffset = Random.insideUnitSphere * 5f;
            randomOffset.y = 0; // Keep them on the same level roughly

            Instantiate(glonkPrefab, transform.position + randomOffset, Quaternion.identity);
        }

        Debug.Log($"<color=red>THE HORDE HAS ARRIVED ({count} Glonks)</color>");
    }
}

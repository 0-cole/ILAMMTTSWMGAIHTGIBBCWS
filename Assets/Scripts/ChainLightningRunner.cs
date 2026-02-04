using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainLightningRunner : MonoBehaviour
{
    private float damage;
    private int maxBounces;
    private float range;
    private float delay;
    private GameObject effectPrefab;

    public void Initialize(Vector3 startPos, GameObject initialTarget, float dmg, int bounces, float bounceRange, float bounceDelay, GameObject prefab)
    {
        damage = dmg;
        maxBounces = bounces;
        range = bounceRange;
        delay = bounceDelay;
        effectPrefab = prefab;

        StartCoroutine(RunChain(startPos, initialTarget));
    }

    private IEnumerator RunChain(Vector3 startPos, GameObject initialTarget)
    {
        HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
        Vector3 currentPosition = startPos;
        GameObject currentTarget = initialTarget;

        // Visual for the first segment (Start -> Target)
        // Note: The controller handles the initial finding and raycast visuals if missed.
        // But if we are here, we have a target.
        if (currentTarget != null)
        {
             SpawnLightningSegment(currentPosition, currentTarget.transform.position);
        }
        else
        {
            // Should not happen based on Controller logic, but safety first
            Destroy(gameObject);
            yield break;
        }

        for (int i = 0; i < maxBounces; i++)
        {
            if (currentTarget == null)
            {
                // Target died during the previous wait? or just null passed?
                // If it was valid at start of loop but null now, we should have cached position.
                // But inside the loop, we check at the top.
                // Let's handle the "Death during wait" logic after the yield.
                break; 
            }

            // Register Hit & Damage
            hitEnemies.Add(currentTarget);
            GlonkEnemy enemyScript = currentTarget.GetComponentInParent<GlonkEnemy>();
            
            // Allow damage to happen even if it kills them immediately
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
                SpawnSmite(currentTarget.transform.position);
            }

            // Cache position BEFORE waiting, in case they die
            Vector3 lastKnownPos = currentTarget.transform.position;

            // Wait
            yield return new WaitForSeconds(delay);

            // Update Start Position for next search
            // If target is still alive, update position. If dead, use last known.
            if (currentTarget != null) 
            {
                currentPosition = currentTarget.transform.position;
            }
            else 
            {
                currentPosition = lastKnownPos;
            }

            // Find Next Target
            GameObject nextTarget = null;
            Collider[] potentialTargets = Physics.OverlapSphere(currentPosition, range);
            float closestNextDist = Mathf.Infinity;

            foreach (var col in potentialTargets)
            {
                GlonkEnemy enemy = col.GetComponentInParent<GlonkEnemy>();
                if (enemy != null && !hitEnemies.Contains(enemy.gameObject))
                {
                    float d = Vector3.Distance(currentPosition, enemy.transform.position);
                    if (d < closestNextDist)
                    {
                        closestNextDist = d;
                        nextTarget = enemy.gameObject;
                    }
                }
            }

            if (nextTarget != null)
            {
                // Visual for the Arc
                SpawnLightningSegment(currentPosition, nextTarget.transform.position);
                currentTarget = nextTarget;
            }
            else
            {
                break; // No more targets
            }
        }

        // Cleanup self after chain finishes
        Destroy(gameObject);
    }

    void SpawnLightningSegment(Vector3 start, Vector3 end)
    {
        if (effectPrefab != null)
        {
            GameObject effectObj = Instantiate(effectPrefab, Vector3.zero, Quaternion.identity);
            LightningEffect effect = effectObj.GetComponent<LightningEffect>();
            if (effect != null)
            {
                List<Vector3> points = new List<Vector3>();
                points.Add(start);
                points.Add(end);
                effect.Setup(points);
            }
        }
    }

    void SpawnSmite(Vector3 targetPos)
    {
        if (effectPrefab != null)
        {
            GameObject effectObj = Instantiate(effectPrefab, Vector3.zero, Quaternion.identity);
            LightningEffect effect = effectObj.GetComponent<LightningEffect>();
            LineRenderer lr = effectObj.GetComponent<LineRenderer>();
            
            if (lr != null)
            {
                lr.widthMultiplier = 2.0f;
            }

            if (effect != null)
            {
                List<Vector3> points = new List<Vector3>();
                points.Add(targetPos + Vector3.up * 20f);
                points.Add(targetPos);
                effect.Setup(points);
            }
        }
    }
}

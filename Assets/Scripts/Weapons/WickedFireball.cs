using UnityEngine;
using System.Collections;

public class WickedFireball : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float enemyDamage = 25f;
    [SerializeField] private float lifetime = 5f;

    [Header("Boost Mechanics")]
    [SerializeField] private float slowStartDuration = 0.25f;
    [SerializeField] private float slowStartSpeed = 2f;

    [Header("Explosion Settings")]
    [SerializeField] private bool createExplosionEffect = true;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private int particleCount = 30;

    [Header("Homing (Player Fireballs Only)")]
    [SerializeField] private float homingDetectionRadius = 30f;
    [SerializeField] private float homingTurnSpeed = 120f; // degrees per second
    [SerializeField] private float homingActivationDelay = 0.3f;
    [SerializeField] private float feelerDistance = 8f; // how far wall-avoidance rays probe

    private Rigidbody rb;
    private float spawnTime;
    
    // Static texture cache
    private static Texture2D cachedParticleTexture;
    private Transform playerTransform;
    private float punchRangeThreshold = 3f;
    private Vector3 launchDirection;
    private bool isPlayerOwned = false;
    private bool homingActive = false;
    private Transform homingTarget;

    public void Initialize(Transform player, float range)
    {
        playerTransform = player;
        punchRangeThreshold = range * 1.2f; // Slight buffer
        isPlayerOwned = true; // Flag this fireball as belonging to the player
    }

    IEnumerator Start()
    {
        spawnTime = Time.time;
        
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Cache Launch Direction (World Space) to prevent physics tumbling
        launchDirection = transform.forward;

        // Slow Start
        rb.linearVelocity = launchDirection * slowStartSpeed;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Smart Acceleration Loop
        float timer = 0f;
        while (timer < slowStartDuration)
        {
            timer += Time.deltaTime;

            if (isBoosted) break; // Already boosted, exit loop

            if (playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist > punchRangeThreshold)
                {
                    // Out of range, accelerate immediately
                    break;
                }
            }
            yield return null;
        }

        // Accelerate if not boosted
        if (!isBoosted)
        {
            // Use cached direction instead of transform.forward
            rb.linearVelocity = launchDirection * speed;
        }

        // Activate homing after slow-start phase (player fireballs only)
        if (isPlayerOwned && !isBoosted)
        {
            yield return new WaitForSeconds(homingActivationDelay);
            homingActive = true;
        }
        
        // Safety destroy in case it wasn't destroyed earlier
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (!homingActive || !isPlayerOwned || isBoosted || rb == null) return;

        // Find or validate homing target
        if (homingTarget == null || !homingTarget.gameObject.activeInHierarchy)
        {
            homingTarget = FindNearestEnemy();
        }

        if (homingTarget == null) return;

        // Determine steering direction
        Vector3 toTarget = (homingTarget.position - transform.position);
        float distToTarget = toTarget.magnitude;
        Vector3 dirToTarget = toTarget.normalized;

        // Line-of-sight check
        Vector3 steerDirection;
        RaycastHit losHit;
        bool hasLineOfSight = !Physics.Raycast(transform.position, dirToTarget, out losHit, distToTarget,
            ~0, QueryTriggerInteraction.Ignore) || IsEnemy(losHit.collider);

        if (hasLineOfSight)
        {
            // Clear path — steer directly toward target
            steerDirection = dirToTarget;
        }
        else
        {
            // Wall in the way — use feeler rays to find a path around
            steerDirection = FindWallAvoidanceDirection(dirToTarget, distToTarget);
        }

        // Apply gentle steering via RotateTowards
        Vector3 currentDir = rb.linearVelocity.normalized;
        float currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < 0.1f) currentSpeed = speed;

        float maxRadians = homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
        Vector3 newDir = Vector3.RotateTowards(currentDir, steerDirection, maxRadians, 0f);

        rb.linearVelocity = newDir * currentSpeed;
        transform.forward = newDir;
    }

    Transform FindNearestEnemy()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, homingDetectionRadius);
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var col in nearby)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                float d = Vector3.Distance(transform.position, damageable.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = damageable.transform;
                }
            }
        }

        return closest;
    }

    bool IsEnemy(Collider col)
    {
        return col.GetComponentInParent<IDamageable>() != null;
    }

    Vector3 FindWallAvoidanceDirection(Vector3 dirToTarget, float distToTarget)
    {
        // Cast feeler rays in 6 directions perpendicular to current velocity
        // to find the best open path that gets us closer to the target
        Vector3 currentDir = rb.linearVelocity.normalized;
        if (currentDir.sqrMagnitude < 0.01f) currentDir = transform.forward;

        // Build a local basis around the current flight direction
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, currentDir).normalized;
        if (right.sqrMagnitude < 0.01f)
        {
            right = Vector3.Cross(Vector3.forward, currentDir).normalized;
        }
        up = Vector3.Cross(currentDir, right).normalized;

        // 6 feeler directions: up, down, left, right, and two diagonals
        Vector3[] feelerOffsets = new Vector3[]
        {
            up,
            -up,
            right,
            -right,
            (up + right).normalized,
            (-up + right).normalized,
        };

        Vector3 bestDirection = currentDir; // fallback: keep going straight
        float bestScore = -Mathf.Infinity;

        foreach (var offset in feelerOffsets)
        {
            // Feeler direction: blend between current forward and the offset
            Vector3 feelerDir = (currentDir + offset * 0.8f).normalized;

            // Check if this feeler direction is blocked
            RaycastHit feelerHit;
            bool feelerBlocked = Physics.Raycast(transform.position, feelerDir, out feelerHit,
                feelerDistance, ~0, QueryTriggerInteraction.Ignore) && !IsEnemy(feelerHit.collider);

            if (feelerBlocked) continue; // This path is also walled off

            // Score: how much does this direction point toward the target?
            float dotToTarget = Vector3.Dot(feelerDir, dirToTarget);
            
            if (dotToTarget > bestScore)
            {
                bestScore = dotToTarget;
                bestDirection = feelerDir;
            }
        }

        return bestDirection;
    }

    void OnTriggerEnter(Collider other)
    {
        // Skip other trigger colliders (e.g. SpawnTriggers, pickup zones)
        if (other.isTrigger) return;

        // Only skip Player collision if this is a player-owned fireball (prevent self-damage)
        // Enemy fireballs must proceed to HandleImpact to damage the player
        if (isPlayerOwned)
        {
            if (other.CompareTag("Player") || other.transform.root.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
                return;
        }

        // Prevent self-collision with the fireball's own other colliders (if any)
        if (other.gameObject == gameObject) return;
            
        HandleImpact(other.gameObject);
    }

    void HandleImpact(GameObject hitObject)
    {
        Debug.Log($"[Fireball] Impact on {hitObject.name} at {transform.position}");
        
        // AOE Damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        Debug.Log($"[Fireball] Found {hitColliders.Length} colliders in explosion radius");
        
        foreach (var hitCollider in hitColliders)
        {
            // First check if we hit the player
            PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Only damage the player if this fireball was shot by an enemy
                if (!isPlayerOwned)
                {
                    Debug.Log($"[Fireball] Damaging Player");
                    playerHealth.TakeDamage(enemyDamage);
                }
                continue;
            }

            // Damage any IDamageable enemy
            IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"[Fireball] Damaging {damageable.transform.name}");
                damageable.TakeDamage(damage);
            }
        }

        if (createExplosionEffect)
        {
            CreateExplosion();
        }

        DetachParticleSystems();

        Destroy(gameObject);
    }

    void CreateExplosion()
    {
        GameObject explosionObj = new GameObject("FireballExplosion");
        explosionObj.transform.position = transform.position;

        ParticleSystem ps = explosionObj.AddComponent<ParticleSystem>();
        
        ps.Stop();

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        
        // Restored logic
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.6f, 0f, 1f),
            new Color(1f, 0.2f, 0f, 1f)
        );
        
        main.gravityModifier = 0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = particleCount;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, particleCount)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = explosionRadius * 0.2f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0.4f),
                new GradientColorKey(Color.black, 0.7f),
                new GradientColorKey(Color.black, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var renderer = explosionObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.mainTexture = GetSoftCircleTexture();

        renderer.sortMode = ParticleSystemSortMode.Distance;

        ps.Play();
        Destroy(explosionObj, 2f);
    }

    private bool isBoosted = false;

    public void Boost(Vector3 newDirection)
    {
        if (isBoosted) return;
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;
        isBoosted = true;

        // Multiply Stats
        damage *= 3f;
        explosionRadius *= 2.5f;
        speed *= 2.5f;

        // Redirect
        transform.forward = newDirection;
        rb.linearVelocity = transform.forward * speed;

        // Visuals (Change color to Blue/Cyan)
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.cyan;

        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.startColor = Color.cyan;
            trail.endColor = Color.white;
            trail.widthMultiplier *= 2f;
        }

        Debug.Log("PROJETILE BOOST!");
    }

    Texture2D GetSoftCircleTexture()
    {
        if (cachedParticleTexture != null) return cachedParticleTexture;

        int res = 64;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] colors = new Color[res * res];
        Vector2 center = new Vector2(res * 0.5f, res * 0.5f);
        float maxRadius = res * 0.5f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (dist / maxRadius));
                alpha = Mathf.Pow(alpha, 2);
                // White base ensures tint color works perfectly
                colors[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        
        cachedParticleTexture = tex;
        return tex;
    }

    void DetachParticleSystems()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            ps.transform.SetParent(null);
            Destroy(ps.gameObject, main.startLifetime.constantMax + 0.5f);
        }
    }
}

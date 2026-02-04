using UnityEngine;

public class WickedFireball : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float lifetime = 5f;

    [Header("Explosion Settings")]
    [SerializeField] private bool createExplosionEffect = true;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private int particleCount = 30;

    private Rigidbody rb;
    private float spawnTime;
    
    // Static texture cache
    private static Texture2D cachedParticleTexture;

    void Start()
    {
        spawnTime = Time.time;
        
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        rb.linearVelocity = transform.forward * speed;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check for Player collision (friendly fire)
        // Check both the object and its root to catch child colliders
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
            return;

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
            // Search parent hierarchy - Glonk component might be on parent object
            GlonkEnemy glonk = hitCollider.GetComponentInParent<GlonkEnemy>();
            if (glonk != null)
            {
                Debug.Log($"[Fireball] Damaging Glonk: {glonk.gameObject.name}");
                glonk.TakeDamage(damage);
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
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f); // Slightly larger
        
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

        // RENDERER FIX: Back to "Particles/Standard Unlit" which we know works in 3D space
        // But explicitly assign our texture to fix the black squares
        var renderer = explosionObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.mainTexture = GetSoftCircleTexture();

        // Ensure particles are sorted correctly
        renderer.sortMode = ParticleSystemSortMode.Distance;

        ps.Play();
        Destroy(explosionObj, 2f);
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

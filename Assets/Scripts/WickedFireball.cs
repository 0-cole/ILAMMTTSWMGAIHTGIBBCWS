using UnityEngine;

public class WickedFireball : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 30f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float lifetime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject impactEffect;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Ensure we have a rigidbody for physical movement/collision events
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false; // Fireballs float... usually
        rb.isKinematic = true; // We'll move it manually or via velocity, straightforward works best for simple projectiles
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Simple forward movement
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Hit a Glonk?
        GlonkEnemy glonk = other.GetComponent<GlonkEnemy>();
        if (glonk != null)
        {
            glonk.TakeDamage(damage);
        }

        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Destroy fireball on any impact (except maybe the player if we add that check later)
        // For now, assuming layer matrix handles player collision ignoring
        Destroy(gameObject);
    }
}

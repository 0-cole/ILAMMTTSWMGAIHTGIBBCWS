using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupType { Health, Mana }

    [Header("Settings")]
    public PickupType pickupType = PickupType.Health;
    public float amount = 20f;

    [Header("Magnet Effect")]
    public float magnetRange = 5f;
    public float magnetSpeed = 8f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    private Transform playerTransform;
    private Vector3 startPosition;
    private float bobOffset;

    // Static cache - all pickups share the same player reference
    private static Transform cachedPlayerTransform;

    void Start()
    {
        // Use cached player reference if available
        if (cachedPlayerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                cachedPlayerTransform = player.transform;
            }
        }
        
        playerTransform = cachedPlayerTransform;

        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f); // Random bobbing phase
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Magnet effect when close
        if (distanceToPlayer < magnetRange)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                playerTransform.position,
                magnetSpeed * Time.deltaTime
            );
        }
        else
        {
            // Gentle bobbing when idle
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Rotate for visibility
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (pickupType == PickupType.Health)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(amount);
            }
        }
        else if (pickupType == PickupType.Mana)
        {
            WeaponController weaponController = other.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.GainMana(amount);
            }
        }

        Destroy(gameObject);
    }
}

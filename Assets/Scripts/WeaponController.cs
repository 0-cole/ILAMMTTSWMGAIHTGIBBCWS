using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform firePoint;     // Assign a child of camera or hand position
    public GameObject fireballPrefab;

    [Header("Stats")]
    public float fireRate = 0.5f;
    public float maxMana = 100f;
    public float currentMana;
    public float manaCost = 5f; // Cost per shot (Lowered for "healing/ammo" feel)
    public float manaRegen = 1f; // Slow regen to encourage pickups
    
    [Header("Lightning Stats")]
    public float lightningDamage = 5f; // Lower damage per bolt since we shoot many
    public float lightningRange = 30f; // Slightly shorter range for shotgun feel
    public float lightningManaCost = 2.5f; 
    public int lightningPellets = 8; // How many bolts?
    public float lightningSpread = 0.1f; // How wide is the spread?
    public GameObject lightningEffectPrefab;
    [Tooltip("Distance in front of the fire point to spawn the fireball")]
    public float spawnOffset = 1.0f; // Added offset

    [Header("Weapon System")]
    public int currentWeaponType = 0; // 0 = Fireball, 1 = Lightning

    private float nextFireTime = 0f;

    void Start()
    {
        currentMana = maxMana;
    }

    void Update()
    {
        // Weapon Switching (Q key)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentWeaponType = (currentWeaponType + 1) % 2; // Toggle between 0 and 1
        }

        // Mana Regen
        if (currentMana < maxMana)
        {
            currentMana += manaRegen * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        // Shooting
        // Shooting
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            float cost = (currentWeaponType == 0) ? manaCost : lightningManaCost;

            if (currentMana >= cost)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                // Play "Out of Mana" sound?
            }
        }
    }

    void Shoot()
    {
        if (currentWeaponType == 0) // Fireball
        {
            currentMana -= manaCost;
            ShootFireball();
        }
        else if (currentWeaponType == 1) // Lightning
        {
            currentMana -= lightningManaCost;
            ShootLightning();
        }
    }

    void ShootFireball()
    {
        // Determine target point
        RaycastHit hit;
        Vector3 targetPoint;
        
        // Raycast from center of screen
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = playerCamera.position + playerCamera.forward * 1000f;
        }

        // Create fireball
        if (fireballPrefab && firePoint)
        {
            // Calculate direction from firePoint to targetPoint
            Vector3 direction = (targetPoint - firePoint.position).normalized;
            
            // Calculate spawn position with offset
            Vector3 spawnPosition = firePoint.position + (direction * spawnOffset);

            // Instantiate and rotate to look at target
            Instantiate(fireballPrefab, spawnPosition, Quaternion.LookRotation(direction));
        }
    }

    void ShootLightning()
    {
        for (int i = 0; i < lightningPellets; i++)
        {
            // Calculate spread
            Vector3 direction = playerCamera.forward;
            direction.x += Random.Range(-lightningSpread, lightningSpread);
            direction.y += Random.Range(-lightningSpread, lightningSpread);
            direction.z += Random.Range(-lightningSpread, lightningSpread);
            direction.Normalize();

            RaycastHit hit;
            Vector3 endPoint;

            // Hitscan logic
            if (Physics.Raycast(playerCamera.position, direction, out hit, lightningRange))
            {
                endPoint = hit.point;
                
                // Damage Glonk (or other enemies)
                GlonkEnemy enemy = hit.collider.GetComponent<GlonkEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(lightningDamage);
                }
            }
            else
            {
                endPoint = playerCamera.position + direction * lightningRange;
            }

            // Visual Effect
            if (lightningEffectPrefab != null && firePoint != null)
            {
                GameObject effectObj = Instantiate(lightningEffectPrefab, Vector3.zero, Quaternion.identity);
                LightningEffect effect = effectObj.GetComponent<LightningEffect>();
                if (effect != null)
                {
                    effect.Setup(firePoint.position, endPoint);
                }
            }
        }
    }

    public void GainMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
    }
}

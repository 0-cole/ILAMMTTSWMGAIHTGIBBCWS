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
    public float manaCost = 10f;
    public float manaRegen = 5f;

    private float nextFireTime = 0f;

    void Start()
    {
        currentMana = maxMana;
    }

    void Update()
    {
        // Mana Regen
        if (currentMana < maxMana)
        {
            currentMana += manaRegen * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        // Shooting
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentMana >= manaCost)
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
        currentMana -= manaCost;

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
            
            // Instantiate and rotate to look at target
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}

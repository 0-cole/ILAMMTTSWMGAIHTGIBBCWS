using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponEntry
    {
        public string name;
        public bool isUnlocked;
        public int weaponTypeIndex; // 0 for Fireball, 1 for Lightning, etc.
        public GameObject modelPrefab; // New: 3D Model for UI
    }

    [Header("References")]
    public Transform playerCamera;
    public Transform firePoint;
    public GameObject fireballPrefab;

    [Header("Stats")]
    public float fireRate = 0.5f;
    public float maxMana = 100f;
    public float currentMana;
    public float manaCost = 5f;
    public float manaRegen = 1f;
    
    [Header("Lightning Stats")]
    public float lightningDamage = 5f;
    public float lightningRange = 30f;
    public float lightningManaCost = 25f; 

    public GameObject lightningEffectPrefab;
    
    [Header("Weapon System")]
    public float spawnOffset = 1.0f;
    public System.Collections.Generic.List<WeaponEntry> weapons = new System.Collections.Generic.List<WeaponEntry>();
    public int currentWeaponIndex = 0;

    private float nextFireTime = 0f;

    void Start()
    {
        currentMana = maxMana;
        InitializeWeapons();
        LoadWeapons();
    }

    void InitializeWeapons()
    {
        if (weapons.Count == 0)
        {
            // Default setup if empty
            weapons.Add(new WeaponEntry { name = "Fireball", isUnlocked = true, weaponTypeIndex = 0 });
            weapons.Add(new WeaponEntry { name = "Lightning", isUnlocked = false, weaponTypeIndex = 1 });
        }
    }

    private float manaBoostTimer = 0f;
    private float manaBoostMultiplier = 2f;
    private float manaBoostDuration = 20f;

    void Update()
    {
        // Weapon Switching (Q key)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon();
        }

        // Mana Regen
        if (currentMana < maxMana)
        {
            float multiplier = (manaBoostTimer > 0) ? manaBoostMultiplier : 1f;
            currentMana += manaRegen * multiplier * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        // Boost Timer
        if (manaBoostTimer > 0)
        {
            manaBoostTimer -= Time.deltaTime;
        }

        // Shooting
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (Time.timeScale == 0f) return; // Block input if paused

            WeaponEntry currentWeapon = weapons[currentWeaponIndex];
            float cost = (currentWeapon.weaponTypeIndex == 0) ? manaCost : lightningManaCost;

            if (currentMana >= cost)
            {
                Shoot(currentWeapon.weaponTypeIndex);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    public void ActivateManaBoost()
    {
        manaBoostTimer = manaBoostDuration;
        Debug.Log($"[Weapon] Mana Boost Activated! Double regen for {manaBoostDuration}s.");
    }

    public bool IsWeaponUnlocked(string weaponName)
    {
        foreach (var w in weapons)
        {
            if (w.name == weaponName) return w.isUnlocked;
        }
        return false;
    }

    void CycleWeapon()
    {
        int originalIndex = currentWeaponIndex;
        int nextIndex = currentWeaponIndex;
        
        // Loop until we find an unlocked weapon or return to start
        do
        {
            nextIndex = (nextIndex + 1) % weapons.Count;
            if (weapons[nextIndex].isUnlocked)
            {
                currentWeaponIndex = nextIndex;
                Debug.Log($"[Weapon] Switched to {weapons[currentWeaponIndex].name}");
                return;
            }
        } while (nextIndex != originalIndex);
    }

    public void UnlockWeapon(string weaponName)
    {
        foreach (var weapon in weapons)
        {
            if (weapon.name == weaponName)
            {
                if (!weapon.isUnlocked)
                {
                    weapon.isUnlocked = true;
                    SaveWeapons();
                    Debug.Log($"[Weapon] Unlocked {weaponName}!");
                }
                return;
            }
        }
        Debug.LogWarning($"[Weapon] Could not find weapon named {weaponName} to unlock.");
    }

    public void ResetWeapons()
    {
        PlayerPrefs.DeleteKey("WeaponUnlocks");
        InitializeWeapons();
        // Reset to defaults
        foreach (var w in weapons)
        {
            w.isUnlocked = (w.name == "Fireball");
        }
        currentWeaponIndex = 0;
        Debug.Log("[Weapon] Data wiped/Reset.");
    }

    void SaveWeapons()
    {
        string data = "";
        foreach (var w in weapons)
        {
            if (w.isUnlocked) data += w.name + ",";
        }
        PlayerPrefs.SetString("WeaponUnlocks", data);
        PlayerPrefs.Save();
    }

    void LoadWeapons()
    {
        if (PlayerPrefs.HasKey("WeaponUnlocks"))
        {
            string data = PlayerPrefs.GetString("WeaponUnlocks");
            string[] unlockedNames = data.Split(',');
            
            foreach (var w in weapons)
            {
                // Fireball always unlocked
                if (w.name == "Fireball") w.isUnlocked = true;
                else
                {
                    bool found = false;
                    foreach (string s in unlockedNames)
                    {
                        if (s == w.name) found = true;
                    }
                    w.isUnlocked = found;
                }
            }
        }
    }

    void Shoot(int weaponType)
    {
        if (weaponType == 0) // Fireball
        {
            currentMana -= manaCost;
            ShootFireball();
        }
        else if (weaponType == 1) // Lightning
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
        StartCoroutine(ChainLightningRoutine());
    }

    System.Collections.IEnumerator ChainLightningRoutine()
    {
        // Settings
        int maxBounces = 50; 
        float currentDamage = lightningDamage; 
        float bounceRange = 15f; 
        float bounceDelay = 0.25f; // Wait time between arcs

        System.Collections.Generic.HashSet<GameObject> hitEnemies = new System.Collections.Generic.HashSet<GameObject>();

        // Start point
        Vector3 currentPosition = (firePoint != null) ? firePoint.position : playerCamera.position;

        // 1. Find Initial Target
        GameObject currentTarget = null;
        RaycastHit hit;
        if (Physics.SphereCast(playerCamera.position, 1f, playerCamera.forward, out hit, lightningRange))
        {
            GlonkEnemy enemy = hit.collider.GetComponentInParent<GlonkEnemy>();
            if (enemy != null) currentTarget = enemy.gameObject;
        }

        if (currentTarget == null)
        {
            Collider[] colliders = Physics.OverlapSphere(playerCamera.position + playerCamera.forward * 5f, 5f);
            float closestDist = Mathf.Infinity;
            foreach (var col in colliders)
            {
                GlonkEnemy enemy = col.GetComponentInParent<GlonkEnemy>();
                if (enemy != null)
                {
                    float d = Vector3.Distance(playerCamera.position, enemy.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        currentTarget = enemy.gameObject;
                    }
                }
            }
        }

        // Initial Shot Visual (Player to Target OR Player to Air)
        Vector3 initialEndPoint = (currentTarget != null) ? currentTarget.transform.position : (playerCamera.position + playerCamera.forward * lightningRange);
        SpawnLightningSegment(currentPosition, initialEndPoint);
        
        if (currentTarget == null) yield break; // Missed initial shot, stop chain

        // 2. Chain Logic
        for (int i = 0; i < maxBounces; i++)
        {
            if (currentTarget == null) break;

            // Register Hit & Damage
            hitEnemies.Add(currentTarget);
            GlonkEnemy enemyScript = currentTarget.GetComponentInParent<GlonkEnemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(currentDamage);
            }

            // Wait before arcing to next
            yield return new WaitForSeconds(bounceDelay);

            // Update Start Position (use current enemy position in case they moved)
            if (currentTarget != null) currentPosition = currentTarget.transform.position;
            else break; // Enemy died/destroyed during wait

            // 3. Find Next Target
            GameObject nextTarget = null;
            Collider[] potentialTargets = Physics.OverlapSphere(currentPosition, bounceRange);
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
    }

    void SpawnLightningSegment(Vector3 start, Vector3 end)
    {
        if (lightningEffectPrefab != null)
        {
            GameObject effectObj = Instantiate(lightningEffectPrefab, Vector3.zero, Quaternion.identity);
            LightningEffect effect = effectObj.GetComponent<LightningEffect>();
            if (effect != null)
            {
                System.Collections.Generic.List<Vector3> points = new System.Collections.Generic.List<Vector3>();
                points.Add(start);
                points.Add(end);
                effect.Setup(points);
            }
        }
    }

    public void GainMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
    }
}

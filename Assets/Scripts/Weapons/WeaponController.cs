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
    public WeaponPreviewManager previewManager;

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
    
    [Header("Punch Stats")]
    public float punchDamage = 50f;
    public float punchRange = 3f;
    public float punchSelfDamage = 10f;
    public float punchCooldown = 0.5f; 
    public float punchVisualDuration = 0.5f; // Duration for the GIF to play
    public GameObject punchOverlay; 
    public LayerMask aimLayerMask; 

    [Header("Audio")]
    [SerializeField] private AudioClip fireballSound;
    [SerializeField] private AudioClip[] lightningCastSounds;
    [SerializeField] private AudioClip lightningStrikeSound;
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private float punchSoundVolume = 0.6f;
    [SerializeField] private float parrySoundVolume = 0.8f;
    private AudioSource audioSource;
    private AudioSource parryAudioSource;

    [Header("Weapon System")]
    public float spawnOffset = 1.0f;
    public System.Collections.Generic.List<WeaponEntry> weapons = new System.Collections.Generic.List<WeaponEntry>();
    public int currentWeaponIndex = 0;

    private float nextFireTime = 0f;
    private float nextPunchTime = 0f;

    void Start()
    {
        currentMana = maxMana;
        InitializeWeapons();
        LoadWeapons();
        
        // Initial Preview Update
        if (previewManager != null && weapons.Count > 0)
        {
            previewManager.UpdateModel(weapons[currentWeaponIndex].modelPrefab);
        }

        if (punchOverlay != null) punchOverlay.SetActive(false);
    }
// ...

    void InitializeWeapons()
    {
        if (weapons.Count == 0)
        {
            // Default setup if empty
            weapons.Add(new WeaponEntry { name = "Fireball", isUnlocked = true, weaponTypeIndex = 0 });
            weapons.Add(new WeaponEntry { name = "Lightning", isUnlocked = false, weaponTypeIndex = 1 });
            weapons.Add(new WeaponEntry { name = "ParryPunch", isUnlocked = false, weaponTypeIndex = 2 });
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
            
            // Mana Check (Fireball/Lightning only)
            // Punch uses HP, not Mana, so we skip mana check for type 2
            bool canFire = false;
            
            if (currentWeapon.weaponTypeIndex == 2) // Punch
            {
                canFire = true; // Always fire if cooldown ready (HP check handled in Shoot)
            }
            else
            {
                float cost = (currentWeapon.weaponTypeIndex == 0) ? manaCost : lightningManaCost;
                if (currentMana >= cost) canFire = true;
            }

            if (canFire)
            {
                Shoot(currentWeapon.weaponTypeIndex);
                // Use custom cooldown for punch if desired, otherwise standard fireRate
                float cooldown = (currentWeapon.weaponTypeIndex == 2) ? punchCooldown : fireRate;
                nextFireTime = Time.time + cooldown;
            }
        }

        // Quick Melee (F Key)
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextPunchTime)
        {
             if (Time.timeScale == 0f) return;
             
             ShootPunch();
             nextPunchTime = Time.time + punchCooldown;
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
                
                // Update 3D Preview
                if (previewManager != null)
                {
                    previewManager.UpdateModel(weapons[currentWeaponIndex].modelPrefab);
                }
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
        else if (weaponType == 2) // Parry Punch
        {
            ShootPunch();
        }
    }

    void ShootPunch()
    {
        // Ensure audio sources exist
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (parryAudioSource == null)
        {
            parryAudioSource = gameObject.AddComponent<AudioSource>();
            parryAudioSource.playOnAwake = false;
            parryAudioSource.spatialBlend = 0f;
        }

        // Play punch sound initially
        if (punchSound != null)
            audioSource.PlayOneShot(punchSound, punchSoundVolume);

        // 1. Visuals
        if (punchOverlay != null)
        {
            StartCoroutine(PunchFlashRoutine());
        }

        // 2. Projectile Boost (Check closely for Fireballs)
        // Use SphereCast to be generous with aiming
        RaycastHit[] hits = Physics.SphereCastAll(playerCamera.position, 1.0f, playerCamera.forward, punchRange, aimLayerMask);
        bool hitProjectile = false;

        foreach (var h in hits)
        {
            WickedFireball fireball = h.collider.GetComponent<WickedFireball>();
            if (fireball != null)
            {
                // Align fireball to look where player is looking
                // We use playerCamera.forward so it goes exactly where we aim
                fireball.Boost(playerCamera.forward);
                hitProjectile = true;
            }
        }

        if (hitProjectile) 
        {
            // Fade out punch sound, play parry on separate source
            StartCoroutine(FadeOutSource(audioSource, 0.05f));
            if (parrySound != null)
                parryAudioSource.PlayOneShot(parrySound, parrySoundVolume);
            return;
        }

        // 3. Hitscan Attack (Only if no projectile was boosted)
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, punchRange, aimLayerMask))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(punchDamage);
                Debug.Log("PUNCH HIT!");
            }
        }
    }

    System.Collections.IEnumerator PunchFlashRoutine()
    {
        // Scale punch overlay based on FOV so it fills the same screen area
        if (playerCamera != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
            {
                float referenceFOV = 60f;
                float fovScaleStrength = 0.4f;
                float rawScale = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad)
                               / Mathf.Tan(referenceFOV * 0.5f * Mathf.Deg2Rad);
                float fovScale = Mathf.Lerp(1f, rawScale, fovScaleStrength);
                punchOverlay.transform.localScale = Vector3.one * fovScale;
            }
        }
        punchOverlay.SetActive(true);
        yield return new WaitForSeconds(punchVisualDuration);
        punchOverlay.SetActive(false);
    }

    void ShootFireball()
    {
        // Determine target point
        RaycastHit hit;
        Vector3 targetPoint;
        
        // Raycast from center of screen
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 1000f, aimLayerMask))
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
            GameObject fireballObj = Instantiate(fireballPrefab, spawnPosition, Quaternion.LookRotation(direction));
            
            // Initialize with player context for Smart Acceleration
            WickedFireball wf = fireballObj.GetComponent<WickedFireball>();
            if (wf != null)
            {
                wf.Initialize(transform, punchRange);
            }
        }
    }


    void ShootLightning()
    {
        // Settings
        int maxBounces = 50; 
        float currentDamage = lightningDamage; 
        float bounceRange = 15f; 
        float bounceDelay = 0.25f;

        // Start point
        Vector3 currentPosition = (firePoint != null) ? firePoint.position : playerCamera.position;

        // 1. Find Initial Target
        GameObject currentTarget = null;
        RaycastHit hit;
        
        // Visual for the initial shot (Raycast logic)
        // We handle the FIRST segment here visually if we miss, otherwise pass control to runner
        
        if (Physics.SphereCast(playerCamera.position, 1f, playerCamera.forward, out hit, lightningRange, aimLayerMask))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null) currentTarget = damageable.transform.gameObject;
        }

        if (currentTarget == null)
        {
            // Sphere Check as fallback
            Collider[] colliders = Physics.OverlapSphere(playerCamera.position + playerCamera.forward * 5f, 5f);
            float closestDist = Mathf.Infinity;
            foreach (var col in colliders)
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    float d = Vector3.Distance(playerCamera.position, damageable.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        currentTarget = damageable.transform.gameObject;
                    }
                }
            }
        }

        if (currentTarget != null)
        {
            // Create the Runner to handle the Chain
            GameObject runnerObj = new GameObject("ChainLightningRunner_" + Time.time);
            ChainLightningRunner runner = runnerObj.AddComponent<ChainLightningRunner>();
            runner.Initialize(currentPosition, currentTarget, currentDamage, maxBounces, bounceRange, bounceDelay, lightningEffectPrefab);
        }
        else
        {
            // Missed completely - Just show a dud line
            Vector3 endPoint = playerCamera.position + playerCamera.forward * lightningRange;
            SpawnLightningVisualCheck(currentPosition, endPoint);
        }
    }

    // Helper for the "Miss" case only, since Runner handles the rest
    void SpawnLightningVisualCheck(Vector3 start, Vector3 end)
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

    private System.Collections.IEnumerator FadeOutSource(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        source.Stop();
        source.volume = startVol;
    }
}

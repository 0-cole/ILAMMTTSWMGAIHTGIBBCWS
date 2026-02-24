using UnityEngine;

public class BillboardSpawner : MonoBehaviour
{
    [Header("Settings")]
    public float respawnDelay = 3.0f;
    public bool spawnOnStart = true;
    public float spawnYOffset = 1.0f; // Raise them up slightly

    [Header("Dependencies")]
    public Sprite[] possibleSprites;
    public GameObject fireballPrefab;
    public GameObject healthOrbPrefab;
    public GameObject manaOrbPrefab;
    public GameObject glonkPrefab; // Used to copy the health bar

    private GameObject currentBillboardEnemy;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnBillboardEnemy();
        }
    }

    void Update()
    {
        // Standard Respawn Logic
        if (currentBillboardEnemy == null && !IsInvoking("SpawnBillboardEnemy"))
        {
            Invoke("SpawnBillboardEnemy", respawnDelay);
        }
    }

    void SpawnBillboardEnemy()
    {
        // 1. Create Base Object
        currentBillboardEnemy = new GameObject("Billboard Enemy");
        currentBillboardEnemy.transform.position = transform.position + Vector3.up * spawnYOffset;
        currentBillboardEnemy.transform.rotation = transform.rotation;

        // 2. Add Sprite Renderer
        SpriteRenderer sr = currentBillboardEnemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10; // Ensure it renders on top
        sr.maskInteraction = SpriteMaskInteraction.None;

        // 3. Add Randomizer
        BillboardSpriteRandomizer randomizer = currentBillboardEnemy.AddComponent<BillboardSpriteRandomizer>();
        randomizer.possibleSprites = possibleSprites;
        
        // Set a default sprite immediately so we don't wait for Start()
        if (possibleSprites != null && possibleSprites.Length > 0)
        {
            sr.sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
        }

        // Set to standard scale
        currentBillboardEnemy.transform.localScale = new Vector3(1, 1, 1);

        // 4. Add Physics
        CapsuleCollider collider = currentBillboardEnemy.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0, 0f, 0);
        collider.radius = 0.5f;
        collider.height = 1f;

        Rigidbody rb = currentBillboardEnemy.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // 5. Add Shooter logic
        BillboardShooter shooter = currentBillboardEnemy.AddComponent<BillboardShooter>();
        // Using reflection to set private fields without changing the other script
        var fieldFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        
        var fbField = shooter.GetType().GetField("fireballPrefab", fieldFlags);
        if (fbField != null) fbField.SetValue(shooter, fireballPrefab);
        
        var hpField = shooter.GetType().GetField("healthPickupPrefab", fieldFlags);
        if (hpField != null) hpField.SetValue(shooter, healthOrbPrefab);

        var mpField = shooter.GetType().GetField("manaPickupPrefab", fieldFlags);
        if (mpField != null) mpField.SetValue(shooter, manaOrbPrefab);

        // 6. Add Health Bar
        if (glonkPrefab != null)
        {
            Transform canvasTransform = glonkPrefab.transform.Find("Canvas");
            if (canvasTransform != null)
            {
                GameObject canvasCopy = Instantiate(canvasTransform.gameObject, currentBillboardEnemy.transform);
                canvasCopy.transform.localPosition = new Vector3(0, 1.5f, 0); // Above the enemy
                canvasCopy.transform.localRotation = Quaternion.identity;

                WorldSpaceHealthBar healthBarObj = canvasCopy.GetComponent<WorldSpaceHealthBar>();
                if (healthBarObj != null)
                {
                    var billboardField = healthBarObj.GetType().GetField("billboard", fieldFlags);
                    if (billboardField != null) billboardField.SetValue(healthBarObj, true);
                }
            }
        }
        else
        {
            Debug.LogWarning("[BillboardSpawner] Glonk Prefab not assigned - skipping health bar setup.");
        }
    }
}

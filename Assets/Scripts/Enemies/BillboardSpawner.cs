using UnityEngine;

public class BillboardSpawner : MonoBehaviour
{
    [Header("Settings")]
    public float respawnDelay = 3.0f;
    public bool spawnOnStart = true;
    public float spawnYOffset = 1.0f;
    public float quadScale = 2.0f; // Size of the billboard quad

    [Header("Dependencies")]
    public Sprite[] possibleSprites;
    public GameObject fireballPrefab;
    public GameObject healthOrbPrefab;
    public GameObject manaOrbPrefab;
    public GameObject glonkPrefab;

    private GameObject currentBillboardEnemy;

    void Start()
    {
        if (possibleSprites == null || possibleSprites.Length == 0)
        {
            Debug.LogError($"[BillboardSpawner] No sprites assigned on {gameObject.name}! " +
                           "Drag your SWMG sprites into the 'Possible Sprites' array in the Inspector.");
        }

        if (spawnOnStart)
        {
            SpawnBillboardEnemy();
        }
    }

    void Update()
    {
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

        // 2. Create a Quad child for the visual (proper 3D — respects depth/occlusion)
        GameObject quadObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadObj.name = "BillboardQuad";
        quadObj.transform.SetParent(currentBillboardEnemy.transform, false);
        quadObj.transform.localPosition = Vector3.zero;
        quadObj.transform.localScale = new Vector3(quadScale, quadScale, 1f);

        // Remove the default quad collider (we add our own on the parent)
        Object.Destroy(quadObj.GetComponent<Collider>());

        // 3. Set up material from sprite
        MeshRenderer quadRenderer = quadObj.GetComponent<MeshRenderer>();
        if (possibleSprites != null && possibleSprites.Length > 0)
        {
            Sprite chosen = possibleSprites[Random.Range(0, possibleSprites.Length)];
            Material mat = CreateSpriteQuadMaterial(chosen);
            quadRenderer.material = mat;
            Debug.Log($"[BillboardSpawner] Spawned with sprite: {chosen.name} at {currentBillboardEnemy.transform.position}");
        }
        else
        {
            Debug.LogError("[BillboardSpawner] Cannot spawn — no sprites assigned!");
        }

        // 4. Add Randomizer (works with MeshRenderer now)
        BillboardSpriteRandomizer randomizer = currentBillboardEnemy.AddComponent<BillboardSpriteRandomizer>();
        randomizer.possibleSprites = possibleSprites;

        // 5. Add Physics on the root object
        CapsuleCollider collider = currentBillboardEnemy.AddComponent<CapsuleCollider>();
        collider.center = Vector3.zero;
        collider.radius = 0.5f;
        collider.height = 1f;

        Rigidbody rb = currentBillboardEnemy.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // 6. Add Shooter logic
        BillboardShooter shooter = currentBillboardEnemy.AddComponent<BillboardShooter>();
        shooter.Initialize(fireballPrefab, healthOrbPrefab, manaOrbPrefab);

        // 7. Add Health Bar
        if (glonkPrefab != null)
        {
            Transform canvasTransform = glonkPrefab.transform.Find("Canvas");
            if (canvasTransform != null)
            {
                GameObject canvasCopy = Instantiate(canvasTransform.gameObject, currentBillboardEnemy.transform);
                canvasCopy.transform.localPosition = new Vector3(0, 1.5f, 0);
                canvasCopy.transform.localRotation = Quaternion.identity;

                WorldSpaceHealthBar healthBarObj = canvasCopy.GetComponent<WorldSpaceHealthBar>();
                if (healthBarObj != null)
                {
                    var billboardField = healthBarObj.GetType().GetField("billboard",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (billboardField != null) billboardField.SetValue(healthBarObj, true);
                }
            }
        }
        else
        {
            Debug.LogWarning("[BillboardSpawner] Glonk Prefab not assigned - skipping health bar setup.");
        }
    }

    /// <summary>
    /// Creates a material from a sprite that uses a cutout shader — 
    /// this makes it render as a proper 3D object with depth testing,
    /// so it's hidden behind walls and other geometry.
    /// </summary>
    Material CreateSpriteQuadMaterial(Sprite sprite)
    {
        // Use URP Lit shader if available, fallback to Standard
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        
        Material mat = new Material(shader);
        mat.mainTexture = sprite.texture;

        // Enable alpha cutout so transparent pixels are clipped
        mat.SetFloat("_AlphaClip", 1f);
        mat.SetFloat("_Cutoff", 0.5f);

        // Set surface type to Opaque with alpha clipping (URP)
        mat.SetFloat("_Surface", 0); // 0 = Opaque
        mat.EnableKeyword("_ALPHATEST_ON");
        mat.renderQueue = 2450; // AlphaTest queue

        // For Standard shader fallback
        mat.SetFloat("_Mode", 1); // Cutout mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);

        return mat;
    }
}

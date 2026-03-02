using UnityEngine;

/// <summary>
/// Place this on empty GameObjects to define where enemies spawn.
/// Link them to a SpawnTrigger via matching spawnGroupId.
/// </summary>
public class EnemySpawnPoint : MonoBehaviour
{
    public enum EnemyType { Glonk, Billboard }
    public enum DropType { None, Health, Mana, Both }

    [Header("Enemy Config")]
    public EnemyType enemyType = EnemyType.Glonk;
    public DropType dropType = DropType.Both;

    [Header("Drop Chances")]
    [Range(0f, 1f)] public float healthDropChance = 0.4f;
    [Range(0f, 1f)] public float manaDropChance = 0.6f;

    [Header("Spawn Group")]
    [Tooltip("Must match the SpawnTrigger's spawnGroupId")]
    public float spawnGroupId = 0;
    [Tooltip("Wave number for sequential spawning (0 = first wave)")]
    public int waveNumber = 0;

    [Header("Prefabs")]
    public GameObject glonkPrefab;
    public GameObject billboardFireballPrefab;
    public GameObject healthPickupPrefab;
    public GameObject manaPickupPrefab;

    [Header("Effects")]
    public GameObject smokeEffectPrefab;
    public AudioClip spawnSound;

    [Header("Billboard Settings")]
    public Sprite[] billboardSprites;
    public float quadScale = 2f;
    public GameObject glonkPrefabForHealthBar;

    [HideInInspector] public bool hasSpawned = false;

    /// <summary>
    /// Called by SpawnTrigger to spawn the enemy at this point.
    /// </summary>
    public GameObject SpawnEnemy()
    {
        if (hasSpawned) return null;
        hasSpawned = true;

        // Smoke plume — emit for 0.5s, particles fade out over 0.25s
        if (smokeEffectPrefab != null)
        {
            GameObject smoke = Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
            var ps = smoke.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = false;
                main.duration = 0.5f;
                main.startLifetime = 0.25f;
                main.stopAction = ParticleSystemStopAction.Destroy;

                // Fade alpha to 0 over each particle's lifetime
                var col = ps.colorOverLifetime;
                col.enabled = true;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                col.color = grad;

                ps.Play();
            }
            else
            {
                Destroy(smoke, 0.75f);
            }
        }

        // Spawn sound
        if (spawnSound != null)
            AudioSource.PlayClipAtPoint(spawnSound, transform.position, 1f);

        if (enemyType == EnemyType.Glonk)
        {
            return SpawnGlonk();
        }
        else if (enemyType == EnemyType.Billboard)
        {
            return SpawnBillboard();
        }
        return null;
    }

    private GameObject SpawnGlonk()
    {
        if (glonkPrefab == null)
        {
            Debug.LogError($"[EnemySpawnPoint] No Glonk prefab on {gameObject.name}!");
            return null;
        }

        GameObject enemy = Instantiate(glonkPrefab, transform.position, transform.rotation);
        ConfigureDrops(enemy);
        return enemy;
    }

    private GameObject SpawnBillboard()
    {
        // Reuse BillboardSpawner's approach but one-shot
        GameObject enemy = new GameObject("Billboard Enemy");
        enemy.transform.position = transform.position + Vector3.up * 1f;
        enemy.transform.rotation = transform.rotation;

        // Quad visual
        GameObject quadObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadObj.name = "BillboardQuad";
        quadObj.transform.SetParent(enemy.transform, false);
        quadObj.transform.localPosition = Vector3.zero;
        quadObj.transform.localScale = new Vector3(quadScale, quadScale, 1f);
        Object.Destroy(quadObj.GetComponent<Collider>());

        // Sprite material
        MeshRenderer quadRenderer = quadObj.GetComponent<MeshRenderer>();
        if (billboardSprites != null && billboardSprites.Length > 0)
        {
            Sprite chosen = billboardSprites[Random.Range(0, billboardSprites.Length)];
            quadRenderer.material = CreateSpriteQuadMaterial(chosen);
        }

        if (billboardSprites != null && billboardSprites.Length > 0)
        {
            BillboardSpriteRandomizer randomizer = enemy.AddComponent<BillboardSpriteRandomizer>();
            randomizer.possibleSprites = billboardSprites;
        }

        // Physics
        CapsuleCollider col = enemy.AddComponent<CapsuleCollider>();
        col.center = Vector3.zero;
        col.radius = 0.5f;
        col.height = 1f;
        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // Shooter
        BillboardShooter shooter = enemy.AddComponent<BillboardShooter>();
        GameObject hp = (dropType == DropType.Health || dropType == DropType.Both) ? healthPickupPrefab : null;
        GameObject mp = (dropType == DropType.Mana || dropType == DropType.Both) ? manaPickupPrefab : null;
        shooter.Initialize(billboardFireballPrefab, hp, mp);

        // Health bar from glonk prefab canvas
        if (glonkPrefabForHealthBar != null)
        {
            Transform canvasTransform = glonkPrefabForHealthBar.transform.Find("Canvas");
            if (canvasTransform != null)
            {
                GameObject canvasCopy = Instantiate(canvasTransform.gameObject, enemy.transform);
                canvasCopy.transform.localPosition = new Vector3(0, 1.5f, 0);
                canvasCopy.transform.localRotation = Quaternion.identity;
            }
        }
        return enemy;
    }

    private void ConfigureDrops(GameObject enemy)
    {
        // For Glonk enemies, configure drop chances via reflection since fields are serialized private
        GlonkEnemy glonk = enemy.GetComponent<GlonkEnemy>();
        if (glonk == null) return;

        var type = glonk.GetType();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        switch (dropType)
        {
            case DropType.None:
                SetField(type, glonk, "healthDropChance", 0f, flags);
                SetField(type, glonk, "manaDropChance", 0f, flags);
                break;
            case DropType.Health:
                SetField(type, glonk, "healthDropChance", healthDropChance, flags);
                SetField(type, glonk, "manaDropChance", 0f, flags);
                break;
            case DropType.Mana:
                SetField(type, glonk, "healthDropChance", 0f, flags);
                SetField(type, glonk, "manaDropChance", manaDropChance, flags);
                break;
            case DropType.Both:
                SetField(type, glonk, "healthDropChance", healthDropChance, flags);
                SetField(type, glonk, "manaDropChance", manaDropChance, flags);
                break;
        }
    }

    private void SetField(System.Type type, object target, string fieldName, float value, System.Reflection.BindingFlags flags)
    {
        var field = type.GetField(fieldName, flags);
        if (field != null) field.SetValue(target, value);
    }

    private Material CreateSpriteQuadMaterial(Sprite sprite)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.mainTexture = sprite.texture;
        mat.SetFloat("_AlphaClip", 1f);
        mat.SetFloat("_Cutoff", 0.5f);
        mat.SetFloat("_Surface", 0);
        mat.EnableKeyword("_ALPHATEST_ON");
        mat.renderQueue = 2450;
        mat.SetFloat("_Mode", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        return mat;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = (enemyType == EnemyType.Glonk) ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up, "d_Monster Icon", true);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
            $"Group {spawnGroupId} | {enemyType}");
#endif
    }
}

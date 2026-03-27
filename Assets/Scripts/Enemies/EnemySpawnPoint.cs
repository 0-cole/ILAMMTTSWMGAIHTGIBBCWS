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
    public AudioClip glonkSpawnSound;
    public AudioClip billboardSpawnSound;

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
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

        if (enemyType == EnemyType.Glonk)
        {
            if (glonkSpawnSound != null)
                AudioSource.PlayClipAtPoint(glonkSpawnSound, transform.position, 1f);
            return SpawnGlonk();
        }
        else if (enemyType == EnemyType.Billboard)
        {
            if (billboardSpawnSound != null)
                AudioSource.PlayClipAtPoint(billboardSpawnSound, transform.position, 1f);
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

        // Sprite visual
        GameObject spriteObj = new GameObject("BillboardSprite");
        spriteObj.transform.SetParent(enemy.transform, false);
        spriteObj.transform.localPosition = Vector3.zero;

        SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();

        if (billboardSprites != null && billboardSprites.Length > 0)
        {
            BillboardSpriteRandomizer randomizer = enemy.AddComponent<BillboardSpriteRandomizer>();
            randomizer.possibleSprites = billboardSprites;
            randomizer.targetWorldHeight = quadScale;
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

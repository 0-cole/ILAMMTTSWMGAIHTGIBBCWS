#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BillboardEnemySetup : Editor
{
    [MenuItem("Tools/SWMG/Create Billboard Enemy Prefab")]
    public static void CreateBillboardEnemy()
    {
        // 0. Ensure images are imported as sprites
        string[] rawImageGuids = AssetDatabase.FindAssets("SWMG t:Texture2D", new[] { "Assets/Sprites" });
        foreach (var guid in rawImageGuids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        // 1. Create the base GameObject
        GameObject enemyObj = new GameObject("Billboard Enemy");

        // 2. Add SpriteRenderer
        SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
        
        // Find the sprites
        string[] guids = AssetDatabase.FindAssets("SWMG t:Sprite", new[] { "Assets/Sprites" });
        Sprite[] sprites = new Sprite[guids.Length];
        for(int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        if(sprites.Length > 0)
        {
            sr.sprite = sprites[0]; // Set a default sprite
        }
        
        // Reset scale based on user feedback. The image is already quite large.
        enemyObj.transform.localScale = new Vector3(1, 1, 1);
        enemyObj.transform.position = Vector3.zero; // Spawn at center, not way out in space

        // 3. Add Randomizer Script
        BillboardSpriteRandomizer randomizer = enemyObj.AddComponent<BillboardSpriteRandomizer>();
        randomizer.possibleSprites = sprites;

        // 4. Add Physics (CapsuleCollider)
        CapsuleCollider collider = enemyObj.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0, 0f, 0); // centered
        collider.radius = 0.5f;
        collider.height = 1f;

        // 5. Add Rigidbody (so it doesn't move but can be hit)
        Rigidbody rb = enemyObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // 6. Add the Billboard Shooter script
        BillboardShooter shooter = enemyObj.AddComponent<BillboardShooter>();
        
        // Load Prefabs to assign to shooter
        GameObject fireballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Fireball.prefab");
        GameObject healthOrbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HealthOrb.prefab");
        GameObject manaOrbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ManaOrb.prefab");

        // Use reflection or SerializedObject to assign private serialized fields
        SerializedObject serializedShooter = new SerializedObject(shooter);
        serializedShooter.FindProperty("fireballPrefab").objectReferenceValue = fireballPrefab;
        serializedShooter.FindProperty("healthPickupPrefab").objectReferenceValue = healthOrbPrefab;
        serializedShooter.FindProperty("manaPickupPrefab").objectReferenceValue = manaOrbPrefab;
        serializedShooter.ApplyModifiedProperties();

        // 7. Add the HealthBar from Glonk
        GameObject glonkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Glonk.prefab");
        if (glonkPrefab != null)
        {
            // Find the Canvas child
            Transform canvasTransform = glonkPrefab.transform.Find("Canvas");
            if (canvasTransform != null)
            {
                // Instantiate a copy of the Canvas and parent it to our new enemy
                GameObject canvasCopy = (GameObject)PrefabUtility.InstantiatePrefab(canvasTransform.gameObject);
                canvasCopy.transform.SetParent(enemyObj.transform);
                canvasCopy.transform.localPosition = new Vector3(0, 1.5f, 0); // Position it above the enemy
                canvasCopy.transform.localRotation = Quaternion.identity;
                canvasCopy.transform.localScale = canvasTransform.localScale;
                
                // Also assign the health fill image reference properly if possible
                WorldSpaceHealthBar healthBarObj = canvasCopy.GetComponent<WorldSpaceHealthBar>();
                if (healthBarObj != null)
                {
                    // Update behavior so it faces the camera properly
                    SerializedObject serializedHealthBar = new SerializedObject(healthBarObj);
                    serializedHealthBar.FindProperty("billboard").boolValue = true;
                    serializedHealthBar.ApplyModifiedProperties();
                }
            }
        }

        // 8. Save as Prefab
        string prefabPath = "Assets/Prefabs/Billboard Enemy.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(enemyObj, prefabPath, InteractionMode.UserAction);

        // Highlight it in the project window
        EditorGUIUtility.PingObject(enemyObj);

        Debug.Log($"<color=green>[Success]</color> Billboard Enemy Prefab created at: {prefabPath}");
    }
}
#endif

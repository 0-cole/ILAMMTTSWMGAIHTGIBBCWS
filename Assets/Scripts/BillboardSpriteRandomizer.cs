using UnityEngine;

/// <summary>
/// Randomly assigns a sprite texture to a billboard enemy's quad mesh.
/// Works with MeshRenderer (3D quad) instead of SpriteRenderer for proper depth testing.
/// </summary>
public class BillboardSpriteRandomizer : MonoBehaviour
{
    [Tooltip("The list of sprites to choose from randomly on spawn.")]
    public Sprite[] possibleSprites;

    void Start()
    {
        if (possibleSprites == null || possibleSprites.Length == 0)
        {
            Debug.LogWarning($"[BillboardSpriteRandomizer] No sprites assigned to {gameObject.name}!");
            return;
        }

        // Find the quad's MeshRenderer (on child named "BillboardQuad")
        MeshRenderer quadRenderer = GetComponentInChildren<MeshRenderer>();
        if (quadRenderer == null)
        {
            Debug.LogWarning($"[BillboardSpriteRandomizer] No MeshRenderer found on {gameObject.name}!");
            return;
        }

        // Pick a random sprite
        int randomIndex = Random.Range(0, possibleSprites.Length);
        Sprite chosen = possibleSprites[randomIndex];

        // Apply the sprite's texture to the quad's material
        if (quadRenderer.material != null)
        {
            quadRenderer.material.mainTexture = chosen.texture;
        }

        // Random horizontal flip
        if (Random.value > 0.5f)
        {
            Transform quadTransform = quadRenderer.transform;
            Vector3 s = quadTransform.localScale;
            s.x = -s.x;
            quadTransform.localScale = s;
        }
    }
}

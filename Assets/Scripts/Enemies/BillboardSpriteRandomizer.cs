using UnityEngine;

/// <summary>
/// Randomly assigns a sprite texture to a billboard enemy's quad mesh.
/// Works with MeshRenderer (3D quad) instead of SpriteRenderer for proper depth testing.
/// </summary>
public class BillboardSpriteRandomizer : MonoBehaviour
{
    [Tooltip("The list of sprites to choose from randomly on spawn.")]
    public Sprite[] possibleSprites;

    [Tooltip("Desired world-space height for the sprite.")]
    public float targetWorldHeight = 2f;

    void Start()
    {
        if (possibleSprites == null || possibleSprites.Length == 0)
        {
            Debug.LogWarning($"[BillboardSpriteRandomizer] No sprites assigned to {gameObject.name}!");
            return;
        }

        // Find the SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[BillboardSpriteRandomizer] No SpriteRenderer found on {gameObject.name}!");
            return;
        }

        // Pick a random sprite
        int randomIndex = Random.Range(0, possibleSprites.Length);
        Sprite chosen = possibleSprites[randomIndex];

        // Apply
        spriteRenderer.sprite = chosen;

        // Normalize scale so sprite height matches targetWorldHeight
        // A sprite's native world height = sprite.rect.height / sprite.pixelsPerUnit
        float nativeHeight = chosen.rect.height / chosen.pixelsPerUnit;
        float scaleFactor = targetWorldHeight / nativeHeight;
        spriteRenderer.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        // Random horizontal flip
        if (Random.value > 0.5f)
        {
            spriteRenderer.flipX = true;
        }
    }
}

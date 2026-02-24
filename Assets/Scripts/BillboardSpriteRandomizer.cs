using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BillboardSpriteRandomizer : MonoBehaviour
{
    [Tooltip("The list of sprites to choose from randomly on spawn.")]
    public Sprite[] possibleSprites;

    void Start()
    {
        if (possibleSprites != null && possibleSprites.Length > 0)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            int randomIndex = Random.Range(0, possibleSprites.Length);
            sr.sprite = possibleSprites[randomIndex];
            
            // Adjust the randomizer to flip the sprite randomly sometimes
            if (Random.value > 0.5f)
            {
                sr.flipX = true;
            }
        }
        else
        {
            Debug.LogWarning($"[BillboardSpriteRandomizer] No sprites assigned to {gameObject.name}!");
        }
    }
}

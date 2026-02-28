using UnityEngine;

public class VoidKiller : MonoBehaviour
{
    [Tooltip("Y Position below which the player dies")]
    [SerializeField] private float killHeight = -20f;

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Check if we fell below the kill height
        if (transform.position.y < killHeight)
        {
            if (playerHealth != null)
            {
                // Kill instantly (simulate massive damage)
                playerHealth.TakeDamage(9999f); 
            }
            else
            {
                // Fallback if no health script: Reload Scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}

using UnityEngine;

public class ProjectileIgnoreOwner : MonoBehaviour
{
    [SerializeField] private float ignoreCollisionTime = 0.2f;
    
    private Collider projectileCollider;
    private GameObject player;

    void Start()
    {
        projectileCollider = GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (projectileCollider != null && player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                // Try CharacterController instead
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    // Disable collision with player temporarily
                    Physics.IgnoreCollision(projectileCollider, cc, true);
                    Invoke(nameof(ReEnableCollision), ignoreCollisionTime);
                }
            }
            else
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider, true);
                Invoke(nameof(ReEnableCollision), ignoreCollisionTime);
            }
        }
    }

    void ReEnableCollision()
    {
        if (projectileCollider != null && player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Physics.IgnoreCollision(projectileCollider, cc, false);
                }
            }
            else
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider, false);
            }
        }
    }
}

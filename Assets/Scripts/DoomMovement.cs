using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DoomMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float airAcceleration = 10f;
    [SerializeField] private float groundAcceleration = 14f;
    [SerializeField] private float friction = 8f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 20f;
    
    [Header("Air Control")]
    [SerializeField] private float airControl = 0.3f;

    [Header("Wall Jump")]
    [SerializeField] private float wallStickDuration = 3f;
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private Camera playerCamera; // Assignment needed in Inspector or detecting Main Camera

    private bool isWallSticking;
    private float wallStickTimer;
    private Vector3 wallNormal;
    private float wallJumpCooldownTimer; // New: prevents instant re-stick after jump
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
    }
    
    void Update()
    {
        // Check if grounded
        isGrounded = controller.isGrounded;
        
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetButton("Jump"); // Changed to GetButton for auto-hop
        
        // Calculate movement direction relative to where player is facing
        Vector3 inputDirection = transform.right * horizontal + transform.forward * vertical;
        inputDirection.Normalize();
        
        // Handle movement
        if (isGrounded)
        {
            GroundMove(inputDirection, sprint);
            
            // Jump
            if (jump)
            {
                velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
            }
        }
        else
        {
            AirMove(inputDirection);
        }
        
        // Apply gravity (if not sticking)
        if (!isWallSticking)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            // Wall Stick Logic
            velocity = Vector3.zero; // Stop all movement
            wallStickTimer -= Time.deltaTime;

            // Jump from wall
            if (Input.GetButtonDown("Jump"))
            {
                // Launch in Camera Direction
                velocity = playerCamera.transform.forward * wallJumpForce;
                
                isWallSticking = false;
                wallJumpCooldownTimer = 0.5f; // New: 0.5s immunity to sticking
            }

            // Fall off if timer ends
            if (wallStickTimer <= 0)
            {
                isWallSticking = false;
                wallJumpCooldownTimer = 0.5f; // Prevent immediate stick if just falling off? User didn't ask for this but it feels safe.
            }
        }
        
        // Cooldown timer logic
        if (wallJumpCooldownTimer > 0)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
        }

        // Move the character
        controller.Move(velocity * Time.deltaTime);
        
        // Reset vertical velocity if grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
            isWallSticking = false; // Reset wall stick on ground
        }
    }
    
    private void GroundMove(Vector3 inputDirection, bool sprint)
    {
        // Calculate target speed
        float targetSpeed = sprint ? runSpeed : moveSpeed;
        
        // Apply friction
        if (inputDirection.magnitude < 0.1f)
        {
            float drop = velocity.magnitude * friction * Time.deltaTime;
            velocity *= Mathf.Max(velocity.magnitude - drop, 0) / Mathf.Max(velocity.magnitude, 0.001f);
        }
        
        // Accelerate
        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = inputDirection * targetSpeed;
            velocity = Vector3.MoveTowards(
                new Vector3(velocity.x, 0, velocity.z),
                targetVelocity,
                groundAcceleration * Time.deltaTime
            );
        }
    }
    
    private void AirMove(Vector3 inputDirection)
    {
        // Air control - allows player to slightly adjust movement in air
        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = inputDirection * moveSpeed * airControl;
            Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                currentHorizontalVelocity + targetVelocity,
                airAcceleration * Time.deltaTime
            );
            velocity.x = newHorizontalVelocity.x;
            velocity.z = newHorizontalVelocity.z;
        }
    }
    
    // Public method to get current speed (useful for effects)
    public float GetSpeed()
    {
        return new Vector3(velocity.x, 0, velocity.z).magnitude;
    }

    // Handle collisions
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Wall Detection (Normal.y roughly 0)
        if (hit.normal.y < 0.7f && hit.normal.y > -0.7f)
        {
            // Ignore Triggers (Orbs, Zones, etc.)
            if (hit.collider.isTrigger) return;

            // Ignore Dynamic Objects (Enemies, Pickups, etc.)
            if (hit.gameObject.CompareTag("Enemy") || 
                hit.gameObject.CompareTag("Orb") || 
                hit.gameObject.CompareTag("Player"))
            {
                return;
            }

            // Only stick if airborne, moving into wall, and not already sticking
            if (!isGrounded && !isWallSticking && velocity.y < 0) // Only stick on way down? Or anytime? User said "jump on a wall". Let's say airborne.
            {
                // Check if moving INTO the wall
                // float projection = Vector3.Dot(velocity, hit.normal); 
                // Simply touching it while airborne should trigger it based on user description "when you jump on a wall"
                
                // Start Stick
                // Only stick if Cooldown is over
                if (wallJumpCooldownTimer <= 0) 
                {
                     isWallSticking = true;
                     wallStickTimer = wallStickDuration;
                     wallNormal = hit.normal;
                }
            }
            
            // Standard Slide Logic (only if NOT sticking to allow sliding during movement? actually stick stops movement)
            if (!isWallSticking)
            {
                float projection = Vector3.Dot(velocity, hit.normal);
                if (projection < 0) velocity -= projection * hit.normal;
            }
        }
    }
}

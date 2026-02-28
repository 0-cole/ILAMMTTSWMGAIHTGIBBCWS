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
    
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpMultiplier = 0.5f;
    private int jumpCount = 0;
    private const int maxJumps = 2;

    [Header("Air Control")]
    [SerializeField] private float airControl = 0.3f;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideDuration = 3f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private Camera playerCamera; // Assignment needed in Inspector or detecting Main Camera

    private bool isWallSliding;
    private float wallSlideTimer;
    private Vector3 wallNormal;
    private float wallJumpCooldownTimer;
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpGrunt;
    [SerializeField] private float doubleJumpPitchBoost = 1.3f;
    [SerializeField] private float jumpGruntVolume = 0.15f;
    private AudioSource audioSource;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
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
            
            // Ground Jump (auto-hop with GetButton)
            if (jump)
            {
                velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
                jumpCount = 1;
                PlayJumpGrunt(1f);
            }
        }
        else
        {
            AirMove(inputDirection);

            // Double Jump (GetButtonDown to prevent auto-double-jump)
            if (Input.GetButtonDown("Jump") && jumpCount < maxJumps && !isWallSliding)
            {
                velocity.y = Mathf.Sqrt(2f * jumpHeight * doubleJumpMultiplier * gravity);
                jumpCount = maxJumps;
                PlayJumpGrunt(doubleJumpPitchBoost);
            }
        }
        
        // Apply gravity
        velocity.y -= gravity * Time.deltaTime;

        if (isWallSliding)
        {
            // Wall Slide Logic - allow lateral movement along the wall
            Vector3 slideRight = Vector3.Cross(wallNormal, Vector3.up).normalized;
            float lateral = Input.GetAxisRaw("Horizontal");
            velocity.x = slideRight.x * lateral * moveSpeed;
            velocity.z = slideRight.z * lateral * moveSpeed;
            velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
            wallSlideTimer -= Time.deltaTime;

            // Jump from wall
            if (Input.GetButtonDown("Jump"))
            {
                velocity = playerCamera.transform.forward * wallJumpForce;
                isWallSliding = false;
                wallJumpCooldownTimer = 0.5f;
            }

            // Stop sliding if timer ends
            if (wallSlideTimer <= 0)
            {
                isWallSliding = false;
                wallJumpCooldownTimer = 0.5f;
            }
        }
        
        // Cooldown timer logic
        if (wallJumpCooldownTimer > 0)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
        }

        // Move the character
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);

        // Kill upward velocity on ceiling hit
        if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0)
        {
            velocity.y = 0;
        }
        
        // Reset vertical velocity if grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
            isWallSliding = false; // Reset wall slide on ground
            jumpCount = 0; // Reset double jump
        }
    }
    
    private void GroundMove(Vector3 inputDirection, bool sprint)
    {
        float targetSpeed = sprint ? runSpeed : moveSpeed;
        
        if (inputDirection.magnitude > 0.1f)
        {
            // Move directly at target speed
            Vector3 targetVelocity = inputDirection * targetSpeed;
            velocity.x = targetVelocity.x;
            velocity.z = targetVelocity.z;
        }
        else
        {
            // Slight deceleration for visual smoothness
            velocity.x *= 0.85f;
            velocity.z *= 0.85f;
            if (new Vector3(velocity.x, 0, velocity.z).magnitude < 0.1f)
            {
                velocity.x = 0;
                velocity.z = 0;
            }
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

    private void PlayJumpGrunt(float pitch)
    {
        if (jumpGrunt == null) return;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(jumpGrunt, jumpGruntVolume);
        audioSource.pitch = 1f;
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
            if (!isGrounded && !isWallSliding && velocity.y < 0)
            {
                if (wallJumpCooldownTimer <= 0) 
                {
                     isWallSliding = true;
                     wallSlideTimer = wallSlideDuration;
                     wallNormal = hit.normal;
                }
            }
            
            // Standard Slide Logic (only if NOT sticking to allow sliding during movement? actually stick stops movement)
            if (!isWallSliding)
            {
                float projection = Vector3.Dot(velocity, hit.normal);
                if (projection < 0) velocity -= projection * hit.normal;
            }
        }
    }
}

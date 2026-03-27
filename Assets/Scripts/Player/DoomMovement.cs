using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DoomMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float runSpeed = 12f;
    public float airAcceleration = 25f;       // Increased for sharper mid-air control
    public float groundAcceleration = 35f;    // Increased so player reaches max speed faster
    public float friction = 25f;              // Increased so player stops sliding instantly
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 20f;
    
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpMultiplier = 0.5f;
    private int jumpCount = 0;
    private const int maxJumps = 2;

    [Header("Air Control")]
    [SerializeField] private float airControl = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepLoop;
    [SerializeField] private float footstepVolume = 0.15f;
    [SerializeField] private float footstepSpeedThreshold = 3f;
    [SerializeField] private AudioClip wallSlideSound;
    [SerializeField] private float wallSlideVolume = 0.5f;
    [SerializeField] private AudioClip fallSound;
    [SerializeField] private float fallVolume = 0.3f;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private float crashVolume = 0.6f;
    private AudioSource footstepSource;
    private AudioSource wallSlideSource;
    private AudioSource fallSource;
    private float lastGroundedY;
    private bool isFalling;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideDuration = 3f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private Camera playerCamera; // Assignment needed in Inspector or detecting Main Camera

    private bool isWallSliding;
    private float wallSlideTimer;
    private Vector3 wallNormal;
    private float wallJumpCooldownTimer;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (footstepLoop != null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.clip = footstepLoop;
            footstepSource.loop = true;
            footstepSource.volume = footstepVolume;
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f;
        }

        if (wallSlideSound != null)
        {
            wallSlideSource = gameObject.AddComponent<AudioSource>();
            wallSlideSource.clip = wallSlideSound;
            wallSlideSource.loop = true;
            wallSlideSource.volume = wallSlideVolume;
            wallSlideSource.playOnAwake = false;
            wallSlideSource.spatialBlend = 0f;
        }

        if (fallSound != null)
        {
            fallSource = gameObject.AddComponent<AudioSource>();
            fallSource.clip = fallSound;
            fallSource.loop = true;
            fallSource.volume = fallVolume;
            fallSource.playOnAwake = false;
            fallSource.spatialBlend = 0f;
        }

        lastGroundedY = transform.position.y;
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
            }
        }
        
        // Apply gravity
        velocity.y -= gravity * Time.deltaTime;

        if (isWallSliding)
        {
            // Wall Slide Logic - slow the fall instead of freezing
            velocity.x = 0;
            velocity.z = 0;
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
            // Crash sound on landing if we were falling
            if (isFalling)
            {
                isFalling = false;
                if (fallSource != null && fallSource.isPlaying)
                    fallSource.Stop();
                if (crashSound != null)
                    AudioSource.PlayClipAtPoint(crashSound, transform.position, crashVolume);
            }

            velocity.y = -2f; // Small downward force to keep grounded
            isWallSliding = false; // Reset wall slide on ground
            jumpCount = 0; // Reset double jump
            lastGroundedY = transform.position.y;
        }

        // Fall detection — trigger when fallen more than double jump height
        if (!isGrounded && velocity.y < 0)
        {
            float doubleJumpHeight = jumpHeight * doubleJumpMultiplier;
            float fallDist = lastGroundedY - transform.position.y;
            if (!isFalling && fallDist > doubleJumpHeight)
            {
                isFalling = true;
                if (fallSource != null && !fallSource.isPlaying)
                    fallSource.Play();
            }
        }

        // Update lastGroundedY when grounded or going up
        if (isGrounded || velocity.y > 0)
            lastGroundedY = Mathf.Max(lastGroundedY, transform.position.y);

        // Footstep audio — only when actively pressing movement keys while grounded
        if (footstepSource != null)
        {
            bool hasInput = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
            float hSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            bool shouldPlay = isGrounded && hasInput && hSpeed > footstepSpeedThreshold && !isWallSliding;
            if (shouldPlay && !footstepSource.isPlaying)
                footstepSource.Play();
            else if (!shouldPlay && footstepSource.isPlaying)
                footstepSource.Stop();
        }

        // Wall slide audio
        if (wallSlideSource != null)
        {
            if (isWallSliding && !wallSlideSource.isPlaying)
                wallSlideSource.Play();
            else if (!isWallSliding && wallSlideSource.isPlaying)
                wallSlideSource.Stop();
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
    
    void OnDisable()
    {
        if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
        if (wallSlideSource != null && wallSlideSource.isPlaying) wallSlideSource.Stop();
        if (fallSource != null && fallSource.isPlaying) fallSource.Stop();
        isFalling = false;
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
            // Also require hit point is beside the player (not a ceiling edge above)
            if (!isGrounded && !isWallSliding && velocity.y < 0)
            {
                float playerMidY = transform.position.y;
                float hitRelativeY = hit.point.y - playerMidY;
                bool hitIsBesidePlayer = hitRelativeY < controller.height * 0.5f;

                if (wallJumpCooldownTimer <= 0 && hitIsBesidePlayer) 
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

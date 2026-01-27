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
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        // Check if grounded
        isGrounded = controller.isGrounded;
        
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetButtonDown("Jump");
        
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
        
        // Apply gravity
        velocity.y -= gravity * Time.deltaTime;
        
        // Move the character
        controller.Move(velocity * Time.deltaTime);
        
        // Reset vertical velocity if grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
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

    // Handle collisions to prevent sticking to walls
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // If we hit a wall (surface that isn't excessively flat like ground/ceiling)
        // Normal.y of 1 is flat up, -1 is flat down. Walls are roughly 0.
        // We use < 0.7f (approx 45 degrees) to define a wall.
        if (hit.normal.y < 0.7f)
        {
            // Debug.DrawRay(hit.point, hit.normal, Color.red, 1f);
            
            // Check if we are moving INTO the wall
            float projection = Vector3.Dot(velocity, hit.normal);
            
            // If dragging into wall (negative dot product)
            if (projection < 0)
            {
                // Project velocity onto the wall plane (remove the component pointing into the wall)
                // This creates a nice "slide" effect and stops the "sticking" behavior
                velocity -= projection * hit.normal;
            }
        }
    }
}

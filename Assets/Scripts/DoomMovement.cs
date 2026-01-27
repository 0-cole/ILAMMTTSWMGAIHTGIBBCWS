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
}

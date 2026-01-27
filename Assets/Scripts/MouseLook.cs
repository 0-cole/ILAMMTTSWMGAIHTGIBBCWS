using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Sensitivity multiplier to make values more manageable (like Source engine's 0.022)")]
    [SerializeField] private float sensitivityMultiplier = 0.01f;
    [SerializeField] private float sensitivityX = 15f;
    [SerializeField] private float sensitivityY = 15f;
    [SerializeField] private float lookXLimit = 90f; // Prevents going upside down
    
    [Header("Smoothing")]
    [SerializeField] private bool smoothing = false;
    [SerializeField] private float smoothTime = 0.1f;
    
    private Transform playerBody;
    private float xRotation = 0f;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    
    void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Get reference to parent (player body)
        playerBody = transform.parent;
        
        if (playerBody == null)
        {
            Debug.LogError("MouseLook: Camera must be a child of the Player object!");
        }
    }
    
    void Update()
    {
        // Get raw mouse input for 1:1 movement
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivityX * sensitivityMultiplier * 100f; // * 100 to keep values reasonable relative to Unity's default
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivityY * sensitivityMultiplier * 100f;
        
        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
        
        // Apply smoothing if enabled
        if (smoothing)
        {
            currentMouseDelta = Vector2.SmoothDamp(
                currentMouseDelta,
                targetMouseDelta,
                ref currentMouseDeltaVelocity,
                smoothTime
            );
        }
        else
        {
            currentMouseDelta = targetMouseDelta;
        }
        
        // Rotate camera up/down (pitch) - clamped to prevent going upside down
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Rotate player body left/right (yaw)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * currentMouseDelta.x);
        }
        
        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Re-lock cursor on mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

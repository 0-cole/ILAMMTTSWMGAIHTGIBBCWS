using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool lockOnStart = true;

    private bool isCursorLocked = false;

    void Start()
    {
        if (lockOnStart)
        {
            LockCursor();
        }
    }

    void Update()
    {
        // Escape to unlock cursor (for menu access)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursor();
        }

        // Click to re-lock cursor when unlocked
        if (!isCursorLocked && Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    public void ToggleCursor()
    {
        if (isCursorLocked)
            UnlockCursor();
        else
            LockCursor();
    }

    public bool IsCursorLocked()
    {
        return isCursorLocked;
    }

    // Re-lock cursor when window regains focus
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && isCursorLocked)
        {
            // Re-apply lock state
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

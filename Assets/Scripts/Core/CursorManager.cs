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

    // Update removed: Input handling moved to PauseManager

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
        // If locked, unlock
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
        }
        // If unlocked (None or Confined), lock
        else
        {
            LockCursor();
        }
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

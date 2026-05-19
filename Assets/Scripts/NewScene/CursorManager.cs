using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private bool cursorLocked = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // Lock curor on left mouse click
        if (Input.GetMouseButton(1))
        {
            LockCursor();
        }

        // Unlock it
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }
}

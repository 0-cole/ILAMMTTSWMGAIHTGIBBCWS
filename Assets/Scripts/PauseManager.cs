using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private CursorManager cursorManager;

    public static bool IsGamePaused = false;

    void Start()
    {
        if (cursorManager == null)
            cursorManager = FindFirstObjectByType<CursorManager>();

        // Ensure menu is closed on start
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
            
        Resume(); // Ensure time is running
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
        
        if (cursorManager != null) cursorManager.LockCursor();
    }

    public void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsGamePaused = true;
        
        if (cursorManager != null) cursorManager.UnlockCursor();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void WipeData()
    {
        PlayerPrefs.DeleteAll();
        Time.timeScale = 1f; // Must be 1 to reload properly
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

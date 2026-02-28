using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseCanvas;    // The entire pause menu canvas (root)
    [SerializeField] private GameObject pauseMenuUI;    // Just the pause buttons panel
    [SerializeField] private CursorManager cursorManager;

    [Header("Settings Integration")]
    [SerializeField] private SettingsMenu settingsMenu;

    public static bool IsGamePaused = false;

    void Start()
    {
        if (cursorManager == null)
            cursorManager = FindFirstObjectByType<CursorManager>();

        // Ensure menu is closed on start
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);
            
        Resume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings are open, close them first
            if (settingsMenu != null && settingsMenu.settingsPanel != null && settingsMenu.settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

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
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsGamePaused = false;
        
        if (cursorManager != null) cursorManager.LockCursor();
    }

    public void Pause()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        // Hide settings if they were somehow left open
        if (settingsMenu != null && settingsMenu.settingsPanel != null)
            settingsMenu.settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;
        IsGamePaused = true;
        
        if (cursorManager != null) cursorManager.UnlockCursor();
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
        {
            // Hide pause buttons, keep canvas active, show settings
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            settingsMenu.Open();
        }
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.Close();
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsGamePaused = false;
        SceneManager.LoadScene("MainMenu");
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

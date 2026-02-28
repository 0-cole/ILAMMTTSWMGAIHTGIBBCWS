using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Menu controller. Handles scene loading, quit, and settings access.
/// Attach to a GameObject in the MainMenu scene.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainPanel;
    public GameObject levelSelectPanel;
    public SettingsMenu settingsMenu;

    void Start()
    {
        // Make sure cursor is visible and unlocked on main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // Ensure GameSettings exists
        if (GameSettings.Instance == null)
        {
            GameObject go = new GameObject("GameSettings");
            go.AddComponent<GameSettings>();
        }

        // Show main panel only if no intro splash is active
        if (mainPanel != null && FindFirstObjectByType<IntroSplash>() == null)
            mainPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    // --- Button Callbacks ---

    public void OnPlayClicked()
    {
        // Show level select (or go directly to GameScene)
        if (levelSelectPanel != null)
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            levelSelectPanel.SetActive(true);
        }
        else
        {
            // No level select panel — go straight to first level
            LoadScene("level1");
        }
    }

    public void OnSettingsClicked()
    {
        if (settingsMenu != null)
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            settingsMenu.Open();
        }
    }

    public void OnSettingsBackClicked()
    {
        if (settingsMenu != null) settingsMenu.Close();
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quitting...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- Level Select ---

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadGameScene() => LoadScene("level1");
    public void LoadLevel2() => LoadScene("level2");
    public void LoadBossScene() => LoadScene("boss");

    public void OnLevelSelectBack()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }
}

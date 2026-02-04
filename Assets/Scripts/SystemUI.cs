using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemUI : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;

    private void Start()
    {
        if (weaponController == null)
            weaponController = FindFirstObjectByType<WeaponController>();
    }

    public void OnQuitButton()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnWipeDataButton()
    {
        if (weaponController != null)
        {
            weaponController.ResetWeapons();
        }
        else
        {
            PlayerPrefs.DeleteAll();
        }
        
        // Reload scene to reflect changes
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

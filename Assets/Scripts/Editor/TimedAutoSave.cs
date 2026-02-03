#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class TimedAutoSave
{
    private static double nextSaveTime;
    private const double saveInterval = 30; // 30 seconds

    static TimedAutoSave()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        // SAFETY: Do NOT save if game is running (prevents scene corruption)
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (EditorApplication.timeSinceStartup >= nextSaveTime)
        {
            SaveAll();
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        }
    }

    private static void SaveAll()
    {
        Debug.Log($"[AutoSave] Saving Project... ({System.DateTime.Now.ToString("HH:mm:ss")})");
        
        // Save Open Scenes
        EditorSceneManager.SaveOpenScenes();
        
        // Save Assets (Prefabs, Materials, etc.)
        AssetDatabase.SaveAssets();
    }
}
#endif

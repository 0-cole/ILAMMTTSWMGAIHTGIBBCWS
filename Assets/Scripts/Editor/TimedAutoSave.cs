#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class TimedAutoSave
{
    private static double nextSaveTime;

    static TimedAutoSave()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + 1.0;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (EditorApplication.timeSinceStartup >= nextSaveTime)
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            nextSaveTime = EditorApplication.timeSinceStartup + 1.0;
        }
    }
}
#endif

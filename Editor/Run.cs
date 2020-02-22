using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Run : Editor
{
    [MenuItem("Scenes/Run Client", false, 0)]
    private static void PlaySampleScene()
    {
        if (SceneManager.GetActiveScene().name != "AlwaysInclude")
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
        //EditorSceneManager.OpenScene("Assets/Scenes/AlwaysInclude.unity");
        PlayerPrefs.SetInt("Run", 0);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Scenes/Run Server", false, 0)]
    private static void PlayServerReadConfig()
    {
        if (SceneManager.GetActiveScene().name != "AlwaysInclude")
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
        //EditorSceneManager.OpenScene("Assets/Scenes/AlwaysInclude.unity");
        PlayerPrefs.SetInt("Run", 1);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Scenes/Open Build Folder", false, 100)]
    private static void OpenMonoBuildFolder()
    {
        Process.Start(@"C:\Reconquer\Revision Mirror\");
    }
}

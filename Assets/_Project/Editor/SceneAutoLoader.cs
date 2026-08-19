#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lattirune.Editor
{
    /// <summary>
    /// Ensures that whenever Play Mode is entered in the Unity Editor,
    /// the authoritative startup scene (Assets/_Project/Scenes/Bootstrap.unity)
    /// is always executed, preventing blank/untitled scene startup.
    /// Also provides a menu item to quickly load the Bootstrap scene.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneAutoLoader
    {
        public const string BOOTSTRAP_SCENE_PATH = "Assets/_Project/Scenes/Bootstrap.unity";

        static SceneAutoLoader()
        {
            SetPlayModeStartScene();
            EditorApplication.delayCall += EnsureBootstrapSceneOpened;
        }

        [MenuItem("Lattirune/Scenes/Open Bootstrap Scene")]
        public static void OpenBootstrapScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(BOOTSTRAP_SCENE_PATH, OpenSceneMode.Single);
            }
        }

        private static void SetPlayModeStartScene()
        {
            SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BOOTSTRAP_SCENE_PATH);
            if (bootstrapScene != null)
            {
                EditorSceneManager.playModeStartScene = bootstrapScene;
            }
        }

        private static void EnsureBootstrapSceneOpened()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path) || !activeScene.path.EndsWith("Bootstrap.unity"))
            {
                if (System.IO.File.Exists(BOOTSTRAP_SCENE_PATH))
                {
                    EditorSceneManager.OpenScene(BOOTSTRAP_SCENE_PATH, OpenSceneMode.Single);
                }
            }
        }
    }
}
#endif

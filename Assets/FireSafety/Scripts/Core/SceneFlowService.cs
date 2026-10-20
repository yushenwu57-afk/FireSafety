using UnityEngine;
using UnityEngine.SceneManagement;

namespace FireSafety.Core
{
    public sealed class SceneFlowService : MonoBehaviour
    {
        public void RestartCurrentScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (!activeScene.IsValid() || activeScene.buildIndex < 0)
            {
                return;
            }

            // This component is destroyed during reload; refresh from a one-shot callback.
            SceneManager.sceneLoaded -= RefreshSceneEnvironment;
            SceneManager.sceneLoaded += RefreshSceneEnvironment;
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        public void LoadLevel(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Scene is not available in Build Settings: {sceneName}", this);
                return;
            }

            Time.timeScale = 1f;
            SceneManager.sceneLoaded -= RefreshSceneEnvironment;
            SceneManager.sceneLoaded += RefreshSceneEnvironment;
            SceneManager.LoadScene(sceneName);
        }

        private static void RefreshSceneEnvironment(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= RefreshSceneEnvironment;

            // Wait until the reloaded scene's RenderSettings are in effect.
            DynamicGI.UpdateEnvironment();
        }
    }
}

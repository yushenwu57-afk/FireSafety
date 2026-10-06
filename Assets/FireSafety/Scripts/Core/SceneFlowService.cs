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

            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}

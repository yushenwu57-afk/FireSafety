using FireSafety.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class EscapeMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CanvasGroup menuPanel;
        [SerializeField] private Button openMenuButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private SceneFlowService sceneFlowService;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuSceneName = "Menu";

        private bool isMenuOpen;

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleGameStateChanged;
            }

            if (openMenuButton != null)
            {
                // Keep the HUD button clickable without letting Space submit it.
                Navigation navigation = openMenuButton.navigation;
                navigation.mode = Navigation.Mode.None;
                openMenuButton.navigation = navigation;
                if (EventSystem.current != null
                    && EventSystem.current.currentSelectedGameObject == openMenuButton.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

                openMenuButton.onClick.AddListener(OpenMenu);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(CloseMenu);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartCurrentScene);
            }

            isMenuOpen = false;
            SetMenuVisible(false);

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleGameStateChanged;
            }

            if (openMenuButton != null)
            {
                openMenuButton.onClick.RemoveListener(OpenMenu);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(CloseMenu);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartCurrentScene);
            }

            bool shouldRestoreExploration = isMenuOpen
                && gameManager != null
                && gameManager.CurrentState == GameState.Paused;

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }

            isMenuOpen = false;
            SetMenuVisible(false);

            if (shouldRestoreExploration)
            {
                gameManager.ChangeState(GameState.Exploration);
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            if (isMenuOpen
                || gameManager == null
                || gameManager.CurrentState != GameState.Exploration)
            {
                return;
            }

            isMenuOpen = true;
            gameManager.ChangeState(GameState.Paused);
            SetMenuVisible(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void CloseMenu()
        {
            if (!isMenuOpen)
            {
                return;
            }

            isMenuOpen = false;
            SetMenuVisible(false);

            if (gameManager != null
                && gameManager.CurrentState == GameState.Paused)
            {
                gameManager.ChangeState(GameState.Exploration);
            }
        }

        private void RestartCurrentScene()
        {
            if (sceneFlowService != null)
            {
                sceneFlowService.RestartCurrentScene();
            }
        }

        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void HandleGameStateChanged(
            GameState previousState,
            GameState newState)
        {
            if (!isMenuOpen || newState == GameState.Paused)
            {
                return;
            }

            isMenuOpen = false;
            SetMenuVisible(false);
        }

        private void SetMenuVisible(bool isVisible)
        {
            if (menuPanel == null)
            {
                return;
            }

            menuPanel.alpha = isVisible ? 1f : 0f;
            menuPanel.interactable = isVisible;
            menuPanel.blocksRaycasts = isVisible;
        }
    }
}

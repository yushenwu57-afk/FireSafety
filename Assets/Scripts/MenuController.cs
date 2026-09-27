using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button closeInstructionsButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject instructionsPanel;

    [Header("Scenes")]
    [SerializeField] private string firstLevelSceneName = "Level_1";

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (instructionsButton != null)
        {
            instructionsButton.onClick.AddListener(ShowInstructions);
        }

        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.AddListener(HideInstructions);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        HideInstructions();
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }

        if (instructionsButton != null)
        {
            instructionsButton.onClick.RemoveListener(ShowInstructions);
        }

        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.RemoveListener(HideInstructions);
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    private void ShowInstructions()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }

    private void HideInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }
    }
}

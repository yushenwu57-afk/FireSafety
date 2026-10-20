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
    [SerializeField] private string firstLevelSceneName = "Level";
    [SerializeField] private string secondLevelSceneName = "Level_2";

    private GameObject levelSelectionPanel;

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(ShowLevelSelection);
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
            startButton.onClick.RemoveListener(ShowLevelSelection);
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

    private void ShowLevelSelection()
    {
        if (levelSelectionPanel == null)
        {
            levelSelectionPanel = new GameObject("LevelSelectionPanel", typeof(RectTransform));
            RectTransform panel = levelSelectionPanel.GetComponent<RectTransform>();
            panel.SetParent(mainPanel.transform, false);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = panel.offsetMax = Vector2.zero;

            CreateLevelButton("Level1Button", "Level 1", new Vector2(0, -128), StartLevelOne);
            CreateLevelButton("Level2Button", "Level 2", new Vector2(0, -214), StartLevelTwo);
            Button back = Instantiate(instructionsButton, panel);
            back.name = "BackToMainMenuButton";
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -300);
            back.GetComponentInChildren<Text>(true).text = "Back";
            back.onClick = new Button.ButtonClickedEvent();
            back.onClick.AddListener(HideLevelSelection);
        }
        startButton.gameObject.SetActive(false);
        instructionsButton.gameObject.SetActive(false);
        levelSelectionPanel.SetActive(true);
    }

    private void CreateLevelButton(string name, string label, Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Button button = Instantiate(startButton, levelSelectionPanel.transform);
        button.name = name;
        button.GetComponent<RectTransform>().anchoredPosition = position;
        button.GetComponentInChildren<Text>(true).text = label;
        // Use a white graphic so the selectable's tint produces a clear red.
        button.GetComponent<Image>().color = Color.white;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(.8f, .12f, .12f, 1);
        colors.highlightedColor = new Color(.95f, .22f, .22f, 1);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(.6f, .06f, .06f, 1);
        button.colors = colors;
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(action);
    }

    private void HideLevelSelection()
    {
        levelSelectionPanel.SetActive(false);
        startButton.gameObject.SetActive(true);
        instructionsButton.gameObject.SetActive(true);
    }

    private void StartLevelOne()
    {
        LoadLevel(firstLevelSceneName);
    }

    private void StartLevelTwo()
    {
        LoadLevel(secondLevelSceneName);
    }

    private void LoadLevel(string sceneName)
    {
        // The menu is destroyed during the load, so use a static, one-shot callback.
        SceneManager.sceneLoaded -= RefreshLevelEnvironment;
        SceneManager.sceneLoaded += RefreshLevelEnvironment;
        SceneManager.LoadScene(sceneName);
    }

    private static void RefreshLevelEnvironment(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= RefreshLevelEnvironment;

        // Refresh after the destination scene's RenderSettings have been loaded.
        DynamicGI.UpdateEnvironment();
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

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text dangerLevelText;
    [SerializeField] private string levelDisplayName = "";
    [SerializeField] private string dangerLevel = "Low";

    [Header("Menu")]
    [SerializeField] private Button escMenuButton;
    [SerializeField] private GameObject escMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "Menu";

    [Header("Context Prompt")]
    [SerializeField] private GameObject contextPromptPanel;
    [SerializeField] private Text contextPromptText;

    [Header("Context Actions")]
    [SerializeField] private GameObject contextActionPanel;
    [SerializeField] private Text contextActionTitleText;
    [SerializeField] private Button[] contextActionButtons;
    [SerializeField] private Text[] contextActionButtonTexts;

    [Header("Reticle")]
    [SerializeField] private GameObject reticle;

    public static GameHud ActiveHud { get; private set; }
    public static bool IsMenuOpen { get; private set; }
    public static bool IsActionPanelOpen { get; private set; }

    private ActionContext currentActionContext;

    private void Awake()
    {
        ActiveHud = this;
    }

    private void OnEnable()
    {
        if (escMenuButton != null)
        {
            escMenuButton.onClick.AddListener(ToggleEscMenu);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartCurrentScene);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        RegisterActionButtonListeners();
    }

    private void Start()
    {
        EnsureReticleExists();
        ApplyLabels();
        SetEscMenuOpen(false);
        HideContextPrompt();
        HideActionPanel();
        UpdateReticleVisibility();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsActionPanelOpen)
            {
                HideActionPanel();
                HideContextPrompt();
                return;
            }

            ToggleEscMenu();
        }
    }

    private void OnDisable()
    {
        if (escMenuButton != null)
        {
            escMenuButton.onClick.RemoveListener(ToggleEscMenu);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartCurrentScene);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }

        UnregisterActionButtonListeners();

        if (IsMenuOpen)
        {
            Time.timeScale = 1f;
            IsMenuOpen = false;
        }

        IsActionPanelOpen = false;

        if (ActiveHud == this)
        {
            ActiveHud = null;
        }
    }

    private void ApplyLabels()
    {
        if (levelText != null)
        {
            levelText.text = string.IsNullOrWhiteSpace(levelDisplayName)
                ? SceneManager.GetActiveScene().name
                : levelDisplayName;
        }

        if (dangerLevelText != null)
        {
            dangerLevelText.text = "Danger: " + dangerLevel;
        }
    }

    private void ToggleEscMenu()
    {
        SetEscMenuOpen(!IsMenuOpen);
    }

    private void SetEscMenuOpen(bool open)
    {
        IsMenuOpen = open;

        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(open);
        }

        if (open)
        {
            HideContextPrompt();
            HideActionPanel();
        }

        Time.timeScale = open ? 0f : 1f;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
        UpdateReticleVisibility();
    }

    public void ShowContextPrompt(ObservableContext context)
    {
        if (context == null || string.IsNullOrWhiteSpace(context.PromptText) || IsMenuOpen)
        {
            HideContextPrompt();
            return;
        }

        if (contextPromptText != null)
        {
            contextPromptText.text = context.PromptText;
        }

        if (contextPromptPanel != null)
        {
            contextPromptPanel.SetActive(true);
        }
    }

    public void HideContextPrompt()
    {
        if (contextPromptPanel != null)
        {
            contextPromptPanel.SetActive(false);
        }

        if (contextPromptText != null)
        {
            contextPromptText.text = "";
        }
    }

    public void ShowActionPanel(ActionContext actionContext)
    {
        currentActionContext = actionContext;

        if (actionContext == null || actionContext.OptionCount == 0 || IsMenuOpen)
        {
            HideActionPanel();
            return;
        }

        if (contextActionTitleText != null)
        {
            contextActionTitleText.text = "Available Actions";
        }

        int buttonCount = contextActionButtons == null ? 0 : contextActionButtons.Length;
        for (int i = 0; i < buttonCount; i++)
        {
            ContextActionOption option = actionContext.GetOption(i);
            bool hasOption = option != null;

            contextActionButtons[i].gameObject.SetActive(hasOption);
            if (hasOption && contextActionButtonTexts != null && i < contextActionButtonTexts.Length && contextActionButtonTexts[i] != null)
            {
                contextActionButtonTexts[i].text = option.Label;
            }
        }

        if (contextActionPanel != null)
        {
            contextActionPanel.SetActive(true);
        }

        IsActionPanelOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateReticleVisibility();
    }

    public void HideActionPanel()
    {
        currentActionContext = null;
        IsActionPanelOpen = false;

        if (contextActionPanel != null)
        {
            contextActionPanel.SetActive(false);
        }

        if (!IsMenuOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UpdateReticleVisibility();
    }

    private void UpdateReticleVisibility()
    {
        if (reticle != null)
        {
            reticle.SetActive(!IsMenuOpen && !IsActionPanelOpen);
        }
    }

    private void EnsureReticleExists()
    {
        if (reticle != null)
        {
            EnsureReticleParts(reticle.transform);
            return;
        }

        GameObject reticleObject = new GameObject("Reticle");
        reticleObject.transform.SetParent(transform, false);

        ReticleGraphic graphic = reticleObject.AddComponent<ReticleGraphic>();
        graphic.color = new Color(1f, 1f, 1f, 0.95f);
        graphic.raycastTarget = false;

        Shadow shadow = reticleObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        RectTransform rectTransform = reticleObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(36f, 36f);

        reticle = reticleObject;
        EnsureReticleParts(reticle.transform);
    }

    private void EnsureReticleParts(Transform parent)
    {
        RemoveReticlePart(parent, "CenterDot");
        RemoveReticlePart(parent, "CenterDotShadow");
        RemoveReticlePart(parent, "TopLine");
        RemoveReticlePart(parent, "BottomLine");
        RemoveReticlePart(parent, "LeftLine");
        RemoveReticlePart(parent, "RightLine");

        const int segmentCount = 32;
        const float radius = 24f;
        Color color = new Color(1f, 1f, 1f, 0.95f);
        Color shadowColor = new Color(0f, 0f, 0f, 0.8f);

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / segmentCount;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 position = direction * radius;
            float degrees = angle * Mathf.Rad2Deg;

            CreateReticlePart(parent, "CircleShadow" + i, position + new Vector2(1.5f, -1.5f), new Vector2(6f, 3f), degrees, shadowColor);
            CreateReticlePart(parent, "CircleSegment" + i, position, new Vector2(6f, 3f), degrees, color);
        }
    }

    private void CreateReticlePart(Transform parent, string name, Vector2 position, Vector2 size, float rotation, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject part = existing == null ? new GameObject(name) : existing.gameObject;
        part.transform.SetParent(parent, false);

        Image image = part.GetComponent<Image>();
        if (image == null)
        {
            image = part.AddComponent<Image>();
        }

        image.color = color;
        image.raycastTarget = false;

        RectTransform rectTransform = part.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private void RemoveReticlePart(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            Destroy(child.gameObject);
        }
    }

    private void RegisterActionButtonListeners()
    {
        if (contextActionButtons == null)
        {
            return;
        }

        for (int i = 0; i < contextActionButtons.Length; i++)
        {
            int optionIndex = i;
            Button button = contextActionButtons[i];
            if (button != null)
            {
                button.onClick.AddListener(() => SelectAction(optionIndex));
            }
        }
    }

    private void UnregisterActionButtonListeners()
    {
        if (contextActionButtons == null)
        {
            return;
        }

        for (int i = 0; i < contextActionButtons.Length; i++)
        {
            Button button = contextActionButtons[i];
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }

    private void SelectAction(int optionIndex)
    {
        if (currentActionContext == null)
        {
            return;
        }

        string resultText = currentActionContext.PerformOption(optionIndex);
        if (!string.IsNullOrWhiteSpace(resultText) && contextPromptText != null)
        {
            contextPromptText.text = resultText;
            if (contextPromptPanel != null)
            {
                contextPromptPanel.SetActive(true);
            }
        }

        HideActionPanel();
    }

    private void ResumeGame()
    {
        SetEscMenuOpen(false);
    }

    private void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

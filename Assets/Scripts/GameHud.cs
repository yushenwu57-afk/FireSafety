using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    private const float PanFollowUpDelaySeconds = 3f;
    private static readonly Vector2 CompactPromptPanelSize = new Vector2(960f, 128f);
    private static readonly Vector2 CompactPromptTextSize = new Vector2(880f, 92f);
    private static readonly Vector2 ResultPromptPanelSize = new Vector2(960f, 560f);
    private static readonly Vector2 ResultPromptTextSize = new Vector2(900f, 500f);

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
    [SerializeField] private Texture2D reticleTexture;

    public static GameHud ActiveHud { get; private set; }
    public static bool IsMenuOpen { get; private set; }
    public static bool IsActionPanelOpen { get; private set; }

    private ActionContext currentActionContext;
    private PanSmokeResponse currentPanFollowUp;
    private PanSmokeResponse delayedPanFollowUpTarget;
    private Coroutine delayedPanFollowUpRoutine;

    private void Awake()
    {
        ActiveHud = this;
        transform.localScale = Vector3.one;
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
        StopDelayedPanFollowUp();

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
                ? GetSceneDisplayName()
                : levelDisplayName;
        }

        if (dangerLevelText != null)
        {
            dangerLevelText.text = "Danger: " + dangerLevel;
        }
    }

    public void SetDangerLevel(string newDangerLevel)
    {
        if (string.IsNullOrWhiteSpace(newDangerLevel))
        {
            return;
        }

        dangerLevel = newDangerLevel;

        if (dangerLevelText != null)
        {
            dangerLevelText.text = "Danger: " + dangerLevel;
        }
    }

    private string GetSceneDisplayName()
    {
        return SceneManager.GetActiveScene().name.Replace('_', ' ');
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
            ApplyPromptLayout(false);
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
        currentPanFollowUp = null;

        if (actionContext == null || actionContext.OptionCount == 0 || IsMenuOpen)
        {
            HideActionPanel();
            return;
        }

        PanSmokeResponse panResponse = actionContext.GetComponent<PanSmokeResponse>();
        if (panResponse != null)
        {
            if (panResponse.HasPendingFollowUp)
            {
                if (delayedPanFollowUpRoutine != null && delayedPanFollowUpTarget == panResponse)
                {
                    return;
                }

                ShowPanFollowUpPanel(panResponse);
                return;
            }

            if (panResponse.IsComplete)
            {
                DisplayResultText(panResponse.FinalResultText);
                HideActionPanel();
                return;
            }
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

        ResizeActionPanel(actionContext.OptionCount);

        if (contextActionPanel != null)
        {
            contextActionPanel.SetActive(true);
        }

        IsActionPanelOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateReticleVisibility();
    }

    private void ResizeActionPanel(int optionCount)
    {
        if (contextActionPanel == null)
        {
            return;
        }

        RectTransform panelTransform = contextActionPanel.GetComponent<RectTransform>();
        if (panelTransform == null)
        {
            return;
        }

        float panelHeight = 128f + Mathf.Max(1, optionCount) * 86f;
        panelTransform.sizeDelta = new Vector2(760f, panelHeight);
    }

    public void HideActionPanel()
    {
        currentActionContext = null;
        currentPanFollowUp = null;
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
            ConfigureReticleImage(reticle);
            return;
        }

        GameObject reticleObject = new GameObject("Reticle");
        reticleObject.transform.SetParent(transform, false);

        reticle = reticleObject;
        ConfigureReticleImage(reticle);
    }

    private void ConfigureReticleImage(GameObject reticleObject)
    {
        RawImage image = reticleObject.GetComponent<RawImage>();
        if (image == null)
        {
            image = reticleObject.AddComponent<RawImage>();
        }

        image.texture = reticleTexture;
        image.color = Color.white;
        image.raycastTarget = false;

        RectTransform rectTransform = reticleObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localScale = Vector3.one;
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
        if (currentPanFollowUp != null)
        {
            string followUpResultText = currentPanFollowUp.PerformFollowUpOption(optionIndex);
            DisplayResultText(followUpResultText);
            HideActionPanel();
            return;
        }

        if (currentActionContext == null)
        {
            return;
        }

        PanSmokeResponse smokeResponse = currentActionContext.GetComponent<PanSmokeResponse>();
        bool hasPanFollowUp = smokeResponse != null && (optionIndex == 0 || optionIndex == 1);
        if (hasPanFollowUp)
        {
            smokeResponse.BeginInitialChoice(optionIndex);
        }

        string resultText = currentActionContext.PerformOption(optionIndex);
        DisplayResultText(resultText);

        if (hasPanFollowUp && smokeResponse.HasPendingFollowUp)
        {
            StartDelayedPanFollowUp(smokeResponse);
            return;
        }

        HideActionPanel();
    }

    private void ShowPanFollowUpPanel(PanSmokeResponse panResponse)
    {
        delayedPanFollowUpRoutine = null;
        delayedPanFollowUpTarget = null;
        currentActionContext = null;
        currentPanFollowUp = panResponse;

        if (panResponse == null || !panResponse.HasPendingFollowUp || IsMenuOpen)
        {
            HideActionPanel();
            return;
        }

        if (contextActionTitleText != null)
        {
            contextActionTitleText.text = panResponse.GetFollowUpTitle();
        }

        int optionCount = panResponse.GetFollowUpOptionCount();
        int buttonCount = contextActionButtons == null ? 0 : contextActionButtons.Length;
        for (int i = 0; i < buttonCount; i++)
        {
            bool hasOption = i < optionCount;
            contextActionButtons[i].gameObject.SetActive(hasOption);
            if (hasOption && contextActionButtonTexts != null && i < contextActionButtonTexts.Length && contextActionButtonTexts[i] != null)
            {
                contextActionButtonTexts[i].text = panResponse.GetFollowUpOptionLabel(i);
            }
        }

        ResizeActionPanel(optionCount);

        if (contextActionPanel != null)
        {
            contextActionPanel.SetActive(true);
        }

        IsActionPanelOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateReticleVisibility();
    }

    private void StartDelayedPanFollowUp(PanSmokeResponse panResponse)
    {
        HideActionPanel();
        StopDelayedPanFollowUp();

        delayedPanFollowUpTarget = panResponse;
        delayedPanFollowUpRoutine = StartCoroutine(ShowPanFollowUpAfterDelay(panResponse));
    }

    private IEnumerator ShowPanFollowUpAfterDelay(PanSmokeResponse panResponse)
    {
        yield return new WaitForSeconds(PanFollowUpDelaySeconds);

        while (IsMenuOpen)
        {
            yield return null;
        }

        if (panResponse != null && panResponse.HasPendingFollowUp)
        {
            ShowPanFollowUpPanel(panResponse);
        }
        else
        {
            delayedPanFollowUpRoutine = null;
            delayedPanFollowUpTarget = null;
        }
    }

    private void StopDelayedPanFollowUp()
    {
        if (delayedPanFollowUpRoutine != null)
        {
            StopCoroutine(delayedPanFollowUpRoutine);
            delayedPanFollowUpRoutine = null;
        }

        delayedPanFollowUpTarget = null;
    }

    private void DisplayResultText(string resultText)
    {
        if (string.IsNullOrWhiteSpace(resultText) || contextPromptText == null)
        {
            return;
        }

        ApplyPromptLayout(resultText.Contains("\n"));
        contextPromptText.text = resultText;
        if (contextPromptPanel != null)
        {
            contextPromptPanel.SetActive(true);
        }
    }

    private void ApplyPromptLayout(bool showLargeResult)
    {
        if (contextPromptPanel != null)
        {
            RectTransform panelTransform = contextPromptPanel.GetComponent<RectTransform>();
            if (panelTransform != null)
            {
                panelTransform.sizeDelta = showLargeResult ? ResultPromptPanelSize : CompactPromptPanelSize;
            }
        }

        if (contextPromptText != null)
        {
            RectTransform textTransform = contextPromptText.GetComponent<RectTransform>();
            if (textTransform != null)
            {
                textTransform.sizeDelta = showLargeResult ? ResultPromptTextSize : CompactPromptTextSize;
            }

            contextPromptText.alignment = showLargeResult ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
            contextPromptText.verticalOverflow = showLargeResult ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
        }
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

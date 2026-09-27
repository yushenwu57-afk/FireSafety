using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MenuSceneBuilder
{
    private const string MenuScenePath = "Assets/Scenes/Menu.unity";
    private const string FirstLevelSceneName = "Level_1";

    [MenuItem("Tools/Menu/Rebuild Main Menu")]
    public static void BuildMenu()
    {
        EditorSceneManager.OpenScene(MenuScenePath);

        Canvas canvas = FindOrCreateCanvas();
        EnsureEventSystemExists();

        GameObject background = CreateBackground(canvas.transform);
        GameObject mainPanel = CreateMainPanel(background.transform);
        GameObject instructionsPanel = CreateInstructionsPanel(background.transform);

        Button startButton = CreateButton(
            mainPanel.transform,
            "StartButton",
            "Start",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -128f),
            new Vector2(300f, 68f),
            new Color(0.8f, 0.22f, 0.08f, 0.95f));

        Button instructionsButton = CreateButton(
            mainPanel.transform,
            "InstructionsButton",
            "Instructions",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -214f),
            new Vector2(300f, 62f),
            new Color(0.1f, 0.13f, 0.16f, 0.92f));

        Button closeInstructionsButton = CreateButton(
            instructionsPanel.transform,
            "BackButton",
            "Back",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 36f),
            new Vector2(240f, 58f),
            new Color(0.8f, 0.22f, 0.08f, 0.95f));

        CreateText(
            mainPanel.transform,
            "TitleText",
            "FireWise",
            TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 118f),
            new Vector2(720f, 120f),
            78,
            new Color(1f, 0.88f, 0.62f, 1f));

        CreateText(
            mainPanel.transform,
            "SubtitleText",
            "Learn how to spot fire risks and choose safer actions.",
            TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 36f),
            new Vector2(820f, 56f),
            28,
            Color.white);

        CreateText(
            instructionsPanel.transform,
            "InstructionsTitleText",
            "Instructions",
            TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -42f),
            new Vector2(600f, 64f),
            44,
            new Color(1f, 0.88f, 0.62f, 1f));

        Text instructionsText = CreateText(
            instructionsPanel.transform,
            "InstructionsBodyText",
            "Move with WASD.\nLook around with the mouse.\nHold Shift to sprint and press Space to jump.\nClick highlighted objects to inspect fire-safety clues.\nPress E to interact with doors and hazards.\nPress ESC during the level to pause.",
            TextAnchor.UpperLeft,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -128f),
            new Vector2(740f, 330f),
            30,
            Color.white);
        instructionsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        instructionsText.verticalOverflow = VerticalWrapMode.Truncate;

        MenuController menuController = canvas.GetComponent<MenuController>();
        if (menuController == null)
        {
            menuController = canvas.gameObject.AddComponent<MenuController>();
        }

        SerializedObject serializedMenu = new SerializedObject(menuController);
        serializedMenu.FindProperty("startButton").objectReferenceValue = startButton;
        serializedMenu.FindProperty("instructionsButton").objectReferenceValue = instructionsButton;
        serializedMenu.FindProperty("closeInstructionsButton").objectReferenceValue = closeInstructionsButton;
        serializedMenu.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        serializedMenu.FindProperty("instructionsPanel").objectReferenceValue = instructionsPanel;
        serializedMenu.FindProperty("firstLevelSceneName").stringValue = FirstLevelSceneName;
        serializedMenu.ApplyModifiedPropertiesWithoutUndo();

        mainPanel.SetActive(true);
        instructionsPanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }

    private static Canvas FindOrCreateCanvas()
    {
        GameObject canvasObject = GameObject.Find("MenuCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("MenuCanvas");
        }

        canvasObject.transform.localScale = Vector3.one;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    private static void EnsureEventSystemExists()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreateBackground(Transform parent)
    {
        GameObject background = FindOrCreateChild(parent, "MenuBackground");
        Image image = background.GetComponent<Image>();
        if (image == null)
        {
            image = background.AddComponent<Image>();
        }

        image.color = new Color(0.05f, 0.07f, 0.08f, 1f);

        RectTransform rectTransform = background.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        return background;
    }

    private static GameObject CreateMainPanel(Transform parent)
    {
        GameObject panel = FindOrCreateChild(parent, "MainPanel");
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        return panel;
    }

    private static GameObject CreateInstructionsPanel(Transform parent)
    {
        GameObject panel = FindOrCreateChild(parent, "InstructionsPanel");

        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = panel.AddComponent<Image>();
        }

        image.color = new Color(0f, 0f, 0f, 0.45f);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(860f, 620f);

        return panel;
    }

    private static Text CreateText(
        Transform parent,
        string name,
        string value,
        TextAnchor alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        Color color)
    {
        GameObject textObject = FindOrCreateChild(parent, name);
        Text text = textObject.GetComponent<Text>();
        if (text == null)
        {
            text = textObject.AddComponent<Text>();
        }

        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        return text;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color normalColor)
    {
        GameObject buttonObject = FindOrCreateChild(parent, name);
        Image image = buttonObject.GetComponent<Image>();
        if (image == null)
        {
            image = buttonObject.AddComponent<Image>();
        }

        image.color = normalColor;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.24f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        RectTransform buttonTransform = button.GetComponent<RectTransform>();
        buttonTransform.anchorMin = anchorMin;
        buttonTransform.anchorMax = anchorMax;
        buttonTransform.pivot = pivot;
        buttonTransform.anchoredPosition = anchoredPosition;
        buttonTransform.sizeDelta = sizeDelta;

        Text text = CreateText(
            buttonObject.transform,
            name + "Text",
            label,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            28,
            Color.white);
        text.raycastTarget = false;

        return button;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child.gameObject;
        }

        GameObject childObject = new GameObject(name);
        childObject.transform.SetParent(parent, false);
        return childObject;
    }
}

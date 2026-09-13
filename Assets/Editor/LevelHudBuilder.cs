using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LevelHudBuilder
{
    private const string LevelScenePath = "Assets/Scenes/Level_1.unity";

    [MenuItem("Tools/HUD/Rebuild Level 1 HUD")]
    public static void BuildLevel1Hud()
    {
        EditorSceneManager.OpenScene(LevelScenePath);

        Canvas canvas = FindOrCreateCanvas();
        EnsureEventSystemExists();

        Text levelText = CreateText(
            canvas.transform,
            "LevelText",
            "Level_1",
            TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -24f),
            new Vector2(640f, 60f),
            34);

        Text dangerLevelText = CreateText(
            canvas.transform,
            "DangerLevelText",
            "Danger: Low",
            TextAnchor.UpperRight,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-24f, -24f),
            new Vector2(420f, 60f),
            30);

        Button escButton = CreateButton(
            canvas.transform,
            "EscMenuButton",
            "ESC",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -24f),
            new Vector2(120f, 52f));

        GameObject reticle = CreateReticle(canvas.transform);

        GameObject escMenuPanel = CreatePanel(canvas.transform);

        Button resumeButton = CreateButton(
            escMenuPanel.transform,
            "ResumeButton",
            "Resume Game",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -72f),
            new Vector2(260f, 56f));

        Button restartButton = CreateButton(
            escMenuPanel.transform,
            "RestartButton",
            "Restart Scene",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -144f),
            new Vector2(260f, 56f));

        Button mainMenuButton = CreateButton(
            escMenuPanel.transform,
            "MainMenuButton",
            "Main Menu",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -216f),
            new Vector2(260f, 56f));

        GameObject contextPromptPanel = CreateContextPromptPanel(canvas.transform);
        Text contextPromptText = CreateText(
            contextPromptPanel.transform,
            "ContextPromptText",
            "",
            TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(880f, 92f),
            24);
        contextPromptText.horizontalOverflow = HorizontalWrapMode.Wrap;
        contextPromptText.verticalOverflow = VerticalWrapMode.Truncate;

        GameObject contextActionPanel = CreateContextActionPanel(canvas.transform);
        Text contextActionTitleText = CreateText(
            contextActionPanel.transform,
            "ContextActionTitleText",
            "Available Actions",
            TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -18f),
            new Vector2(400f, 44f),
            26);

        Button[] actionButtons = new Button[4];
        Text[] actionButtonTexts = new Text[4];
        for (int i = 0; i < actionButtons.Length; i++)
        {
            actionButtons[i] = CreateButton(
                contextActionPanel.transform,
                "ContextActionButton" + (i + 1),
                "Action",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -72f - i * 62f),
                new Vector2(360f, 48f));
            actionButtonTexts[i] = actionButtons[i].GetComponentInChildren<Text>();
        }

        GameHud gameHud = canvas.GetComponent<GameHud>();
        if (gameHud == null)
        {
            gameHud = canvas.gameObject.AddComponent<GameHud>();
        }

        SerializedObject serializedHud = new SerializedObject(gameHud);
        serializedHud.FindProperty("levelText").objectReferenceValue = levelText;
        serializedHud.FindProperty("dangerLevelText").objectReferenceValue = dangerLevelText;
        serializedHud.FindProperty("levelDisplayName").stringValue = "Level_1";
        serializedHud.FindProperty("dangerLevel").stringValue = "Low";
        serializedHud.FindProperty("escMenuButton").objectReferenceValue = escButton;
        serializedHud.FindProperty("escMenuPanel").objectReferenceValue = escMenuPanel;
        serializedHud.FindProperty("resumeButton").objectReferenceValue = resumeButton;
        serializedHud.FindProperty("restartButton").objectReferenceValue = restartButton;
        serializedHud.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
        serializedHud.FindProperty("mainMenuSceneName").stringValue = "Menu";
        serializedHud.FindProperty("contextPromptPanel").objectReferenceValue = contextPromptPanel;
        serializedHud.FindProperty("contextPromptText").objectReferenceValue = contextPromptText;
        serializedHud.FindProperty("contextActionPanel").objectReferenceValue = contextActionPanel;
        serializedHud.FindProperty("contextActionTitleText").objectReferenceValue = contextActionTitleText;
        SetObjectReferenceArray(serializedHud.FindProperty("contextActionButtons"), actionButtons);
        SetObjectReferenceArray(serializedHud.FindProperty("contextActionButtonTexts"), actionButtonTexts);
        serializedHud.FindProperty("reticle").objectReferenceValue = reticle;
        serializedHud.ApplyModifiedPropertiesWithoutUndo();

        escMenuPanel.SetActive(false);
        contextPromptPanel.SetActive(false);
        contextActionPanel.SetActive(false);
        ConfigureKettleContext();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }

    private static Canvas FindOrCreateCanvas()
    {
        GameObject canvasObject = GameObject.Find("GameHUDCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("GameHUDCanvas");
        }

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
        int fontSize)
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
        text.color = Color.white;
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
        Vector2 sizeDelta)
    {
        GameObject buttonObject = FindOrCreateChild(parent, name);
        Image image = buttonObject.GetComponent<Image>();
        if (image == null)
        {
            image = buttonObject.AddComponent<Image>();
        }

        image.color = new Color(0.08f, 0.1f, 0.12f, 0.9f);

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.08f, 0.1f, 0.12f, 0.9f);
        colors.highlightedColor = new Color(0.18f, 0.22f, 0.26f, 0.95f);
        colors.pressedColor = new Color(0.02f, 0.04f, 0.06f, 1f);
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
            26);
        text.raycastTarget = false;

        return button;
    }

    private static GameObject CreateReticle(Transform parent)
    {
        GameObject reticle = FindOrCreateChild(parent, "Reticle");

        ReticleGraphic graphic = reticle.GetComponent<ReticleGraphic>();
        if (graphic == null)
        {
            graphic = reticle.AddComponent<ReticleGraphic>();
        }

        graphic.color = new Color(1f, 1f, 1f, 0.86f);
        graphic.raycastTarget = false;

        SerializedObject serializedReticle = new SerializedObject(graphic);
        serializedReticle.FindProperty("radius").floatValue = 14f;
        serializedReticle.FindProperty("thickness").floatValue = 3f;
        serializedReticle.FindProperty("segments").intValue = 64;
        serializedReticle.ApplyModifiedPropertiesWithoutUndo();

        Shadow shadow = reticle.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = reticle.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        RectTransform rectTransform = reticle.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(64f, 64f);

        RemoveReticlePart(reticle.transform, "CenterDot");
        RemoveReticlePart(reticle.transform, "CenterDotShadow");
        RemoveReticlePart(reticle.transform, "TopLine");
        RemoveReticlePart(reticle.transform, "BottomLine");
        RemoveReticlePart(reticle.transform, "LeftLine");
        RemoveReticlePart(reticle.transform, "RightLine");

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

            CreateReticlePart(reticle.transform, "CircleShadow" + i, position + new Vector2(1.5f, -1.5f), new Vector2(6f, 3f), degrees, shadowColor);
            CreateReticlePart(reticle.transform, "CircleSegment" + i, position, new Vector2(6f, 3f), degrees, color);
        }

        return reticle;
    }

    private static void CreateReticlePart(Transform parent, string name, Vector2 position, Vector2 size, float rotation, Color color)
    {
        GameObject part = FindOrCreateChild(parent, name);

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

    private static void RemoveReticlePart(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = FindOrCreateChild(parent, "EscMenuPanel");

        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = panel.AddComponent<Image>();
        }

        image.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(320f, 308f);

        CreateText(
            panel.transform,
            "EscMenuTitle",
            "Paused",
            TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -22f),
            new Vector2(280f, 48f),
            30);

        return panel;
    }

    private static GameObject CreateContextPromptPanel(Transform parent)
    {
        GameObject panel = FindOrCreateChild(parent, "ContextPromptPanel");

        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = panel.AddComponent<Image>();
        }

        image.color = new Color(0f, 0f, 0f, 0.68f);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 96f);
        rectTransform.sizeDelta = new Vector2(960f, 128f);

        return panel;
    }

    private static GameObject CreateContextActionPanel(Transform parent)
    {
        GameObject panel = FindOrCreateChild(parent, "ContextActionPanel");

        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = panel.AddComponent<Image>();
        }

        image.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -36f);
        rectTransform.sizeDelta = new Vector2(440f, 340f);

        return panel;
    }

    private static void ConfigureKettleContext()
    {
        GameObject kettle = GameObject.Find("Kettle");
        if (kettle == null)
        {
            Debug.LogWarning("LevelHudBuilder could not find a GameObject named Kettle in Level_1.");
            return;
        }

        ObservableContext context = kettle.GetComponent<ObservableContext>();
        if (context == null)
        {
            context = kettle.AddComponent<ObservableContext>();
        }

        if (kettle.GetComponent<HighlightableObject>() == null)
        {
            kettle.AddComponent<HighlightableObject>();
        }

        SerializedObject serializedContext = new SerializedObject(context);
        serializedContext.FindProperty("promptText").stringValue =
            "Faint smoke rises around the kettle. The metal surface looks hot, and the surrounding air shimmers slightly.";
        serializedContext.ApplyModifiedPropertiesWithoutUndo();

        ActionContext actionContext = kettle.GetComponent<ActionContext>();
        if (actionContext == null)
        {
            actionContext = kettle.AddComponent<ActionContext>();
        }

        SerializedObject serializedActionContext = new SerializedObject(actionContext);
        SerializedProperty options = serializedActionContext.FindProperty("options");
        options.arraySize = 4;
        SetActionOption(options.GetArrayElementAtIndex(0), "Operate nearby power controls", "You operate the nearby controls. The kettle's immediate activity changes.");
        SetActionOption(options.GetArrayElementAtIndex(1), "Notify people in the room", "You alert nearby people to the visible smoke and heat.");
        SetActionOption(options.GetArrayElementAtIndex(2), "Contact emergency services", "You start contacting emergency services and report the observed smoke and heat.");
        SetActionOption(options.GetArrayElementAtIndex(3), "Begin evacuating the area", "You begin moving people away from the immediate area.");
        serializedActionContext.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetActionOption(SerializedProperty option, string label, string resultText)
    {
        option.FindPropertyRelative("label").stringValue = label;
        option.FindPropertyRelative("resultText").stringValue = resultText;
    }

    private static void SetObjectReferenceArray(SerializedProperty property, Object[] values)
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
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

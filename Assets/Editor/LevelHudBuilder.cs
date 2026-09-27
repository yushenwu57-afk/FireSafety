using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LevelHudBuilder
{
    private const string LevelScenePath = "Assets/Scenes/Level_1.unity";
    private const string ReticleTexturePath = "Assets/circle.png";

    [MenuItem("Tools/HUD/Rebuild Level 1 HUD")]
    public static void BuildLevel1Hud()
    {
        EditorSceneManager.OpenScene(LevelScenePath);

        Canvas canvas = FindOrCreateCanvas();
        EnsureEventSystemExists();

        Text levelText = CreateText(
            canvas.transform,
            "LevelText",
            "Level 1",
            TextAnchor.UpperRight,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-24f, -24f),
            new Vector2(500f, 64f),
            38);

        Text dangerLevelText = CreateText(
            canvas.transform,
            "DangerLevelText",
            "Danger: Low",
            TextAnchor.UpperRight,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-24f, -82f),
            new Vector2(500f, 64f),
            38);

        Button escButton = CreateButton(
            canvas.transform,
            "EscMenuButton",
            "ESC",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -24f),
            new Vector2(160f, 70f),
            32);

        GameObject reticle = CreateReticle(canvas.transform);

        GameObject escMenuPanel = CreatePanel(canvas.transform);

        Button resumeButton = CreateButton(
            escMenuPanel.transform,
            "ResumeButton",
            "Resume Game",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -90f),
            new Vector2(680f, 70f),
            32);

        Button restartButton = CreateButton(
            escMenuPanel.transform,
            "RestartButton",
            "Restart Scene",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -176f),
            new Vector2(680f, 70f),
            32);

        Button mainMenuButton = CreateButton(
            escMenuPanel.transform,
            "MainMenuButton",
            "Main Menu",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -262f),
            new Vector2(680f, 70f),
            32);

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
            new Vector2(0f, -24f),
            new Vector2(700f, 62f),
            34);

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
                new Vector2(0f, -90f - i * 86f),
                new Vector2(680f, 70f),
                32);
            actionButtonTexts[i] = actionButtons[i].GetComponentInChildren<Text>();
        }

        GameHud gameHud = canvas.GetComponent<GameHud>();
        if (gameHud == null)
        {
            gameHud = canvas.gameObject.AddComponent<GameHud>();
        }

        DelayedFireEvent delayedFireEvent = canvas.GetComponent<DelayedFireEvent>();
        if (delayedFireEvent == null)
        {
            delayedFireEvent = canvas.gameObject.AddComponent<DelayedFireEvent>();
        }

        SerializedObject serializedHud = new SerializedObject(gameHud);
        serializedHud.FindProperty("levelText").objectReferenceValue = levelText;
        serializedHud.FindProperty("dangerLevelText").objectReferenceValue = dangerLevelText;
        serializedHud.FindProperty("levelDisplayName").stringValue = "Level 1";
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
        serializedHud.FindProperty("reticleTexture").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(ReticleTexturePath);
        serializedHud.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject serializedFireEvent = new SerializedObject(delayedFireEvent);
        serializedFireEvent.FindProperty("targetEffectName").stringValue = "Particles_Fire_02";
        SerializedProperty additionalTargetEffectNames = serializedFireEvent.FindProperty("additionalTargetEffectNames");
        additionalTargetEffectNames.arraySize = 1;
        additionalTargetEffectNames.GetArrayElementAtIndex(0).stringValue = "Particles_Dust_01";
        serializedFireEvent.FindProperty("delaySeconds").floatValue = 5f;
        serializedFireEvent.FindProperty("dangerLevelAfterDelay").stringValue = "Medium";
        serializedFireEvent.FindProperty("gameHud").objectReferenceValue = gameHud;
        serializedFireEvent.ApplyModifiedPropertiesWithoutUndo();

        escMenuPanel.SetActive(false);
        contextPromptPanel.SetActive(false);
        contextActionPanel.SetActive(false);
        ConfigurePanContext();

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
        Vector2 sizeDelta,
        int fontSize = 26)
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
            fontSize);
        text.raycastTarget = false;

        return button;
    }

    private static GameObject CreateReticle(Transform parent)
    {
        GameObject reticle = FindOrCreateChild(parent, "Reticle");

        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(reticle);

        Shadow oldShadow = reticle.GetComponent<Shadow>();
        if (oldShadow != null)
        {
            Object.DestroyImmediate(oldShadow);
        }

        RawImage image = reticle.GetComponent<RawImage>();
        if (image == null)
        {
            image = reticle.AddComponent<RawImage>();
        }

        image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ReticleTexturePath);
        image.color = Color.white;
        image.raycastTarget = false;

        RectTransform rectTransform = reticle.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = Vector2.zero;

        return reticle;
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
        rectTransform.sizeDelta = new Vector2(760f, 360f);

        CreateText(
            panel.transform,
            "EscMenuTitle",
            "Paused",
            TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -24f),
            new Vector2(700f, 62f),
            34);

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
        rectTransform.anchoredPosition = new Vector2(0f, -28f);
        rectTransform.sizeDelta = new Vector2(760f, 300f);

        return panel;
    }

    private static void ConfigurePanContext()
    {
        GameObject pan = GameObject.Find("Pan_02");
        if (pan == null)
        {
            Debug.LogWarning("LevelHudBuilder could not find a GameObject named Pan_02 in Level_1.");
            return;
        }

        ObservableContext context = pan.GetComponent<ObservableContext>();
        if (context == null)
        {
            context = pan.AddComponent<ObservableContext>();
        }

        if (pan.GetComponent<HighlightableObject>() == null)
        {
            pan.AddComponent<HighlightableObject>();
        }

        if (pan.GetComponent<PanSmokeResponse>() == null)
        {
            pan.AddComponent<PanSmokeResponse>();
        }

        SerializedObject serializedContext = new SerializedObject(context);
        serializedContext.FindProperty("promptText").stringValue =
            "Flames and smoke are rising from the pan. Choose a fire-safety response quickly.";
        serializedContext.ApplyModifiedPropertiesWithoutUndo();

        ActionContext actionContext = pan.GetComponent<ActionContext>();
        if (actionContext == null)
        {
            actionContext = pan.AddComponent<ActionContext>();
        }

        SerializedObject serializedActionContext = new SerializedObject(actionContext);
        SerializedProperty options = serializedActionContext.FindProperty("options");
        options.arraySize = 2;
        SetActionOption(options.GetArrayElementAtIndex(0), "A. Turn off stove", "The heat source is off. Flames remain, but there is less smoke.");
        SetActionOption(options.GetArrayElementAtIndex(1), "B. Pour water", "The fire suddenly expands, and the smoke becomes much thicker.");
        serializedActionContext.ApplyModifiedPropertiesWithoutUndo();

        ProximityActionMenu proximityMenu = pan.GetComponent<ProximityActionMenu>();
        if (proximityMenu == null)
        {
            proximityMenu = pan.AddComponent<ProximityActionMenu>();
        }

        SerializedObject serializedProximityMenu = new SerializedObject(proximityMenu);
        serializedProximityMenu.FindProperty("actionContext").objectReferenceValue = actionContext;
        serializedProximityMenu.FindProperty("triggerDistance").floatValue = 2f;
        serializedProximityMenu.FindProperty("activationDelaySeconds").floatValue = 5f;
        serializedProximityMenu.ApplyModifiedPropertiesWithoutUndo();
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

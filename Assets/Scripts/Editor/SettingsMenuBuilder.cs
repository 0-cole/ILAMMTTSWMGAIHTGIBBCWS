#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script: Tools → Build Pause Menu Settings UI
/// Automatically generates a Settings panel as a child of the currently selected
/// Canvas or Pause Menu UI object. Useful for adding settings into your existing
/// gameplay scenes' pause menus.
/// </summary>
public class SettingsMenuBuilder : Editor
{
    // --- Color Palette (matches MainMenuBuilder) ---
    private static readonly Color panelColor = new Color(0.1f, 0.08f, 0.12f, 0.95f);
    private static readonly Color accentRed = new Color(0.85f, 0.15f, 0.1f, 1f);
    private static readonly Color accentOrange = new Color(1f, 0.5f, 0.1f, 1f);
    private static readonly Color textWhite = new Color(0.95f, 0.92f, 0.88f, 1f);
    private static readonly Color subtleGray = new Color(0.6f, 0.55f, 0.5f, 1f);

    [MenuItem("Tools/Build Settings UI (In Selected Canvas)")]
    public static void BuildSettingsUI()
    {
        // Find parent - use selected object or find any Canvas
        Transform parent = null;
        
        if (Selection.activeGameObject != null)
        {
            Canvas canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                parent = canvas.transform;
            }
            else
            {
                parent = Selection.activeGameObject.transform;
            }
        }

        if (parent == null)
        {
            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                parent = existingCanvas.transform;
            }
        }

        if (parent == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "No Canvas found. Select a Canvas or a child of a Canvas first.", "OK");
            return;
        }

        // Build settings panel
        GameObject settingsPanel = CreatePanel(parent, "SettingsPanel", Vector2.zero, new Vector2(700, 750));
        SettingsMenu settingsMenuComp = settingsPanel.AddComponent<SettingsMenu>();
        settingsMenuComp.settingsPanel = settingsPanel;

        // Title
        CreateText(settingsPanel.transform, "SettingsTitle", "SETTINGS",
            new Vector2(0, 310), new Vector2(400, 50), 28, accentOrange, FontStyles.Bold, TextAlignmentOptions.Center);

        float yPos = 230f;
        float yStep = -70f;

        // Sensitivity
        var sens = CreateSliderRow(settingsPanel.transform, "Sensitivity", yPos);
        settingsMenuComp.sensitivitySlider = sens.slider;
        settingsMenuComp.sensitivityValueText = sens.valueText;
        yPos += yStep;

        // Master Volume
        var master = CreateSliderRow(settingsPanel.transform, "Master Volume", yPos);
        settingsMenuComp.masterVolumeSlider = master.slider;
        settingsMenuComp.masterVolumeValueText = master.valueText;
        yPos += yStep;

        // Music Volume
        var music = CreateSliderRow(settingsPanel.transform, "Music Volume", yPos);
        settingsMenuComp.musicVolumeSlider = music.slider;
        settingsMenuComp.musicVolumeValueText = music.valueText;
        yPos += yStep;

        // SFX Volume
        var sfx = CreateSliderRow(settingsPanel.transform, "SFX Volume", yPos);
        settingsMenuComp.sfxVolumeSlider = sfx.slider;
        settingsMenuComp.sfxVolumeValueText = sfx.valueText;
        yPos += yStep;

        // FOV
        var fov = CreateSliderRow(settingsPanel.transform, "Field of View", yPos);
        settingsMenuComp.fovSlider = fov.slider;
        settingsMenuComp.fovValueText = fov.valueText;
        yPos += yStep;

        // Fullscreen
        var fullscreen = CreateToggleRow(settingsPanel.transform, "Fullscreen", yPos);
        settingsMenuComp.fullscreenToggle = fullscreen;
        yPos += yStep;

        // Quality
        var quality = CreateDropdownRow(settingsPanel.transform, "Quality", yPos);
        settingsMenuComp.qualityDropdown = quality;
        yPos += yStep;

        // Reset
        GameObject resetBtn = CreateButton(settingsPanel.transform, "ResetDefaults", "RESET DEFAULTS",
            new Vector2(0, yPos), new Vector2(250, 45), panelColor, accentOrange, 16);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            resetBtn.GetComponent<Button>().onClick, settingsMenuComp.ResetDefaults);
        yPos += yStep;

        // Back
        GameObject backBtn = CreateButton(settingsPanel.transform, "SettingsBack", "← BACK",
            new Vector2(0, yPos), new Vector2(200, 45), panelColor, subtleGray, 16);

        // Try to wire back to PauseManager
        PauseManager pm = FindFirstObjectByType<PauseManager>();
        if (pm != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                backBtn.GetComponent<Button>().onClick, pm.CloseSettings);
        }

        settingsPanel.SetActive(false);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Settings UI Built! ✅",
            "Settings panel has been added to your Canvas.\n\n" +
            "Wire the PauseManager's 'settingsMenu' field to the new SettingsPanel object.",
            "Got it!");

        Selection.activeGameObject = settingsPanel;
        Debug.Log("[SettingsMenuBuilder] Settings panel created and selected.");
    }

    // ==========================================
    // HELPER METHODS (same style as MainMenuBuilder)
    // ==========================================

    static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = panel.AddComponent<Image>();
        img.color = panelColor;
        panel.AddComponent<CanvasGroup>();
        return panel;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string content,
        Vector2 position, Vector2 size, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        return tmp;
    }

    static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor, Color textColor, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.9f, 0.25f, 0.15f, 1f);
        colors.pressedColor = accentRed;
        btn.colors = colors;
        CreateText(btnObj.transform, name + "_Text", label,
            Vector2.zero, size, fontSize, textColor, FontStyles.Bold, TextAlignmentOptions.Center);
        return btnObj;
    }

    struct SliderRow
    {
        public Slider slider;
        public TextMeshProUGUI valueText;
    }

    static SliderRow CreateSliderRow(Transform parent, string label, float yPos)
    {
        GameObject row = new GameObject(label.Replace(" ", "") + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0, yPos);
        rowRt.sizeDelta = new Vector2(600, 50);

        CreateText(row.transform, label.Replace(" ", "") + "Label", label,
            new Vector2(-200, 0), new Vector2(180, 40), 16, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);

        GameObject sliderObj = CreateDefaultSlider(row.transform, label.Replace(" ", "") + "Slider",
            new Vector2(40, 0), new Vector2(250, 30));
        Slider slider = sliderObj.GetComponent<Slider>();

        TextMeshProUGUI valueText = CreateText(row.transform, label.Replace(" ", "") + "Value", "0",
            new Vector2(220, 0), new Vector2(80, 40), 14, accentOrange, FontStyles.Normal, TextAlignmentOptions.Center);

        return new SliderRow { slider = slider, valueText = valueText };
    }

    static GameObject CreateDefaultSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0; slider.maxValue = 1; slider.value = 0.5f;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.18f, 1f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f); fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.offsetMin = new Vector2(5, 0); fillAreaRt.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one; fillRt.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = accentRed;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero; handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10, 0); handleAreaRt.offsetMax = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = textWhite;

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        return sliderObj;
    }

    static Toggle CreateToggleRow(Transform parent, string label, float yPos)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0, yPos);
        rowRt.sizeDelta = new Vector2(600, 50);

        CreateText(row.transform, label + "Label", label,
            new Vector2(-200, 0), new Vector2(180, 40), 16, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);

        GameObject toggleObj = new GameObject(label + "Toggle");
        toggleObj.transform.SetParent(row.transform, false);
        RectTransform toggleRt = toggleObj.AddComponent<RectTransform>();
        toggleRt.anchorMin = new Vector2(0.5f, 0.5f);
        toggleRt.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRt.anchoredPosition = new Vector2(40, 0);
        toggleRt.sizeDelta = new Vector2(40, 40);

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.12f, 0.18f, 1f);
        toggle.targetGraphic = bgImg;

        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform, false);
        RectTransform checkRt = checkObj.AddComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.1f, 0.1f); checkRt.anchorMax = new Vector2(0.9f, 0.9f);
        checkRt.sizeDelta = Vector2.zero;
        Image checkImg = checkObj.AddComponent<Image>();
        checkImg.color = accentRed;
        toggle.graphic = checkImg;

        return toggle;
    }

    static TMP_Dropdown CreateDropdownRow(Transform parent, string label, float yPos)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0, yPos);
        rowRt.sizeDelta = new Vector2(600, 50);

        CreateText(row.transform, label + "Label", label,
            new Vector2(-200, 0), new Vector2(180, 40), 16, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);

        GameObject dropObj = new GameObject(label + "Dropdown");
        dropObj.transform.SetParent(row.transform, false);
        RectTransform dropRt = dropObj.AddComponent<RectTransform>();
        dropRt.anchorMin = new Vector2(0.5f, 0.5f);
        dropRt.anchorMax = new Vector2(0.5f, 0.5f);
        dropRt.anchoredPosition = new Vector2(40, 0);
        dropRt.sizeDelta = new Vector2(250, 40);

        dropObj.AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.18f, 1f);
        TMP_Dropdown dropdown = dropObj.AddComponent<TMP_Dropdown>();

        TextMeshProUGUI captionText = CreateText(dropObj.transform, "CaptionText", "Select...",
            Vector2.zero, new Vector2(230, 35), 14, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);
        dropdown.captionText = captionText;

        // Template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropObj.transform, false);
        RectTransform tempRt = template.AddComponent<RectTransform>();
        tempRt.anchorMin = new Vector2(0, 0); tempRt.anchorMax = new Vector2(1, 0);
        tempRt.pivot = new Vector2(0.5f, 1f);
        tempRt.anchoredPosition = Vector2.zero; tempRt.sizeDelta = new Vector2(0, 150);
        template.AddComponent<Image>().color = panelColor;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = panelColor;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = Vector2.one;
        contentRt.pivot = new Vector2(0.5f, 1f); contentRt.sizeDelta = new Vector2(0, 28);

        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        RectTransform itemRt = item.AddComponent<RectTransform>();
        itemRt.anchorMin = new Vector2(0, 0.5f); itemRt.anchorMax = new Vector2(1, 0.5f);
        itemRt.sizeDelta = new Vector2(0, 28);

        Toggle itemToggle = item.AddComponent<Toggle>();

        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(item.transform, false);
        RectTransform itemBgRt = itemBg.AddComponent<RectTransform>();
        itemBgRt.anchorMin = Vector2.zero; itemBgRt.anchorMax = Vector2.one; itemBgRt.sizeDelta = Vector2.zero;
        Image itemBgImg = itemBg.AddComponent<Image>();
        itemBgImg.color = new Color(0.15f, 0.12f, 0.18f, 0.5f);

        GameObject itemCheck = new GameObject("Item Checkmark");
        itemCheck.transform.SetParent(item.transform, false);
        RectTransform itemCheckRt = itemCheck.AddComponent<RectTransform>();
        itemCheckRt.anchorMin = new Vector2(0, 0.5f); itemCheckRt.anchorMax = new Vector2(0, 0.5f);
        itemCheckRt.sizeDelta = new Vector2(20, 20); itemCheckRt.anchoredPosition = new Vector2(10, 0);
        Image itemCheckImg = itemCheck.AddComponent<Image>();
        itemCheckImg.color = accentRed;

        itemToggle.targetGraphic = itemBgImg;
        itemToggle.graphic = itemCheckImg;

        TextMeshProUGUI itemLabel = CreateText(item.transform, "Item Label", "Option",
            new Vector2(15, 0), new Vector2(200, 25), 14, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);

        dropdown.template = tempRt;
        dropdown.itemText = itemLabel;
        template.SetActive(false);

        return dropdown;
    }
}
#endif

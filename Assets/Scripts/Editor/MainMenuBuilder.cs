#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script: Tools → Build Main Menu Scene
/// Automatically generates a complete MainMenu scene with camera, canvas,
/// title, play/settings/quit buttons, and level select panel.
/// </summary>
public class MainMenuBuilder : Editor
{
    // --- Color Palette (DOOM-esque dark theme) ---
    private static readonly Color bgDark = new Color(0.06f, 0.04f, 0.08f, 1f);
    private static readonly Color panelColor = new Color(0.1f, 0.08f, 0.12f, 0.95f);
    private static readonly Color accentRed = new Color(0.85f, 0.15f, 0.1f, 1f);
    private static readonly Color accentOrange = new Color(1f, 0.5f, 0.1f, 1f);
    private static readonly Color textWhite = new Color(0.95f, 0.92f, 0.88f, 1f);
    private static readonly Color buttonHover = new Color(0.9f, 0.25f, 0.15f, 1f);
    private static readonly Color subtleGray = new Color(0.6f, 0.55f, 0.5f, 1f);

    [MenuItem("Tools/Build Main Menu Scene")]
    public static void BuildMainMenu()
    {
        // 1. Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bgDark;
        cam.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();

        // 3. GameSettings (persists across scenes)
        GameObject settingsObj = new GameObject("GameSettings");
        settingsObj.AddComponent<GameSettings>();

        // 4. EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // 5. Root Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 5.5 Custom Title Background
        GameObject bgObj = new GameObject("BackgroundTitle");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        
        string spritePath = "Assets/Sprites/CustomTitleScreen.png";
        
        // Ensure Unity recognizes the file if it was just copied
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
        
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            bool needsReimport = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                needsReimport = true;
            }
            if (needsReimport)
            {
                importer.SaveAndReimport();
            }
        }
        
        // Load the sprite robustly by iterating over all assets at path
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        foreach (var asset in assets)
        {
            if (asset is Sprite s)
            {
                bgImg.sprite = s;
                bgImg.preserveAspect = true;
                break;
            }
        }

        // 6. MainMenu controller
        GameObject menuController = new GameObject("MainMenuController");
        MainMenu mainMenu = menuController.AddComponent<MainMenu>();

        // ==========================================
        // MAIN PANEL
        // ==========================================
        GameObject mainPanel = CreatePanel(canvasObj.transform, "MainPanel", new Vector2(-50, 50), new Vector2(400, 400));
        RectTransform mainPanelRt = mainPanel.GetComponent<RectTransform>();
        mainPanelRt.anchorMin = new Vector2(1f, 0f);
        mainPanelRt.anchorMax = new Vector2(1f, 0f);
        mainPanelRt.pivot = new Vector2(1f, 0f);
        mainMenu.mainPanel = mainPanel;

        // Play Button
        GameObject playBtn = CreateButton(mainPanel.transform, "PlayButton", "> PLAY",
            new Vector2(0, 80), new Vector2(350, 65), accentRed, textWhite, 24);
        playBtn.AddComponent<RainbowColor>();
        playBtn.GetComponent<Button>().onClick.AddListener(() => { });

        // Settings Button
        GameObject settingsBtn = CreateButton(mainPanel.transform, "SettingsButton", "SETTINGS",
            new Vector2(0, 0), new Vector2(350, 65), panelColor, textWhite, 22);
        settingsBtn.GetComponent<Button>().onClick.AddListener(() => { });

        // Quit Button
        GameObject quitBtn = CreateButton(mainPanel.transform, "QuitButton", "QUIT",
            new Vector2(0, -80), new Vector2(350, 65), panelColor, accentRed, 22);
        quitBtn.GetComponent<Button>().onClick.AddListener(() => { });

        // Version text
        CreateText(mainPanel.transform, "VersionText", "v0.1 - Alpha Build",
            new Vector2(0, -160), new Vector2(300, 30), 12, subtleGray, FontStyles.Normal, TextAlignmentOptions.Center);

        // ==========================================
        // LEVEL SELECT PANEL
        // ==========================================
        GameObject levelPanel = CreatePanel(canvasObj.transform, "LevelSelectPanel", Vector2.zero, new Vector2(600, 600));
        levelPanel.SetActive(false);
        mainMenu.levelSelectPanel = levelPanel;

        CreateText(levelPanel.transform, "LevelSelectTitle", "SELECT LEVEL",
            new Vector2(0, 200), new Vector2(400, 60), 30, accentOrange, FontStyles.Bold, TextAlignmentOptions.Center);

        GameObject lvl1Btn = CreateButton(levelPanel.transform, "Level1Button", "LEVEL 1 — The Arena",
            new Vector2(0, 80), new Vector2(400, 60), panelColor, textWhite, 20);
        lvl1Btn.GetComponent<Button>().onClick.AddListener(() => { });

        GameObject lvl2Btn = CreateButton(levelPanel.transform, "Level2Button", "LEVEL 2 — The Depths",
            new Vector2(0, 5), new Vector2(400, 60), panelColor, textWhite, 20);
        lvl2Btn.GetComponent<Button>().onClick.AddListener(() => { });

        GameObject bossBtn = CreateButton(levelPanel.transform, "BossButton", "BOSS — Final Stand",
            new Vector2(0, -70), new Vector2(400, 60), accentRed, textWhite, 20);
        bossBtn.GetComponent<Button>().onClick.AddListener(() => { });

        GameObject backBtn = CreateButton(levelPanel.transform, "BackButton", "← BACK",
            new Vector2(0, -180), new Vector2(200, 50), panelColor, subtleGray, 18);
        backBtn.GetComponent<Button>().onClick.AddListener(() => { });

        // ==========================================
        // SETTINGS PANEL (built by SettingsMenuBuilder logic, inline here)
        // ==========================================
        GameObject settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", Vector2.zero, new Vector2(700, 750));
        SettingsMenu settingsMenuComp = settingsPanel.AddComponent<SettingsMenu>();
        settingsMenuComp.settingsPanel = settingsPanel;
        settingsPanel.SetActive(false);
        mainMenu.settingsMenu = settingsMenuComp;

        float yPos = 250f;
        float yStep = -75f;

        // Sensitivity
        var sensPair = CreateSliderRow(settingsPanel.transform, "Sensitivity", yPos);
        settingsMenuComp.sensitivitySlider = sensPair.slider;
        settingsMenuComp.sensitivityValueText = sensPair.valueText;
        yPos += yStep;

        // Master Volume
        var masterPair = CreateSliderRow(settingsPanel.transform, "Master Volume", yPos);
        settingsMenuComp.masterVolumeSlider = masterPair.slider;
        settingsMenuComp.masterVolumeValueText = masterPair.valueText;
        yPos += yStep;

        // Music Volume
        var musicPair = CreateSliderRow(settingsPanel.transform, "Music Volume", yPos);
        settingsMenuComp.musicVolumeSlider = musicPair.slider;
        settingsMenuComp.musicVolumeValueText = musicPair.valueText;
        yPos += yStep;

        // SFX Volume
        var sfxPair = CreateSliderRow(settingsPanel.transform, "SFX Volume", yPos);
        settingsMenuComp.sfxVolumeSlider = sfxPair.slider;
        settingsMenuComp.sfxVolumeValueText = sfxPair.valueText;
        yPos += yStep;

        // FOV
        var fovPair = CreateSliderRow(settingsPanel.transform, "Field of View", yPos);
        settingsMenuComp.fovSlider = fovPair.slider;
        settingsMenuComp.fovValueText = fovPair.valueText;
        yPos += yStep;

        // View Bob Toggle
        var viewBobToggle = CreateToggleRow(settingsPanel.transform, "View Bob", yPos);
        settingsMenuComp.viewBobToggle = viewBobToggle;
        yPos += yStep;

        // Fullscreen Toggle
        var fullscreenToggle = CreateToggleRow(settingsPanel.transform, "Fullscreen", yPos);
        settingsMenuComp.fullscreenToggle = fullscreenToggle;
        yPos += yStep;

        // Quality Dropdown
        var qualityDropdown = CreateDropdownRow(settingsPanel.transform, "Quality", yPos);
        settingsMenuComp.qualityDropdown = qualityDropdown;
        yPos += yStep;

        // Reset Defaults
        GameObject resetBtn = CreateButton(settingsPanel.transform, "ResetButton", "RESET DEFAULTS",
            new Vector2(0, yPos), new Vector2(250, 45), panelColor, accentOrange, 16);
        resetBtn.GetComponent<Button>().onClick.AddListener(() => { });
        yPos += yStep;

        // Back button
        GameObject settingsBackBtn = CreateButton(settingsPanel.transform, "SettingsBackButton", "← BACK",
            new Vector2(0, yPos), new Vector2(200, 45), panelColor, subtleGray, 16);
        settingsBackBtn.GetComponent<Button>().onClick.AddListener(() => { });

        // ==========================================
        // WIRE UP BUTTONS (via UnityEditor serialized events)
        // ==========================================
        // Play
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            playBtn.GetComponent<Button>().onClick, mainMenu.OnPlayClicked);
        // Settings
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            settingsBtn.GetComponent<Button>().onClick, mainMenu.OnSettingsClicked);
        // Quit
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            quitBtn.GetComponent<Button>().onClick, mainMenu.OnQuitClicked);
        // Level1
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            lvl1Btn.GetComponent<Button>().onClick, mainMenu.LoadGameScene);
        // Level2
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            lvl2Btn.GetComponent<Button>().onClick, mainMenu.LoadLevel2);
        // Boss
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            bossBtn.GetComponent<Button>().onClick, mainMenu.LoadBossScene);
        // Level Select Back
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            backBtn.GetComponent<Button>().onClick, mainMenu.OnLevelSelectBack);
        // Reset Defaults
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            resetBtn.GetComponent<Button>().onClick, settingsMenuComp.ResetDefaults);
        // Settings Back
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            settingsBackBtn.GetComponent<Button>().onClick, mainMenu.OnSettingsBackClicked);

        // ==========================================
        // Save the scene
        // ==========================================
        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorUtility.DisplayDialog("Main Menu Built! ✅",
            $"Scene saved to:\n{scenePath}\n\n" +
            "Don't forget to add it to Build Settings!\n" +
            "(File → Build Settings → Add Open Scenes)",
            "Got it!");

        Debug.Log($"[MainMenuBuilder] Scene created at {scenePath}");
    }

    // ==========================================
    // HELPER METHODS
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

        // Rounded look via CanvasGroup
        CanvasGroup cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

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
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

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
        colors.normalColor = bgColor;
        colors.highlightedColor = buttonHover;
        colors.pressedColor = accentRed;
        colors.selectedColor = bgColor;
        btn.colors = colors;

        // Button text
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
        // Container
        GameObject row = new GameObject(label.Replace(" ", "") + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0, yPos);
        rowRt.sizeDelta = new Vector2(600, 50);

        // Label
        CreateText(row.transform, label.Replace(" ", "") + "Label", label,
            new Vector2(-200, 0), new Vector2(180, 40), 16, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);

        // Slider
        GameObject sliderObj = CreateDefaultSlider(row.transform, label.Replace(" ", "") + "Slider",
            new Vector2(40, 0), new Vector2(250, 30));
        Slider slider = sliderObj.GetComponent<Slider>();

        // Value text
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
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.5f;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.12f, 0.18f, 1f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.offsetMin = new Vector2(5, 0);
        fillAreaRt.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = accentRed;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10, 0);
        handleAreaRt.offsetMax = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = textWhite;

        // Wire up slider references
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

        // Toggle
        GameObject toggleObj = new GameObject(label + "Toggle");
        toggleObj.transform.SetParent(row.transform, false);
        RectTransform toggleRt = toggleObj.AddComponent<RectTransform>();
        toggleRt.anchorMin = new Vector2(0.5f, 0.5f);
        toggleRt.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRt.anchoredPosition = new Vector2(40, 0);
        toggleRt.sizeDelta = new Vector2(40, 40);

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.12f, 0.18f, 1f);
        toggle.targetGraphic = bgImg;

        // Checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform, false);
        RectTransform checkRt = checkObj.AddComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.1f, 0.1f);
        checkRt.anchorMax = new Vector2(0.9f, 0.9f);
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

        // Dropdown container
        GameObject dropObj = new GameObject(label + "Dropdown");
        dropObj.transform.SetParent(row.transform, false);
        RectTransform dropRt = dropObj.AddComponent<RectTransform>();
        dropRt.anchorMin = new Vector2(0.5f, 0.5f);
        dropRt.anchorMax = new Vector2(0.5f, 0.5f);
        dropRt.anchoredPosition = new Vector2(40, 0);
        dropRt.sizeDelta = new Vector2(250, 40);

        Image dropBg = dropObj.AddComponent<Image>();
        dropBg.color = new Color(0.15f, 0.12f, 0.18f, 1f);

        TMP_Dropdown dropdown = dropObj.AddComponent<TMP_Dropdown>();

        // Caption text
        TextMeshProUGUI captionText = CreateText(dropObj.transform, "CaptionText", "Select...",
            Vector2.zero, new Vector2(230, 35), 14, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);
        dropdown.captionText = captionText;

        // Template (required for dropdown to work)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropObj.transform, false);
        RectTransform tempRt = template.AddComponent<RectTransform>();
        tempRt.anchorMin = new Vector2(0, 0);
        tempRt.anchorMax = new Vector2(1, 0);
        tempRt.pivot = new Vector2(0.5f, 1f);
        tempRt.anchoredPosition = Vector2.zero;
        tempRt.sizeDelta = new Vector2(0, 150);
        Image tempImg = template.AddComponent<Image>();
        tempImg.color = panelColor;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = panelColor;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = Vector2.one;
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0, 28);

        // Item template
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        RectTransform itemRt = item.AddComponent<RectTransform>();
        itemRt.anchorMin = new Vector2(0, 0.5f);
        itemRt.anchorMax = new Vector2(1, 0.5f);
        itemRt.sizeDelta = new Vector2(0, 28);

        Toggle itemToggle = item.AddComponent<Toggle>();

        // Item background
        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(item.transform, false);
        RectTransform itemBgRt = itemBg.AddComponent<RectTransform>();
        itemBgRt.anchorMin = Vector2.zero;
        itemBgRt.anchorMax = Vector2.one;
        itemBgRt.sizeDelta = Vector2.zero;
        Image itemBgImg = itemBg.AddComponent<Image>();
        itemBgImg.color = new Color(0.15f, 0.12f, 0.18f, 0.5f);

        // Item checkmark
        GameObject itemCheck = new GameObject("Item Checkmark");
        itemCheck.transform.SetParent(item.transform, false);
        RectTransform itemCheckRt = itemCheck.AddComponent<RectTransform>();
        itemCheckRt.anchorMin = new Vector2(0, 0.5f);
        itemCheckRt.anchorMax = new Vector2(0, 0.5f);
        itemCheckRt.sizeDelta = new Vector2(20, 20);
        itemCheckRt.anchoredPosition = new Vector2(10, 0);
        Image itemCheckImg = itemCheck.AddComponent<Image>();
        itemCheckImg.color = accentRed;

        itemToggle.targetGraphic = itemBgImg;
        itemToggle.graphic = itemCheckImg;

        // Item label
        TextMeshProUGUI itemLabel = CreateText(item.transform, "Item Label", "Option",
            new Vector2(15, 0), new Vector2(200, 25), 14, textWhite, FontStyles.Normal, TextAlignmentOptions.Left);

        dropdown.template = tempRt;
        dropdown.itemText = itemLabel;

        template.SetActive(false);

        return dropdown;
    }
}
#endif

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Editor
{
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenuScene.unity";

        [MenuItem("Tools/Witcher Right Version/Build Main Menu Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            RemoveIfExists("MainMenuCanvas");
            RemoveIfExists("MainMenuEventSystem");
            RemoveIfExists("MainMenuController");

            EnsureCamera();
            var font = GetDefaultFont();
            var controllerHost = new GameObject("MainMenuController");
            var controller = controllerHost.AddComponent<MainMenuController>();

            var canvas = CreateCanvas();
            var background = CreatePanel("Background", canvas.transform, new Color(0.035f, 0.055f, 0.045f, 1f));
            Stretch(background);

            var accent = CreatePanel("LeftAccent", background.transform, new Color(0.56f, 0.12f, 0.11f, 0.85f));
            SetRect(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(14f, 0f));

            var title = CreateText("Title", background.transform, font, "The Witcher 4", 58, FontStyle.Bold, new Color(0.87f, 0.72f, 0.35f, 1f));
            SetRect(title.gameObject, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.92f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            title.alignment = TextAnchor.MiddleLeft;

            var subtitle = CreateText("Subtitle", background.transform, font, "Right Version", 30, FontStyle.Normal, new Color(0.75f, 0.78f, 0.72f, 1f));
            SetRect(subtitle.gameObject, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.75f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            subtitle.alignment = TextAnchor.MiddleLeft;

            var mainPanel = CreatePanel("MainPanel", background.transform, new Color(0f, 0f, 0f, 0f));
            SetRect(mainPanel, new Vector2(0.08f, 0.18f), new Vector2(0.38f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var settingsPanel = CreatePanel("SettingsPanel", background.transform, new Color(0.08f, 0.095f, 0.085f, 0.94f));
            SetRect(settingsPanel, new Vector2(0.56f, 0.16f), new Vector2(0.9f, 0.66f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var status = CreateText("StatusText", background.transform, font, "Ready for a new contract.", 18, FontStyle.Normal, new Color(0.76f, 0.78f, 0.73f, 1f));
            SetRect(status.gameObject, new Vector2(0.08f, 0.07f), new Vector2(0.88f, 0.14f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            status.alignment = TextAnchor.MiddleLeft;

            var newGameButton = CreateButton("NewGameButton", mainPanel.transform, font, "New Game");
            var worldGameButton = CreateButton("VelemarWorldButton", mainPanel.transform, font, "Velemar World");
            var continueButton = CreateButton("ContinueButton", mainPanel.transform, font, "Continue");
            var settingsButton = CreateButton("SettingsButton", mainPanel.transform, font, "Settings");
            var exitButton = CreateButton("ExitButton", mainPanel.transform, font, "Exit");

            SetStackedButtonRect(newGameButton.gameObject, 0);
            SetStackedButtonRect(worldGameButton.gameObject, 1);
            SetStackedButtonRect(continueButton.gameObject, 2);
            SetStackedButtonRect(settingsButton.gameObject, 3);
            SetStackedButtonRect(exitButton.gameObject, 4);

            UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.StartNewGame);
            UnityEventTools.AddPersistentListener(worldGameButton.onClick, controller.StartVelemarWorld);
            UnityEventTools.AddPersistentListener(continueButton.onClick, controller.ContinueGame);
            UnityEventTools.AddPersistentListener(settingsButton.onClick, controller.ShowSettingsPanel);
            UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitGame);

            var settingsTitle = CreateText("SettingsTitle", settingsPanel.transform, font, "Settings", 30, FontStyle.Bold, new Color(0.87f, 0.72f, 0.35f, 1f));
            SetRect(settingsTitle.gameObject, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.96f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            settingsTitle.alignment = TextAnchor.MiddleLeft;

            var volumeLabel = CreateText("VolumeLabel", settingsPanel.transform, font, "Volume", 18, FontStyle.Normal, Color.white);
            SetRect(volumeLabel.gameObject, new Vector2(0.08f, 0.68f), new Vector2(0.42f, 0.76f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            volumeLabel.alignment = TextAnchor.MiddleLeft;

            var volumeSlider = CreateSlider("VolumeSlider", settingsPanel.transform);
            SetRect(volumeSlider.gameObject, new Vector2(0.42f, 0.68f), new Vector2(0.9f, 0.76f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var musicToggle = CreateToggle("MusicToggle", settingsPanel.transform, font, "Music");
            SetRect(musicToggle.gameObject, new Vector2(0.08f, 0.56f), new Vector2(0.9f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var resolutionLabel = CreateText("ResolutionLabel", settingsPanel.transform, font, "Resolution", 18, FontStyle.Normal, Color.white);
            SetRect(resolutionLabel.gameObject, new Vector2(0.08f, 0.43f), new Vector2(0.42f, 0.51f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            resolutionLabel.alignment = TextAnchor.MiddleLeft;

            var resolutionDropdown = CreateDropdown("ResolutionDropdown", settingsPanel.transform, font, new[] { "1280 x 720", "1600 x 900", "1920 x 1080" });
            SetRect(resolutionDropdown.gameObject, new Vector2(0.42f, 0.43f), new Vector2(0.9f, 0.51f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var graphicsLabel = CreateText("GraphicsLabel", settingsPanel.transform, font, "Graphics", 18, FontStyle.Normal, Color.white);
            SetRect(graphicsLabel.gameObject, new Vector2(0.08f, 0.31f), new Vector2(0.42f, 0.39f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            graphicsLabel.alignment = TextAnchor.MiddleLeft;

            var graphicsDropdown = CreateDropdown("GraphicsDropdown", settingsPanel.transform, font, new[] { "Low", "Medium", "High" });
            SetRect(graphicsDropdown.gameObject, new Vector2(0.42f, 0.31f), new Vector2(0.9f, 0.39f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var languageLabel = CreateText("LanguageLabel", settingsPanel.transform, font, "Language", 18, FontStyle.Normal, Color.white);
            SetRect(languageLabel.gameObject, new Vector2(0.08f, 0.21f), new Vector2(0.42f, 0.29f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            languageLabel.alignment = TextAnchor.MiddleLeft;

            var languageDropdown = CreateDropdown("LanguageDropdown", settingsPanel.transform, font, new[] { "English", "Русский" });
            SetRect(languageDropdown.gameObject, new Vector2(0.42f, 0.21f), new Vector2(0.9f, 0.29f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var applyButton = CreateButton("ApplySettingsButton", settingsPanel.transform, font, "Apply");
            SetRect(applyButton.gameObject, new Vector2(0.08f, 0.09f), new Vector2(0.46f, 0.2f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var backButton = CreateButton("BackButton", settingsPanel.transform, font, "Back");
            SetRect(backButton.gameObject, new Vector2(0.52f, 0.09f), new Vector2(0.9f, 0.2f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            UnityEventTools.AddPersistentListener(applyButton.onClick, controller.ApplySettings);
            UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowMainPanel);

            controller.mainPanel = mainPanel;
            controller.settingsPanel = settingsPanel;
            controller.titleText = title;
            controller.subtitleText = subtitle;
            controller.statusText = status;
            controller.newGameButtonText = GetButtonLabel(newGameButton);
            controller.worldGameButtonText = GetButtonLabel(worldGameButton);
            controller.continueButtonText = GetButtonLabel(continueButton);
            controller.settingsButtonText = GetButtonLabel(settingsButton);
            controller.exitButtonText = GetButtonLabel(exitButton);
            controller.settingsTitleText = settingsTitle;
            controller.volumeLabelText = volumeLabel;
            controller.musicLabelText = musicToggle.GetComponentInChildren<Text>();
            controller.resolutionLabelText = resolutionLabel;
            controller.graphicsLabelText = graphicsLabel;
            controller.languageLabelText = languageLabel;
            controller.applyButtonText = GetButtonLabel(applyButton);
            controller.backButtonText = GetButtonLabel(backButton);
            controller.volumeSlider = volumeSlider;
            controller.musicToggle = musicToggle;
            controller.resolutionDropdown = resolutionDropdown;
            controller.graphicsDropdown = graphicsDropdown;
            controller.languageDropdown = languageDropdown;

            settingsPanel.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("MainMenuCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("MainMenuEventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            return canvas;
        }

        private static void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.045f, 1f);
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = CreateUiObject(name, parent);
            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(string name, Transform parent, Font font, string text, int size, FontStyle style, Color color)
        {
            var textObject = CreateUiObject(name, parent);
            var label = textObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = false;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label)
        {
            var buttonObject = CreatePanel(name, parent, new Color(0.12f, 0.16f, 0.13f, 0.95f));
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();

            var labelText = CreateText("Label", buttonObject.transform, font, label, 24, FontStyle.Bold, new Color(0.92f, 0.9f, 0.82f, 1f));
            Stretch(labelText.gameObject);
            return button;
        }

        private static Text GetButtonLabel(Button button)
        {
            return button != null ? button.GetComponentInChildren<Text>() : null;
        }

        private static Slider CreateSlider(string name, Transform parent)
        {
            var root = CreateUiObject(name, parent);
            var background = CreatePanel("Background", root.transform, new Color(0.16f, 0.18f, 0.16f, 1f));
            Stretch(background);

            var fillArea = CreateUiObject("Fill Area", root.transform);
            SetRect(fillArea, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(8f, 0f), new Vector2(-8f, 0f));

            var fill = CreatePanel("Fill", fillArea.transform, new Color(0.67f, 0.47f, 0.16f, 1f));
            Stretch(fill);

            var handleArea = CreateUiObject("Handle Slide Area", root.transform);
            Stretch(handleArea);

            var handle = CreatePanel("Handle", handleArea.transform, new Color(0.9f, 0.82f, 0.55f, 1f));
            SetRect(handle, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-10f, 0f), new Vector2(10f, 0f));

            var slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static Toggle CreateToggle(string name, Transform parent, Font font, string label)
        {
            var root = CreateUiObject(name, parent);

            var box = CreatePanel("Box", root.transform, new Color(0.16f, 0.18f, 0.16f, 1f));
            SetRect(box, new Vector2(0f, 0.15f), new Vector2(0f, 0.85f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(34f, 0f));

            var checkmark = CreatePanel("Checkmark", box.transform, new Color(0.67f, 0.47f, 0.16f, 1f));
            SetRect(checkmark, new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var labelText = CreateText("Label", root.transform, font, label, 18, FontStyle.Normal, Color.white);
            SetRect(labelText.gameObject, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(46f, 0f), Vector2.zero);
            labelText.alignment = TextAnchor.MiddleLeft;

            var toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = box.GetComponent<Image>();
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.isOn = true;
            return toggle;
        }

        private static Dropdown CreateDropdown(string name, Transform parent, Font font, IEnumerable<string> options)
        {
            var root = CreatePanel(name, parent, new Color(0.16f, 0.18f, 0.16f, 1f));
            var label = CreateText("Label", root.transform, font, "", 16, FontStyle.Normal, Color.white);
            SetRect(label.gameObject, new Vector2(0.06f, 0f), new Vector2(0.9f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            label.alignment = TextAnchor.MiddleLeft;

            var arrow = CreateText("Arrow", root.transform, font, "v", 18, FontStyle.Bold, new Color(0.87f, 0.72f, 0.35f, 1f));
            SetRect(arrow.gameObject, new Vector2(0.86f, 0f), new Vector2(0.98f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var template = CreatePanel("Template", root.transform, new Color(0.1f, 0.12f, 0.1f, 0.98f));
            SetRect(template, new Vector2(0f, -3f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 120f));

            var viewport = CreatePanel("Viewport", template.transform, new Color(0f, 0f, 0f, 0f));
            Stretch(viewport);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateUiObject("Content", viewport.transform);
            Stretch(content);

            var item = CreateToggle("Item", content.transform, font, "Option");
            Stretch(item.gameObject);

            template.SetActive(false);

            var dropdown = root.AddComponent<Dropdown>();
            dropdown.targetGraphic = root.GetComponent<Image>();
            dropdown.captionText = label;
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.itemText = item.GetComponentInChildren<Text>();
            dropdown.options.Clear();

            foreach (var option in options)
            {
                dropdown.options.Add(new Dropdown.OptionData(option));
            }

            dropdown.value = 0;
            dropdown.RefreshShownValue();
            return dropdown;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                   Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void Stretch(GameObject obj)
        {
            SetRect(obj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static void SetStackedButtonRect(GameObject button, int index)
        {
            var top = 1f - index * 0.19f;
            SetRect(button, new Vector2(0f, top - 0.135f), new Vector2(1f, top), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static void SetRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void RemoveIfExists(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
    }
}

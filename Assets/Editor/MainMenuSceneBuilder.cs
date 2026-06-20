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
        private const string MenuBackgroundImagePath = "Assets/Art/Menu/MainMenuBackground.jpg";

        [MenuItem("Tools/Witcher Right Version/Build Main Menu Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            RemoveIfExists("MainMenuCanvas");
            RemoveIfExists("MainMenuEventSystem");
            RemoveIfExists("MainMenuController");
            RemoveIfExists("MainMenuDiorama");

            EnsureCamera();
            var backgroundSprite = EnsureMenuBackgroundSprite();
            var font = GetDefaultFont();
            var controllerHost = new GameObject("MainMenuController");
            var controller = controllerHost.AddComponent<MainMenuController>();

            var canvas = CreateCanvas();
            var background = CreatePanel("Background", canvas.transform, new Color(0.012f, 0.016f, 0.022f, 0.34f));
            Stretch(background);
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.preserveAspect = false;
            backgroundImage.color = Color.white;

            var centerShade = CreatePanel("MainMenuCenterShadow", background.transform, new Color(0.005f, 0.007f, 0.01f, 0.28f));
            SetRect(centerShade, new Vector2(0.24f, 0f), new Vector2(0.76f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var bottomShade = CreatePanel("MainMenuBottomShadow", background.transform, new Color(0.005f, 0.006f, 0.008f, 0.72f));
            SetRect(bottomShade, new Vector2(0f, 0f), new Vector2(1f, 0.32f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);

            var title = CreateText("Title", background.transform, font, "", 1, FontStyle.Bold, new Color(0.87f, 0.72f, 0.35f, 0f));
            SetRect(title.gameObject, new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var subtitle = CreateText("Subtitle", background.transform, font, "", 1, FontStyle.Normal, new Color(0.75f, 0.78f, 0.72f, 0f));
            SetRect(subtitle.gameObject, new Vector2(0.5f, 0.79f), new Vector2(0.5f, 0.79f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var mainPanel = CreatePanel("MainPanel", background.transform, new Color(0f, 0f, 0f, 0f));
            SetRect(mainPanel, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.5f), new Vector2(-220f, 0f), new Vector2(220f, 0f));

            // Translucent like the menu buttons (alpha ~0.56) so the forest shows through,
            // instead of a near-opaque dark slab — matches the main-menu style.
            var settingsPanel = CreatePanel("SettingsPanel", background.transform, new Color(0.1f, 0.11f, 0.12f, 0.62f));
            SetRect(settingsPanel, new Vector2(0.48f, 0.12f), new Vector2(0.92f, 0.76f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var status = CreateText("StatusText", background.transform, font, "Готов к новому контракту.", 18, FontStyle.Normal, new Color(0.76f, 0.78f, 0.73f, 1f));
            SetRect(status.gameObject, new Vector2(0.28f, 0.05f), new Vector2(0.72f, 0.12f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            status.alignment = TextAnchor.MiddleCenter;

            var newGameButton = CreateButton("NewGameButton", mainPanel.transform, font, "Новая игра");
            var continueButton = CreateButton("ContinueButton", mainPanel.transform, font, "Продолжить");
            var settingsButton = CreateButton("SettingsButton", mainPanel.transform, font, "Настройки");
            var exitButton = CreateButton("ExitButton", mainPanel.transform, font, "Выйти");

            SetStackedButtonRect(newGameButton.gameObject, 0);
            SetStackedButtonRect(continueButton.gameObject, 1);
            SetStackedButtonRect(settingsButton.gameObject, 2);
            SetStackedButtonRect(exitButton.gameObject, 3);

            UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.StartNewGame);
            UnityEventTools.AddPersistentListener(continueButton.onClick, controller.ContinueGame);
            UnityEventTools.AddPersistentListener(settingsButton.onClick, controller.ShowSettingsPanel);
            UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitGame);

            var confirmationPanel = CreatePanel("ConfirmationPanel", background.transform, new Color(0.025f, 0.028f, 0.03f, 0.98f));
            SetRect(confirmationPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300f, -125f), new Vector2(300f, 125f));
            var confirmationText = CreateText("ConfirmationText", confirmationPanel.transform, font, "Начать новую игру?", 28, FontStyle.Bold, new Color(0.94f, 0.88f, 0.75f, 1f));
            SetRect(confirmationText.gameObject, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var confirmButton = CreateButton("ConfirmActionButton", confirmationPanel.transform, font, "Подтвердить");
            SetRect(confirmButton.gameObject, new Vector2(0.08f, 0.12f), new Vector2(0.46f, 0.4f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var cancelButton = CreateButton("CancelActionButton", confirmationPanel.transform, font, "Отмена");
            SetRect(cancelButton.gameObject, new Vector2(0.54f, 0.12f), new Vector2(0.92f, 0.4f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UnityEventTools.AddPersistentListener(confirmButton.onClick, controller.ConfirmAction);
            UnityEventTools.AddPersistentListener(cancelButton.onClick, controller.HideConfirmation);

            var settingsTitle = CreateText("SettingsTitle", settingsPanel.transform, font, "Настройки", 30, FontStyle.Bold, new Color(0.87f, 0.72f, 0.35f, 1f));
            SetRect(settingsTitle.gameObject, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.98f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            settingsTitle.alignment = TextAnchor.MiddleLeft;

            var effectsTab = CreateButton("EffectsTabButton", settingsPanel.transform, font, "Эффекты");
            var soundTab = CreateButton("SoundTabButton", settingsPanel.transform, font, "Звук");
            var resolutionTab = CreateButton("ResolutionTabButton", settingsPanel.transform, font, "Разрешение");
            var graphicsTab = CreateButton("GraphicsTabButton", settingsPanel.transform, font, "Графика");
            SetRect(effectsTab.gameObject, new Vector2(0.05f, 0.75f), new Vector2(0.27f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetRect(soundTab.gameObject, new Vector2(0.28f, 0.75f), new Vector2(0.49f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetRect(resolutionTab.gameObject, new Vector2(0.5f, 0.75f), new Vector2(0.72f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetRect(graphicsTab.gameObject, new Vector2(0.73f, 0.75f), new Vector2(0.95f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UnityEventTools.AddPersistentListener(effectsTab.onClick, controller.ShowEffectsSettings);
            UnityEventTools.AddPersistentListener(soundTab.onClick, controller.ShowSoundSettings);
            UnityEventTools.AddPersistentListener(resolutionTab.onClick, controller.ShowResolutionSettings);
            UnityEventTools.AddPersistentListener(graphicsTab.onClick, controller.ShowGraphicsSettings);

            var effectsPage = CreatePanel("EffectsSettingsPanel", settingsPanel.transform, new Color(0f, 0f, 0f, 0f));
            var soundPage = CreatePanel("SoundSettingsPanel", settingsPanel.transform, new Color(0f, 0f, 0f, 0f));
            var resolutionPage = CreatePanel("ResolutionSettingsPanel", settingsPanel.transform, new Color(0f, 0f, 0f, 0f));
            var graphicsPage = CreatePanel("GraphicsSettingsPanel", settingsPanel.transform, new Color(0f, 0f, 0f, 0f));
            SetRect(effectsPage, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetRect(soundPage, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetRect(resolutionPage, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetRect(graphicsPage, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var sharpnessToggle = CreateToggle("SharpnessToggle", effectsPage.transform, font, "Резкость");
            SetRect(sharpnessToggle.gameObject, new Vector2(0.08f, 0.6f), new Vector2(0.92f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var blurToggle = CreateToggle("BlurToggle", effectsPage.transform, font, "Мягкие края");
            SetRect(blurToggle.gameObject, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var volumeLabel = CreateText("VolumeLabel", soundPage.transform, font, "Громкость", 18, FontStyle.Normal, Color.white);
            SetRect(volumeLabel.gameObject, new Vector2(0.08f, 0.62f), new Vector2(0.42f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            volumeLabel.alignment = TextAnchor.MiddleLeft;

            var volumeSlider = CreateSlider("VolumeSlider", soundPage.transform);
            SetRect(volumeSlider.gameObject, new Vector2(0.42f, 0.62f), new Vector2(0.9f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var musicToggle = CreateToggle("MusicToggle", soundPage.transform, font, "Музыка");
            SetRect(musicToggle.gameObject, new Vector2(0.08f, 0.34f), new Vector2(0.9f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var resolutionLabel = CreateText("ResolutionLabel", resolutionPage.transform, font, "Разрешение", 18, FontStyle.Normal, Color.white);
            SetRect(resolutionLabel.gameObject, new Vector2(0.08f, 0.62f), new Vector2(0.42f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            resolutionLabel.alignment = TextAnchor.MiddleLeft;

            var resolutionDropdown = CreateDropdown("ResolutionDropdown", resolutionPage.transform, font, new[] { "1280 x 720", "1600 x 900", "1920 x 1080" });
            SetRect(resolutionDropdown.gameObject, new Vector2(0.42f, 0.62f), new Vector2(0.9f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var screenModeLabel = CreateText("ScreenModeLabel", resolutionPage.transform, font, "Режим экрана", 18, FontStyle.Normal, Color.white);
            SetRect(screenModeLabel.gameObject, new Vector2(0.08f, 0.34f), new Vector2(0.42f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            screenModeLabel.alignment = TextAnchor.MiddleLeft;
            var screenModeDropdown = CreateDropdown("ScreenModeDropdown", resolutionPage.transform, font, new[] { "Полный экран", "Без рамки", "Окно" });
            SetRect(screenModeDropdown.gameObject, new Vector2(0.42f, 0.34f), new Vector2(0.9f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var graphicsLabel = CreateText("GraphicsLabel", graphicsPage.transform, font, "Шаблон графики", 18, FontStyle.Normal, Color.white);
            SetRect(graphicsLabel.gameObject, new Vector2(0.08f, 0.54f), new Vector2(0.42f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            graphicsLabel.alignment = TextAnchor.MiddleLeft;

            var graphicsDropdown = CreateDropdown("GraphicsDropdown", graphicsPage.transform, font, new[] { "Низкое", "Среднее", "Высокое" });
            SetRect(graphicsDropdown.gameObject, new Vector2(0.42f, 0.54f), new Vector2(0.9f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var applyButton = CreateButton("ApplySettingsButton", settingsPanel.transform, font, "Применить");
            SetRect(applyButton.gameObject, new Vector2(0.06f, 0.05f), new Vector2(0.46f, 0.15f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var backButton = CreateButton("BackButton", settingsPanel.transform, font, "Назад");
            SetRect(backButton.gameObject, new Vector2(0.54f, 0.05f), new Vector2(0.94f, 0.15f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            UnityEventTools.AddPersistentListener(applyButton.onClick, controller.ApplySettings);
            UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowMainPanel);

            controller.mainPanel = mainPanel;
            controller.settingsPanel = settingsPanel;
            controller.confirmationPanel = confirmationPanel;
            controller.effectsSettingsPanel = effectsPage;
            controller.soundSettingsPanel = soundPage;
            controller.resolutionSettingsPanel = resolutionPage;
            controller.graphicsSettingsPanel = graphicsPage;
            controller.titleText = title;
            controller.subtitleText = subtitle;
            controller.statusText = status;
            controller.newGameButtonText = GetButtonLabel(newGameButton);
            controller.continueButtonText = GetButtonLabel(continueButton);
            controller.settingsButtonText = GetButtonLabel(settingsButton);
            controller.exitButtonText = GetButtonLabel(exitButton);
            controller.settingsTitleText = settingsTitle;
            controller.volumeLabelText = volumeLabel;
            controller.musicLabelText = musicToggle.GetComponentInChildren<Text>();
            controller.resolutionLabelText = resolutionLabel;
            controller.graphicsLabelText = graphicsLabel;
            controller.applyButtonText = GetButtonLabel(applyButton);
            controller.backButtonText = GetButtonLabel(backButton);
            controller.confirmationText = confirmationText;
            controller.confirmButtonText = GetButtonLabel(confirmButton);
            controller.cancelButtonText = GetButtonLabel(cancelButton);
            controller.effectsTabText = GetButtonLabel(effectsTab);
            controller.soundTabText = GetButtonLabel(soundTab);
            controller.resolutionTabText = GetButtonLabel(resolutionTab);
            controller.graphicsTabText = GetButtonLabel(graphicsTab);
            controller.sharpnessLabelText = sharpnessToggle.GetComponentInChildren<Text>();
            controller.blurLabelText = blurToggle.GetComponentInChildren<Text>();
            controller.screenModeLabelText = screenModeLabel;
            controller.volumeSlider = volumeSlider;
            controller.musicToggle = musicToggle;
            controller.resolutionDropdown = resolutionDropdown;
            controller.graphicsDropdown = graphicsDropdown;
            controller.screenModeDropdown = screenModeDropdown;
            controller.sharpnessToggle = sharpnessToggle;
            controller.blurToggle = blurToggle;

            settingsPanel.SetActive(false);
            confirmationPanel.SetActive(false);
            soundPage.SetActive(false);
            resolutionPage.SetActive(false);
            graphicsPage.SetActive(false);

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

        private static Sprite EnsureMenuBackgroundSprite()
        {
            if (!System.IO.File.Exists(MenuBackgroundImagePath))
            {
                Debug.LogWarning($"Main menu background image is missing: {MenuBackgroundImagePath}");
                return null;
            }

            var importer = AssetImporter.GetAtPath(MenuBackgroundImagePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(MenuBackgroundImagePath);
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
            camera.backgroundColor = new Color(0.011f, 0.015f, 0.025f, 1f);
            camera.transform.position = new Vector3(0f, 2.1f, -11.6f);
            camera.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
            camera.fieldOfView = 47f;
        }

        private static void CreateMenuDiorama()
        {
            var root = new GameObject("MainMenuDiorama");

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "MenuForestWetGround";
            ground.transform.SetParent(root.transform, false);
            ground.transform.position = new Vector3(0f, -0.03f, 5f);
            ground.transform.localScale = new Vector3(3.7f, 1f, 4.8f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/MenuForestWetGround.mat", new Color(0.018f, 0.025f, 0.027f, 1f));

            var moon = new GameObject("MenuColdMoonLight");
            moon.transform.SetParent(root.transform, false);
            moon.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            var moonLight = moon.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.42f, 0.52f, 0.72f, 1f);
            moonLight.intensity = 0.85f;
            moonLight.shadows = LightShadows.Soft;

            CreateMenuPointLight(root.transform, "MenuFarCampfireLight", new Vector3(0f, 1.05f, 7.7f), new Color(1f, 0.42f, 0.12f, 1f), 3.1f, 13.5f);
            CreateMenuPointLight(root.transform, "MenuLeftTorchLight", new Vector3(-2.7f, 0.85f, 6.8f), new Color(1f, 0.36f, 0.1f, 1f), 1.2f, 7.5f);
            CreateMenuPointLight(root.transform, "MenuRightTorchLight", new Vector3(2.4f, 0.8f, 6.9f), new Color(1f, 0.34f, 0.09f, 1f), 1.05f, 7f);
            CreateMenuPointLight(root.transform, "MenuWitcherRimLight", new Vector3(0.05f, 2.2f, 5.2f), new Color(0.34f, 0.46f, 0.68f, 1f), 1.25f, 5f);

            for (var i = 0; i < 46; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var row = i / 2;
                var x = side * (1.8f + (row % 8) * 0.62f + (i % 5) * 0.07f);
                var z = 0.6f + row * 0.62f;
                var height = 4.6f + (i % 7) * 0.42f;
                var radius = 0.09f + (i % 4) * 0.018f;
                CreateMenuTree(root.transform, $"MenuForestTree_{i + 1:00}", new Vector3(x, height * 0.5f, z), radius, height, i * 17f);
            }

            for (var i = 0; i < 16; i++)
            {
                var x = -5.5f + i * 0.75f;
                CreateMenuTree(root.transform, $"MenuBackgroundPine_{i + 1:00}", new Vector3(x, 2.8f + (i % 3) * 0.2f, 9.4f + (i % 2) * 0.35f), 0.06f, 5.6f + (i % 4) * 0.35f, i * 11f);
            }

            CreateMenuWitcherSilhouette(root.transform);
            CreateMenuFireCluster(root.transform, "MenuCampfireCore", new Vector3(0f, 0.22f, 7.65f), 1.0f);
            CreateMenuFireCluster(root.transform, "MenuLeftFire", new Vector3(-2.7f, 0.16f, 6.85f), 0.58f);
            CreateMenuFireCluster(root.transform, "MenuRightFire", new Vector3(2.4f, 0.16f, 6.95f), 0.5f);

            for (var i = 0; i < 10; i++)
            {
                var mist = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mist.name = $"MenuGroundMist_{i + 1:00}";
                mist.transform.SetParent(root.transform, false);
                mist.transform.position = new Vector3(-4.5f + i * 1.0f, 0.08f, 3.0f + (i % 3) * 1.3f);
                mist.transform.localScale = new Vector3(1.5f + (i % 4) * 0.3f, 0.025f, 0.34f);
                mist.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/MenuGroundMist_{i + 1:00}.mat", new Color(0.12f, 0.16f, 0.18f, 0.34f));
                Object.DestroyImmediate(mist.GetComponent<Collider>());
            }
        }

        private static void CreateMenuTree(Transform parent, string name, Vector3 position, float radius, float height, float yaw)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name;
            trunk.transform.SetParent(parent, false);
            trunk.transform.position = position;
            trunk.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            trunk.transform.localScale = new Vector3(radius, height * 0.5f, radius);
            trunk.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/MenuTreeTrunk.mat", new Color(0.025f, 0.022f, 0.021f, 1f));
            Object.DestroyImmediate(trunk.GetComponent<Collider>());

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            canopy.name = $"{name}_Canopy";
            canopy.transform.SetParent(parent, false);
            canopy.transform.position = position + new Vector3(0f, height * 0.42f, 0f);
            canopy.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            canopy.transform.localScale = new Vector3(radius * 8.5f, height * 0.34f, radius * 8.5f);
            canopy.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/MenuPineCanopy.mat", new Color(0.012f, 0.029f, 0.031f, 1f));
            Object.DestroyImmediate(canopy.GetComponent<Collider>());
        }

        private static void CreateMenuWitcherSilhouette(Transform parent)
        {
            var root = new GameObject("MenuReynardSilhouette");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 0f, 4.75f);
            root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var dark = CreateMaterial("Assets/Materials/MenuReynardSilhouette.mat", new Color(0.02f, 0.024f, 0.026f, 1f));
            CreateMenuBodyPart(root.transform, "Body", PrimitiveType.Capsule, new Vector3(0f, 1.05f, 0f), new Vector3(0.38f, 0.72f, 0.28f), Quaternion.identity, dark);
            CreateMenuBodyPart(root.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.82f, 0f), new Vector3(0.26f, 0.28f, 0.26f), Quaternion.identity, dark);
            CreateMenuBodyPart(root.transform, "LeftLeg", PrimitiveType.Capsule, new Vector3(-0.13f, 0.42f, 0f), new Vector3(0.12f, 0.48f, 0.12f), Quaternion.Euler(6f, 0f, -5f), dark);
            CreateMenuBodyPart(root.transform, "RightLeg", PrimitiveType.Capsule, new Vector3(0.13f, 0.42f, 0f), new Vector3(0.12f, 0.48f, 0.12f), Quaternion.Euler(-4f, 0f, 5f), dark);
            CreateMenuBodyPart(root.transform, "LeftArm", PrimitiveType.Capsule, new Vector3(-0.36f, 1.08f, 0f), new Vector3(0.1f, 0.46f, 0.1f), Quaternion.Euler(0f, 0f, -18f), dark);
            CreateMenuBodyPart(root.transform, "RightArm", PrimitiveType.Capsule, new Vector3(0.36f, 1.08f, 0f), new Vector3(0.1f, 0.46f, 0.1f), Quaternion.Euler(0f, 0f, 18f), dark);
            CreateMenuBodyPart(root.transform, "SteelSword", PrimitiveType.Cube, new Vector3(-0.22f, 1.55f, -0.14f), new Vector3(0.055f, 1.45f, 0.035f), Quaternion.Euler(0f, 0f, -36f), dark);
            CreateMenuBodyPart(root.transform, "SilverSword", PrimitiveType.Cube, new Vector3(0.24f, 1.52f, -0.14f), new Vector3(0.055f, 1.38f, 0.035f), Quaternion.Euler(0f, 0f, 34f), dark);
        }

        private static void CreateMenuBodyPart(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            var part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.transform.localRotation = localRotation;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<Collider>());
        }

        private static void CreateMenuFireCluster(Transform parent, string name, Vector3 position, float scale)
        {
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = name;
            core.transform.SetParent(parent, false);
            core.transform.position = position;
            core.transform.localScale = Vector3.one * scale;
            core.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", new Color(1f, 0.42f, 0.08f, 1f));
            Object.DestroyImmediate(core.GetComponent<Collider>());

            for (var i = 0; i < 5; i++)
            {
                var ember = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ember.name = $"{name}_Ember_{i + 1:00}";
                ember.transform.SetParent(parent, false);
                ember.transform.position = position + new Vector3(-0.32f + i * 0.16f, 0.18f + (i % 2) * 0.15f, 0.12f + (i % 3) * 0.1f) * scale;
                ember.transform.localScale = Vector3.one * (0.08f * scale);
                ember.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}_Ember.mat", new Color(1f, 0.68f, 0.22f, 1f));
                Object.DestroyImmediate(ember.GetComponent<Collider>());
            }
        }

        private static void CreateMenuPointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            // Near-matte: Standard's smoothness slider is "_Glossiness"; left at its 0.5
            // default the surface mirrors the skybox and washes out under a pale sky.
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.03f);
            }
            if (material.HasProperty("_GlossyReflections"))
            {
                material.SetFloat("_GlossyReflections", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
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
            var buttonObject = CreatePanel(name, parent, new Color(0.18f, 0.19f, 0.2f, 0.56f));
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();

            var colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.19f, 0.2f, 0.56f);
            colors.highlightedColor = new Color(0.32f, 0.33f, 0.35f, 0.72f);
            colors.pressedColor = new Color(0.45f, 0.38f, 0.3f, 0.82f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            var labelText = CreateText("Label", buttonObject.transform, font, label, 30, FontStyle.Normal, Color.white);
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
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0f, -4f);
            templateRect.sizeDelta = new Vector2(0f, 104f);

            var viewport = CreatePanel("Viewport", template.transform, new Color(0f, 0f, 0f, 0f));
            Stretch(viewport);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateUiObject("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 96f);

            var item = CreateToggle("Item", content.transform, font, "Option");
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = new Vector2(0f, 44f);

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
            var top = 1f - index * 0.21f;
            SetRect(button, new Vector2(0f, top - 0.145f), new Vector2(1f, top), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(0f, -8f));
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

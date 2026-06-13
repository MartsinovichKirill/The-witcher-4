using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Core;
using WitcherRightVersion.Crafting;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Player;
using WitcherRightVersion.Quest;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Editor
{
    public static class ForestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/ForestScene.unity";

        [MenuItem("Tools/Witcher Right Version/Build Forest Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            AssetDatabase.Refresh();

            RemoveIfExists("Reynard_Player");
            RemoveIfExists("ThirdPersonCamera");
            RemoveIfExists("ForestBlockoutGround");
            RemoveIfExists("ForestMoodRoot");
            RemoveIfExists("ForestAtmosphereLights");
            RemoveIfExists("ForestInteractionRoot");
            RemoveIfExists("InteractionCanvas");
            RemoveIfExists("InventoryCanvas");
            RemoveIfExists("RuntimeServices");

            CreateRuntimeServices();
            CreateLighting();
            CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateForestMood();
            CreateAtmosphereLights();
            CreateInteractionObjects();
            CreateInteractionCanvas();
            CreateInventoryCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateRuntimeServices()
        {
            var services = new GameObject("RuntimeServices");
            services.AddComponent<AudioFeedbackService>();
            services.AddComponent<DecisionFlagService>();
            services.AddComponent<EndingService>();
            services.AddComponent<PlayerRewardService>();
            services.AddComponent<InventoryService>();
            services.AddComponent<CraftingService>();
            services.AddComponent<QuestService>();
            services.AddComponent<SaveService>();
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("ForestMoonLight");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.62f, 0.72f, 0.95f, 1f);
            light.intensity = 0.55f;
            light.transform.rotation = Quaternion.Euler(32f, -58f, 0f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.62f;

            var baseAmbient = new Color(0.18f, 0.21f, 0.18f, 1f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.21f, 0.30f, 1f);
            RenderSettings.ambientEquatorColor = baseAmbient;
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.08f, 0.06f, 1f);
            RenderSettings.ambientLight = baseAmbient;

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.16f, 0.22f, 0.24f, 1f);
            RenderSettings.fogDensity = 0.022f;

            // Authored moonlit night skybox; the sun reference draws the moon disc.
            RenderSettings.sun = light;
            RenderSettings.skybox = CreateSkyboxMaterial(
                "Assets/Materials/ForestNightSkybox.mat",
                0.035f, 0.65f,
                new Color(0.3f, 0.38f, 0.55f, 1f),
                new Color(0.06f, 0.07f, 0.075f, 1f),
                0.55f);
        }

        private static Material CreateSkyboxMaterial(string path, float sunSize, float atmosphereThickness, Color skyTint, Color groundColor, float exposure)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetFloat("_SunSize", sunSize);
            material.SetFloat("_SunSizeConvergence", 6f);
            material.SetFloat("_AtmosphereThickness", atmosphereThickness);
            material.SetColor("_SkyTint", skyTint);
            material.SetColor("_GroundColor", groundColor);
            material.SetFloat("_Exposure", exposure);
            return material;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ForestBlockoutGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ForestGround.mat", new Color(0.11f, 0.18f, 0.11f, 1f));
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Reynard_Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.05f, -4.2f);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2.05f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 1.02f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;

            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionController>();
            var health = player.AddComponent<Health>();
            health.Configure("Reynard", 120f);
            player.AddComponent<CombatController>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "ReynardForestPlaceholder";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            visual.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ReynardPlaceholder.mat", new Color(0.22f, 0.25f, 0.23f, 1f));
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            return player;
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("ThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -9f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            camera.allowHDR = true;

            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<ThirdPersonCamera>();
            follow.Target = target;
        }

        private static void CreateForestMood()
        {
            var root = new GameObject("ForestMoodRoot");
            root.transform.position = Vector3.zero;

            CreateTree(root.transform, "ForestTree_01", new Vector3(-3.8f, 0f, -1.8f), 1.25f);
            CreateTree(root.transform, "ForestTree_02", new Vector3(3.4f, 0f, -1.2f), 1.1f);
            CreateTree(root.transform, "ForestTree_03", new Vector3(-2.9f, 0f, 3.2f), 1.35f);
            CreateTree(root.transform, "ForestTree_04", new Vector3(4.8f, 0f, 3.8f), 1.2f);
            CreateRock(root.transform, "ForestRock_01", new Vector3(1.9f, 0.25f, 2.4f), new Vector3(0.85f, 0.45f, 0.65f));
            CreateRock(root.transform, "ForestRock_02", new Vector3(-4.7f, 0.2f, 1.1f), new Vector3(0.65f, 0.35f, 0.5f));

            // Outer treeline clusters (irregular spacing, varied scale) to build depth toward the fog.
            CreateTree(root.transform, "ForestTree_05", new Vector3(-6.9f, 0f, 4.6f), 1.45f);
            CreateTree(root.transform, "ForestTree_06", new Vector3(-5.6f, 0f, 6.3f), 1.15f);
            CreateTree(root.transform, "ForestTree_07", new Vector3(-7.8f, 0f, 1.4f), 1.3f);
            CreateTree(root.transform, "ForestTree_08", new Vector3(6.4f, 0f, 5.7f), 1.4f);
            CreateTree(root.transform, "ForestTree_09", new Vector3(7.5f, 0f, 2.1f), 1.2f);
            CreateTree(root.transform, "ForestTree_10", new Vector3(5.1f, 0f, 7.2f), 1.05f);
            CreateTree(root.transform, "ForestTree_11", new Vector3(-1.1f, 0f, 7.6f), 1.5f);
            CreateTree(root.transform, "ForestTree_12", new Vector3(1.8f, 0f, 6.9f), 1.25f);
            CreateTree(root.transform, "ForestTree_13", new Vector3(-8.3f, 0f, -2.4f), 1.35f);
            CreateTree(root.transform, "ForestTree_14", new Vector3(7.9f, 0f, -2.9f), 1.3f);

            // Scattered undergrowth rocks tucked against the treelines.
            CreateRock(root.transform, "ForestRock_03", new Vector3(-6.2f, 0.18f, 3.5f), new Vector3(0.55f, 0.3f, 0.45f));
            CreateRock(root.transform, "ForestRock_04", new Vector3(6.1f, 0.22f, 4.4f), new Vector3(0.7f, 0.4f, 0.55f));
            CreateRock(root.transform, "ForestRock_05", new Vector3(0.4f, 0.16f, 6.2f), new Vector3(0.5f, 0.28f, 0.42f));
            CreateRock(root.transform, "ForestRock_06", new Vector3(-3.4f, 0.2f, 5.4f), new Vector3(0.62f, 0.34f, 0.5f));

            // Low ground-mist markers nestled in the clusters to catch the moonlight.
            CreateMistMarker(root.transform, "ForestMist_01", new Vector3(-5.0f, 0.05f, 4.8f), 2.4f);
            CreateMistMarker(root.transform, "ForestMist_02", new Vector3(5.6f, 0.05f, 5.1f), 2.1f);
            CreateMistMarker(root.transform, "ForestMist_03", new Vector3(0.2f, 0.05f, 6.6f), 2.7f);
            CreateMistMarker(root.transform, "ForestMist_04", new Vector3(-1.4f, 0.05f, 1.6f), 1.8f);
        }

        private static void CreateMistMarker(Transform parent, string name, Vector3 position, float radius)
        {
            var mist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mist.name = name;
            mist.transform.SetParent(parent, true);
            mist.transform.position = position;
            mist.transform.localScale = new Vector3(radius, 0.35f, radius);
            mist.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ForestGroundMist.mat", new Color(0.6f, 0.66f, 0.7f, 0.12f));
            Object.DestroyImmediate(mist.GetComponent<Collider>());
        }

        private static void CreateAtmosphereLights()
        {
            var root = new GameObject("ForestAtmosphereLights");
            root.transform.position = Vector3.zero;

            // Cool moon-pool point lights resting on the forest floor between the clusters.
            CreatePointLight(root.transform, "MoonPool_01", new Vector3(-5.0f, 1.2f, 4.6f), new Color(0.55f, 0.66f, 0.92f, 1f), 0.65f, 7.5f);
            CreatePointLight(root.transform, "MoonPool_02", new Vector3(5.4f, 1.1f, 5.0f), new Color(0.5f, 0.62f, 0.9f, 1f), 0.6f, 7f);
            CreatePointLight(root.transform, "MoonPool_03", new Vector3(0.3f, 1.0f, 6.4f), new Color(0.52f, 0.64f, 0.9f, 1f), 0.55f, 6.5f);

            // Faint warm ember glow at the hunter camp landmark.
            CreatePointLight(root.transform, "CampEmberGlow", new Vector3(-1.65f, 0.6f, -1.2f), new Color(0.95f, 0.55f, 0.25f, 1f), 0.45f, 4.5f);

            // A single moon shaft cutting down through a canopy gap.
            CreateMoonShaft(root.transform, "MoonShaft_01", new Vector3(-2.0f, 7.5f, 3.0f), Quaternion.Euler(70f, 20f, 0f), new Color(0.6f, 0.7f, 0.95f, 1f), 0.85f, 16f, 34f);
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, true);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static void CreateMoonShaft(Transform parent, string name, Vector3 position, Quaternion rotation, Color color, float intensity, float range, float spotAngle)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, true);
            lightObject.transform.position = position;
            lightObject.transform.rotation = rotation;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = spotAngle;
            light.shadows = LightShadows.None;
        }

        private static void CreateInteractionObjects()
        {
            var root = new GameObject("ForestInteractionRoot");

            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "VillagePathTransition";
            back.transform.SetParent(root.transform, true);
            back.transform.position = new Vector3(0f, 0.55f, -5.65f);
            back.transform.localScale = new Vector3(2.2f, 1.1f, 0.55f);
            back.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/VillagePathTransition.mat", new Color(0.27f, 0.2f, 0.12f, 1f));

            var interactable = back.AddComponent<SceneTransitionInteractable>();
            interactable.Configure("Path back to Vereskovy Brod", "Travel", "VillageScene");

            CreateQuestObject(
                root.transform,
                "HunterCamp_Start",
                "Hunter camp",
                "Inspect",
                QuestService.ActionStartMissingHunter,
                "A cold campfire, torn bedroll, and fresh boot marks. Someone left in a hurry.",
                "The hunter camp has already been searched.",
                new Vector3(-1.65f, 0.12f, -1.2f),
                new Vector3(0.9f, 0.08f, 0.65f),
                new Color(0.24f, 0.15f, 0.08f, 1f));

            CreateQuestObject(
                root.transform,
                "HunterClue_BloodTrail",
                "Blood trail",
                "Inspect",
                QuestService.ActionMissingHunterClueFound,
                "Dark blood on fern leaves. The trail bends toward the deeper trees.",
                "This blood trail matters after the missing hunter search starts.",
                new Vector3(0.9f, 0.08f, 0.65f),
                new Vector3(0.65f, 0.035f, 0.95f),
                new Color(0.32f, 0.03f, 0.025f, 1f));

            CreateQuestObject(
                root.transform,
                "HunterClue_BrokenKnife",
                "Broken hunting knife",
                "Inspect",
                QuestService.ActionMissingHunterClueFound,
                "The blade snapped against something hard. No wolf did this.",
                "The knife is only a broken tool until the hunter search starts.",
                new Vector3(2.75f, 0.1f, 1.95f),
                new Vector3(0.55f, 0.04f, 0.25f),
                new Color(0.36f, 0.36f, 0.32f, 1f));

            CreateQuestObject(
                root.transform,
                "HunterCamp_RewardPouch",
                "Hunter's emergency pouch",
                "Take",
                QuestService.ActionMissingHunterReturned,
                "The signs are enough: the hunter was dragged north. The pouch marks the completed search.",
                "Find both hunter signs before claiming the pouch.",
                new Vector3(-2.25f, 0.18f, -0.65f),
                new Vector3(0.35f, 0.18f, 0.25f),
                new Color(0.48f, 0.34f, 0.16f, 1f),
                true);

            CreateQuestObject(
                root.transform,
                "OldCampBlade",
                "Old camp blade",
                "Take",
                QuestService.ActionOldCampBladeFound,
                "The blade is old, nicked, and still useful. Boris will want this.",
                "Boris has not asked Reynard to recover this blade yet.",
                new Vector3(4.05f, 0.12f, 2.85f),
                new Vector3(0.75f, 0.055f, 0.22f),
                new Color(0.42f, 0.4f, 0.34f, 1f));
        }

        private static void CreateQuestObject(Transform parent, string objectName, string displayName, string prompt, string questAction, string successMessage, string blockedMessage, Vector3 position, Vector3 scale, Color color, bool rewardPouch = false)
        {
            var marker = GameObject.CreatePrimitive(rewardPouch ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.SetParent(parent, true);
            marker.transform.position = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);

            var interactable = marker.AddComponent<QuestProgressInteractable>();
            interactable.Configure(displayName, prompt, questAction, successMessage, blockedMessage);
        }

        private static void CreateTree(Transform parent, string name, Vector3 position, float scale)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name;
            trunk.transform.SetParent(parent, true);
            trunk.transform.position = position + Vector3.up * (1.05f * scale);
            trunk.transform.localScale = new Vector3(0.22f * scale, 1.05f * scale, 0.22f * scale);
            trunk.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ForestTreeTrunk.mat", new Color(0.18f, 0.12f, 0.07f, 1f));
            Object.DestroyImmediate(trunk.GetComponent<Collider>());

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = $"{name}_Crown";
            crown.transform.SetParent(parent, true);
            crown.transform.position = position + Vector3.up * (2.35f * scale);
            crown.transform.localScale = new Vector3(1.25f * scale, 1f * scale, 1.25f * scale);
            crown.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ForestTreeCrown.mat", new Color(0.09f, 0.22f, 0.1f, 1f));
            Object.DestroyImmediate(crown.GetComponent<Collider>());
        }

        private static void CreateRock(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = name;
            rock.transform.SetParent(parent, true);
            rock.transform.position = position;
            rock.transform.localScale = scale;
            rock.transform.rotation = Quaternion.Euler(0f, 22f, 6f);
            rock.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ForestRock.mat", new Color(0.22f, 0.23f, 0.2f, 1f));
            Object.DestroyImmediate(rock.GetComponent<Collider>());
        }

        private static void CreateInteractionCanvas()
        {
            var canvasObject = new GameObject("InteractionCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var promptRoot = CreatePanel(canvasObject.transform, "InteractionPrompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(600f, 86f), new Vector2(0f, 76f), new Color(0.05f, 0.045f, 0.035f, 0.86f));
            var title = CreateText(promptRoot.transform, "PromptTitle", new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-36f, -16f), new Vector2(0f, -10f), 22, TextAnchor.MiddleLeft, new Color(0.94f, 0.84f, 0.58f, 1f));
            var action = CreateText(promptRoot.transform, "PromptAction", new Vector2(0f, 0f), new Vector2(1f, 0.58f), new Vector2(0.5f, 0f), new Vector2(-36f, -12f), new Vector2(0f, 10f), 18, TextAnchor.MiddleLeft, Color.white);

            var messageRoot = CreatePanel(canvasObject.transform, "InteractionMessage", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(720f, 64f), new Vector2(0f, 178f), new Color(0.08f, 0.07f, 0.055f, 0.9f));
            var message = CreateText(messageRoot.transform, "MessageText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-40f, -18f), Vector2.zero, 19, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.84f, 1f));

            var prompt = canvasObject.AddComponent<InteractionPromptUI>();
            SetSerializedObjectReference(prompt, "promptRoot", promptRoot);
            SetSerializedObjectReference(prompt, "titleText", title);
            SetSerializedObjectReference(prompt, "actionText", action);
            SetSerializedObjectReference(prompt, "messageRoot", messageRoot);
            SetSerializedObjectReference(prompt, "messageText", message);
        }

        private static void CreateInventoryCanvas()
        {
            var canvasObject = new GameObject("InventoryCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var panel = CreatePanel(
                canvasObject.transform,
                "InventoryPanel",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(720f, 600f),
                new Vector2(44f, -44f),
                new Color(0.045f, 0.04f, 0.033f, 0.93f));

            var content = CreateText(
                panel.transform,
                "InventoryContent",
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-56f, -48f),
                Vector2.zero,
                18,
                TextAnchor.UpperLeft,
                new Color(0.93f, 0.9f, 0.82f, 1f));

            var hud = canvasObject.AddComponent<InventoryHudUI>();
            SetSerializedObjectReference(hud, "panelRoot", panel);
            SetSerializedObjectReference(hud, "contentText", content);
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                material.color = color;
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;
            panel.AddComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition, int fontSize, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void SetSerializedObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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

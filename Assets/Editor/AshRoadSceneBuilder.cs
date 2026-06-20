using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    public static class AshRoadSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/AshRoadScene.unity";

        [MenuItem("Tools/Witcher Right Version/Build Ash Road Scene")]
        public static void Create()
        {
            var scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AssetDatabase.Refresh();

            RemoveIfExists("Reynard_Player");
            RemoveIfExists("ThirdPersonCamera");
            RemoveIfExists("AshRoadBlockoutGround");
            RemoveIfExists("AshRoadMoodRoot");
            RemoveIfExists("FinalTruthAltar");
            RemoveIfExists("InteractionCanvas");
            RemoveIfExists("EndingCanvas");
            RemoveIfExists("RuntimeServices");

            RemoveIfExists("AshRoadAtmosphereLights");

            CreateRuntimeServices();
            CreateLighting();
            CreateAtmosphereLights();
            CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateAshRoadMood();
            CreateFinalAltar();
            CreateInteractionCanvas();
            CreateEndingCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
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
            var lightObject = new GameObject("AshRoadSunsetLight");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            // Dim, warm-grey sun filtered through smoke; low harsh angle, soft shadows.
            light.intensity = 0.52f;
            light.color = new Color(0.82f, 0.7f, 0.58f, 1f);
            light.transform.rotation = Quaternion.Euler(16f, 38f, 0f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.6f;
            light.shadowBias = 0.05f;
            light.shadowNormalBias = 0.4f;

            // Trilight ambient derived from the previous flat ash color: warm-grey sky,
            // neutral equator, dark ashen ground.
            var baseAmbient = new Color(0.24f, 0.2f, 0.18f, 1f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.29f, 0.25f, 1f);
            RenderSettings.ambientEquatorColor = baseAmbient;
            RenderSettings.ambientGroundColor = new Color(0.11f, 0.1f, 0.09f, 1f);

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.34f, 0.29f, 0.26f, 1f);
            RenderSettings.fogDensity = 0.028f;

            // Authored smoke-hazed skybox; the sun reference draws a large filtered sun disc.
            RenderSettings.sun = light;
            RenderSettings.skybox = CreateSkyboxMaterial(
                "Assets/Materials/AshRoadSmokeSkybox.mat",
                0.07f, 1.9f,
                new Color(0.5f, 0.4f, 0.32f, 1f),
                new Color(0.14f, 0.12f, 0.1f, 1f),
                0.8f);
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

        private static void CreateAtmosphereLights()
        {
            var root = new GameObject("AshRoadAtmosphereLights");
            root.transform.position = Vector3.zero;

            var ember = new Color(1f, 0.42f, 0.13f, 1f);
            var emberRed = new Color(1f, 0.27f, 0.1f, 1f);
            var fireWarm = new Color(1f, 0.58f, 0.24f, 1f);

            // Ember glows at burned structures (BrokenPost markers) and ash drifts.
            CreatePointLight(root.transform, "EmberGlow_Post_01", new Vector3(-1.8f, 0.9f, 2.9f), emberRed, 1.1f, 3.4f);
            CreatePointLight(root.transform, "EmberGlow_Post_02", new Vector3(1.9f, 0.7f, 3.3f), ember, 0.95f, 3.0f);
            CreatePointLight(root.transform, "EmberGlow_Ash_01", new Vector3(-2.4f, 0.35f, -0.6f), ember, 0.7f, 2.6f);
            CreatePointLight(root.transform, "EmberGlow_Ash_02", new Vector3(2.7f, 0.35f, 1.7f), emberRed, 0.65f, 2.6f);

            // Campfire near the road with a stronger flicker-warm core.
            CreatePointLight(root.transform, "Campfire_Core", new Vector3(-3.1f, 0.55f, -2.4f), fireWarm, 1.8f, 5.2f);
            CreatePointLight(root.transform, "Campfire_Bounce", new Vector3(-3.1f, 1.6f, -2.4f), ember, 0.6f, 4.0f);

            // One warm firelight spot grazing the road surface.
            CreateFireSpot(root.transform, "FirelightSpot", new Vector3(-3.1f, 2.6f, -2.4f), Quaternion.Euler(70f, 40f, 0f), fireWarm, 1.4f, 9f, 62f);
        }

        private static Light CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
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
            return light;
        }

        private static Light CreateFireSpot(Transform parent, string name, Vector3 position, Quaternion rotation, Color color, float intensity, float range, float spotAngle)
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
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.45f;
            return light;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "AshRoadBlockoutGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/AshRoadGround.mat", new Color(0.15f, 0.13f, 0.12f, 1f));
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Reynard_Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.05f, -4.4f);

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
            visual.name = "ReynardAshRoadPlaceholder";
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

        private static void CreateAshRoadMood()
        {
            var root = new GameObject("AshRoadMoodRoot");
            root.transform.position = Vector3.zero;

            CreateMarker(root.transform, "BurnedRoadStrip", new Vector3(0f, 0.04f, 0f), new Vector3(2.2f, 0.035f, 9.4f), new Color(0.09f, 0.08f, 0.075f, 1f));
            CreateMarker(root.transform, "AshPile_01", new Vector3(-2.4f, 0.12f, -0.6f), new Vector3(0.9f, 0.22f, 0.7f), new Color(0.22f, 0.21f, 0.19f, 1f));
            CreateMarker(root.transform, "AshPile_02", new Vector3(2.7f, 0.12f, 1.7f), new Vector3(1.1f, 0.2f, 0.8f), new Color(0.2f, 0.18f, 0.16f, 1f));
            CreateMarker(root.transform, "BrokenPost_01", new Vector3(-1.8f, 0.65f, 2.9f), new Vector3(0.18f, 1.3f, 0.18f), new Color(0.18f, 0.11f, 0.07f, 1f));
            CreateMarker(root.transform, "BrokenPost_02", new Vector3(1.9f, 0.48f, 3.3f), new Vector3(0.16f, 0.95f, 0.16f), new Color(0.17f, 0.1f, 0.065f, 1f));

            // --- Added density: burned debris, charred stumps and ash drifts in believable clusters.
            var ashLight = new Color(0.23f, 0.22f, 0.2f, 1f);
            var ashDark = new Color(0.18f, 0.17f, 0.15f, 1f);
            var charred = new Color(0.13f, 0.09f, 0.07f, 1f);
            var emberCharcoal = new Color(0.1f, 0.075f, 0.065f, 1f);

            // Campfire cluster (south-west) tying in with the atmosphere lights.
            CreateRotatedMarker(root.transform, "Campfire_Ring", new Vector3(-3.1f, 0.1f, -2.4f), new Vector3(0.78f, 0.14f, 0.78f), Quaternion.Euler(0f, 22f, 0f), charred);
            CreateRotatedMarker(root.transform, "Campfire_Log_01", new Vector3(-3.32f, 0.14f, -2.28f), new Vector3(0.12f, 0.12f, 0.62f), Quaternion.Euler(4f, 34f, 8f), emberCharcoal);
            CreateRotatedMarker(root.transform, "Campfire_Log_02", new Vector3(-2.86f, 0.14f, -2.52f), new Vector3(0.12f, 0.12f, 0.58f), Quaternion.Euler(-3f, -28f, -6f), emberCharcoal);
            CreateMarker(root.transform, "Campfire_AshBed", new Vector3(-3.1f, 0.07f, -2.4f), new Vector3(0.5f, 0.06f, 0.5f), ashLight);

            // Charred stump cluster along the west verge.
            CreateRotatedMarker(root.transform, "CharredStump_01", new Vector3(-2.9f, 0.32f, 0.9f), new Vector3(0.42f, 0.6f, 0.42f), Quaternion.Euler(6f, 18f, 4f), charred);
            CreateRotatedMarker(root.transform, "CharredStump_02", new Vector3(-3.4f, 0.22f, 1.8f), new Vector3(0.34f, 0.42f, 0.34f), Quaternion.Euler(-5f, 52f, -7f), charred);
            CreateRotatedMarker(root.transform, "FallenTrunk_West", new Vector3(-3.05f, 0.2f, 0.2f), new Vector3(0.28f, 0.28f, 2.1f), Quaternion.Euler(2f, 74f, 90f), emberCharcoal);

            // Ash drifts feathering off both road edges.
            CreateRotatedMarker(root.transform, "AshDrift_01", new Vector3(-1.45f, 0.07f, -3.2f), new Vector3(1.3f, 0.1f, 0.9f), Quaternion.Euler(0f, 12f, 0f), ashDark);
            CreateRotatedMarker(root.transform, "AshDrift_02", new Vector3(1.5f, 0.07f, -2.0f), new Vector3(1.0f, 0.09f, 0.8f), Quaternion.Euler(0f, -24f, 0f), ashLight);
            CreateRotatedMarker(root.transform, "AshDrift_03", new Vector3(2.1f, 0.07f, 4.4f), new Vector3(1.4f, 0.1f, 1.0f), Quaternion.Euler(0f, 36f, 0f), ashDark);
            CreateRotatedMarker(root.transform, "AshDrift_04", new Vector3(-2.0f, 0.07f, 4.8f), new Vector3(1.1f, 0.08f, 0.85f), Quaternion.Euler(0f, -14f, 0f), ashLight);

            // Burned ruin debris cluster (east) – collapsed beams and a leaning post.
            CreateRotatedMarker(root.transform, "RuinBeam_01", new Vector3(2.6f, 0.16f, 0.4f), new Vector3(0.16f, 0.16f, 1.7f), Quaternion.Euler(0f, 18f, 84f), charred);
            CreateRotatedMarker(root.transform, "RuinBeam_02", new Vector3(2.9f, 0.24f, 0.7f), new Vector3(0.14f, 0.14f, 1.4f), Quaternion.Euler(0f, -42f, 70f), emberCharcoal);
            CreateRotatedMarker(root.transform, "LeaningPost_East", new Vector3(3.2f, 0.55f, 2.4f), new Vector3(0.16f, 1.1f, 0.16f), Quaternion.Euler(14f, 0f, 9f), charred);
            CreateRotatedMarker(root.transform, "RubbleHeap_East", new Vector3(2.95f, 0.18f, 2.0f), new Vector3(0.8f, 0.34f, 0.7f), Quaternion.Euler(0f, 28f, 0f), ashDark);

            // Scattered charcoal chunks dotting the road shoulders.
            CreateRotatedMarker(root.transform, "Charcoal_01", new Vector3(-0.9f, 0.09f, -1.8f), new Vector3(0.22f, 0.16f, 0.22f), Quaternion.Euler(0f, 24f, 0f), emberCharcoal);
            CreateRotatedMarker(root.transform, "Charcoal_02", new Vector3(0.8f, 0.09f, 0.6f), new Vector3(0.18f, 0.14f, 0.2f), Quaternion.Euler(0f, -36f, 0f), emberCharcoal);
            CreateRotatedMarker(root.transform, "Charcoal_03", new Vector3(1.1f, 0.09f, 5.1f), new Vector3(0.24f, 0.15f, 0.22f), Quaternion.Euler(0f, 50f, 0f), emberCharcoal);
        }

        private static void CreateRotatedMarker(Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, true);
            marker.transform.position = position;
            marker.transform.rotation = rotation;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static void CreateFinalAltar()
        {
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = "FinalTruthAltar";
            altar.transform.position = new Vector3(0f, 0.45f, 3.8f);
            altar.transform.localScale = new Vector3(0.95f, 0.45f, 0.95f);
            altar.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/FinalTruthAltar.mat", new Color(0.23f, 0.2f, 0.27f, 1f));

            var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = "FinalTruthAltar_MirrorShard";
            shard.transform.SetParent(altar.transform, true);
            shard.transform.position = new Vector3(0f, 1.05f, 3.8f);
            shard.transform.rotation = Quaternion.Euler(0f, 45f, 12f);
            shard.transform.localScale = new Vector3(0.15f, 0.9f, 0.55f);
            shard.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/FinalTruthShard.mat", new Color(0.36f, 0.28f, 0.48f, 1f));
            Object.DestroyImmediate(shard.GetComponent<Collider>());

            var interactable = altar.AddComponent<EndingAltarInteractable>();
            interactable.Configure("Final truth altar", "Choose truth");
        }

        private static void CreateMarker(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, true);
            marker.transform.position = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
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

            var promptRoot = CreatePanel(canvasObject.transform, "InteractionPrompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(620f, 86f), new Vector2(0f, 76f), new Color(0.05f, 0.045f, 0.035f, 0.86f));
            var title = CreateText(promptRoot.transform, "PromptTitle", new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-36f, -16f), new Vector2(0f, -10f), 22, TextAnchor.MiddleLeft, new Color(0.94f, 0.84f, 0.58f, 1f));
            var action = CreateText(promptRoot.transform, "PromptAction", new Vector2(0f, 0f), new Vector2(1f, 0.58f), new Vector2(0.5f, 0f), new Vector2(-36f, -12f), new Vector2(0f, 10f), 18, TextAnchor.MiddleLeft, Color.white);

            var messageRoot = CreatePanel(canvasObject.transform, "InteractionMessage", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(820f, 64f), new Vector2(0f, 178f), new Color(0.08f, 0.07f, 0.055f, 0.9f));
            var message = CreateText(messageRoot.transform, "MessageText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-40f, -18f), Vector2.zero, 18, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.84f, 1f));

            var prompt = canvasObject.AddComponent<InteractionPromptUI>();
            SetSerializedObjectReference(prompt, "promptRoot", promptRoot);
            SetSerializedObjectReference(prompt, "titleText", title);
            SetSerializedObjectReference(prompt, "actionText", action);
            SetSerializedObjectReference(prompt, "messageRoot", messageRoot);
            SetSerializedObjectReference(prompt, "messageText", message);
        }

        private static void CreateEndingCanvas()
        {
            var canvasObject = new GameObject("EndingCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var panel = CreatePanel(canvasObject.transform, "EndingPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(920f, 340f), Vector2.zero, new Color(0.045f, 0.038f, 0.032f, 0.94f));
            var title = CreateText(panel.transform, "EndingTitle", new Vector2(0f, 0.68f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-96f, -42f), new Vector2(0f, -34f), 42, TextAnchor.MiddleCenter, new Color(0.94f, 0.78f, 0.42f, 1f));
            var body = CreateText(panel.transform, "EndingBody", new Vector2(0f, 0f), new Vector2(1f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(-128f, -64f), new Vector2(0f, 18f), 24, TextAnchor.MiddleCenter, new Color(0.94f, 0.91f, 0.84f, 1f));

            var endingHud = canvasObject.AddComponent<EndingHudUI>();
            SetSerializedObjectReference(endingHud, "panelRoot", panel);
            SetSerializedObjectReference(endingHud, "titleText", title);
            SetSerializedObjectReference(endingHud, "bodyText", body);
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

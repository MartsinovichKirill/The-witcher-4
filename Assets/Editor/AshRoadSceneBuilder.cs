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
            RemoveIfExists("RuntimeServices");

            CreateRuntimeServices();
            CreateLighting();
            CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateAshRoadMood();
            CreateFinalAltar();
            CreateInteractionCanvas();

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
            light.intensity = 0.74f;
            light.transform.rotation = Quaternion.Euler(42f, 28f, 0f);

            RenderSettings.ambientLight = new Color(0.24f, 0.2f, 0.18f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.36f, 0.31f, 0.28f, 1f);
            RenderSettings.fogDensity = 0.02f;
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
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;

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

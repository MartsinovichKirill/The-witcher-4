using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.Player;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Editor
{
    public static class VillageSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/VillageScene.unity";

        [MenuItem("Tools/Witcher Right Version/Build Village Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            AssetDatabase.Refresh();

            RemoveIfExists("Reynard_Player");
            RemoveIfExists("ThirdPersonCamera");
            RemoveIfExists("VillageMovementTestArea");
            RemoveIfExists("VillageBlockoutGround");
            RemoveIfExists("VillagePropRoot");
            RemoveIfExists("InteractionCanvas");
            RemoveIfExists("InteractionDemoRoot");

            CreateLighting();
            var ground = CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateMovementMarkers(ground.transform);
            CreateVillageProps();
            CreateInteractionDemoObjects();
            CreateInteractionCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "VillageBlockoutGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);

            var material = CreateMaterial("Assets/Materials/VillageGround.mat", new Color(0.16f, 0.22f, 0.15f, 1f));
            ground.GetComponent<Renderer>().sharedMaterial = material;
            return ground;
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Reynard_Player");
            player.name = "Reynard_Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.05f, 0f);
            player.transform.rotation = Quaternion.identity;

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2.05f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 1.02f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;

            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionController>();
            CreatePlayerVisual(player.transform);
            return player;
        }

        private static void CreatePlayerVisual(Transform player)
        {
            var knight = InstantiateModel(
                "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/KnightCharacter.fbx",
                "ReynardKnightModel",
                player,
                new Vector3(0f, -0.02f, 0f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one);

            if (knight == null)
            {
                CreateFallbackPlayerVisual(player);
                return;
            }

            InstantiateModel(
                "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/Sword.fbx",
                "ReynardSteelSword_Visual",
                player,
                new Vector3(-0.32f, 1.2f, -0.18f),
                Quaternion.Euler(65f, 0f, 25f),
                new Vector3(0.9f, 0.9f, 0.9f));

            InstantiateModel(
                "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/ShortSword.fbx",
                "ReynardSilverSword_Visual",
                player,
                new Vector3(0.32f, 1.12f, -0.18f),
                Quaternion.Euler(65f, 0f, -25f),
                new Vector3(0.9f, 0.9f, 0.9f));
        }

        private static void CreateFallbackPlayerVisual(Transform player)
        {
            var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "ReynardPlaceholderCapsule";
            fallback.transform.SetParent(player, false);
            fallback.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            fallback.transform.localRotation = Quaternion.identity;
            fallback.transform.localScale = Vector3.one;

            var collider = fallback.GetComponent<CapsuleCollider>();
            Object.DestroyImmediate(collider);

            var material = CreateMaterial("Assets/Materials/ReynardPlaceholder.mat", new Color(0.22f, 0.25f, 0.23f, 1f));
            fallback.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("ThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -6f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;

            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<ThirdPersonCamera>();
            follow.Target = target;
        }

        private static void CreateLighting()
        {
            var light = Object.FindAnyObjectByType<Light>();
            if (light == null)
            {
                var lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.name = "VillageSunLight";
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.28f, 0.3f, 0.26f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.38f, 0.42f, 0.38f, 1f);
            RenderSettings.fogDensity = 0.012f;
        }

        private static void CreateMovementMarkers(Transform parent)
        {
            var root = new GameObject("VillageMovementTestArea");
            root.transform.position = Vector3.zero;

            CreateMarker(root.transform, "NorthMarker", new Vector3(0f, 0.5f, 10f), new Color(0.53f, 0.42f, 0.18f, 1f));
            CreateMarker(root.transform, "EastMarker", new Vector3(10f, 0.5f, 0f), new Color(0.42f, 0.16f, 0.12f, 1f));
            CreateMarker(root.transform, "SouthMarker", new Vector3(0f, 0.5f, -10f), new Color(0.18f, 0.32f, 0.18f, 1f));
            CreateMarker(root.transform, "WestMarker", new Vector3(-10f, 0.5f, 0f), new Color(0.22f, 0.2f, 0.28f, 1f));
        }

        private static void CreateMarker(Transform parent, string name, Vector3 position, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, true);
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(1f, 1f, 1f);
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
        }

        private static void CreateVillageProps()
        {
            var root = new GameObject("VillagePropRoot");
            root.transform.position = Vector3.zero;

            PlaceProp(root.transform, "VillageRoad_01", "road.fbx", new Vector3(0f, 0.02f, -4f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageRoad_02", "road.fbx", new Vector3(0f, 0.02f, 0f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageRoad_03", "road.fbx", new Vector3(0f, 0.02f, 4f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageCart_01", "cart.fbx", new Vector3(-3.6f, 0f, 2.4f), Quaternion.Euler(0f, 35f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageFence_01", "fence.fbx", new Vector3(3.2f, 0f, 2.8f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageFence_02", "fence-broken.fbx", new Vector3(3.2f, 0f, 4.2f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageLantern_01", "lantern.fbx", new Vector3(-1.9f, 0f, 3.8f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageRock_01", "rock-small.fbx", new Vector3(4.4f, 0f, -2.6f), Quaternion.Euler(0f, 20f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageBanner_01", "banner-red.fbx", new Vector3(-4.4f, 0f, -1.4f), Quaternion.Euler(0f, -35f, 0f), Vector3.one);
        }

        private static void CreateInteractionDemoObjects()
        {
            var root = new GameObject("InteractionDemoRoot");
            root.transform.position = Vector3.zero;

            CreateInteractableCapsule(
                root.transform,
                "ElderVoytsekh_Prototype",
                "Elder Voytsekh",
                "Talk",
                "Voytsekh: The beast was seen near the swamp road.",
                new Vector3(2.2f, 1f, -1.7f),
                new Color(0.22f, 0.18f, 0.13f, 1f));

            CreateInteractableCapsule(
                root.transform,
                "MartaLozovaya_Prototype",
                "Marta Lozovaya",
                "Talk",
                "Marta: Bring me herbs and I will teach you antitoxin.",
                new Vector3(-2.4f, 1f, -1.3f),
                new Color(0.16f, 0.24f, 0.17f, 1f));

            CreateInteractableTrace(
                root.transform,
                "SwampTrace_Prototype",
                "Swamp tracks",
                "Inspect",
                "Fresh mud, torn reeds, and a rotten smell. The trail leads south.",
                new Vector3(0.8f, 0.08f, 3.2f));
        }

        private static void CreateInteractableCapsule(Transform parent, string objectName, string displayName, string prompt, string message, Vector3 position, Color color)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = objectName;
            npc.transform.SetParent(parent, true);
            npc.transform.position = position;
            npc.transform.rotation = Quaternion.identity;
            npc.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            npc.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);

            var interactable = npc.AddComponent<SimpleInteractable>();
            interactable.Configure(displayName, prompt, message);
        }

        private static void CreateInteractableTrace(Transform parent, string objectName, string displayName, string prompt, string message, Vector3 position)
        {
            var trace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trace.name = objectName;
            trace.transform.SetParent(parent, true);
            trace.transform.position = position;
            trace.transform.rotation = Quaternion.identity;
            trace.transform.localScale = new Vector3(0.75f, 0.04f, 0.75f);
            trace.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", new Color(0.18f, 0.12f, 0.08f, 1f));

            var interactable = trace.AddComponent<SimpleInteractable>();
            interactable.Configure(displayName, prompt, message);
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

            var promptRoot = CreatePanel(canvasObject.transform, "InteractionPrompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(520f, 86f), new Vector2(0f, 76f), new Color(0.05f, 0.045f, 0.035f, 0.86f));
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

        private static void PlaceProp(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var prop = InstantiateModel(
                $"Assets/Art/External/Kenney_FantasyTownKit/Models/FBX format/{modelName}",
                objectName,
                parent,
                position,
                rotation,
                scale);

            if (prop != null)
            {
                return;
            }

            var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = objectName + "_Placeholder";
            placeholder.transform.SetParent(parent, false);
            placeholder.transform.localPosition = position + Vector3.up * 0.3f;
            placeholder.transform.localRotation = rotation;
            placeholder.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            placeholder.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/VillagePropPlaceholder.mat", new Color(0.28f, 0.22f, 0.14f, 1f));
        }

        private static GameObject InstantiateModel(string assetPath, string objectName, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning($"Model asset not found: {assetPath}");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(asset);
            }

            instance.name = objectName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            return instance;
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

            var image = panel.AddComponent<Image>();
            image.color = color;
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

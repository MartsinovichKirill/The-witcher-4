using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WitcherRightVersion.Player;

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

            CreateLighting();
            var ground = CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateMovementMarkers(ground.transform);
            CreateVillageProps();

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

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

            RemoveIfExists("Reynard_Player");
            RemoveIfExists("ThirdPersonCamera");
            RemoveIfExists("VillageMovementTestArea");
            RemoveIfExists("VillageBlockoutGround");

            CreateLighting();
            var ground = CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateMovementMarkers(ground.transform);

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
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Reynard_Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.05f, 0f);
            player.transform.rotation = Quaternion.identity;

            var collider = player.GetComponent<CapsuleCollider>();
            Object.DestroyImmediate(collider);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2.05f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 1.02f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;

            player.AddComponent<PlayerController>();

            var material = CreateMaterial("Assets/Materials/ReynardPlaceholder.mat", new Color(0.22f, 0.25f, 0.23f, 1f));
            player.GetComponent<Renderer>().sharedMaterial = material;
            return player;
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
            var light = Object.FindFirstObjectByType<Light>();
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

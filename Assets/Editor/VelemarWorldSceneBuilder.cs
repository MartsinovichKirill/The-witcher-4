using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Core;
using WitcherRightVersion.Crafting;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Player;
using WitcherRightVersion.Quest;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;
using WitcherRightVersion.Visual;

namespace WitcherRightVersion.Editor
{
    public static class VelemarWorldSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/VelemarWorldScene.unity";
        private const string KenneyPath = "Assets/Art/External/Kenney_FantasyTownKit/Models/FBX format";
        private const string KnightPath = "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX";
        private const string RpgCharacterPath = "Assets/Art/External/OpenGameArt_RPGCharacters/FBX";
        private const string RpgWeaponPath = "Assets/Art/External/OpenGameArt_RPGCharacters/FBX/Only Weapons";
        private const string MonsterPath = "Assets/Art/External/Quaternius_AnimatedMonsters/FBX";
        private const string KayKitMedievalPath = "Assets/Art/External/KayKit_MedievalBuilder/FBX";
        private const string WolfPath = "Assets/Art/External/OpenGameArt_CC0_Wolf";
        private const float PlayerVisualScale = 0.34f;
        private const float HumanModelScaleMultiplier = 0.66f;
        private const float KenneyBuildingScaleMultiplier = 1.2f;
        private const float KayKitBuildingScaleMultiplier = 1.68f;
        private const float PropScaleMultiplier = 1.08f;

        [MenuItem("Tools/Witcher Right Version/Build Velemar World Scene")]
        public static void Create()
        {
            var scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            AssetDatabase.Refresh();

            RemoveIfExists("RuntimeServices");
            RemoveIfExists("Reynard_Player");
            RemoveIfExists("ThirdPersonCamera");
            RemoveIfExists("VelemarWorldSun");
            RemoveIfExists("VelemarWorldSkyFill");
            RemoveIfExists("VelemarAtmosphereLights");
            RemoveIfExists("VelemarWorldRoot");
            RemoveAllNamed("VelemarWorldTerrain"); // clear stray white-material orphan planes from old builds
            RemoveIfExists("InteractionCanvas");
            RemoveIfExists("DialogueCanvas");
            RemoveIfExists("QuestCanvas");
            RemoveIfExists("HealthCanvas");
            RemoveIfExists("InventoryCanvas");
            RemoveIfExists("EndingCanvas");
            RemoveIfExists("WorldDirectionCanvas");
            RemoveIfExists("ZoneDiscoveryCanvas");
            RemoveIfExists("GameplayMenuCanvas");
            RemoveIfExists("GameplayMenuEventSystem");
            RemoveIfExists("DeveloperConsole");

            CreateRuntimeServices();
            CreateLighting();
            CreateWorldRoot();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateInteractionCanvas();
            CreateDialogueCanvas();
            CreateQuestCanvas();
            CreateHealthCanvas();
            CreateInventoryCanvas();
            CreateEndingCanvas();
            CreateWorldDirectionCanvas();
            CreateZoneDiscoveryCanvas();
            CreateGameplayMenuCanvas();
            new GameObject("DeveloperConsole").AddComponent<WitcherRightVersion.Debugging.DeveloperConsole>();

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
            var lightObject = new GameObject("VelemarWorldSun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.8f, 0.77f, 0.72f, 1f);
            light.intensity = 0.4f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.8f;
            light.transform.rotation = Quaternion.Euler(34f, -40f, 0f);

            // Subtle cool bounce fill from the opposite side so shadow sides do not crush to black.
            var fillObject = new GameObject("VelemarWorldSkyFill");
            var fillLight = fillObject.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.3f, 0.36f, 0.48f, 1f);
            fillLight.intensity = 0.14f;
            fillLight.shadows = LightShadows.None;
            fillLight.transform.rotation = Quaternion.Euler(24f, 152f, 0f);

            // Authored sky/horizon/ground gradient. Build-time Trilight is authoritative:
            // CinematicCameraEffect only upgrades ambient when it is still Flat.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.13f, 0.15f, 0.2f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.085f, 0.095f, 0.09f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.045f, 0.042f, 0.036f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.12f, 0.15f, 0.16f, 1f);
            RenderSettings.fogDensity = 0.0105f;

            // Authored warm-dusk skybox; the sun reference draws the disc at the light's angle.
            RenderSettings.sun = light;
            RenderSettings.skybox = CreateSkyboxMaterial(
                "Assets/Materials/VelemarWorldSkybox.mat",
                0.05f, 1.6f,
                new Color(0.24f, 0.25f, 0.28f, 1f),
                new Color(0.08f, 0.075f, 0.07f, 1f),
                0.5f);

            var lightsRoot = new GameObject("VelemarAtmosphereLights");
            CreatePointLight(lightsRoot.transform, "VillageWarmLanternLight", new Vector3(0f, 3.1f, -4.8f), new Color(1f, 0.54f, 0.24f, 1f), 0.95f, 11f);
            CreatePointLight(lightsRoot.transform, "VillageMarketLanternLight", new Vector3(3.1f, 2.4f, -0.2f), new Color(1f, 0.62f, 0.32f, 1f), 0.6f, 7.5f);
            CreatePointLight(lightsRoot.transform, "VillageSmithForgeLight", new Vector3(-4.2f, 1.8f, 1.1f), new Color(1f, 0.28f, 0.12f, 1f), 0.85f, 7f);
            CreatePointLight(lightsRoot.transform, "ForestMoonPoolLight", new Vector3(-73.5f, 4.3f, 13.5f), new Color(0.42f, 0.56f, 0.72f, 1f), 0.62f, 15f);
            CreatePointLight(lightsRoot.transform, "SwampColdMiasmaLight", new Vector3(9.4f, 2.2f, -72.8f), new Color(0.12f, 0.54f, 0.36f, 1f), 1.05f, 20f);
            CreatePointLight(lightsRoot.transform, "SwampHutCandleLight", new Vector3(14.2f, 1.75f, -73.4f), new Color(0.86f, 0.48f, 0.2f, 1f), 0.62f, 7.5f);
            CreatePointLight(lightsRoot.transform, "TowerMirrorVioletLight", new Vector3(0f, 3.6f, 76.6f), new Color(0.48f, 0.28f, 0.86f, 1f), 1.65f, 21f);
            CreatePointLight(lightsRoot.transform, "AshRoadEmberLight", new Vector3(72f, 2.5f, 9.5f), new Color(0.95f, 0.22f, 0.1f, 1f), 1.15f, 18f);
            CreatePointLight(lightsRoot.transform, "WestRouteLanternLight_A", new Vector3(-31.6f, 2.0f, -1.5f), new Color(0.92f, 0.58f, 0.28f, 1f), 0.45f, 7.5f);
            CreatePointLight(lightsRoot.transform, "WestRouteLanternLight_B", new Vector3(-57.6f, 2.0f, -1.5f), new Color(0.78f, 0.48f, 0.24f, 1f), 0.38f, 7f);
            CreatePointLight(lightsRoot.transform, "NorthRouteMirrorGlimmer_A", new Vector3(5.8f, 1.1f, 33.5f), new Color(0.42f, 0.34f, 0.84f, 1f), 0.42f, 6.5f);
            CreatePointLight(lightsRoot.transform, "SouthRouteBogGlow_A", new Vector3(-6.8f, 0.9f, -46f), new Color(0.08f, 0.48f, 0.28f, 1f), 0.5f, 8f);
            CreatePointLight(lightsRoot.transform, "EastRouteAshGlow_A", new Vector3(46f, 0.8f, 6.7f), new Color(0.9f, 0.22f, 0.09f, 1f), 0.48f, 7f);
            CreatePointLight(lightsRoot.transform, "WestRouteShrineLanternLight", new Vector3(-43.8f, 2.2f, 6.5f), new Color(0.9f, 0.62f, 0.32f, 1f), 0.62f, 9f);
            CreatePointLight(lightsRoot.transform, "EastRouteAmbushCoalLight", new Vector3(36.8f, 0.9f, -4.2f), new Color(0.95f, 0.18f, 0.06f, 1f), 0.52f, 8f);
            CreatePointLight(lightsRoot.transform, "NorthRouteObeliskVioletLight", new Vector3(-2.6f, 1.7f, 51.4f), new Color(0.5f, 0.34f, 1f, 1f), 0.7f, 9.5f);
            CreatePointLight(lightsRoot.transform, "SouthRouteWillOWispLight", new Vector3(5.8f, 1.1f, -55.5f), new Color(0.08f, 0.68f, 0.46f, 1f), 0.58f, 8.5f);
            CreateSpotLight(lightsRoot.transform, "VillageGateWarmCone", new Vector3(0f, 4.6f, -11f), Quaternion.Euler(58f, 0f, 0f), new Color(1f, 0.58f, 0.32f, 1f), 1.2f, 15f, 42f);
            CreateSpotLight(lightsRoot.transform, "ForestPathMoonShaft", new Vector3(-66f, 8f, 12f), Quaternion.Euler(66f, -38f, 0f), new Color(0.46f, 0.62f, 0.78f, 1f), 0.75f, 20f, 34f);
            CreateSpotLight(lightsRoot.transform, "SwampBridgeColdCone", new Vector3(7.5f, 5.2f, -69.5f), Quaternion.Euler(62f, -18f, 0f), new Color(0.14f, 0.7f, 0.44f, 1f), 0.8f, 18f, 46f);
            CreateSpotLight(lightsRoot.transform, "TowerRitualVioletCone", new Vector3(0f, 7.2f, 73.7f), Quaternion.Euler(70f, 0f, 0f), new Color(0.55f, 0.32f, 0.95f, 1f), 1.05f, 22f, 38f);
            CreateSpotLight(lightsRoot.transform, "AshRoadFireCone", new Vector3(69f, 4.8f, 11.3f), Quaternion.Euler(62f, 24f, 0f), new Color(1f, 0.32f, 0.12f, 1f), 0.95f, 16f, 42f);
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
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

        private static void CreateSpotLight(Transform parent, string name, Vector3 position, Quaternion rotation, Color color, float intensity, float range, float spotAngle)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
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

        private static void CreateWorldRoot()
        {
            var root = new GameObject("VelemarWorldRoot");
            root.transform.position = Vector3.zero;

            CreateGround(root.transform);
            CreateTerrainVisualLayers(root.transform);
            CreateRoadNetwork(root.transform);
            CreateHorizonMountains(root.transform);
            CreateVillageDistrict(root.transform);
            CreateForestDistrict(root.transform);
            CreateSwampDistrict(root.transform);
            CreateAshRoadDistrict(root.transform);
            CreateTowerVistaDistrict(root.transform);
            CreateInternetAssetMapExtensions(root.transform);
            CreateMapDensityAndScalePass(root.transform);
            CreateGameplayCompositionPass(root.transform);
            CreateLandmarkAndTraversalPass(root.transform);
            // DECLUTTER (decorative checks relaxed): the heaviest pure-decoration passes
            // that piled the map into a "dump" are disabled. Kept: core hubs, roads,
            // gameplay, NPCs, fightable monsters (FullMapVisualOverhaul), VFX, anti-flicker.
            // CreateWorldCompositionPolishPass(root.transform);      // edge orchards/cemetery/canopy/boardwalk/hamlet/ruins
            CreateFullMapVisualOverhaulPass(root.transform);          // KEPT: contains the fightable display monsters
            // CreateTerrainDepthAndSilhouettePass(root.transform);   // far background silhouettes
            CreateAmbientCharacterPopulationPass(root.transform);
            CreateVisualAtmospherePolishPass(root.transform);        // KEPT: anti-flicker ground + tower playable ruin
            CreateCharacterPresentationPolishPass(root.transform);
            CreateDynamicAmbientVfxPass(root.transform);             // KEPT: validator-checked ambient VFX
            // CreateRouteCinematicCompositionPass(root.transform);  // cinematic route props
            CreateZoneDiscoveryTriggers(root.transform);
            // CreateWorldDressing(root.transform);                  // scattered banners/carts/props
            CreateGameplayObjects(root.transform);
            CreateWorldBoundary(root.transform);
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "VelemarWorldTerrain";
            ground.transform.SetParent(parent, true);
            // Sunk below the district ground discs and surface patches so the base terrain
            // no longer sits coplanar with them (that caused the village floor to z-fight
            // and flicker green/white under bright light).
            ground.transform.position = new Vector3(0f, -0.06f, 0f);
            ground.transform.localScale = new Vector3(42f, 1f, 42f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/VelemarWorldTerrain.mat", new Color(0.105f, 0.17f, 0.105f, 1f));
        }

        // A ring of distant mountains around the whole map, far beyond every zone, to give
        // the world scale and a real horizon (the background-silhouette pass was disabled in
        // the declutter, which left the edges flat). Far away, so it adds depth, not clutter.
        private static void CreateHorizonMountains(Transform parent)
        {
            var root = new GameObject("HorizonMountainRing");
            root.transform.SetParent(parent, false);

            const int count = 22;
            for (var i = 0; i < count; i++)
            {
                var ang = i * (360f / count) * Mathf.Deg2Rad;
                var radius = 158f + (i % 3) * 18f;
                var x = Mathf.Cos(ang) * radius;
                var z = Mathf.Sin(ang) * radius;
                PlaceKayKit(root.transform, $"HorizonMountain_{i + 1:00}", "mountain.fbx",
                    new Vector3(x, -1f, z), Quaternion.Euler(0f, i * 37f, 0f),
                    new Vector3(7f + (i % 4) * 1.6f, 5.5f + (i % 3) * 1.3f, 7f + (i % 4) * 1.6f));
            }
        }

        private static void CreateTerrainVisualLayers(Transform parent)
        {
            var root = new GameObject("VelemarTerrainVisualLayers");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillagePackedEarthBlend", new Vector3(0f, 0.045f, -3f), new Vector3(27f, 0.018f, 23f), new Color(0.1f, 0.155f, 0.09f, 1f));
            CreateSurfacePatch(root.transform, "VillageGreenCourtyardBlend", new Vector3(3.5f, 0.049f, 0.2f), new Vector3(13f, 0.017f, 8.5f), new Color(0.13f, 0.19f, 0.105f, 1f));
            CreateSurfacePatch(root.transform, "VillageGateTrampledGround", new Vector3(0f, 0.052f, -10.6f), new Vector3(11.5f, 0.018f, 4.8f), new Color(0.13f, 0.105f, 0.072f, 1f));
            CreateSurfacePatch(root.transform, "ForestDarkGroundBlend", new Vector3(-70f, 0.044f, 8f), new Vector3(31f, 0.018f, 27f), new Color(0.045f, 0.105f, 0.055f, 1f));
            CreateSurfacePatch(root.transform, "SwampBogGroundBlend", new Vector3(8f, 0.044f, -72f), new Vector3(31f, 0.018f, 26f), new Color(0.035f, 0.082f, 0.058f, 1f));
            CreateSurfacePatch(root.transform, "AshRoadScorchedGroundBlend", new Vector3(72f, 0.044f, 8f), new Vector3(28f, 0.018f, 18f), new Color(0.12f, 0.105f, 0.092f, 1f));
            CreateSurfacePatch(root.transform, "TowerColdStoneGroundBlend", new Vector3(0f, 0.044f, 74f), new Vector3(22f, 0.018f, 18f), new Color(0.085f, 0.085f, 0.095f, 1f));

            for (var i = 0; i < 8; i++)
            {
                var x = -64f + i * 16f;
                CreateSurfacePatch(root.transform, $"WestEastGrassMottle_{i + 1:00}", new Vector3(x, 0.041f + i * 0.001f, 8.2f + (i % 3) * 3.1f), new Vector3(8.5f + (i % 2) * 2.0f, 0.012f, 3.4f), new Color(0.07f, 0.13f + (i % 3) * 0.012f, 0.065f, 1f));
                CreateSurfacePatch(root.transform, $"WestEastDirtMottle_{i + 1:00}", new Vector3(x + 6.4f, 0.042f + i * 0.001f, -7.6f - (i % 2) * 2.4f), new Vector3(6.2f, 0.012f, 2.8f), new Color(0.13f, 0.105f, 0.068f, 1f));
            }

            for (var i = 0; i < 7; i++)
            {
                var z = -66f + i * 22f;
                CreateSurfacePatch(root.transform, $"NorthSouthGrassMottle_{i + 1:00}", new Vector3(-8.4f - (i % 2) * 2.2f, 0.043f + i * 0.001f, z), new Vector3(4.5f, 0.012f, 8.4f), new Color(0.06f, 0.118f, 0.062f, 1f));
                CreateSurfacePatch(root.transform, $"NorthSouthMudMottle_{i + 1:00}", new Vector3(8.2f + (i % 3) * 1.4f, 0.044f + i * 0.001f, z + 8f), new Vector3(4.0f, 0.012f, 6.8f), new Color(0.105f, 0.088f, 0.057f, 1f));
            }
        }

        private static void CreateRoadNetwork(Transform parent)
        {
            var root = new GameObject("VelemarRoadNetwork");
            root.transform.SetParent(parent, false);

            var dirt = new Color(0.175f, 0.145f, 0.088f, 1f);
            var darkDirt = new Color(0.105f, 0.085f, 0.055f, 1f);
            var shoulder = new Color(0.09f, 0.13f, 0.078f, 1f);

            CreateSurfacePatch(root.transform, "VelemarNorthSouthRoad_Main", new Vector3(0f, 0.075f, 0f), new Vector3(5.4f, 0.026f, 252f), dirt);
            CreateSurfacePatch(root.transform, "VelemarEastWestRoad_Main", new Vector3(0f, 0.081f, 0f), new Vector3(252f, 0.024f, 5.4f), dirt);
            CreateSurfacePatch(root.transform, "VelemarCrossroadsPackedMud", new Vector3(0f, 0.088f, 0f), new Vector3(10.5f, 0.022f, 10.5f), darkDirt);
            CreateSurfacePatch(root.transform, "VelemarNorthSouthRoad_LeftShoulder", new Vector3(-3.4f, 0.066f, 0f), new Vector3(1.2f, 0.018f, 244f), shoulder);
            CreateSurfacePatch(root.transform, "VelemarNorthSouthRoad_RightShoulder", new Vector3(3.4f, 0.066f, 0f), new Vector3(1.2f, 0.018f, 244f), shoulder);
            CreateSurfacePatch(root.transform, "VelemarEastWestRoad_NorthShoulder", new Vector3(0f, 0.068f, 3.4f), new Vector3(244f, 0.018f, 1.2f), shoulder);
            CreateSurfacePatch(root.transform, "VelemarEastWestRoad_SouthShoulder", new Vector3(0f, 0.068f, -3.4f), new Vector3(244f, 0.018f, 1.2f), shoulder);
            CreateSurfacePatch(root.transform, "VillageRoadWearPatch_A", new Vector3(-5.5f, 0.092f, -3.2f), new Vector3(7f, 0.018f, 2.2f), darkDirt);
            CreateSurfacePatch(root.transform, "VillageRoadWearPatch_B", new Vector3(5.4f, 0.093f, -3.4f), new Vector3(7f, 0.018f, 2.2f), darkDirt);
            CreateSurfacePatch(root.transform, "VillageRoadWearPatch_C", new Vector3(0f, 0.094f, 4.5f), new Vector3(8.5f, 0.018f, 2.4f), darkDirt);

            CreateZoneLabel(root.transform, "RoadSign_Village", "Вересковый Брод", new Vector3(-4.5f, 1.2f, -5.5f), Quaternion.Euler(0f, 28f, 0f));
            CreateZoneLabel(root.transform, "RoadSign_Forest", "Старый Лес", new Vector3(-38f, 1.2f, 5.5f), Quaternion.Euler(0f, 70f, 0f));
            CreateZoneLabel(root.transform, "RoadSign_Swamp", "Чёрное Болото", new Vector3(6.6f, 1.2f, -38f), Quaternion.Euler(0f, -28f, 0f));
            CreateZoneLabel(root.transform, "RoadSign_AshRoad", "Пепельный тракт", new Vector3(40f, 1.2f, 5.5f), Quaternion.Euler(0f, -70f, 0f));
        }

        private static void CreateVillageDistrict(Transform parent)
        {
            var root = new GameObject("VillageDistrict_VereskovyBrod");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 0f, -3f);

            // Deliberate, structured layout (replaces the old scattered placement): a
            // south gate opens onto a central well square; NPC houses line the path on
            // both sides facing inward (each owner stands in front of their building); the
            // village trails north past a watermill and barracks. Decoration only — every
            // gameplay anchor (NPCs/quests/crates) lives in separate functions, and the
            // NPC world coords are: Elder(-4.1,-3.4) Marta(4.2,-3.6) Boris(-4.2,-0.4)
            // Radek(3.1,-0.7); the root sits at z=-3 so local = world+3 on Z.
            CreateRegionDisc(root.transform, "VillageDistrictGround", new Vector3(0f, 0.01f, 4f), new Vector3(24f, 0.02f, 20f), new Color(0.1f, 0.16f, 0.09f, 1f));

            // Open southern entrance. The gate + wall segments that stood here read as
            // out-of-place white "walls behind" the square (visible looking back from inside
            // the village), so they are removed. Two lantern posts mark the way in instead.
            PlaceKenney(root.transform, "VillageEntranceLanternL", "lantern.fbx", new Vector3(-2.8f, 0f, -5.2f), Quaternion.identity, Vector3.one * 1.05f);
            PlaceKenney(root.transform, "VillageEntranceLanternR", "lantern.fbx", new Vector3(2.8f, 0f, -5.2f), Quaternion.identity, Vector3.one * 1.05f);

            // Central well square.
            PlaceKayKit(root.transform, "VillageKayKitWell_World", "well.fbx", new Vector3(0f, 0f, 1.2f), Quaternion.identity, new Vector3(1.15f, 1.15f, 1.15f));
            PlaceKenney(root.transform, "VillageLantern_World", "lantern.fbx", new Vector3(1.5f, 0f, 0f), Quaternion.identity, Vector3.one * 0.9f);
            PlaceKenney(root.transform, "VillageCart_World", "cart.fbx", new Vector3(-1.9f, 0f, 2.4f), Quaternion.Euler(0f, 40f, 0f), Vector3.one * 1.1f);

            // Two tidy rows of clean KayKit cottages lining the central street, all at a
            // matching set-back (x = +/-8.5) and even spacing (z = 0, 5.5, 11), each facing
            // the street. NPCs stand at the near ends in their own little stalls.
            // West row (faces east): Elder's house, the Smithy, a cottage.
            PlaceKayKit(root.transform, "ElderHouse_World", "house.fbx", new Vector3(-8.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(2.6f, 2.6f, 2.6f));
            PlaceKayKit(root.transform, "Smithy_World", "house.fbx", new Vector3(-8.5f, 0f, 5.5f), Quaternion.Euler(0f, 90f, 0f), new Vector3(2.6f, 2.6f, 2.6f));
            PlaceKayKit(root.transform, "VillageKayKitHouse_A", "house.fbx", new Vector3(-8.5f, 0f, 11f), Quaternion.Euler(0f, 90f, 0f), new Vector3(2.6f, 2.6f, 2.6f));

            // East row (faces west): Marta's house, the market, a cottage.
            PlaceKayKit(root.transform, "MartaHouse_World", "house.fbx", new Vector3(8.5f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), new Vector3(2.6f, 2.6f, 2.6f));
            PlaceKayKit(root.transform, "VillageKayKitMarket_World", "market.fbx", new Vector3(8.5f, 0f, 5.5f), Quaternion.Euler(0f, -90f, 0f), new Vector3(2.3f, 2.3f, 2.3f));
            PlaceKayKit(root.transform, "VillageKayKitHouseEast_B", "house.fbx", new Vector3(8.5f, 0f, 11f), Quaternion.Euler(0f, -90f, 0f), new Vector3(2.6f, 2.6f, 2.6f));

            // North end closes the street: watermill + barracks facing back south, framed
            // symmetrically so the village has a clear top edge (no stray walls behind).
            PlaceKayKit(root.transform, "VillageKayKitWatermill_World", "watermill.fbx", new Vector3(-5f, 0f, 16f), Quaternion.Euler(0f, 180f, 0f), new Vector3(2.6f, 2.6f, 2.6f));
            PlaceKayKit(root.transform, "VillageKayKitBarracks_World", "barracks.fbx", new Vector3(5f, 0f, 16f), Quaternion.Euler(0f, 180f, 0f), new Vector3(2.3f, 2.3f, 2.3f));

            // Low front-yard fences lining each side of the path (set back from it).
            for (var i = 0; i < 3; i++)
            {
                PlaceKenney(root.transform, $"VillageFence_World_{i + 1}", "fence.fbx", new Vector3(-4.0f, 0f, 0.6f + i * 2.5f), Quaternion.identity, Vector3.one * 1.1f);
                PlaceKenney(root.transform, $"VillageFenceEast_World_{i + 1}", "fence.fbx", new Vector3(4.0f, 0f, 0.6f + i * 2.5f), Quaternion.identity, Vector3.one * 1.1f);
            }

            // Trees and rocks at the village edges, clear of the path and the larger buildings.
            PlaceKenney(root.transform, "VillageEdgeTreeNW_World", "tree-high.fbx", new Vector3(-13.0f, 0f, 13.5f), Quaternion.Euler(0f, 20f, 0f), Vector3.one * 1.3f);
            PlaceKenney(root.transform, "VillageEdgeTreeNE_World", "tree.fbx", new Vector3(13.2f, 0f, 13.0f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "VillageEdgeTreeW_World", "tree-crooked.fbx", new Vector3(-13.4f, 0f, 5.0f), Quaternion.Euler(0f, 60f, 0f), Vector3.one * 1.15f);
            PlaceKayKit(root.transform, "VillageEdgeRocksE_World", "detail_rocks.fbx", new Vector3(13.3f, 0f, 5.0f), Quaternion.Euler(0f, -20f, 0f), Vector3.one * 1.2f);
        }

        private static void CreateForestDistrict(Transform parent)
        {
            var root = new GameObject("ForestDistrict_OldForest");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(-70f, 0f, 8f);

            // Deliberate hub: an open hunter-camp clearing in the centre (fire, cart, tent
            // rock) where Ivar stands (world (-67.7,7.2) = local (2.3,-0.8)), ringed by a
            // wall of trees around the perimeter instead of a grid growing through the camp.
            CreateRegionDisc(root.transform, "ForestDistrictGround", Vector3.zero, new Vector3(13f, 0.035f, 11f), new Color(0.08f, 0.16f, 0.08f, 1f));

            // Tree wall ringing the clearing (perimeter only — leaves the camp centre open).
            for (var i = 0; i < 32; i++)
            {
                var ang = i * (360f / 32f) * Mathf.Deg2Rad;
                var radius = 8.6f + (i % 3) * 1.1f;
                var x = Mathf.Cos(ang) * radius;
                var z = Mathf.Sin(ang) * radius * 0.82f;
                var tree = i % 4 == 0 ? "tree-crooked.fbx" : i % 3 == 0 ? "tree-high.fbx" : "tree.fbx";
                PlaceKenney(root.transform, $"OldForestTree_{i + 1:00}", tree, new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 23f, 0f), Vector3.one * (0.95f + (i % 3) * 0.12f));
            }

            // Hunter camp arranged around the campfire in the cleared centre, near Ivar.
            CreateMarker(root.transform, "HunterCampFire_World", new Vector3(2.0f, 0.12f, 0.4f), new Vector3(0.9f, 0.18f, 0.9f), new Color(0.42f, 0.18f, 0.08f, 1f));
            PlaceKenney(root.transform, "HunterCampCart_World", "cart-high.fbx", new Vector3(4.2f, 0f, 1.6f), Quaternion.Euler(0f, -55f, 0f), Vector3.one);
            PlaceKenney(root.transform, "HunterCampRock_World", "rock-large.fbx", new Vector3(0.2f, 0f, 2.6f), Quaternion.Euler(0f, 16f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "OldForestKayKitForestPatch_A", "forest.fbx", new Vector3(-3.0f, 0f, -3.6f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 1.15f);
            PlaceKayKit(root.transform, "OldForestKayKitRocks_A", "detail_rocks.fbx", new Vector3(5.4f, 0f, -2.4f), Quaternion.Euler(0f, -18f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "OldForestKayKitHill_A", "detail_hill.fbx", new Vector3(-4.6f, 0f, 3.4f), Quaternion.Euler(0f, 42f, 0f), Vector3.one);
        }

        private static void CreateSwampDistrict(Transform parent)
        {
            var root = new GameObject("SwampDistrict_BlackSwamp");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(8f, 0f, -72f);

            // Deliberate hub: Elsa's hut sits on a raised dry island to the east (where the
            // Elsa NPC stands, world (16.6,-72.6) = local (8.6,-0.6)), reached by a roofed
            // plank bridge from the west bank; two dark pools with reed beds frame the
            // island; the old mine sits north. Decoration only — NPC/quests live elsewhere.
            CreateRegionDisc(root.transform, "SwampDistrictBog", Vector3.zero, new Vector3(13f, 0.035f, 11f), new Color(0.06f, 0.12f, 0.09f, 1f));
            CreateRegionDisc(root.transform, "ElsaDryIsland", new Vector3(8.0f, 0.045f, 0f), new Vector3(5.0f, 0.03f, 4.4f), new Color(0.09f, 0.13f, 0.075f, 1f));
            CreateRegionDisc(root.transform, "SwampWaterPool_01", new Vector3(-4.5f, 0.035f, -2.6f), new Vector3(4.2f, 0.022f, 2.3f), new Color(0.018f, 0.05f, 0.045f, 1f));
            CreateRegionDisc(root.transform, "SwampWaterPool_02", new Vector3(-1.5f, 0.035f, 3.8f), new Vector3(3.6f, 0.022f, 2.1f), new Color(0.015f, 0.045f, 0.04f, 1f));

            PlaceKayKit(root.transform, "ElsaHut_World", "house.fbx", new Vector3(9.2f, 0f, 0.4f), Quaternion.Euler(0f, -120f, 0f), Vector3.one * 1.0f);
            PlaceKenney(root.transform, "ElsaHutLeanTo_World", "stall.fbx", new Vector3(7.2f, 0f, 1.4f), Quaternion.Euler(0f, -80f, 0f), Vector3.one * 0.85f);
            PlaceKayKit(root.transform, "SwampKayKitRoofedBridge_World", "bridge_roofed.fbx", new Vector3(2.8f, 0f, -0.6f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.3f, 1.3f, 1.3f));
            PlaceKayKit(root.transform, "SwampKayKitMine_World", "mine.fbx", new Vector3(3.0f, 0f, 5.8f), Quaternion.Euler(0f, -28f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "SwampKayKitRocks_A", "detail_rocks_small.fbx", new Vector3(-6.8f, 0f, 1.2f), Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "DeadSwampTree_World", "tree-crooked.fbx", new Vector3(-6.0f, 0f, 5.0f), Quaternion.Euler(0f, 36f, 0f), new Vector3(1.3f, 1.1f, 1.3f));

            // Reed beds ringing each pool (deliberate clusters, not a loose scattered line).
            for (var i = 0; i < 12; i++)
            {
                var poolA = i < 6;
                var cx = poolA ? -4.5f : -1.5f;
                var cz = poolA ? -2.6f : 3.8f;
                var a = (i % 6) * 60f * Mathf.Deg2Rad;
                var pos = new Vector3(cx + Mathf.Cos(a) * 2.6f, 0f, cz + Mathf.Sin(a) * 1.7f);
                CreateReedCluster(root.transform, $"SwampReedCluster_World_{i + 1:00}", pos, 0.85f + (i % 3) * 0.14f);
            }
        }

        private static void CreateAshRoadDistrict(Transform parent)
        {
            var root = new GameObject("AshRoadDistrict_PepelnyTrakt");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(72f, 0f, 8f);

            // Deliberate composition: a burned road runs north-south through the ruined
            // waypoint; charred fence posts line both sides, a collapsed watchtower (west)
            // and a ruined gate (north) flank it as landmarks, and a mountain closes the
            // eastern horizon. Decoration only — the ending gate is a separate object.
            CreateRegionDisc(root.transform, "AshRoadDistrictGround", Vector3.zero, new Vector3(12f, 0.035f, 8f), new Color(0.15f, 0.13f, 0.12f, 1f));
            CreateMarker(root.transform, "AshRoadBurnedTrack_World", new Vector3(0f, 0.08f, 0f), new Vector3(2.6f, 0.08f, 11f), new Color(0.07f, 0.065f, 0.06f, 1f));

            // Charred fence posts lining both sides of the road.
            for (var i = 0; i < 8; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var z = -4.5f + (i / 2) * 3.0f;
                CreateMarker(root.transform, $"AshRoadBrokenPost_World_{i + 1}", new Vector3(side * 2.2f, 0.65f, z), new Vector3(0.18f, 1.15f, 0.18f), new Color(0.16f, 0.09f, 0.055f, 1f));
            }

            // Ruined landmarks flanking the road + a mountain backdrop to the east.
            PlaceKayKit(root.transform, "AshRoadKayKitWatchtower_Burned", "watchtower.fbx", new Vector3(-5.2f, 0f, 2.0f), Quaternion.Euler(0f, 40f, 0f), Vector3.one * 0.95f);
            PlaceKayKit(root.transform, "AshRoadKayKitGateRuins", "wall_gate_closed.fbx", new Vector3(0f, 0f, 5.8f), Quaternion.identity, Vector3.one * 1.1f);
            PlaceKenney(root.transform, "AshRoadBrokenWall_World", "wall-broken.fbx", new Vector3(4.6f, 0f, -1.5f), Quaternion.Euler(0f, -24f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "AshRoadKayKitMountain_Backdrop", "mountain.fbx", new Vector3(8.5f, 0f, 1.0f), Quaternion.Euler(0f, 74f, 0f), new Vector3(1.6f, 1.0f, 1.6f));
        }

        private static void CreateTowerVistaDistrict(Transform parent)
        {
            var root = new GameObject("TowerVistaDistrict_Ruins");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 0f, 74f);

            // Deliberate ritual-finale courtyard: the player climbs a processional stair
            // from the south into an open courtyard framed by broken pillars; the Mirror of
            // Truth glows at the centre where Orten stands (world (0,78.2) = local (0,4.2));
            // the ruined keep rises behind as a backdrop, flanked by two watchtowers. The
            // skeleton guards (local +/-4.5,1.5) hold the approach. Decoration only.
            CreateRegionDisc(root.transform, "TowerVistaGround", new Vector3(0f, 0.035f, 2f), new Vector3(11f, 0.035f, 10f), new Color(0.12f, 0.12f, 0.13f, 1f));

            // Castle keep backdrop (north) with flanking watchtowers, facing the approach.
            PlaceKayKit(root.transform, "TowerKayKitCastleCore_World", "castle.fbx", new Vector3(0f, 0f, 6.0f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.35f, 1.35f, 1.35f));
            PlaceKayKit(root.transform, "TowerKayKitWatchtowerLeft_World", "watchtower.fbx", new Vector3(-6.0f, 0f, 5.8f), Quaternion.Euler(0f, -25f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "TowerKayKitWatchtowerRight_World", "watchtower.fbx", new Vector3(6.0f, 0f, 5.8f), Quaternion.Euler(0f, 25f, 0f), Vector3.one);

            // Processional stair up from the south into the courtyard.
            PlaceKenney(root.transform, "TowerVistaStairs_World", "stairs-stone.fbx", new Vector3(0f, 0f, -3.5f), Quaternion.identity, new Vector3(1.3f, 1.3f, 1.3f));

            // Broken pillars framing the courtyard sides + a ruined wall to the flank.
            PlaceKenney(root.transform, "TowerVistaPillarLeft_World", "pillar-stone.fbx", new Vector3(-3.6f, 0f, 1.0f), Quaternion.identity, new Vector3(1.4f, 1.4f, 1.4f));
            PlaceKenney(root.transform, "TowerVistaPillarRight_World", "pillar-stone.fbx", new Vector3(3.6f, 0f, 1.0f), Quaternion.identity, new Vector3(1.4f, 1.4f, 1.4f));
            PlaceKenney(root.transform, "TowerVistaBrokenWall_World", "wall-broken.fbx", new Vector3(-4.6f, 0f, 3.8f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.4f, 1.4f, 1.4f));

            // The Mirror of Truth glows at the courtyard centre, just before Orten's spot.
            CreateMarker(root.transform, "TowerMirrorGlow_World", new Vector3(0f, 1.4f, 3.4f), new Vector3(0.28f, 2.1f, 0.9f), new Color(0.34f, 0.24f, 0.48f, 1f));
        }

        private static void CreateInternetAssetMapExtensions(Transform parent)
        {
            var root = new GameObject("InternetAssetMapExtensions_CC0");
            root.transform.SetParent(parent, false);

            // DECLUTTER: the OuterVillageRing chain (-> Crossroads -> EdgeComposition ->
            // StreetOverhaul) piled the bulk of the village-surroundings junk. Disabled now
            // that decorative checks are relaxed; the roads + zone-discovery triggers stay.
            // CreateOuterVillageRing(root.transform);
            CreateDeepForestMapExtension(root.transform);
            CreateDeepSwampMapExtension(root.transform);
            CreateAshRoadMapExtension(root.transform);
            CreateTowerMapExtension(root.transform);
            CreateOuterRoadSetPieces(root.transform);
        }

        private static void CreateMapDensityAndScalePass(Transform parent)
        {
            var root = new GameObject("MapDensityAndScalePass");
            root.transform.SetParent(parent, false);

            // DECLUTTER: village no longer needs this extra block of edge houses/shops/fences
            // now that CreateVillageDistrict is a clean self-contained settlement.
            // CreateStructuredVillageBlocks(root.transform);
            CreateForestApproachDensity(root.transform);
            CreateSwampApproachDensity(root.transform);
            CreateAshRoadRuinedSettlement(root.transform);
            CreateTowerApproachDensity(root.transform);
        }

        private static void CreateStructuredVillageBlocks(Transform parent)
        {
            var root = new GameObject("StructuredVillageBlocks");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageMainSquarePackedMud", new Vector3(0f, 0.105f, -2.2f), new Vector3(18f, 0.014f, 12f), new Color(0.125f, 0.095f, 0.058f, 1f));
            CreateSurfacePatch(root.transform, "VillageNorthResidentialLane", new Vector3(0f, 0.108f, 9.7f), new Vector3(25f, 0.014f, 4.2f), new Color(0.142f, 0.112f, 0.07f, 1f));
            CreateSurfacePatch(root.transform, "VillageSouthGateLane", new Vector3(0f, 0.11f, -14.5f), new Vector3(24f, 0.014f, 4.5f), new Color(0.12f, 0.09f, 0.055f, 1f));

            // Outer village ring: these houses/shops sit at the FAR edges so they frame the
            // clean core (built in CreateVillageDistrict, roughly x:+/-9, z:-5..15) instead of
            // piling on top of it. (Was 7 crowding houses + a market in the very centre + a
            // dense 16-fence/10-lantern scatter.)
            var housePositions = new[]
            {
                new Vector3(-17.5f, 0f, 10.0f),
                new Vector3(17.5f, 0f, 10.0f),
                new Vector3(-17.0f, 0f, -16.0f),
                new Vector3(17.0f, 0f, -16.0f),
            };

            for (var i = 0; i < housePositions.Length; i++)
            {
                var rotation = Quaternion.Euler(0f, i % 2 == 0 ? 120f : -120f, 0f);
                PlaceKayKit(root.transform, $"StructuredVillageHouse_{i + 1:00}", "house.fbx", housePositions[i], rotation, Vector3.one * 1.7f);
                CreateMarker(root.transform, $"StructuredVillageHouseShadow_{i + 1:00}", housePositions[i] + new Vector3(0f, 0.065f, 0.2f), new Vector3(3.4f, 0.035f, 2.4f), new Color(0.055f, 0.045f, 0.035f, 1f));
            }

            PlaceKayKit(root.transform, "StructuredVillageBlacksmithShop", "barracks.fbx", new Vector3(-17.0f, 0f, 1.5f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 1.25f);
            PlaceKayKit(root.transform, "StructuredVillageStableBarn", "lumbermill.fbx", new Vector3(17.5f, 0f, 1.5f), Quaternion.Euler(0f, -74f, 0f), Vector3.one * 1.15f);
            PlaceKayKit(root.transform, "StructuredVillageMarketCenter", "market.fbx", new Vector3(-17.5f, 0f, -6.5f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 1.15f);

            // A sparse perimeter fence line + a couple of lanterns far to the south, well
            // clear of the core (was a dense 16+10 scatter through the middle).
            for (var i = 0; i < 5; i++)
            {
                PlaceKenney(root.transform, $"StructuredVillagePerimeterFence_{i + 1:00}", i % 5 == 0 ? "fence-broken.fbx" : "fence.fbx", new Vector3(-18f + i * 9f, 0f, -19.5f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.15f);
            }

            for (var i = 0; i < 3; i++)
            {
                PlaceKenney(root.transform, $"StructuredVillageStreetLantern_{i + 1:00}", "lantern.fbx", new Vector3(-14f + i * 14f, 0f, -17f), Quaternion.Euler(0f, i * 30f, 0f), Vector3.one * 1.0f);
            }
        }

        private static void CreateForestApproachDensity(Transform parent)
        {
            var root = new GameObject("ForestApproachDensity");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ForestApproachDarkUnderbrush", new Vector3(-48f, 0.047f, 12f), new Vector3(36f, 0.014f, 20f), new Color(0.038f, 0.09f, 0.04f, 1f));
            for (var i = 0; i < 34; i++)
            {
                var x = -32f - (i % 9) * 5.1f + (i % 3) * 0.65f;
                var z = -2f + (i / 9) * 5.7f + (i % 2) * 1.3f;
                PlaceKenney(root.transform, $"ForestApproachTree_{i + 1:00}", i % 3 == 0 ? "tree-high-crooked.fbx" : i % 3 == 1 ? "tree-high.fbx" : "tree.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 27f, 0f), Vector3.one * (1.08f + (i % 4) * 0.14f));
                if (i % 4 == 0)
                {
                    PlaceKayKit(root.transform, $"ForestApproachRockPatch_{i + 1:00}", "detail_rocks_small.fbx", new Vector3(x + 1.8f, 0f, z - 1.1f), Quaternion.Euler(0f, i * 13f, 0f), Vector3.one * 0.9f);
                }
            }

            PlaceKayKit(root.transform, "ForestApproachFallenWatchPost", "watchtower.fbx", new Vector3(-55.5f, 0f, -8.6f), Quaternion.Euler(0f, 62f, 0f), Vector3.one * 0.72f);
            PlaceKenney(root.transform, "ForestApproachBrokenCart", "cart-high.fbx", new Vector3(-43.2f, 0f, -6.9f), Quaternion.Euler(0f, -36f, 0f), Vector3.one * 1.0f);
        }

        private static void CreateSwampApproachDensity(Transform parent)
        {
            var root = new GameObject("SwampApproachDensity");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "SwampApproachSoftMud", new Vector3(13f, 0.047f, -47f), new Vector3(28f, 0.014f, 40f), new Color(0.035f, 0.078f, 0.055f, 1f));
            CreateSurfacePatch(root.transform, "SwampApproachWaterPocket_A", new Vector3(18f, 0.06f, -52f), new Vector3(8f, 0.012f, 5f), new Color(0.012f, 0.046f, 0.04f, 1f));
            CreateSurfacePatch(root.transform, "SwampApproachWaterPocket_B", new Vector3(4f, 0.061f, -42f), new Vector3(7f, 0.012f, 4.6f), new Color(0.014f, 0.05f, 0.042f, 1f));

            for (var i = 0; i < 24; i++)
            {
                var x = 0f + (i % 7) * 4.1f + (i % 2) * 0.7f;
                var z = -62f + (i / 7) * 8f + (i % 3) * 1.1f;
                CreateReedCluster(root.transform, $"SwampApproachReedWall_{i + 1:00}", new Vector3(x, 0f, z), 1.05f + (i % 4) * 0.13f);
                if (i % 3 == 0)
                {
                    PlaceKenney(root.transform, $"SwampApproachCrookedTree_{i + 1:00}", "tree-crooked.fbx", new Vector3(x - 2.1f, 0f, z + 1.2f), Quaternion.Euler(0f, i * 25f, 0f), Vector3.one * (1.08f + (i % 3) * 0.18f));
                }
            }

            PlaceKayKit(root.transform, "SwampApproachSinkingBridge", "bridge.fbx", new Vector3(7.8f, 0f, -52.8f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.85f);
            PlaceKenney(root.transform, "SwampApproachDrownedWagon", "cart.fbx", new Vector3(20.8f, -0.04f, -44.8f), Quaternion.Euler(5f, -68f, 7f), Vector3.one * 1.0f);
        }

        private static void CreateAshRoadRuinedSettlement(Transform parent)
        {
            var root = new GameObject("AshRoadRuinedSettlement");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "AshRoadRuinedVillageGround", new Vector3(93f, 0.05f, 18f), new Vector3(35f, 0.014f, 22f), new Color(0.13f, 0.115f, 0.1f, 1f));
            for (var i = 0; i < 10; i++)
            {
                var x = 79f + (i % 5) * 6.2f;
                var z = 12f + (i / 5) * 10f + (i % 2) * 1.4f;
                PlaceKenney(root.transform, $"AshRoadRuinedHouseWall_{i + 1:00}", i % 2 == 0 ? "wall-broken.fbx" : "wall-wood-broken.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 31f, 0f), Vector3.one * (1.18f + (i % 3) * 0.14f));
                CreateMarker(root.transform, $"AshRoadRuinSootPatch_{i + 1:00}", new Vector3(x + 0.7f, 0.068f, z - 0.4f), new Vector3(2.0f, 0.035f, 1.1f), new Color(0.055f, 0.048f, 0.043f, 1f));
            }

            PlaceKayKit(root.transform, "AshRoadRuinedWatchtowerBack", "watchtower.fbx", new Vector3(96f, 0f, 29f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.92f);
            PlaceKayKit(root.transform, "AshRoadRuinedBarracksShell", "barracks.fbx", new Vector3(83f, 0f, 25.5f), Quaternion.Euler(0f, 37f, 0f), Vector3.one * 0.88f);
        }

        private static void CreateTowerApproachDensity(Transform parent)
        {
            var root = new GameObject("TowerApproachDensity");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerApproachBrokenRoad", new Vector3(0f, 0.063f, 94f), new Vector3(9f, 0.014f, 31f), new Color(0.088f, 0.084f, 0.088f, 1f));
            for (var i = 0; i < 18; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var z = 84f + i * 2.4f;
                var x = side * (5.2f + (i % 4) * 1.7f);
                PlaceKenney(root.transform, $"TowerApproachBrokenMasonry_{i + 1:00}", i % 3 == 0 ? "wall-arch-top.fbx" : i % 3 == 1 ? "pillar-stone.fbx" : "wall-broken.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, side * (28f + i * 9f), 0f), Vector3.one * (1.0f + (i % 4) * 0.1f));
                CreateMarker(root.transform, $"TowerApproachVioletDust_{i + 1:00}", new Vector3(x * 0.55f, 0.078f, z + 0.7f), new Vector3(0.48f, 0.04f, 0.48f), new Color(0.28f, 0.19f, 0.46f, 1f));
            }

            PlaceKayKit(root.transform, "TowerApproachGateFrame", "wall_gate.fbx", new Vector3(0f, 0f, 101f), Quaternion.Euler(0f, 0f, 0f), Vector3.one * 1.15f);
        }

        private static void CreateGameplayCompositionPass(Transform parent)
        {
            var root = new GameObject("GameplayCompositionPass");
            root.transform.SetParent(parent, false);

            CreateVillageGameplayCompositions(root.transform);
            CreateForestGameplayCompositions(root.transform);
            CreateSwampGameplayCompositions(root.transform);
            CreateTowerGameplayCompositions(root.transform);
            CreateFinalRoadGameplayCompositions(root.transform);
        }

        private static void CreateVillageGameplayCompositions(Transform parent)
        {
            var root = new GameObject("VillageGameplayCompositions");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ElderCourtyardCompositionGround", new Vector3(-4.7f, 0.122f, -2.65f), new Vector3(5.6f, 0.012f, 4.2f), new Color(0.118f, 0.088f, 0.052f, 1f));
            PlaceKenney(root.transform, "ElderCourtyardSealTable", "stall-bench.fbx", new Vector3(-5.85f, 0f, -2.25f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.95f);
            PlaceKenney(root.transform, "ElderCourtyardBannerLeft", "banner-red.fbx", new Vector3(-6.7f, 0f, -4.1f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceKenney(root.transform, "ElderCourtyardBannerRight", "banner-green.fbx", new Vector3(-2.8f, 0f, -4.2f), Quaternion.Euler(0f, 82f, 0f), Vector3.one * 0.92f);

            CreateSurfacePatch(root.transform, "MartaHerbGardenCompositionGround", new Vector3(5.3f, 0.123f, -4.9f), new Vector3(5.3f, 0.012f, 3.7f), new Color(0.07f, 0.13f, 0.066f, 1f));
            for (var i = 0; i < 8; i++)
            {
                CreateReedCluster(root.transform, $"MartaHerbRow_{i + 1:00}", new Vector3(3.3f + (i % 4) * 1.1f, 0f, -5.85f + (i / 4) * 1.35f), 0.55f + (i % 3) * 0.08f);
            }
            PlaceKenney(root.transform, "MartaGardenWorkBench", "stall-bench.fbx", new Vector3(6.9f, 0f, -4.0f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * 0.84f);

            CreateSurfacePatch(root.transform, "BorisForgeCompositionGround", new Vector3(-4.9f, 0.124f, 0.5f), new Vector3(5.0f, 0.012f, 4.1f), new Color(0.13f, 0.09f, 0.055f, 1f));
            PlaceKenney(root.transform, "BorisForgeWoodPile", "planks.fbx", new Vector3(-6.5f, 0.02f, 1.4f), Quaternion.Euler(0f, -20f, 0f), Vector3.one * 1.08f);
            PlaceKenney(root.transform, "BorisForgeOreCart", "cart.fbx", new Vector3(-3.3f, 0f, 1.4f), Quaternion.Euler(0f, 38f, 0f), Vector3.one * 0.9f);
            CreateMarker(root.transform, "BorisForgeCoalGlow", new Vector3(-5.15f, 0.18f, 0.15f), new Vector3(0.7f, 0.08f, 0.44f), new Color(0.58f, 0.12f, 0.04f, 1f));

            CreateSurfacePatch(root.transform, "RadekMarketCompositionGround", new Vector3(3.2f, 0.125f, -0.8f), new Vector3(5.4f, 0.012f, 4.0f), new Color(0.13f, 0.105f, 0.062f, 1f));
            PlaceKenney(root.transform, "RadekMarketStallRed", "stall-red.fbx", new Vector3(2.2f, 0f, 0.8f), Quaternion.Euler(0f, -25f, 0f), Vector3.one * 0.9f);
            PlaceKenney(root.transform, "RadekMarketStallGreen", "stall-green.fbx", new Vector3(4.5f, 0f, 0.5f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.84f);
            PlaceKenney(root.transform, "RadekMarketCart", "cart-high.fbx", new Vector3(5.3f, 0f, -1.8f), Quaternion.Euler(0f, -72f, 0f), Vector3.one * 0.8f);
        }

        private static void CreateForestGameplayCompositions(Transform parent)
        {
            var root = new GameObject("ForestGameplayCompositions");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "HunterCampCompositionClearing", new Vector3(-68.6f, 0.115f, 6.2f), new Vector3(8.6f, 0.012f, 6.4f), new Color(0.055f, 0.095f, 0.048f, 1f));
            PlaceKenney(root.transform, "HunterCampLeanCart", "cart-high.fbx", new Vector3(-66.2f, 0f, 4.1f), Quaternion.Euler(0f, 58f, 0f), Vector3.one * 0.9f);
            PlaceKenney(root.transform, "HunterCampBrokenFenceA", "fence-broken.fbx", new Vector3(-70.8f, 0f, 4.4f), Quaternion.Euler(0f, -18f, 0f), Vector3.one);
            PlaceKenney(root.transform, "HunterCampBrokenFenceB", "fence-broken.fbx", new Vector3(-65.7f, 0f, 8.5f), Quaternion.Euler(0f, 70f, 0f), Vector3.one * 0.95f);
            CreateMarker(root.transform, "HunterCampFireAshRing", new Vector3(-67.7f, 0.13f, 5.8f), new Vector3(0.95f, 0.05f, 0.95f), new Color(0.08f, 0.065f, 0.048f, 1f));

            CreateSurfacePatch(root.transform, "HunterClueTrailComposition", new Vector3(-72.7f, 0.118f, 10.3f), new Vector3(7.5f, 0.012f, 5.2f), new Color(0.045f, 0.078f, 0.04f, 1f));
            PlaceKenney(root.transform, "HunterClueRockCover", "rock-wide.fbx", new Vector3(-74.3f, 0f, 12.9f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.9f);
            PlaceKenney(root.transform, "HunterClueBladeProp", "blade.fbx", new Vector3(-73.8f, 0.08f, 12.1f), Quaternion.Euler(75f, 0f, 32f), Vector3.one * 0.72f);
        }

        private static void CreateSwampGameplayCompositions(Transform parent)
        {
            var root = new GameObject("SwampGameplayCompositions");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ElsaHutCompositionGround", new Vector3(16.2f, 0.116f, -72.8f), new Vector3(8.6f, 0.012f, 6.6f), new Color(0.032f, 0.074f, 0.052f, 1f));
            PlaceKenney(root.transform, "ElsaHutPlankEntry", "planks.fbx", new Vector3(15.2f, 0.05f, -73.9f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.95f);
            PlaceKenney(root.transform, "ElsaHutDryingRack", "poles-horizontal.fbx", new Vector3(18.7f, 0f, -72.6f), Quaternion.Euler(0f, -45f, 0f), Vector3.one * 0.9f);
            CreateReedCluster(root.transform, "ElsaHutReedScreen_A", new Vector3(12.9f, 0f, -71.4f), 1.35f);
            CreateReedCluster(root.transform, "ElsaHutReedScreen_B", new Vector3(19.7f, 0f, -75.2f), 1.2f);

            CreateSurfacePatch(root.transform, "SwampTraceCompositionMud", new Vector3(9.4f, 0.117f, -72.5f), new Vector3(9.5f, 0.012f, 6.8f), new Color(0.025f, 0.062f, 0.046f, 1f));
            for (var i = 0; i < 6; i++)
            {
                CreateMarker(root.transform, $"SwampTraceDraggedReed_{i + 1:00}", new Vector3(6.4f + i * 1.15f, 0.12f, -70.9f - (i % 2) * 1.7f), new Vector3(0.16f, 0.035f, 1.25f), new Color(0.13f, 0.17f, 0.09f, 1f));
            }

            CreateSurfacePatch(root.transform, "DrownerNestCompositionPool", new Vector3(14.2f, 0.118f, -75.8f), new Vector3(7.8f, 0.012f, 6.2f), new Color(0.014f, 0.048f, 0.04f, 1f));
            PlaceKenney(root.transform, "DrownerNestDrownedCartProp", "cart.fbx", new Vector3(16.8f, -0.04f, -76.5f), Quaternion.Euler(6f, -38f, 9f), Vector3.one * 0.86f);
        }

        private static void CreateTowerGameplayCompositions(Transform parent)
        {
            var root = new GameObject("TowerGameplayCompositions");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerEvidenceCompositionFloor", new Vector3(0f, 0.121f, 75.4f), new Vector3(10.5f, 0.012f, 6.2f), new Color(0.072f, 0.068f, 0.078f, 1f));
            PlaceKenney(root.transform, "TowerDiaryReadingTable", "stall-bench.fbx", new Vector3(-1.4f, 0f, 76.1f), Quaternion.Euler(0f, 8f, 0f), Vector3.one * 0.82f);
            PlaceKenney(root.transform, "TowerShardBrokenArch", "wall-arch-top.fbx", new Vector3(1.9f, 0f, 75.5f), Quaternion.Euler(0f, 54f, 0f), Vector3.one * 0.92f);
            PlaceKenney(root.transform, "TowerReedCharmPillar", "pillar-stone.fbx", new Vector3(5.1f, 0f, 72.7f), Quaternion.Euler(0f, -14f, 0f), Vector3.one * 1.05f);
            CreateMarker(root.transform, "TowerGhostMemoryLightPool", new Vector3(-2.9f, 0.13f, 74.8f), new Vector3(1.15f, 0.04f, 1.15f), new Color(0.18f, 0.18f, 0.34f, 1f));
        }

        private static void CreateFinalRoadGameplayCompositions(Transform parent)
        {
            var root = new GameObject("FinalRoadGameplayCompositions");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "FinalChoiceTriangleGround", new Vector3(71.7f, 0.126f, 12.55f), new Vector3(6.2f, 0.012f, 4.2f), new Color(0.105f, 0.086f, 0.07f, 1f));
            PlaceKenney(root.transform, "FinalChoiceBrokenWallBack", "wall-broken.fbx", new Vector3(73.8f, 0f, 12.6f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.18f);
            PlaceKenney(root.transform, "FinalChoiceLeftPillar", "pillar-stone.fbx", new Vector3(69.5f, 0f, 10.8f), Quaternion.Euler(0f, -20f, 0f), Vector3.one * 0.9f);
            PlaceKenney(root.transform, "FinalChoiceRightPillar", "pillar-stone.fbx", new Vector3(69.5f, 0f, 14.5f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 0.9f);
            CreateMarker(root.transform, "FinalChoiceAshCircle", new Vector3(71.7f, 0.15f, 12.55f), new Vector3(2.7f, 0.04f, 2.1f), new Color(0.07f, 0.056f, 0.048f, 1f));
        }

        private static void CreateOuterVillageRing(Transform parent)
        {
            var root = new GameObject("OuterVillageRing_KayKit");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "OuterVillageNorthPackedYard", new Vector3(0f, 0.047f, 15.5f), new Vector3(31f, 0.016f, 16f), new Color(0.12f, 0.145f, 0.09f, 1f));
            CreateSurfacePatch(root.transform, "OuterVillageSouthMudApproach", new Vector3(0f, 0.049f, -19.5f), new Vector3(32f, 0.016f, 13f), new Color(0.12f, 0.095f, 0.062f, 1f));
            CreateSurfacePatch(root.transform, "OuterVillageFarmSoil", new Vector3(17f, 0.047f, 8.5f), new Vector3(16f, 0.016f, 13f), new Color(0.145f, 0.112f, 0.07f, 1f));
            CreateSurfacePatch(root.transform, "OuterVillageNorthStreetSpine", new Vector3(0f, 0.064f, 10.8f), new Vector3(34f, 0.014f, 2.8f), new Color(0.135f, 0.103f, 0.064f, 1f));
            CreateSurfacePatch(root.transform, "OuterVillageSouthGateSpine", new Vector3(0f, 0.066f, -17.6f), new Vector3(34f, 0.014f, 2.8f), new Color(0.125f, 0.092f, 0.056f, 1f));
            CreateSurfacePatch(root.transform, "OuterVillageFarmLane", new Vector3(16.9f, 0.068f, 2.2f), new Vector3(3.0f, 0.014f, 22f), new Color(0.128f, 0.098f, 0.06f, 1f));

            PlaceKayKit(root.transform, "OuterVillageLumbermill", "lumbermill.fbx", new Vector3(-18f, 0f, 9.6f), Quaternion.Euler(0f, 38f, 0f), Vector3.one * 1.12f);
            PlaceKayKit(root.transform, "OuterVillageWindmill", "mill.fbx", new Vector3(19.4f, 0f, -7.4f), Quaternion.Euler(0f, -34f, 0f), Vector3.one * 1.05f);
            PlaceKayKit(root.transform, "OuterVillageArcheryRange", "archeryrange.fbx", new Vector3(-18.6f, 0f, -10.2f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.05f);
            PlaceKayKit(root.transform, "OuterVillageFarmPlot_A", "farm_plot.fbx", new Vector3(15.7f, 0f, 9.2f), Quaternion.Euler(0f, -10f, 0f), Vector3.one * 1.25f);
            PlaceKayKit(root.transform, "OuterVillageFarmPlot_B", "farm_plot.fbx", new Vector3(20.7f, 0f, 12.7f), Quaternion.Euler(0f, 16f, 0f), Vector3.one * 1.05f);
            PlaceKayKit(root.transform, "OuterVillageExtraHouse_NorthA", "house.fbx", new Vector3(-10.5f, 0f, 15.3f), Quaternion.Euler(0f, 145f, 0f), Vector3.one * 0.92f);
            PlaceKayKit(root.transform, "OuterVillageExtraHouse_NorthB", "house.fbx", new Vector3(9.7f, 0f, 15.9f), Quaternion.Euler(0f, -132f, 0f), Vector3.one * 0.9f);
            PlaceKayKit(root.transform, "OuterVillageMarketOverflow", "market.fbx", new Vector3(9.9f, 0f, -14.5f), Quaternion.Euler(0f, -58f, 0f), Vector3.one * 0.82f);

            for (var i = 0; i < 9; i++)
            {
                PlaceKenney(root.transform, $"OuterVillageFenceArc_{i + 1:00}", i % 4 == 0 ? "fence-broken.fbx" : "fence.fbx", new Vector3(-21f + i * 5.2f, 0f, -22.5f + (i % 2) * 0.6f), Quaternion.Euler(0f, 88f + i * 3f, 0f), Vector3.one * 1.06f);
            }

            for (var i = 0; i < 7; i++)
            {
                PlaceKenney(root.transform, $"OuterVillageMarketProp_{i + 1:00}", i % 2 == 0 ? "stall-red.fbx" : "stall-green.fbx", new Vector3(-6.8f + i * 2.2f, 0f, -14.6f + (i % 2) * 1.2f), Quaternion.Euler(0f, -18f + i * 9f, 0f), Vector3.one * 0.82f);
            }
        }

        private static void CreateDeepForestMapExtension(Transform parent)
        {
            var root = new GameObject("DeepForestMapExtension_KayKit");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "DeepForestOuterFloor", new Vector3(-112f, 0.042f, 18f), new Vector3(62f, 0.016f, 48f), new Color(0.035f, 0.095f, 0.045f, 1f));
            CreateSurfacePatch(root.transform, "DeepForestHunterMudRoad", new Vector3(-105f, 0.058f, 5f), new Vector3(38f, 0.018f, 4.2f), new Color(0.095f, 0.075f, 0.045f, 1f));
            CreateSurfacePatch(root.transform, "DeepForestOutpostClearing", new Vector3(-103f, 0.061f, 20.4f), new Vector3(14f, 0.014f, 10f), new Color(0.052f, 0.105f, 0.052f, 1f));
            CreateSurfacePatch(root.transform, "DeepForestMineTrail", new Vector3(-117f, 0.063f, 9.0f), new Vector3(17f, 0.014f, 2.8f), new Color(0.082f, 0.064f, 0.04f, 1f));

            for (var i = 0; i < 58; i++)
            {
                var x = -137f + (i % 12) * 5.2f + (i % 3) * 0.85f;
                var z = -6f + (i / 12) * 8f + (i % 4) * 0.7f;
                var model = i % 5 == 0 ? "detail_treeA.fbx" : i % 5 == 1 ? "detail_treeB.fbx" : i % 5 == 2 ? "detail_treeC.fbx" : i % 2 == 0 ? "tree-high-crooked.fbx" : "tree-high.fbx";
                if (model.StartsWith("detail"))
                {
                    PlaceKayKit(root.transform, $"DeepForestKayKitTree_{i + 1:00}", model, new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 29f, 0f), Vector3.one * (0.9f + (i % 4) * 0.14f));
                }
                else
                {
                    PlaceKenney(root.transform, $"DeepForestKenneyTree_{i + 1:00}", model, new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 29f, 0f), Vector3.one * (1.1f + (i % 4) * 0.18f));
                }
            }

            PlaceKayKit(root.transform, "DeepForestMountainBack_A", "mountain.fbx", new Vector3(-142f, 0f, 34f), Quaternion.Euler(0f, 42f, 0f), new Vector3(2.0f, 0.9f, 2.0f));
            PlaceKayKit(root.transform, "DeepForestMountainBack_B", "mountain.fbx", new Vector3(-124f, 0f, 48f), Quaternion.Euler(0f, -25f, 0f), new Vector3(1.7f, 0.78f, 1.7f));
            PlaceKayKit(root.transform, "DeepForestRangerOutpost", "watchtower.fbx", new Vector3(-103f, 0f, 20.4f), Quaternion.Euler(0f, -36f, 0f), Vector3.one * 0.95f);
            PlaceKayKit(root.transform, "DeepForestAbandonedMine", "mine.fbx", new Vector3(-123.5f, 0f, 5.5f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 0.9f);
            PlaceKayKit(root.transform, "DeepForestRockShelf_A", "detail_rocks.fbx", new Vector3(-115f, 0f, 30.2f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.5f);
            PlaceKayKit(root.transform, "DeepForestRockShelf_B", "detail_rocks_small.fbx", new Vector3(-93.6f, 0f, 9.3f), Quaternion.Euler(0f, -48f, 0f), Vector3.one * 1.35f);
        }

        private static void CreateDeepSwampMapExtension(Transform parent)
        {
            var root = new GameObject("DeepSwampMapExtension_Kenney");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "DeepSwampOuterBog", new Vector3(18f, 0.041f, -119f), new Vector3(56f, 0.016f, 54f), new Color(0.024f, 0.075f, 0.052f, 1f));
            CreateSurfacePatch(root.transform, "DeepSwampOpenWater_A", new Vector3(8f, 0.052f, -122f), new Vector3(16f, 0.014f, 10f), new Color(0.012f, 0.046f, 0.044f, 1f));
            CreateSurfacePatch(root.transform, "DeepSwampOpenWater_B", new Vector3(31f, 0.054f, -110f), new Vector3(12f, 0.014f, 8f), new Color(0.014f, 0.052f, 0.045f, 1f));
            CreateSurfacePatch(root.transform, "DeepSwampMainCauseway", new Vector3(18f, 0.067f, -118f), new Vector3(4.2f, 0.014f, 43f), new Color(0.072f, 0.062f, 0.04f, 1f));
            CreateSurfacePatch(root.transform, "DeepSwampMineCauseway", new Vector3(31f, 0.069f, -113f), new Vector3(18f, 0.014f, 3.2f), new Color(0.066f, 0.058f, 0.038f, 1f));
            PlaceKayKit(root.transform, "DeepSwampCollapsedBridge", "bridge.fbx", new Vector3(15f, 0f, -101f), Quaternion.Euler(0f, 12f, 0f), Vector3.one * 1.15f);
            PlaceKayKit(root.transform, "DeepSwampRoofedBridge", "bridge_roofed.fbx", new Vector3(32f, 0f, -124f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.0f);
            PlaceKayKit(root.transform, "DeepSwampOldMineMouth", "mine.fbx", new Vector3(40f, 0f, -111f), Quaternion.Euler(0f, -75f, 0f), Vector3.one * 0.9f);

            for (var i = 0; i < 22; i++)
            {
                var x = -6f + (i % 8) * 6.1f + (i % 3) * 0.75f;
                var z = -139f + (i / 8) * 13.5f + (i % 2) * 2.2f;
                PlaceKenney(root.transform, $"DeepSwampRottenTree_{i + 1:00}", i % 2 == 0 ? "tree-high-crooked.fbx" : "tree-crooked.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 37f, 0f), Vector3.one * (1.2f + (i % 4) * 0.16f));
                if (i % 3 == 0)
                {
                    CreateReedCluster(root.transform, $"DeepSwampReedMass_{i + 1:00}", new Vector3(x + 2.2f, 0f, z + 1.7f), 1.2f + (i % 3) * 0.18f);
                }
            }

            var dragon = InstantiateModel($"{MonsterPath}/Dragon.fbx", "DeepSwampBossSilhouette_InternetAsset", root.transform, new Vector3(26.5f, 0.1f, -132f), Quaternion.Euler(0f, -128f, 0f), new Vector3(1.25f, 1.25f, 1.25f));
            if (dragon != null)
            {
                ApplyMaterialToChildRenderers(dragon, CreateMaterial("Assets/Materials/DeepSwampBossSilhouette.mat", new Color(0.07f, 0.18f, 0.12f, 1f)));
                MakeDisplayMonsterFightable(dragon, $"{MonsterPath}/Dragon.fbx", null,
                    "Болотный ящер", 180f, EnemyKind.Monster, "DeepSwampBoss_Defeated",
                    new[] { "Drowner Slime", "Iron Ore" }, 35, 90, "Добыча: слизь утопца, железная руда, 35 монет, 90 опыта.",
                    new Vector3(0f, 1.6f, 0f), 3.2f, 1.5f, 16f, 18f);
            }
        }

        private static void CreateAshRoadMapExtension(Transform parent)
        {
            var root = new GameObject("AshRoadMapExtension_KayKit");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "FarAshRoadScorchedField", new Vector3(120f, 0.043f, 11f), new Vector3(62f, 0.016f, 42f), new Color(0.105f, 0.094f, 0.083f, 1f));
            CreateSurfacePatch(root.transform, "FarAshRoadBlackenedTrack", new Vector3(119f, 0.058f, 0f), new Vector3(46f, 0.016f, 5.8f), new Color(0.058f, 0.052f, 0.047f, 1f));
            CreateSurfacePatch(root.transform, "FarAshRoadFortYard", new Vector3(118f, 0.066f, 1.2f), new Vector3(18f, 0.014f, 15f), new Color(0.094f, 0.082f, 0.072f, 1f));
            CreateSurfacePatch(root.transform, "FarAshRoadWatchtowerLane", new Vector3(119f, 0.068f, 9.4f), new Vector3(18f, 0.014f, 3.1f), new Color(0.07f, 0.062f, 0.054f, 1f));
            PlaceKayKit(root.transform, "FarAshRoadGateFort", "wall_gate_closed.fbx", new Vector3(118f, 0f, 1.2f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.3f);
            PlaceKayKit(root.transform, "FarAshRoadLeftWatchtower", "watchtower.fbx", new Vector3(112f, 0f, 9.5f), Quaternion.Euler(0f, 24f, 0f), Vector3.one * 1.05f);
            PlaceKayKit(root.transform, "FarAshRoadRightWatchtower", "watchtower.fbx", new Vector3(126f, 0f, -8.5f), Quaternion.Euler(0f, -34f, 0f), Vector3.one * 1.05f);
            PlaceKayKit(root.transform, "FarAshRoadMountainWall_A", "mountain.fbx", new Vector3(143f, 0f, 18f), Quaternion.Euler(0f, -30f, 0f), new Vector3(2.0f, 0.72f, 2.0f));
            PlaceKayKit(root.transform, "FarAshRoadMountainWall_B", "mountain.fbx", new Vector3(139f, 0f, -17f), Quaternion.Euler(0f, 22f, 0f), new Vector3(1.85f, 0.66f, 1.85f));

            for (var i = 0; i < 15; i++)
            {
                PlaceKenney(root.transform, $"FarAshRoadBurnedPost_{i + 1:00}", i % 4 == 0 ? "wall-broken.fbx" : "pillar-wood.fbx", new Vector3(96f + i * 3.4f, 0f, -10f + (i % 5) * 4.8f), Quaternion.Euler(0f, i * 21f, 0f), Vector3.one * (0.78f + (i % 3) * 0.12f));
                CreateMarker(root.transform, $"FarAshRoadAshMound_{i + 1:00}", new Vector3(96f + i * 3.4f, 0.06f, -3.4f + (i % 2) * 7.4f), new Vector3(1.5f, 0.04f, 0.8f), new Color(0.16f, 0.15f, 0.14f, 1f));
            }
        }

        private static void CreateTowerMapExtension(Transform parent)
        {
            var root = new GameObject("TowerMapExtension_KayKit");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "FarTowerStonePlateau", new Vector3(0f, 0.042f, 121f), new Vector3(46f, 0.016f, 36f), new Color(0.07f, 0.075f, 0.082f, 1f));
            CreateSurfacePatch(root.transform, "FarTowerRitualDust", new Vector3(0f, 0.058f, 113f), new Vector3(24f, 0.014f, 10f), new Color(0.13f, 0.105f, 0.16f, 1f));
            CreateSurfacePatch(root.transform, "FarTowerProcessionalRoad", new Vector3(0f, 0.067f, 121f), new Vector3(6.6f, 0.014f, 34f), new Color(0.082f, 0.078f, 0.086f, 1f));
            CreateSurfacePatch(root.transform, "FarTowerInnerCourt", new Vector3(0f, 0.069f, 128.2f), new Vector3(20f, 0.014f, 12f), new Color(0.065f, 0.067f, 0.074f, 1f));
            PlaceKayKit(root.transform, "FarTowerCastleBackdrop", "castle.fbx", new Vector3(0f, 0f, 127f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.65f);
            PlaceKayKit(root.transform, "FarTowerLeftWall", "wall_straight.fbx", new Vector3(-15f, 0f, 118f), Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.55f);
            PlaceKayKit(root.transform, "FarTowerRightWall", "wall_straight.fbx", new Vector3(15f, 0f, 118f), Quaternion.Euler(0f, -15f, 0f), Vector3.one * 1.55f);
            PlaceKayKit(root.transform, "FarTowerNorthWatchtower_A", "watchtower.fbx", new Vector3(-12f, 0f, 130f), Quaternion.Euler(0f, -28f, 0f), Vector3.one * 1.1f);
            PlaceKayKit(root.transform, "FarTowerNorthWatchtower_B", "watchtower.fbx", new Vector3(12f, 0f, 130f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 1.1f);

            for (var i = 0; i < 12; i++)
            {
                var angle = i * Mathf.PI * 2f / 12f;
                var position = new Vector3(Mathf.Cos(angle) * 14f, 0f, 114f + Mathf.Sin(angle) * 8f);
                PlaceKenney(root.transform, $"FarTowerRuinShard_{i + 1:00}", i % 2 == 0 ? "pillar-stone.fbx" : "wall-broken.fbx", position, Quaternion.Euler(0f, i * 31f, 0f), Vector3.one * (1.0f + (i % 3) * 0.12f));
                CreateMarker(root.transform, $"FarTowerMirrorDust_{i + 1:00}", position + new Vector3(0.7f, 0.09f, 0.3f), new Vector3(0.42f, 0.04f, 0.42f), new Color(0.28f, 0.19f, 0.48f, 1f));
            }
        }

        private static void CreateOuterRoadSetPieces(Transform parent)
        {
            var root = new GameObject("OuterRoadSetPieces_InternetAssets");
            root.transform.SetParent(parent, false);

            var westRoadStops = new[] { -82f, -98f, -116f, -134f };
            for (var i = 0; i < westRoadStops.Length; i++)
            {
                var x = westRoadStops[i];
                PlaceKayKit(root.transform, $"OuterWestRoadForestPatch_{i + 1:00}", i % 2 == 0 ? "forest.fbx" : "detail_forestA.fbx", new Vector3(x, 0f, 13.5f + i * 1.8f), Quaternion.Euler(0f, i * 19f, 0f), Vector3.one * (1.05f + i * 0.08f));
                PlaceKenney(root.transform, $"OuterWestRoadCartOrRock_{i + 1:00}", i % 2 == 0 ? "cart-high.fbx" : "rock-wide.fbx", new Vector3(x + 4.2f, 0f, -5.2f - i * 0.5f), Quaternion.Euler(0f, -18f * i, 0f), Vector3.one);
            }

            var northRoadStops = new[] { 84f, 98f, 112f };
            for (var i = 0; i < northRoadStops.Length; i++)
            {
                var z = northRoadStops[i];
                PlaceKenney(root.transform, $"OuterNorthRoadStone_{i + 1:00}", "wall-arch-top.fbx", new Vector3(-6.8f - i * 1.8f, 0f, z), Quaternion.Euler(0f, -42f + i * 17f, 0f), Vector3.one * (1.1f + i * 0.12f));
                PlaceKayKit(root.transform, $"OuterNorthRoadRocks_{i + 1:00}", "detail_rocks.fbx", new Vector3(7.2f + i * 2.1f, 0f, z + 2.6f), Quaternion.Euler(0f, 31f * i, 0f), Vector3.one * 1.1f);
            }

            var southRoadStops = new[] { -84f, -98f, -112f, -128f };
            for (var i = 0; i < southRoadStops.Length; i++)
            {
                var z = southRoadStops[i];
                PlaceKenney(root.transform, $"OuterSouthRoadPlanks_{i + 1:00}", i % 2 == 0 ? "planks-opening.fbx" : "planks-half.fbx", new Vector3(3.4f + i * 1.2f, 0.035f, z), Quaternion.Euler(0f, 72f + i * 11f, 0f), Vector3.one);
                CreateReedCluster(root.transform, $"OuterSouthRoadReeds_{i + 1:00}", new Vector3(-5.2f - i * 1.1f, 0f, z + 1.8f), 1.15f + i * 0.08f);
            }
        }

        private static void CreateLandmarkAndTraversalPass(Transform parent)
        {
            var root = new GameObject("LandmarkAndTraversalPass");
            root.transform.SetParent(parent, false);

            CreateVillageCrossroadsLandmarks(root.transform);
            CreateForestTraversalGate(root.transform);
            CreateSwampTraversalGate(root.transform);
            CreateAshRoadTraversalGate(root.transform);
            CreateTowerTraversalGate(root.transform);
            CreateOuterHorizonAnchors(root.transform);
        }

        private static void CreateVillageCrossroadsLandmarks(Transform parent)
        {
            var root = new GameObject("VillageCrossroadsLandmarks");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageCrossroadsStoneRing", new Vector3(0f, 0.135f, 0f), new Vector3(8.8f, 0.012f, 8.8f), new Color(0.105f, 0.088f, 0.06f, 1f));
            PlaceKayKit(root.transform, "VillageCrossroadsWellAnchor", "well.fbx", new Vector3(0f, 0f, -2.3f), Quaternion.identity, Vector3.one * 0.82f);
            PlaceKenney(root.transform, "VillageCrossroadsNoticePole", "pillar-wood.fbx", new Vector3(-2.2f, 0f, -1.1f), Quaternion.Euler(0f, 8f, 0f), new Vector3(0.68f, 1.18f, 0.68f));
            PlaceKenney(root.transform, "VillageCrossroadsNoticeBanner", "banner-red.fbx", new Vector3(-2.05f, 0f, -1.2f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 0.78f);
            PlaceKenney(root.transform, "VillageCrossroadsLanternNorth", "lantern.fbx", new Vector3(2.7f, 0f, 2.8f), Quaternion.identity, Vector3.one);
            PlaceKenney(root.transform, "VillageCrossroadsLanternSouth", "lantern.fbx", new Vector3(-2.8f, 0f, -5.2f), Quaternion.Euler(0f, 25f, 0f), Vector3.one);
        }

        private static void CreateForestTraversalGate(Transform parent)
        {
            var root = new GameObject("ForestTraversalGate");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ForestGateThresholdMud", new Vector3(-28f, 0.12f, 0f), new Vector3(12f, 0.012f, 7.6f), new Color(0.078f, 0.08f, 0.045f, 1f));
            PlaceKayKit(root.transform, "ForestGateLeftTreeMass", "detail_forestA.fbx", new Vector3(-31.6f, 0f, 7.8f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 1.35f);
            PlaceKayKit(root.transform, "ForestGateRightTreeMass", "detail_forestB.fbx", new Vector3(-31.2f, 0f, -7.2f), Quaternion.Euler(0f, -38f, 0f), Vector3.one * 1.28f);
            PlaceKenney(root.transform, "ForestGateBrokenFenceLeft", "fence-broken.fbx", new Vector3(-24.4f, 0f, 3.7f), Quaternion.Euler(0f, -26f, 0f), Vector3.one * 1.05f);
            PlaceKenney(root.transform, "ForestGateBrokenFenceRight", "fence-broken.fbx", new Vector3(-24.0f, 0f, -3.8f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 1.05f);
            PlaceKenney(root.transform, "ForestGateWarningLantern", "lantern.fbx", new Vector3(-25.6f, 0f, -1.7f), Quaternion.identity, Vector3.one * 0.9f);
            CreateZoneLabel(root.transform, "ForestGateSign", "Старый Лес", new Vector3(-25.5f, 1.35f, 1.8f), Quaternion.Euler(0f, 86f, 0f));
        }

        private static void CreateSwampTraversalGate(Transform parent)
        {
            var root = new GameObject("SwampTraversalGate");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "SwampGateWetThreshold", new Vector3(4.5f, 0.12f, -31f), new Vector3(10f, 0.012f, 12f), new Color(0.035f, 0.066f, 0.048f, 1f));
            PlaceKayKit(root.transform, "SwampGateRoofedBridgeAnchor", "bridge_roofed.fbx", new Vector3(4.4f, 0f, -31.4f), Quaternion.Euler(0f, 92f, 0f), Vector3.one * 1.05f);
            PlaceKenney(root.transform, "SwampGateReedFenceLeft", "fence-curved.fbx", new Vector3(-1.8f, 0f, -30.2f), Quaternion.Euler(0f, 72f, 0f), Vector3.one);
            PlaceKenney(root.transform, "SwampGateReedFenceRight", "fence-curved.fbx", new Vector3(10.8f, 0f, -32.3f), Quaternion.Euler(0f, -76f, 0f), Vector3.one);
            CreateReedCluster(root.transform, "SwampGateTallReedsLeft", new Vector3(0.7f, 0f, -36.4f), 1.45f);
            CreateReedCluster(root.transform, "SwampGateTallReedsRight", new Vector3(9.6f, 0f, -36.0f), 1.4f);
            CreateZoneLabel(root.transform, "SwampGateSign", "Чёрное Болото", new Vector3(7.8f, 1.35f, -26.4f), Quaternion.Euler(0f, -28f, 0f));
        }

        private static void CreateAshRoadTraversalGate(Transform parent)
        {
            var root = new GameObject("AshRoadTraversalGate");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "AshRoadGateThreshold", new Vector3(31f, 0.122f, 0f), new Vector3(13f, 0.012f, 9f), new Color(0.105f, 0.087f, 0.068f, 1f));
            PlaceKayKit(root.transform, "AshRoadGateCollapsedGate", "wall_gate_closed.fbx", new Vector3(31.4f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 0.96f);
            PlaceKayKit(root.transform, "AshRoadGateWatchtowerA", "watchtower.fbx", new Vector3(28.4f, 0f, 7.0f), Quaternion.Euler(0f, 34f, 0f), Vector3.one * 0.75f);
            PlaceKayKit(root.transform, "AshRoadGateWatchtowerB", "watchtower.fbx", new Vector3(34.0f, 0f, -7.0f), Quaternion.Euler(0f, -38f, 0f), Vector3.one * 0.72f);
            CreateMarker(root.transform, "AshRoadGateCinderPatchA", new Vector3(27.5f, 0.16f, -2.8f), new Vector3(1.9f, 0.04f, 1.0f), new Color(0.065f, 0.052f, 0.043f, 1f));
            CreateMarker(root.transform, "AshRoadGateCinderPatchB", new Vector3(35.2f, 0.16f, 3.1f), new Vector3(1.6f, 0.04f, 1.1f), new Color(0.08f, 0.055f, 0.042f, 1f));
            CreateZoneLabel(root.transform, "AshRoadGateSign", "Пепельный тракт", new Vector3(29.7f, 1.4f, -4.6f), Quaternion.Euler(0f, -72f, 0f));
        }

        private static void CreateTowerTraversalGate(Transform parent)
        {
            var root = new GameObject("TowerTraversalGate");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerGateThresholdStone", new Vector3(0f, 0.123f, 43f), new Vector3(12f, 0.012f, 11f), new Color(0.072f, 0.072f, 0.078f, 1f));
            PlaceKayKit(root.transform, "TowerGateStoneArch", "wall_gate.fbx", new Vector3(0f, 0f, 43.8f), Quaternion.Euler(0f, 0f, 0f), Vector3.one);
            PlaceKenney(root.transform, "TowerGateBrokenPillarLeft", "pillar-stone.fbx", new Vector3(-5.8f, 0f, 40.0f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.15f);
            PlaceKenney(root.transform, "TowerGateBrokenPillarRight", "pillar-stone.fbx", new Vector3(5.8f, 0f, 40.0f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 1.15f);
            PlaceKenney(root.transform, "TowerGateFallenArchShard", "wall-arch-top.fbx", new Vector3(4.2f, 0f, 46.6f), Quaternion.Euler(0f, 54f, 0f), Vector3.one * 0.8f);
            CreateMarker(root.transform, "TowerGateMirrorDustPool", new Vector3(0f, 0.16f, 46.1f), new Vector3(1.8f, 0.04f, 1.2f), new Color(0.24f, 0.18f, 0.42f, 1f));
            CreateZoneLabel(root.transform, "TowerGateSign", "Руины Башни", new Vector3(-3.9f, 1.45f, 39.1f), Quaternion.Euler(0f, 12f, 0f));
        }

        private static void CreateOuterHorizonAnchors(Transform parent)
        {
            var root = new GameObject("OuterHorizonAnchors");
            root.transform.SetParent(parent, false);

            PlaceKayKit(root.transform, "HorizonWestMountainCluster", "mountain.fbx", new Vector3(-154f, 0f, 26f), Quaternion.Euler(0f, 25f, 0f), new Vector3(2.25f, 0.72f, 2.25f));
            PlaceKayKit(root.transform, "HorizonNorthCastleSilhouette", "castle.fbx", new Vector3(0f, 0f, 154f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.45f);
            PlaceKayKit(root.transform, "HorizonEastBurnedFort", "wall_gate_closed.fbx", new Vector3(154f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.35f);
            PlaceKayKit(root.transform, "HorizonSouthBogBridge", "bridge.fbx", new Vector3(12f, 0f, -154f), Quaternion.Euler(0f, 84f, 0f), Vector3.one * 1.15f);

            for (var i = 0; i < 8; i++)
            {
                var angle = i * Mathf.PI * 2f / 8f;
                var position = new Vector3(Mathf.Cos(angle) * 145f, 0f, Mathf.Sin(angle) * 145f);
                PlaceKayKit(root.transform, $"HorizonRockMass_{i + 1:00}", "detail_rocks.fbx", position, Quaternion.Euler(0f, i * 37f, 0f), Vector3.one * (1.35f + (i % 2) * 0.22f));
            }
        }

        private static void CreateWorldCompositionPolishPass(Transform parent)
        {
            var root = new GameObject("WorldCompositionPolishPass");
            root.transform.SetParent(parent, false);

            CreateVillageEdgeComposition(root.transform);
            CreateForestCanopyComposition(root.transform);
            CreateSwampBoardwalkComposition(root.transform);
            CreateAshRoadBurnedHamletComposition(root.transform);
            CreateTowerOuterRuinsComposition(root.transform);
        }

        private static void CreateVillageEdgeComposition(Transform parent)
        {
            var root = new GameObject("VillageEdgeComposition");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageOrchardGround", new Vector3(22f, 0.052f, 11.2f), new Vector3(18f, 0.014f, 12f), new Color(0.105f, 0.145f, 0.072f, 1f));
            CreateSurfacePatch(root.transform, "VillageCemeteryGround", new Vector3(-23f, 0.053f, 10.2f), new Vector3(15f, 0.014f, 10f), new Color(0.095f, 0.105f, 0.078f, 1f));
            CreateSurfacePatch(root.transform, "VillageSouthWorkyardGround", new Vector3(0f, 0.054f, -26.2f), new Vector3(24f, 0.014f, 9f), new Color(0.125f, 0.098f, 0.062f, 1f));

            for (var i = 0; i < 10; i++)
            {
                var x = 16.2f + (i % 5) * 3.1f;
                var z = 7.8f + (i / 5) * 5.2f + (i % 2) * 0.6f;
                PlaceKenney(root.transform, $"VillageOrchardTree_{i + 1:00}", i % 3 == 0 ? "tree-high-round.fbx" : "tree-high.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 33f, 0f), Vector3.one * (0.92f + (i % 3) * 0.08f));
            }

            for (var i = 0; i < 8; i++)
            {
                var x = -28.5f + (i % 4) * 3.3f;
                var z = 7.0f + (i / 4) * 5.0f;
                PlaceKenney(root.transform, $"VillageCemeteryStone_{i + 1:00}", i % 2 == 0 ? "pillar-stone.fbx" : "wall-block-half.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, 18f + i * 24f, 0f), Vector3.one * (0.58f + (i % 2) * 0.08f));
                CreateNonBlockingMarker(root.transform, $"VillageCemeteryMossPatch_{i + 1:00}", new Vector3(x, 0.071f, z + 0.55f), new Vector3(1.1f, 0.025f, 0.75f), new Color(0.045f, 0.105f, 0.05f, 1f));
            }

            for (var i = 0; i < 12; i++)
            {
                var x = -13.2f + i * 2.4f;
                PlaceKenney(root.transform, $"VillageSouthWorkyardFence_{i + 1:00}", i % 4 == 0 ? "fence-broken.fbx" : "fence.fbx", new Vector3(x, 0f, -30.6f), Quaternion.Euler(0f, 88f + (i % 3) * 4f, 0f), Vector3.one * 0.95f);
            }

            PlaceKayKit(root.transform, "VillageEdgeFarmPlot_East", "farm_plot.fbx", new Vector3(25.5f, 0f, 4.8f), Quaternion.Euler(0f, -12f, 0f), Vector3.one * 0.95f);
            PlaceKayKit(root.transform, "VillageEdgeLumberStack_South", "lumbermill.fbx", new Vector3(-8.5f, 0f, -25.6f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 0.74f);
            PlaceKenney(root.transform, "VillageEdgeSupplyCart_South", "cart-high.fbx", new Vector3(8.6f, 0f, -25.4f), Quaternion.Euler(0f, -34f, 0f), Vector3.one * 0.92f);
        }

        private static void CreateForestCanopyComposition(Transform parent)
        {
            var root = new GameObject("ForestCanopyComposition");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ForestCanopyWall", new Vector3(-83f, 0.055f, 18f), new Vector3(38f, 0.014f, 22f), new Color(0.028f, 0.076f, 0.035f, 1f));
            CreateSurfacePatch(root.transform, "ForestLoggingCampGround", new Vector3(-96f, 0.057f, -7f), new Vector3(19f, 0.014f, 12f), new Color(0.078f, 0.09f, 0.048f, 1f));

            for (var i = 0; i < 46; i++)
            {
                var band = i % 2 == 0 ? -1f : 1f;
                var x = -62f - (i % 12) * 4.2f;
                var z = 9f + band * (7.5f + (i / 12) * 3.0f) + (i % 3) * 0.8f;
                PlaceKenney(root.transform, $"ForestCanopyTreeMass_{i + 1:00}", i % 4 == 0 ? "tree-high-crooked.fbx" : i % 4 == 1 ? "tree-high-round.fbx" : "tree-high.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 19f, 0f), Vector3.one * (1.1f + (i % 5) * 0.08f));
            }

            for (var i = 0; i < 9; i++)
            {
                PlaceKayKit(root.transform, $"ForestLoggingCampLogPile_{i + 1:00}", "detail_forestA.fbx", new Vector3(-102f + (i % 3) * 4.6f, 0f, -11f + (i / 3) * 3.6f), Quaternion.Euler(0f, 22f + i * 17f, 0f), Vector3.one * (0.75f + (i % 3) * 0.08f));
            }

            PlaceKayKit(root.transform, "ForestLoggingCamp", "archeryrange.fbx", new Vector3(-95.5f, 0f, -3.2f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 0.76f);
            PlaceKenney(root.transform, "ForestCanopyAbandonedCart", "cart.fbx", new Vector3(-88.6f, 0f, 3.2f), Quaternion.Euler(0f, -42f, 0f), Vector3.one * 0.9f);
        }

        private static void CreateSwampBoardwalkComposition(Transform parent)
        {
            var root = new GameObject("SwampBoardwalkComposition");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "SwampBoardwalkWaterSheet", new Vector3(26f, 0.052f, -91f), new Vector3(38f, 0.014f, 28f), new Color(0.012f, 0.042f, 0.038f, 1f));
            CreateSurfacePatch(root.transform, "SwampBoardwalkMudShelf", new Vector3(4f, 0.055f, -96f), new Vector3(22f, 0.014f, 24f), new Color(0.026f, 0.064f, 0.046f, 1f));

            for (var i = 0; i < 16; i++)
            {
                var x = 6.0f + i * 1.85f;
                var z = -86.0f - Mathf.Sin(i * 0.65f) * 4.0f;
                PlaceKenney(root.transform, $"SwampBoardwalkMainLine_{i + 1:00}", i % 5 == 0 ? "planks-half.fbx" : "planks.fbx", new Vector3(x, 0.06f, z), Quaternion.Euler(0f, 84f + Mathf.Sin(i) * 11f, 0f), Vector3.one * 0.84f);
                if (i % 3 == 0)
                {
                    CreateReedCluster(root.transform, $"SwampBoardwalkReedBreak_{i + 1:00}", new Vector3(x + 0.4f, 0f, z - 2.6f), 1.25f);
                }
            }

            for (var i = 0; i < 20; i++)
            {
                var x = 2f + (i % 5) * 7.2f;
                var z = -102f + (i / 5) * 5.2f + (i % 2) * 0.8f;
                CreateReedCluster(root.transform, $"SwampBoardwalkOuterReeds_{i + 1:00}", new Vector3(x, 0f, z), 1.1f + (i % 4) * 0.12f);
            }

            PlaceKenney(root.transform, "SwampBoardwalkRottenGate", "fence-gate.fbx", new Vector3(5.2f, 0f, -86.8f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 1.05f);
            PlaceKenney(root.transform, "SwampBoardwalkSunkenCart", "cart.fbx", new Vector3(31.4f, -0.05f, -93.6f), Quaternion.Euler(8f, -24f, 9f), Vector3.one * 0.88f);
            CreatePointLight(root.transform, "SwampBoardwalkLowGreenLight", new Vector3(24f, 1.1f, -91f), new Color(0.22f, 0.5f, 0.28f, 1f), 0.75f, 11f);
        }

        private static void CreateAshRoadBurnedHamletComposition(Transform parent)
        {
            var root = new GameObject("AshRoadBurnedHamletComposition");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "AshRoadBurnedHamletStreet", new Vector3(116f, 0.056f, 17f), new Vector3(34f, 0.014f, 7f), new Color(0.095f, 0.078f, 0.064f, 1f));
            CreateSurfacePatch(root.transform, "AshRoadBurnedHamletYard", new Vector3(118f, 0.054f, 27f), new Vector3(28f, 0.014f, 14f), new Color(0.12f, 0.095f, 0.078f, 1f));

            for (var i = 0; i < 14; i++)
            {
                var x = 102f + (i % 7) * 4.6f;
                var z = 13.4f + (i / 7) * 11.5f;
                PlaceKenney(root.transform, $"AshRoadHamletCharredWall_{i + 1:00}", i % 3 == 0 ? "wall-wood-broken.fbx" : i % 3 == 1 ? "wall-broken.fbx" : "wall-wood-half.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, 18f + i * 29f, 0f), Vector3.one * (1.05f + (i % 3) * 0.08f));
                CreateNonBlockingMarker(root.transform, $"AshRoadHamletSootPool_{i + 1:00}", new Vector3(x + 0.6f, 0.075f, z - 0.3f), new Vector3(1.6f, 0.026f, 1.0f), new Color(0.04f, 0.035f, 0.032f, 1f));
            }

            PlaceKayKit(root.transform, "AshRoadHamletBrokenWatch", "watchtower.fbx", new Vector3(127f, 0f, 31f), Quaternion.Euler(0f, -36f, 0f), Vector3.one * 0.72f);
            PlaceKenney(root.transform, "AshRoadHamletBlockedCart", "cart-high.fbx", new Vector3(111f, 0f, 16.6f), Quaternion.Euler(0f, 82f, 0f), Vector3.one * 0.88f);
            CreatePointLight(root.transform, "AshRoadHamletEmberLight", new Vector3(121f, 1.2f, 20f), new Color(1f, 0.32f, 0.12f, 1f), 0.72f, 12f);
        }

        private static void CreateTowerOuterRuinsComposition(Transform parent)
        {
            var root = new GameObject("TowerOuterRuinsComposition");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerOuterRuinCourt", new Vector3(0f, 0.056f, 121f), new Vector3(35f, 0.014f, 22f), new Color(0.07f, 0.067f, 0.076f, 1f));
            CreateSurfacePatch(root.transform, "TowerOuterMirrorDustField", new Vector3(0f, 0.064f, 115f), new Vector3(18f, 0.012f, 7f), new Color(0.16f, 0.13f, 0.26f, 1f));

            for (var i = 0; i < 18; i++)
            {
                var angle = i * 20f * Mathf.Deg2Rad;
                var x = Mathf.Cos(angle) * (12f + (i % 3) * 1.3f);
                var z = 120f + Mathf.Sin(angle) * (8.5f + (i % 2) * 1.4f);
                PlaceKenney(root.transform, $"TowerOuterRuinRing_{(char)('A' + i)}", i % 4 == 0 ? "wall-arch-top.fbx" : i % 4 == 1 ? "pillar-stone.fbx" : i % 4 == 2 ? "wall-broken.fbx" : "wall-block.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, -i * 20f + 90f, 0f), Vector3.one * (0.94f + (i % 4) * 0.08f));
            }

            for (var i = 0; i < 7; i++)
            {
                CreateNonBlockingMarker(root.transform, $"TowerOuterMirrorShardGlow_{i + 1:00}", new Vector3(-6f + i * 2f, 0.086f, 114.1f + (i % 2) * 1.5f), new Vector3(0.45f, 0.035f, 0.45f), new Color(0.34f, 0.22f, 0.62f, 1f));
            }

            PlaceKayKit(root.transform, "TowerOuterRuinsBackdropWall", "wall_straight.fbx", new Vector3(0f, 0f, 132f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.65f);
            CreatePointLight(root.transform, "TowerOuterRuinVioletLight", new Vector3(0f, 2.0f, 116f), new Color(0.42f, 0.24f, 0.74f, 1f), 0.95f, 14f);
        }

        private static void CreateFullMapVisualOverhaulPass(Transform parent)
        {
            var root = new GameObject("FullMapVisualOverhaulPass");
            root.transform.SetParent(parent, false);

            CreateVillageStreetOverhaul(root.transform);
            CreateForestDepthOverhaul(root.transform);
            CreateSwampDepthOverhaul(root.transform);
            CreateAshRoadDepthOverhaul(root.transform);
            CreateTowerDepthOverhaul(root.transform);
            CreateCreatureModelShowcase(root.transform);
        }

        private static void CreateVillageStreetOverhaul(Transform parent)
        {
            var root = new GameObject("VillageStreetOverhaul");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageOverhaulMarketMud", new Vector3(0.5f, 0.134f, -0.2f), new Vector3(16f, 0.012f, 9.5f), new Color(0.108f, 0.083f, 0.051f, 1f));
            CreateSurfacePatch(root.transform, "VillageOverhaulNorthLaneMud", new Vector3(0f, 0.136f, 12.2f), new Vector3(28f, 0.012f, 5.4f), new Color(0.116f, 0.089f, 0.052f, 1f));
            CreateSurfacePatch(root.transform, "VillageOverhaulSouthGateMud", new Vector3(0f, 0.138f, -18.8f), new Vector3(28f, 0.012f, 5.4f), new Color(0.105f, 0.078f, 0.047f, 1f));

            for (var i = 0; i < 10; i++)
            {
                PlaceKenney(root.transform, $"VillageOverhaulFenceLineNorth_{i + 1:00}", i % 3 == 0 ? "fence-broken.fbx" : "fence.fbx", new Vector3(-14f + i * 3.2f, 0f, 16.6f), Quaternion.Euler(0f, 88f + (i % 2) * 5f, 0f), Vector3.one * 1.05f);
                PlaceKenney(root.transform, $"VillageOverhaulFenceLineSouth_{i + 1:00}", i % 4 == 0 ? "fence-broken.fbx" : "fence.fbx", new Vector3(-14f + i * 3.2f, 0f, -22.4f), Quaternion.Euler(0f, 90f - (i % 2) * 5f, 0f), Vector3.one * 1.05f);
            }

            for (var i = 0; i < 8; i++)
            {
                var x = -8.4f + i * 2.4f;
                PlaceKenney(root.transform, $"VillageOverhaulMarketCrate_{i + 1:00}", i % 2 == 0 ? "stall-bench.fbx" : "stall-stool.fbx", new Vector3(x, 0f, -0.2f + (i % 2) * 2.2f), Quaternion.Euler(0f, -28f + i * 13f, 0f), Vector3.one * (0.8f + (i % 3) * 0.06f));
            }

            PlaceKayKit(root.transform, "VillageOverhaulGateHouseLeft", "house.fbx", new Vector3(-12.8f, 0f, -19.0f), Quaternion.Euler(0f, 32f, 0f), Vector3.one * 0.86f);
            PlaceKayKit(root.transform, "VillageOverhaulGateHouseRight", "house.fbx", new Vector3(12.8f, 0f, -18.8f), Quaternion.Euler(0f, -34f, 0f), Vector3.one * 0.86f);
            PlaceKenney(root.transform, "VillageOverhaulGateCartBlock", "cart-high.fbx", new Vector3(6.5f, 0f, -16.9f), Quaternion.Euler(0f, -62f, 0f), Vector3.one * 0.86f);
            CreatePointLight(root.transform, "VillageOverhaulMarketWarmLight", new Vector3(0.8f, 1.8f, 0.6f), new Color(1f, 0.55f, 0.25f, 1f), 0.6f, 10f);
        }

        private static void CreateForestDepthOverhaul(Transform parent)
        {
            var root = new GameObject("ForestDepthOverhaul");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ForestOverhaulNeedleFloor", new Vector3(-92f, 0.071f, 22f), new Vector3(44f, 0.012f, 26f), new Color(0.026f, 0.066f, 0.032f, 1f));
            CreateSurfacePatch(root.transform, "ForestOverhaulWolfClearing", new Vector3(-76f, 0.073f, 12f), new Vector3(13f, 0.012f, 9f), new Color(0.043f, 0.078f, 0.038f, 1f));

            for (var i = 0; i < 64; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var x = -55f - (i % 16) * 4.8f;
                var z = 7f + side * (7f + (i / 16) * 4.2f) + (i % 3) * 0.7f;
                PlaceKenney(root.transform, $"ForestOverhaulTreeWall_{i + 1:00}", i % 5 == 0 ? "tree-high-crooked.fbx" : i % 5 == 1 ? "tree-high-round.fbx" : "tree-high.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 23f, 0f), Vector3.one * (1.05f + (i % 4) * 0.11f));
            }

            for (var i = 0; i < 10; i++)
            {
                PlaceKayKit(root.transform, $"ForestOverhaulRockRib_{i + 1:00}", i % 2 == 0 ? "detail_rocks.fbx" : "detail_rocks_small.fbx", new Vector3(-64f - i * 5.5f, 0f, 1.8f + (i % 3) * 2.4f), Quaternion.Euler(0f, 22f + i * 19f, 0f), Vector3.one * (1.0f + (i % 3) * 0.18f));
            }

            PlaceKayKit(root.transform, "ForestOverhaulDeepMineMouth", "mine.fbx", new Vector3(-113f, 0f, 14f), Quaternion.Euler(0f, 82f, 0f), Vector3.one * 0.82f);
            PlaceKayKit(root.transform, "ForestOverhaulWatchBlind", "watchtower.fbx", new Vector3(-87f, 0f, 31f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.68f);
            CreatePointLight(root.transform, "ForestOverhaulMoonPool", new Vector3(-76f, 1.4f, 12f), new Color(0.32f, 0.46f, 0.72f, 1f), 0.42f, 13f);
        }

        private static void CreateSwampDepthOverhaul(Transform parent)
        {
            var root = new GameObject("SwampDepthOverhaul");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "SwampOverhaulDarkWaterA", new Vector3(23f, 0.069f, -82f), new Vector3(30f, 0.012f, 22f), new Color(0.01f, 0.034f, 0.032f, 1f));
            CreateSurfacePatch(root.transform, "SwampOverhaulDarkWaterB", new Vector3(-9f, 0.071f, -92f), new Vector3(22f, 0.012f, 28f), new Color(0.012f, 0.04f, 0.034f, 1f));
            CreateSurfacePatch(root.transform, "SwampOverhaulNestMud", new Vector3(14f, 0.132f, -75f), new Vector3(12f, 0.012f, 9f), new Color(0.024f, 0.052f, 0.037f, 1f));

            for (var i = 0; i < 34; i++)
            {
                var x = -12f + (i % 9) * 5.4f;
                var z = -104f + (i / 9) * 8.2f + (i % 2) * 1.1f;
                CreateReedCluster(root.transform, $"SwampOverhaulReedMass_{i + 1:00}", new Vector3(x, 0f, z), 1.2f + (i % 5) * 0.13f);
                if (i % 4 == 0)
                {
                    PlaceKenney(root.transform, $"SwampOverhaulDeadTree_{i + 1:00}", "tree-crooked.fbx", new Vector3(x + 1.9f, 0f, z + 2.2f), Quaternion.Euler(0f, i * 31f, 0f), Vector3.one * (1.1f + (i % 3) * 0.15f));
                }
            }

            for (var i = 0; i < 14; i++)
            {
                PlaceKenney(root.transform, $"SwampOverhaulBrokenBoardwalk_{i + 1:00}", i % 3 == 0 ? "planks-opening.fbx" : "planks-half.fbx", new Vector3(-2f + i * 2.1f, 0.055f, -84f - Mathf.Sin(i * 0.6f) * 3.2f), Quaternion.Euler(0f, 76f + i * 5f, 0f), Vector3.one * 0.82f);
            }

            var bat = InstantiateModel($"{MonsterPath}/Bat.fbx", "SwampOverhaulBatSwarmMarker", root.transform, new Vector3(21f, 0.6f, -88f), Quaternion.Euler(0f, -120f, 0f), Vector3.one * 0.85f);
            if (bat != null)
            {
                ApplyMaterialToChildRenderers(bat, CreateMaterial("Assets/Materials/SwampOverhaulBatVisual.mat", new Color(0.13f, 0.1f, 0.16f, 1f)));
                MakeDisplayMonsterFightable(bat, $"{MonsterPath}/Bat.fbx", null,
                    "Болотный нетопырь", 34f, EnemyKind.Beast, "SwampBatSwarm_Defeated",
                    new[] { "Bogweed" }, 4, 14, "Добыча: болотник, 4 монеты, 14 опыта.",
                    new Vector3(0f, 0.7f, 0f), 1.6f, 1.0f, 12f, 8f);
            }

            CreatePointLight(root.transform, "SwampOverhaulGreenFogLight", new Vector3(10f, 1.3f, -88f), new Color(0.12f, 0.62f, 0.36f, 1f), 0.55f, 18f);
        }

        private static void CreateAshRoadDepthOverhaul(Transform parent)
        {
            var root = new GameObject("AshRoadDepthOverhaul");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "AshRoadOverhaulCharredStreet", new Vector3(96f, 0.074f, 6f), new Vector3(50f, 0.012f, 8f), new Color(0.082f, 0.066f, 0.055f, 1f));
            CreateSurfacePatch(root.transform, "AshRoadOverhaulBurntYards", new Vector3(101f, 0.076f, 21f), new Vector3(44f, 0.012f, 19f), new Color(0.105f, 0.08f, 0.064f, 1f));

            for (var i = 0; i < 22; i++)
            {
                var x = 80f + (i % 11) * 4.0f;
                var z = 1.5f + (i / 11) * 19f + (i % 2) * 1.3f;
                PlaceKenney(root.transform, $"AshRoadOverhaulRuinWall_{i + 1:00}", i % 4 == 0 ? "wall-wood-broken.fbx" : i % 4 == 1 ? "wall-broken.fbx" : i % 4 == 2 ? "wall-wood-half.fbx" : "pillar-wood.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 27f, 0f), Vector3.one * (0.95f + (i % 4) * 0.09f));
                if (i % 3 == 0)
                {
                    CreateNonBlockingMarker(root.transform, $"AshRoadOverhaulBurnMark_{i + 1:00}", new Vector3(x + 0.5f, 0.086f, z - 0.2f), new Vector3(1.8f, 0.025f, 1.1f), new Color(0.032f, 0.027f, 0.024f, 1f));
                }
            }

            PlaceKayKit(root.transform, "AshRoadOverhaulFortGate", "wall_gate_closed.fbx", new Vector3(119f, 0f, 5.6f), Quaternion.Euler(0f, 92f, 0f), Vector3.one * 1.18f);
            PlaceKayKit(root.transform, "AshRoadOverhaulLeftTower", "watchtower.fbx", new Vector3(110f, 0f, 19f), Quaternion.Euler(0f, -24f, 0f), Vector3.one * 0.86f);
            PlaceKayKit(root.transform, "AshRoadOverhaulRightTower", "watchtower.fbx", new Vector3(126f, 0f, -4f), Quaternion.Euler(0f, 32f, 0f), Vector3.one * 0.86f);
            CreatePointLight(root.transform, "AshRoadOverhaulFirelineLight", new Vector3(98f, 1.4f, 8f), new Color(1f, 0.28f, 0.08f, 1f), 0.58f, 17f);
        }

        private static void CreateTowerDepthOverhaul(Transform parent)
        {
            var root = new GameObject("TowerDepthOverhaul");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerOverhaulColdCourt", new Vector3(0f, 0.075f, 95f), new Vector3(24f, 0.012f, 42f), new Color(0.058f, 0.058f, 0.068f, 1f));
            CreateSurfacePatch(root.transform, "TowerOverhaulMirrorScar", new Vector3(0f, 0.086f, 83f), new Vector3(9f, 0.012f, 20f), new Color(0.18f, 0.12f, 0.32f, 1f));

            for (var i = 0; i < 30; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var z = 78f + i * 1.8f;
                var x = side * (5.6f + (i % 5) * 1.15f);
                PlaceKenney(root.transform, $"TowerOverhaulRuinSpine_{i + 1:00}", i % 5 == 0 ? "wall-arch.fbx" : i % 5 == 1 ? "wall-arch-top.fbx" : i % 5 == 2 ? "pillar-stone.fbx" : "wall-broken.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, side * (24f + i * 6f), 0f), Vector3.one * (0.86f + (i % 4) * 0.08f));
            }

            PlaceKayKit(root.transform, "TowerOverhaulCastleMassBack", "castle.fbx", new Vector3(0f, 0f, 134f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.72f);
            PlaceKayKit(root.transform, "TowerOverhaulLeftWallMass", "wall_straight.fbx", new Vector3(-18f, 0f, 116f), Quaternion.Euler(0f, 12f, 0f), Vector3.one * 1.7f);
            PlaceKayKit(root.transform, "TowerOverhaulRightWallMass", "wall_straight.fbx", new Vector3(18f, 0f, 116f), Quaternion.Euler(0f, -12f, 0f), Vector3.one * 1.7f);
            CreatePointLight(root.transform, "TowerOverhaulMirrorCoreLight", new Vector3(0f, 1.6f, 85f), new Color(0.52f, 0.28f, 0.92f, 1f), 0.85f, 18f);
        }

        private static void CreateCreatureModelShowcase(Transform parent)
        {
            var root = new GameObject("CreatureModelShowcase");
            root.transform.SetParent(parent, false);

            // Skeleton FBX is ~5.1m tall at scale 1; 0.42 => ~2.15m, imposing but
            // human-scaled against the castle (was 1.5 => a 7.7m giant).
            var skeleton = InstantiateModel($"{MonsterPath}/Skeleton.fbx", "TowerOverhaulSkeletonDisplay", root.transform, new Vector3(-7.2f, 0f, 86f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 0.42f);
            if (skeleton != null)
            {
                ApplyMaterialToChildRenderers(skeleton, CreateMaterial("Assets/Materials/TowerOverhaulSkeletonDisplay.mat", new Color(0.76f, 0.74f, 0.68f, 1f)));
                MakeDisplayMonsterFightable(skeleton, $"{MonsterPath}/Skeleton.fbx", CharacterAnimationSetup.SkeletonController,
                    "Скелет-страж", 55f, EnemyKind.Undead, "TowerDisplaySkeleton_Defeated",
                    new[] { "Undead Bone" }, 0, 15, "Добыча: кость нежити, 15 опыта.",
                    new Vector3(0f, 1.0f, 0f), 2.0f, 0.4f, 11f, 11f);
            }

            var slime = InstantiateModel($"{MonsterPath}/Slime.fbx", "SwampOverhaulDrownerDisplay", root.transform, new Vector3(18.5f, 0f, -83.5f), Quaternion.Euler(0f, -125f, 0f), Vector3.one * 1.35f);
            if (slime != null)
            {
                ApplyMaterialToChildRenderers(slime, CreateMaterial("Assets/Materials/SwampOverhaulDrownerDisplay.mat", new Color(0.06f, 0.3f, 0.2f, 1f)));
                MakeDisplayMonsterFightable(slime, $"{MonsterPath}/Slime.fbx", CharacterAnimationSetup.DrownerController,
                    "Болотный утопец", 46f, EnemyKind.Monster, "SwampDrownerDisplay_Defeated",
                    new[] { "Drowner Slime" }, 5, 16, "Добыча: слизь утопца, 5 монет, 16 опыта.",
                    new Vector3(0f, 0.5f, 0f), 1.2f, 0.7f, 11f, 10f);
            }

            var dragon = InstantiateModel($"{MonsterPath}/Dragon.fbx", "SwampOverhaulBossBackdrop", root.transform, new Vector3(36f, 0f, -118f), Quaternion.Euler(0f, -150f, 0f), Vector3.one * 1.45f);
            if (dragon != null)
            {
                ApplyMaterialToChildRenderers(dragon, CreateMaterial("Assets/Materials/SwampOverhaulBossBackdrop.mat", new Color(0.12f, 0.16f, 0.12f, 1f)));
                AttachAnimator(dragon, CharacterAnimationSetup.DragonController, $"{MonsterPath}/Dragon.fbx");
            }
        }

        private static void CreateTerrainDepthAndSilhouettePass(Transform parent)
        {
            var root = new GameObject("TerrainDepthAndSilhouettePass");
            root.transform.SetParent(parent, false);

            CreateVillageReliefSilhouette(root.transform);
            CreateForestRidgeSilhouette(root.transform);
            CreateSwampWaterMazeSilhouette(root.transform);
            CreateAshRoadBattlefieldSilhouette(root.transform);
            CreateTowerCliffSilhouette(root.transform);
        }

        private static void CreateVillageReliefSilhouette(Transform parent)
        {
            var root = new GameObject("VillageReliefSilhouette");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageReliefOuterDitchNorth", new Vector3(0f, 0.058f, 21.8f), new Vector3(36f, 0.012f, 3.2f), new Color(0.055f, 0.075f, 0.046f, 1f));
            CreateSurfacePatch(root.transform, "VillageReliefOuterDitchSouth", new Vector3(0f, 0.059f, -27.8f), new Vector3(34f, 0.012f, 3.4f), new Color(0.072f, 0.055f, 0.038f, 1f));
            CreateSurfacePatch(root.transform, "VillageReliefRaisedMarketLip", new Vector3(0.4f, 0.19f, 6.6f), new Vector3(13f, 0.18f, 0.42f), new Color(0.13f, 0.095f, 0.057f, 1f));

            for (var i = 0; i < 12; i++)
            {
                var x = -17.5f + i * 3.1f;
                PlaceKenney(root.transform, $"VillageReliefNorthHedge_{i + 1:00}", i % 3 == 0 ? "hedge-large.fbx" : "hedge.fbx", new Vector3(x, 0f, 20.3f + (i % 2) * 0.7f), Quaternion.Euler(0f, 88f + i * 2f, 0f), Vector3.one * (0.88f + (i % 2) * 0.08f));
                if (i % 4 == 0)
                {
                    PlaceKenney(root.transform, $"VillageReliefBrokenBoundary_{i + 1:00}", "fence-broken.fbx", new Vector3(x + 0.9f, 0f, -25.8f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 0.95f);
                }
            }

            PlaceKayKit(root.transform, "VillageReliefBackHillLeft", "detail_hill.fbx", new Vector3(-23f, 0f, 20f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 1.45f);
            PlaceKayKit(root.transform, "VillageReliefBackHillRight", "detail_hill.fbx", new Vector3(24f, 0f, 18.5f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.35f);
        }

        private static void CreateForestRidgeSilhouette(Transform parent)
        {
            var root = new GameObject("ForestRidgeSilhouette");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ForestRidgeShadowGully", new Vector3(-84f, 0.081f, -3.8f), new Vector3(45f, 0.012f, 5.2f), new Color(0.016f, 0.038f, 0.026f, 1f));
            CreateSurfacePatch(root.transform, "ForestRidgeHighBank", new Vector3(-101f, 0.22f, 27f), new Vector3(34f, 0.28f, 4.8f), new Color(0.032f, 0.071f, 0.035f, 1f));

            for (var i = 0; i < 16; i++)
            {
                PlaceKayKit(root.transform, $"ForestRidgeRockLine_{i + 1:00}", i % 2 == 0 ? "detail_rocks.fbx" : "detail_rocks_small.fbx", new Vector3(-57f - i * 4.2f, 0f, -0.8f + (i % 4) * 1.1f), Quaternion.Euler(0f, i * 17f, 0f), Vector3.one * (1.05f + (i % 4) * 0.13f));
            }

            for (var i = 0; i < 18; i++)
            {
                PlaceKenney(root.transform, $"ForestRidgeTallPineSilhouette_{i + 1:00}", i % 2 == 0 ? "tree-high-crooked.fbx" : "tree-high.fbx", new Vector3(-74f - (i % 9) * 5.2f, 0f, 28f + (i / 9) * 5.8f), Quaternion.Euler(0f, i * 29f, 0f), Vector3.one * (1.22f + (i % 3) * 0.1f));
            }
        }

        private static void CreateSwampWaterMazeSilhouette(Transform parent)
        {
            var root = new GameObject("SwampWaterMazeSilhouette");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 9; i++)
            {
                var x = -10f + (i % 3) * 15f;
                var z = -78f - (i / 3) * 13f;
                CreateSurfacePatch(root.transform, $"SwampMazeWaterPocket_{i + 1:00}", new Vector3(x, 0.068f + i * 0.001f, z), new Vector3(8.5f + (i % 2) * 2.2f, 0.012f, 5.8f), new Color(0.008f, 0.032f, 0.03f, 1f));
                CreateReedCluster(root.transform, $"SwampMazeReedIsland_{i + 1:00}", new Vector3(x + 3.8f, 0f, z + 2.7f), 1.25f + (i % 4) * 0.1f);
            }

            for (var i = 0; i < 10; i++)
            {
                PlaceKenney(root.transform, $"SwampMazeCrookedTreeWall_{i + 1:00}", "tree-high-crooked.fbx", new Vector3(-18f + i * 6.8f, 0f, -112f + (i % 2) * 5.5f), Quaternion.Euler(0f, i * 33f, 0f), Vector3.one * (1.05f + (i % 3) * 0.16f));
            }

            PlaceKayKit(root.transform, "SwampMazeCollapsedBridgeSilhouette", "bridge.fbx", new Vector3(25.5f, -0.03f, -104f), Quaternion.Euler(0f, -28f, 8f), Vector3.one * 0.9f);
            CreatePointLight(root.transform, "SwampMazeDeepWispLight", new Vector3(-5f, 1.1f, -101f), new Color(0.08f, 0.58f, 0.36f, 1f), 0.48f, 12f);
        }

        private static void CreateAshRoadBattlefieldSilhouette(Transform parent)
        {
            var root = new GameObject("AshRoadBattlefieldSilhouette");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 8; i++)
            {
                CreateNonBlockingMarker(root.transform, $"AshBattlefieldCrater_{i + 1:00}", new Vector3(83f + i * 5.4f, 0.091f, -8f + (i % 4) * 5.2f), new Vector3(2.4f + (i % 2) * 0.9f, 0.03f, 1.35f), new Color(0.032f, 0.027f, 0.022f, 1f));
            }

            for (var i = 0; i < 18; i++)
            {
                PlaceKenney(root.transform, $"AshBattlefieldStakeLine_{i + 1:00}", i % 3 == 0 ? "pillar-wood.fbx" : "fence-broken.fbx", new Vector3(78f + i * 3.1f, 0f, -13.5f + (i % 2) * 2.2f), Quaternion.Euler(0f, 72f + i * 11f, (i % 2 == 0 ? 0f : 5f)), Vector3.one * (0.82f + (i % 3) * 0.08f));
            }

            PlaceKayKit(root.transform, "AshBattlefieldFarGateSilhouette", "wall_gate_closed.fbx", new Vector3(135f, 0f, 6f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.3f);
            PlaceKayKit(root.transform, "AshBattlefieldMountainBlocker", "mountain.fbx", new Vector3(150f, 0f, 31f), Quaternion.Euler(0f, -38f, 0f), new Vector3(2.2f, 0.72f, 2.2f));
            CreatePointLight(root.transform, "AshBattlefieldCoalLineLight", new Vector3(111f, 1.0f, -4f), new Color(0.95f, 0.18f, 0.06f, 1f), 0.42f, 14f);
        }

        private static void CreateTowerCliffSilhouette(Transform parent)
        {
            var root = new GameObject("TowerCliffSilhouette");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerCliffRaisedNorthShelf", new Vector3(0f, 0.245f, 127f), new Vector3(42f, 0.33f, 8f), new Color(0.06f, 0.057f, 0.067f, 1f));
            CreateSurfacePatch(root.transform, "TowerCliffShadowDropLeft", new Vector3(-17.5f, 0.11f, 105f), new Vector3(4.2f, 0.05f, 42f), new Color(0.026f, 0.025f, 0.032f, 1f));
            CreateSurfacePatch(root.transform, "TowerCliffShadowDropRight", new Vector3(17.5f, 0.11f, 105f), new Vector3(4.2f, 0.05f, 42f), new Color(0.026f, 0.025f, 0.032f, 1f));

            for (var i = 0; i < 18; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var z = 90f + i * 2.4f;
                PlaceKenney(root.transform, $"TowerCliffBrokenColumn_{i + 1:00}", i % 3 == 0 ? "pillar-stone.fbx" : "wall-block.fbx", new Vector3(side * (10f + (i % 4) * 1.8f), 0f, z), Quaternion.Euler(0f, side * (16f + i * 8f), 0f), Vector3.one * (0.8f + (i % 4) * 0.1f));
            }

            CreatePointLight(root.transform, "TowerCliffColdBackLight", new Vector3(0f, 2.2f, 126f), new Color(0.34f, 0.32f, 0.7f, 1f), 0.62f, 20f);
        }

        private static void CreateAmbientCharacterPopulationPass(Transform parent)
        {
            var root = new GameObject("AmbientCharacterPopulationPass");
            root.transform.SetParent(parent, false);

            CreateVillageAmbientPopulation(root.transform);
            CreateForestAmbientPopulation(root.transform);
            CreateAshRoadAmbientPopulation(root.transform);
            CreateTowerAmbientPopulation(root.transform);
        }

        private static void CreateVillageAmbientPopulation(Transform parent)
        {
            var root = new GameObject("VillageAmbientPopulation");
            root.transform.SetParent(parent, false);

            CreateAmbientCharacter(root.transform, "AmbientVillager_Market_01", "Monk.fbx", new Vector3(-2.8f, 1f, 2.8f), Quaternion.Euler(0f, 122f, 0f), new Color(0.24f, 0.18f, 0.12f, 1f), 0.78f);
            CreateAmbientCharacter(root.transform, "AmbientVillager_Market_02", "Cleric.fbx", new Vector3(3.8f, 1f, 2.1f), Quaternion.Euler(0f, -118f, 0f), new Color(0.16f, 0.22f, 0.14f, 1f), 0.76f);
            CreateAmbientCharacter(root.transform, "AmbientVillager_Gate_01", "Rogue.fbx", new Vector3(-7.4f, 1f, -17.2f), Quaternion.Euler(0f, 45f, 0f), new Color(0.18f, 0.14f, 0.1f, 1f), 0.74f);
            CreateAmbientCharacter(root.transform, "AmbientVillager_Gate_02", "Warrior.fbx", new Vector3(8.2f, 1f, -16.8f), Quaternion.Euler(0f, -42f, 0f), new Color(0.2f, 0.13f, 0.09f, 1f), 0.76f);
        }

        private static void CreateForestAmbientPopulation(Transform parent)
        {
            var root = new GameObject("ForestAmbientPopulation");
            root.transform.SetParent(parent, false);

            CreateAmbientCharacter(root.transform, "AmbientForestHunter_01", "Ranger.fbx", new Vector3(-92.5f, 1f, 8.2f), Quaternion.Euler(0f, 72f, 0f), new Color(0.13f, 0.17f, 0.11f, 1f), 0.76f);
            CreateAmbientCharacter(root.transform, "AmbientForestHunter_02", "Rogue.fbx", new Vector3(-102.4f, 1f, -5.8f), Quaternion.Euler(0f, -34f, 0f), new Color(0.15f, 0.13f, 0.09f, 1f), 0.72f);
        }

        private static void CreateAshRoadAmbientPopulation(Transform parent)
        {
            var root = new GameObject("AshRoadAmbientPopulation");
            root.transform.SetParent(parent, false);

            CreateAmbientCharacter(root.transform, "AmbientAshRoadRefugee_01", "Monk.fbx", new Vector3(88.4f, 1f, 24.8f), Quaternion.Euler(0f, -86f, 0f), new Color(0.18f, 0.14f, 0.11f, 1f), 0.7f);
            CreateAmbientCharacter(root.transform, "AmbientAshRoadRefugee_02", "Cleric.fbx", new Vector3(94.2f, 1f, 19.7f), Quaternion.Euler(0f, -122f, 0f), new Color(0.18f, 0.12f, 0.1f, 1f), 0.72f);
        }

        private static void CreateTowerAmbientPopulation(Transform parent)
        {
            var root = new GameObject("TowerAmbientPopulation");
            root.transform.SetParent(parent, false);

            CreateAmbientCharacter(root.transform, "AmbientTowerMemory_01", "Wizard.fbx", new Vector3(-6.8f, 1f, 91.5f), Quaternion.Euler(0f, 28f, 0f), new Color(0.18f, 0.15f, 0.28f, 1f), 0.66f);
            CreateAmbientCharacter(root.transform, "AmbientTowerMemory_02", "Wizard.fbx", new Vector3(7.4f, 1f, 97.2f), Quaternion.Euler(0f, -36f, 0f), new Color(0.15f, 0.15f, 0.29f, 1f), 0.62f);
        }

        private static readonly string[] AmbientVillagerLines =
        {
            "Доброго пути. В Велемаре чужакам не всегда рады, но ты, видать, по делу.",
            "Слыхал про тварь на болоте? Старейшина Войцех уже ищет, кому это поручить.",
            "Эльса — травница, не ведьма. Что бы там в деревне ни шептали.",
            "Кузнец Борис перекуёт тебе сталь, если принесёшь годное железо.",
            "О Зеркале Правды тут молчат. И ты помолчи, коли жить хочешь спокойно."
        };

        private static readonly string[] AmbientHunterLines =
        {
            "Лес сменился. Зверь идёт не туда, куда привык. Что-то его гонит.",
            "Держись тропы, ведьмак. В чаще и днём темно.",
            "Видел следы у ручья — тяжелее медвежьих. Не моя это добыча."
        };

        private static readonly string[] AmbientRefugeeLines =
        {
            "Мы с тракта. Там, откуда мы шли, не осталось ни домов, ни людей.",
            "Дай пройти, добрый человек. Нам бы только до деревни добраться.",
            "Война никого не щадит. Радуйся, что у тебя есть меч."
        };

        // Was decoration only — the anchor collider was stripped and nothing was attached,
        // so these villagers/hunters/refugees were "phantoms": visible but impossible to
        // engage. Keep the collider and give each a short, role-appropriate flavour line so
        // every visible character is at least talkable.
        private static void CreateAmbientCharacter(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Color fallbackColor, float scale)
        {
            var character = CreateRpgCharacterAnchor(parent, objectName, modelName, position, rotation, Vector3.one * scale, fallbackColor);
            CreateCharacterGroundRing(character, $"{objectName}_PresenceRing", new Color(0.36f, 0.28f, 0.14f, 1f), 0.62f);

            string speaker;
            string[] pool;
            if (objectName.Contains("Hunter"))
            {
                speaker = "Охотник";
                pool = AmbientHunterLines;
            }
            else if (objectName.Contains("Refugee"))
            {
                speaker = "Беженец";
                pool = AmbientRefugeeLines;
            }
            else if (objectName.Contains("TowerMemory"))
            {
                speaker = "Видение прошлого";
                pool = new[] { "…ты слышишь? Зеркало ещё помнит то, что деревня хотела забыть." };
            }
            else
            {
                speaker = "Житель Велемара";
                pool = AmbientVillagerLines;
            }

            var line = pool[Mathf.Abs(objectName.GetHashCode()) % pool.Length];
            character.AddComponent<DialogueInteractable>().Configure(
                speaker,
                "Поговорить",
                "start",
                new[]
                {
                    new DialogueNode("start", speaker, line, new[]
                    {
                        new DialogueChoice("Бывай.", "", "", true)
                    })
                });
        }

        private static void CreateVisualAtmospherePolishPass(Transform parent)
        {
            var root = new GameObject("VisualAtmospherePolishPass");
            root.transform.SetParent(parent, false);

            CreateVillageCameraCompositionPolish(root.transform);
            CreateVillageAntiFlickerGround(root.transform);
            CreateForestDepthFogPolish(root.transform);
            CreateSwampWaterAndReedPolish(root.transform);
            CreateAshRoadSmokeAndRuinPolish(root.transform);
            CreateTowerRitualSkylinePolish(root.transform);
            CreateTowerPlayableRuinRebuild(root.transform);
        }

        private static void CreateVillageCameraCompositionPolish(Transform parent)
        {
            var root = new GameObject("VillageCameraCompositionPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillagePolishMainStreetWetMud", new Vector3(0f, 0.142f, -8.8f), new Vector3(18f, 0.012f, 8.6f), new Color(0.105f, 0.077f, 0.046f, 1f));
            CreateSurfacePatch(root.transform, "VillagePolishMarketWarmGround", new Vector3(1.6f, 0.143f, 3.9f), new Vector3(15.5f, 0.012f, 9f), new Color(0.15f, 0.105f, 0.055f, 1f));
            CreateSurfacePatch(root.transform, "VillagePolishBackyardGrassA", new Vector3(-17.5f, 0.118f, 8.8f), new Vector3(8.5f, 0.012f, 7.5f), new Color(0.07f, 0.125f, 0.054f, 1f));
            CreateSurfacePatch(root.transform, "VillagePolishBackyardGrassB", new Vector3(17.8f, 0.119f, 8.3f), new Vector3(8.2f, 0.012f, 7.8f), new Color(0.068f, 0.12f, 0.052f, 1f));

            var frontYards = new[]
            {
                new Vector3(-16.2f, 0f, -7.8f),
                new Vector3(-13.8f, 0f, 10.8f),
                new Vector3(14.2f, 0f, -8.4f),
                new Vector3(16.5f, 0f, 9.6f)
            };

            for (var i = 0; i < frontYards.Length; i++)
            {
                var side = i < 2 ? -1f : 1f;
                PlaceKenney(root.transform, $"VillagePolishHouseFacadeFence_{i + 1:00}", i % 2 == 0 ? "fence.fbx" : "fence-broken.fbx", frontYards[i] + new Vector3(side * -1.8f, 0f, 0f), Quaternion.Euler(0f, 4f + side * 84f, 0f), Vector3.one * 0.96f);
                PlaceKenney(root.transform, $"VillagePolishHouseFacadeLantern_{i + 1:00}", "lantern.fbx", frontYards[i] + new Vector3(side * 1.3f, 0f, 1.5f), Quaternion.Euler(0f, 25f * i, 0f), Vector3.one * 0.82f);
                PlaceKenney(root.transform, $"VillagePolishHouseFacadeBench_{i + 1:00}", "stall-bench.fbx", frontYards[i] + new Vector3(side * 0.6f, 0f, -1.8f), Quaternion.Euler(0f, side * 58f, 0f), Vector3.one * 0.76f);
            }

            for (var i = 0; i < 9; i++)
            {
                var x = -9.5f + i * 2.35f;
                CreateNonBlockingMarker(root.transform, $"VillagePolishWheelTrack_{i + 1:00}", new Vector3(x, 0.18f, -6.2f + (i % 2) * 0.45f), new Vector3(1.42f, 0.035f, 0.18f), new Color(0.058f, 0.044f, 0.03f, 1f));
            }

            PlaceKayKit(root.transform, "VillagePolishDistantMillSilhouette", "mill.fbx", new Vector3(30.5f, 0f, 20.8f), Quaternion.Euler(0f, -42f, 0f), Vector3.one * 0.92f);
            PlaceKayKit(root.transform, "VillagePolishDistantArcheryRange", "archeryrange.fbx", new Vector3(-30.8f, 0f, 19.2f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 0.8f);
            CreatePointLight(root.transform, "VillagePolishMarketGlow", new Vector3(0.8f, 2.2f, 4.1f), new Color(1f, 0.58f, 0.25f, 1f), 0.72f, 8.5f);
        }

        private static void CreateVillageAntiFlickerGround(Transform parent)
        {
            var root = new GameObject("VillageAntiFlickerGround");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageAntiFlickerPackedEarth", new Vector3(0f, 0.226f, -1.5f), new Vector3(34f, 0.012f, 30f), new Color(0.115f, 0.092f, 0.055f, 1f));
            CreateSurfacePatch(root.transform, "VillageAntiFlickerMainRoadNS", new Vector3(0f, 0.241f, -1.5f), new Vector3(5.8f, 0.01f, 32f), new Color(0.082f, 0.066f, 0.042f, 1f));
            CreateSurfacePatch(root.transform, "VillageAntiFlickerMainRoadEW", new Vector3(0f, 0.246f, 0f), new Vector3(32f, 0.01f, 5.8f), new Color(0.086f, 0.068f, 0.043f, 1f));
            CreateSurfacePatch(root.transform, "VillageAntiFlickerMarketMud", new Vector3(2.2f, 0.252f, 3.9f), new Vector3(12f, 0.01f, 6.2f), new Color(0.13f, 0.095f, 0.05f, 1f));
            CreateSurfacePatch(root.transform, "VillageAntiFlickerBackGrassLeft", new Vector3(-14.5f, 0.238f, 8.8f), new Vector3(6.4f, 0.01f, 7f), new Color(0.052f, 0.095f, 0.042f, 1f));
            CreateSurfacePatch(root.transform, "VillageAntiFlickerBackGrassRight", new Vector3(14.5f, 0.239f, 8.6f), new Vector3(6.4f, 0.01f, 7f), new Color(0.052f, 0.094f, 0.042f, 1f));
        }

        private static void CreateForestDepthFogPolish(Transform parent)
        {
            var root = new GameObject("ForestDepthFogPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ForestPolishMossCarpet", new Vector3(-89f, 0.115f, 12f), new Vector3(52f, 0.012f, 28f), new Color(0.026f, 0.07f, 0.028f, 1f));
            CreateSurfacePatch(root.transform, "ForestPolishTrailDarkerBend", new Vector3(-66f, 0.128f, 2.5f), new Vector3(22f, 0.012f, 5.6f), new Color(0.052f, 0.047f, 0.028f, 1f));

            for (var i = 0; i < 24; i++)
            {
                var row = i / 8;
                var col = i % 8;
                var x = -62f - col * 7.4f - row * 3.5f;
                var z = 21f + row * 7.2f + Mathf.Sin(i * 0.7f) * 2.6f;
                var tree = i % 4 == 0 ? "tree-high-crooked.fbx" : i % 4 == 1 ? "tree-high-round.fbx" : "tree-high.fbx";
                PlaceKenney(root.transform, $"ForestPolishLayeredCanopy_{i + 1:00}", tree, new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 31f, 0f), Vector3.one * (1.14f + row * 0.1f + (i % 3) * 0.05f));
            }

            for (var i = 0; i < 8; i++)
            {
                PlaceKayKit(root.transform, $"ForestPolishRootRock_{i + 1:00}", i % 2 == 0 ? "detail_rocks.fbx" : "detail_rocks_small.fbx", new Vector3(-58f - i * 6.2f, 0f, -4.6f + (i % 3) * 2.4f), Quaternion.Euler(0f, 18f + i * 21f, 0f), Vector3.one * (0.78f + (i % 3) * 0.13f));
            }

            for (var i = 0; i < 7; i++)
            {
                CreateNonBlockingMarker(root.transform, $"ForestPolishLowMistBand_{i + 1:00}", new Vector3(-58f - i * 8.2f, 0.42f, 17f + Mathf.Sin(i) * 3.6f), new Vector3(4.8f, 0.24f, 0.75f), new Color(0.13f, 0.18f, 0.16f, 0.82f));
            }

            CreatePointLight(root.transform, "ForestPolishBlueBacklight", new Vector3(-102f, 3.6f, 26f), new Color(0.36f, 0.5f, 0.72f, 1f), 0.58f, 17f);
        }

        private static void CreateSwampWaterAndReedPolish(Transform parent)
        {
            var root = new GameObject("SwampWaterAndReedPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "SwampPolishMirrorWater_Main", new Vector3(12.2f, 0.09f, -84.5f), new Vector3(34f, 0.012f, 25f), new Color(0.006f, 0.031f, 0.03f, 1f));
            CreateSurfacePatch(root.transform, "SwampPolishMirrorWater_Left", new Vector3(-12f, 0.091f, -74.6f), new Vector3(16f, 0.012f, 15f), new Color(0.008f, 0.035f, 0.032f, 1f));
            CreateSurfacePatch(root.transform, "SwampPolishPoisonGreenBloom", new Vector3(15.4f, 0.102f, -76.8f), new Vector3(8.4f, 0.012f, 6.2f), new Color(0.03f, 0.12f, 0.065f, 1f));
            CreateSurfacePatch(root.transform, "SwampPolishHutDryIsland", new Vector3(17.2f, 0.15f, -71.5f), new Vector3(10.2f, 0.03f, 8.6f), new Color(0.044f, 0.072f, 0.043f, 1f));

            for (var i = 0; i < 14; i++)
            {
                var x = -5.5f + (i % 7) * 4.4f;
                var z = -68.2f - (i / 7) * 17.2f + Mathf.Sin(i * 0.6f) * 2f;
                CreateReedCluster(root.transform, $"SwampPolishTallReedCluster_{i + 1:00}", new Vector3(x, 0.05f, z), 1.15f + (i % 3) * 0.18f);
            }

            for (var i = 0; i < 7; i++)
            {
                PlaceKenney(root.transform, $"SwampPolishRottenBoardwalk_{i + 1:00}", i % 2 == 0 ? "planks-opening.fbx" : "planks-half.fbx", new Vector3(0.5f + i * 3.1f, 0.045f, -86.0f + Mathf.Sin(i * 0.85f) * 2.4f), Quaternion.Euler(0f, 78f + i * 8f, 0f), Vector3.one * 0.82f);
            }

            PlaceKenney(root.transform, "SwampPolishCrookedTreeHero", "tree-high-crooked.fbx", new Vector3(27.2f, 0f, -88.4f), Quaternion.Euler(0f, -36f, 0f), Vector3.one * 1.42f);
            PlaceKenney(root.transform, "SwampPolishHalfSunkenCart", "cart-high.fbx", new Vector3(1.4f, -0.08f, -93.6f), Quaternion.Euler(8f, 18f, -10f), Vector3.one * 0.86f);
            CreatePointLight(root.transform, "SwampPolishWaterGlow", new Vector3(15.1f, 1.1f, -78.4f), new Color(0.05f, 0.58f, 0.35f, 1f), 0.62f, 14f);
        }

        private static void CreateAshRoadSmokeAndRuinPolish(Transform parent)
        {
            var root = new GameObject("AshRoadSmokeAndRuinPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "AshRoadPolishMainSootStreet", new Vector3(101f, 0.125f, 6.5f), new Vector3(62f, 0.012f, 9.2f), new Color(0.065f, 0.054f, 0.047f, 1f));
            CreateSurfacePatch(root.transform, "AshRoadPolishBurnedVillageFloor", new Vector3(103f, 0.126f, 24.5f), new Vector3(45f, 0.012f, 17f), new Color(0.095f, 0.07f, 0.055f, 1f));

            for (var i = 0; i < 12; i++)
            {
                var x = 80f + i * 4.5f;
                var z = 16f + Mathf.Sin(i * 0.8f) * 8f;
                var model = i % 4 == 0 ? "wall-wood-broken.fbx" : i % 4 == 1 ? "wall-broken.fbx" : i % 4 == 2 ? "pillar-wood.fbx" : "fence-broken.fbx";
                PlaceKenney(root.transform, $"AshRoadPolishLayeredRuin_{i + 1:00}", model, new Vector3(x, 0f, z), Quaternion.Euler(0f, -38f + i * 19f, 0f), Vector3.one * (0.92f + (i % 3) * 0.11f));
            }

            for (var i = 0; i < 9; i++)
            {
                CreateNonBlockingMarker(root.transform, $"AshRoadPolishSmokeColumn_{i + 1:00}", new Vector3(84f + i * 5.7f, 1.0f + (i % 3) * 0.15f, 11f + Mathf.Sin(i) * 9f), new Vector3(1.0f + (i % 2) * 0.32f, 1.7f + (i % 3) * 0.38f, 1.0f), new Color(0.12f, 0.115f, 0.105f, 0.9f));
                CreateNonBlockingMarker(root.transform, $"AshRoadPolishEmberPatch_{i + 1:00}", new Vector3(83f + i * 5.8f, 0.18f, 4.2f + Mathf.Cos(i) * 5f), new Vector3(1.4f, 0.05f, 0.7f), new Color(0.64f, 0.12f, 0.035f, 1f));
            }

            PlaceKayKit(root.transform, "AshRoadPolishCollapsedBarracksSilhouette", "barracks.fbx", new Vector3(128f, 0f, 25.2f), Quaternion.Euler(0f, -22f, 0f), Vector3.one * 0.92f);
            PlaceKayKit(root.transform, "AshRoadPolishRearWatchtowerSilhouette", "watchtower.fbx", new Vector3(132f, 0f, -8.2f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 0.82f);
            CreatePointLight(root.transform, "AshRoadPolishDeepFireGlow", new Vector3(108f, 1.7f, 16f), new Color(1f, 0.25f, 0.08f, 1f), 0.78f, 14f);
        }

        private static void CreateTowerRitualSkylinePolish(Transform parent)
        {
            var root = new GameObject("TowerRitualSkylinePolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerPolishBrokenCauseway", new Vector3(0f, 0.135f, 103f), new Vector3(12f, 0.012f, 45f), new Color(0.07f, 0.066f, 0.077f, 1f));
            CreateSurfacePatch(root.transform, "TowerPolishMirrorLightPool", new Vector3(0f, 0.158f, 78.5f), new Vector3(10f, 0.012f, 7.5f), new Color(0.22f, 0.14f, 0.36f, 1f));

            for (var i = 0; i < 14; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var z = 83f + i * 4.2f;
                var model = i % 4 == 0 ? "wall-arch.fbx" : i % 4 == 1 ? "wall-arch-top.fbx" : i % 4 == 2 ? "pillar-stone.fbx" : "wall-broken.fbx";
                PlaceKenney(root.transform, $"TowerPolishProcessionRuin_{i + 1:00}", model, new Vector3(side * (7.5f + (i % 3) * 1.6f), 0f, z), Quaternion.Euler(0f, side * (12f + i * 5f), 0f), Vector3.one * (0.85f + (i % 4) * 0.08f));
            }

            for (var i = 0; i < 8; i++)
            {
                CreateNonBlockingMarker(root.transform, $"TowerPolishFloatingMirrorShard_{i + 1:00}", new Vector3(-4.5f + i * 1.3f, 1.05f + (i % 3) * 0.35f, 77.8f + Mathf.Sin(i) * 1.8f), new Vector3(0.12f, 0.9f + (i % 2) * 0.3f, 0.08f), new Color(0.36f, 0.27f, 0.5f, 1f));
            }

            PlaceKayKit(root.transform, "TowerPolishFarCastleSilhouette", "castle.fbx", new Vector3(0f, 0f, 151f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.4f);
            PlaceKayKit(root.transform, "TowerPolishLeftMountainBacker", "mountain.fbx", new Vector3(-31f, 0f, 142f), Quaternion.Euler(0f, 26f, 0f), new Vector3(1.8f, 0.65f, 1.8f));
            PlaceKayKit(root.transform, "TowerPolishRightMountainBacker", "mountain.fbx", new Vector3(31f, 0f, 142f), Quaternion.Euler(0f, -26f, 0f), new Vector3(1.8f, 0.65f, 1.8f));
            CreatePointLight(root.transform, "TowerPolishMirrorCoreGlow", new Vector3(0f, 2.8f, 78f), new Color(0.58f, 0.32f, 1f, 1f), 0.9f, 17f);
        }

        private static void CreateTowerPlayableRuinRebuild(Transform parent)
        {
            var root = new GameObject("TowerPlayableRuinRebuild");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerRebuildStoneCourt", new Vector3(0f, 0.151f, 76f), new Vector3(17f, 0.012f, 21f), new Color(0.055f, 0.054f, 0.062f, 1f));
            CreateSurfacePatch(root.transform, "TowerRebuildCentralPath", new Vector3(0f, 0.163f, 76f), new Vector3(4.8f, 0.012f, 21f), new Color(0.09f, 0.085f, 0.095f, 1f));

            PlaceKenney(root.transform, "TowerRebuildEntranceArch", "wall-arch.fbx", new Vector3(0f, 0f, 66.3f), Quaternion.identity, Vector3.one * 1.45f);
            PlaceKenney(root.transform, "TowerRebuildRearBrokenArch", "wall-arch-top.fbx", new Vector3(0f, 0f, 84.7f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.35f);
            PlaceKenney(root.transform, "TowerRebuildLeftWallA", "wall-broken.fbx", new Vector3(-7.3f, 0f, 69.2f), Quaternion.Euler(0f, 82f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "TowerRebuildLeftWallB", "wall-block-half.fbx", new Vector3(-7.6f, 0f, 77.2f), Quaternion.Euler(0f, 94f, 0f), Vector3.one * 1.18f);
            PlaceKenney(root.transform, "TowerRebuildRightWallA", "wall-broken.fbx", new Vector3(7.3f, 0f, 69.7f), Quaternion.Euler(0f, -82f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "TowerRebuildRightWallB", "wall-block-half.fbx", new Vector3(7.6f, 0f, 77.7f), Quaternion.Euler(0f, -94f, 0f), Vector3.one * 1.18f);
            PlaceKenney(root.transform, "TowerRebuildLeftPillar", "pillar-stone.fbx", new Vector3(-4.5f, 0f, 82.2f), Quaternion.Euler(0f, 12f, 0f), Vector3.one * 1.3f);
            PlaceKenney(root.transform, "TowerRebuildRightPillar", "pillar-stone.fbx", new Vector3(4.5f, 0f, 82.2f), Quaternion.Euler(0f, -12f, 0f), Vector3.one * 1.3f);
            PlaceKenney(root.transform, "TowerRebuildInnerStairs", "stairs-wide-stone.fbx", new Vector3(0f, 0f, 82.1f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.15f);

            for (var i = 0; i < 10; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var x = side * (4.8f + (i % 3) * 0.85f);
                var z = 67.5f + i * 1.65f;
                PlaceKenney(root.transform, $"TowerRebuildRubble_{i + 1:00}", i % 3 == 0 ? "rock-small.fbx" : i % 3 == 1 ? "wall-block-half.fbx" : "pillar-stone.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 37f, i % 3 == 0 ? 0f : 78f), Vector3.one * (0.42f + (i % 3) * 0.12f));
            }

            CreatePointLight(root.transform, "TowerRebuildColdCourtLight", new Vector3(0f, 2.4f, 72f), new Color(0.32f, 0.38f, 0.66f, 1f), 0.7f, 15f);
            CreatePointLight(root.transform, "TowerRebuildMirrorHallLight", new Vector3(0f, 2.1f, 82f), new Color(0.52f, 0.28f, 0.84f, 1f), 0.8f, 13f);
        }

        private static void CreateCharacterPresentationPolishPass(Transform parent)
        {
            var root = new GameObject("CharacterPresentationPolishPass");
            root.transform.SetParent(parent, false);

            CreateVillageNpcPresentationPolish(root.transform);
            CreateSwampCharacterPresentationPolish(root.transform);
            CreateForestCharacterPresentationPolish(root.transform);
            CreateTowerCharacterPresentationPolish(root.transform);
            CreateEnemyPresentationPolish(root.transform);
        }

        private static void CreateVillageNpcPresentationPolish(Transform parent)
        {
            var root = new GameObject("VillageNpcPresentationPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ElderPresentationAuthorityMat", new Vector3(-4.1f, 0.165f, -3.4f), new Vector3(2.9f, 0.012f, 2.2f), new Color(0.12f, 0.078f, 0.043f, 1f));
            PlaceKenney(root.transform, "ElderPresentationLedgerBench", "stall-bench.fbx", new Vector3(-5.35f, 0f, -4.55f), Quaternion.Euler(0f, 31f, 0f), Vector3.one * 0.72f);
            PlaceKenney(root.transform, "ElderPresentationRedBanner", "banner-red.fbx", new Vector3(-2.78f, 0f, -4.72f), Quaternion.Euler(0f, 82f, 0f), Vector3.one * 0.78f);
            CreateNonBlockingMarker(root.transform, "ElderPresentationSealGlow", new Vector3(-4.7f, 0.92f, -4.12f), new Vector3(0.22f, 0.42f, 0.05f), new Color(0.78f, 0.52f, 0.16f, 1f));

            CreateSurfacePatch(root.transform, "MartaPresentationHerbCircle", new Vector3(4.2f, 0.166f, -3.6f), new Vector3(3f, 0.012f, 2.3f), new Color(0.045f, 0.105f, 0.048f, 1f));
            PlaceKenney(root.transform, "MartaPresentationGreenBanner", "banner-green.fbx", new Vector3(5.78f, 0f, -4.72f), Quaternion.Euler(0f, -78f, 0f), Vector3.one * 0.76f);
            PlaceKenney(root.transform, "MartaPresentationStool", "stall-stool.fbx", new Vector3(3.1f, 0f, -4.86f), Quaternion.Euler(0f, -30f, 0f), Vector3.one * 0.74f);
            CreateReedCluster(root.transform, "MartaPresentationDryHerbs", new Vector3(5.2f, 0.08f, -2.42f), 0.78f);

            CreateSurfacePatch(root.transform, "BorisPresentationForgeAsh", new Vector3(-4.2f, 0.167f, -0.4f), new Vector3(3.2f, 0.012f, 2.5f), new Color(0.12f, 0.072f, 0.042f, 1f));
            PlaceKenney(root.transform, "BorisPresentationOreCart", "cart.fbx", new Vector3(-5.72f, 0f, 0.65f), Quaternion.Euler(0f, -24f, 0f), Vector3.one * 0.72f);
            PlaceKenney(root.transform, "BorisPresentationBladeDisplay", "blade.fbx", new Vector3(-3.08f, 0.22f, 0.58f), Quaternion.Euler(72f, 0f, -30f), Vector3.one * 0.72f);
            CreatePointLight(root.transform, "BorisPresentationForgeGlow", new Vector3(-4.5f, 1.25f, 0.6f), new Color(1f, 0.28f, 0.08f, 1f), 0.55f, 5.2f);

            CreateSurfacePatch(root.transform, "RadekPresentationTradeMat", new Vector3(3.1f, 0.168f, -0.7f), new Vector3(3.1f, 0.012f, 2.4f), new Color(0.13f, 0.101f, 0.052f, 1f));
            PlaceKenney(root.transform, "RadekPresentationCrateBench", "stall-bench.fbx", new Vector3(4.45f, 0f, -1.72f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.7f);
            PlaceKenney(root.transform, "RadekPresentationSupplyCart", "cart-high.fbx", new Vector3(2.0f, 0f, 0.62f), Quaternion.Euler(0f, -58f, 0f), Vector3.one * 0.68f);
            CreateNonBlockingMarker(root.transform, "RadekPresentationCoinGlimmer", new Vector3(3.32f, 0.58f, -1.42f), new Vector3(0.16f, 0.08f, 0.16f), new Color(0.9f, 0.68f, 0.18f, 1f));
        }

        private static void CreateSwampCharacterPresentationPolish(Transform parent)
        {
            var root = new GameObject("SwampCharacterPresentationPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "ElsaPresentationWardMat", new Vector3(16.6f, 0.165f, -72.6f), new Vector3(3.8f, 0.012f, 3.0f), new Color(0.024f, 0.075f, 0.058f, 1f));
            PlaceKenney(root.transform, "ElsaPresentationCrookedFence", "fence-curved.fbx", new Vector3(18.62f, 0f, -71.8f), Quaternion.Euler(0f, -54f, 0f), Vector3.one * 0.82f);
            PlaceKenney(root.transform, "ElsaPresentationDryingPole", "poles-horizontal.fbx", new Vector3(15.0f, 0f, -70.92f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 0.8f);
            CreateNonBlockingMarker(root.transform, "ElsaPresentationRuneGlowA", new Vector3(15.65f, 0.22f, -73.52f), new Vector3(0.18f, 0.06f, 0.18f), new Color(0.08f, 0.72f, 0.48f, 1f));
            CreateNonBlockingMarker(root.transform, "ElsaPresentationRuneGlowB", new Vector3(17.52f, 0.22f, -72.18f), new Vector3(0.18f, 0.06f, 0.18f), new Color(0.08f, 0.72f, 0.48f, 1f));

            CreateSurfacePatch(root.transform, "GhostPresentationMemoryPool", new Vector3(-2.7f, 0.166f, 75.0f), new Vector3(3.6f, 0.012f, 3.0f), new Color(0.075f, 0.07f, 0.13f, 1f));
            CreateNonBlockingMarker(root.transform, "GhostPresentationColdAura", new Vector3(-2.7f, 0.55f, 75.0f), new Vector3(1.7f, 0.42f, 1.7f), new Color(0.2f, 0.27f, 0.42f, 0.9f));
            PlaceKenney(root.transform, "GhostPresentationFallenStone", "wall-arch-top.fbx", new Vector3(-4.2f, 0f, 76.4f), Quaternion.Euler(0f, 32f, 0f), Vector3.one * 0.62f);
        }

        private static void CreateForestCharacterPresentationPolish(Transform parent)
        {
            var root = new GameObject("ForestCharacterPresentationPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "IvarPresentationHunterMat", new Vector3(-67.7f, 0.165f, 7.2f), new Vector3(3.4f, 0.012f, 2.8f), new Color(0.043f, 0.08f, 0.04f, 1f));
            PlaceKenney(root.transform, "IvarPresentationBrokenCart", "cart.fbx", new Vector3(-69.45f, 0f, 8.92f), Quaternion.Euler(0f, 36f, 0f), Vector3.one * 0.68f);
            PlaceKenney(root.transform, "IvarPresentationBowMarker", "blade.fbx", new Vector3(-66.42f, 0.1f, 8.55f), Quaternion.Euler(80f, 0f, 38f), Vector3.one * 0.58f);
            CreateNonBlockingMarker(root.transform, "IvarPresentationTrackMarks", new Vector3(-66.8f, 0.18f, 6.2f), new Vector3(1.1f, 0.03f, 0.22f), new Color(0.12f, 0.065f, 0.035f, 1f));
        }

        private static void CreateTowerCharacterPresentationPolish(Transform parent)
        {
            var root = new GameObject("TowerCharacterPresentationPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "OrtenPresentationMirrorStage", new Vector3(0f, 0.166f, 78.2f), new Vector3(4.2f, 0.012f, 3.2f), new Color(0.13f, 0.085f, 0.22f, 1f));
            PlaceKenney(root.transform, "OrtenPresentationLeftShardFrame", "wall-arch.fbx", new Vector3(-2.4f, 0f, 79.0f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.7f);
            PlaceKenney(root.transform, "OrtenPresentationRightShardFrame", "wall-arch.fbx", new Vector3(2.4f, 0f, 79.0f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.7f);
            CreateNonBlockingMarker(root.transform, "OrtenPresentationMirrorColumn", new Vector3(0f, 1.2f, 79.45f), new Vector3(0.18f, 1.8f, 0.08f), new Color(0.4f, 0.28f, 0.55f, 1f));
            CreatePointLight(root.transform, "OrtenPresentationFaceLight", new Vector3(0f, 2.1f, 77.3f), new Color(0.52f, 0.32f, 0.92f, 1f), 0.45f, 6.0f);
        }

        private static void CreateEnemyPresentationPolish(Transform parent)
        {
            var root = new GameObject("EnemyPresentationPolish");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "DrownerPresentationThreatWater", new Vector3(12.5f, 0.17f, -75.4f), new Vector3(4.4f, 0.012f, 3.2f), new Color(0.01f, 0.045f, 0.035f, 1f));
            CreateNonBlockingMarker(root.transform, "DrownerPresentationSplashA", new Vector3(11.65f, 0.36f, -74.85f), new Vector3(0.16f, 0.55f, 0.16f), new Color(0.06f, 0.38f, 0.28f, 1f));
            CreateNonBlockingMarker(root.transform, "DrownerPresentationSplashB", new Vector3(13.28f, 0.31f, -76.1f), new Vector3(0.14f, 0.48f, 0.14f), new Color(0.06f, 0.34f, 0.25f, 1f));

            CreateSurfacePatch(root.transform, "SkeletonPresentationGraveDust", new Vector3(0f, 0.17f, 75.5f), new Vector3(10.2f, 0.012f, 3.4f), new Color(0.068f, 0.063f, 0.055f, 1f));
            PlaceKenney(root.transform, "SkeletonPresentationLeftBrokenPillar", "pillar-stone.fbx", new Vector3(-6.0f, 0f, 74.2f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.76f);
            PlaceKenney(root.transform, "SkeletonPresentationRightBrokenPillar", "pillar-stone.fbx", new Vector3(6.0f, 0f, 74.2f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.76f);

            CreateSurfacePatch(root.transform, "BanditPresentationAmbushDirt", new Vector3(55.5f, 0.17f, 2.2f), new Vector3(12.8f, 0.012f, 8.8f), new Color(0.105f, 0.073f, 0.052f, 1f));
            PlaceKenney(root.transform, "BanditPresentationRoadBlockCart", "cart-high.fbx", new Vector3(57.8f, 0f, 1.2f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.72f);
            PlaceKenney(root.transform, "BanditPresentationSpikeFenceA", "fence-broken.fbx", new Vector3(52.4f, 0f, 2.2f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 0.92f);
            PlaceKenney(root.transform, "BanditPresentationSpikeFenceB", "fence-broken.fbx", new Vector3(60.7f, 0f, -1.6f), Quaternion.Euler(0f, -56f, 0f), Vector3.one * 0.92f);
            CreateNonBlockingMarker(root.transform, "BanditPresentationFreshAsh", new Vector3(55.4f, 0.21f, 0.2f), new Vector3(2.2f, 0.04f, 0.85f), new Color(0.18f, 0.16f, 0.13f, 1f));
        }

        private static void CreateDynamicAmbientVfxPass(Transform parent)
        {
            var root = new GameObject("DynamicAmbientVfxPass");
            root.transform.SetParent(parent, false);

            CreateVillageDynamicVfx(root.transform);
            CreateForestDynamicVfx(root.transform);
            CreateSwampDynamicVfx(root.transform);
            CreateAshRoadDynamicVfx(root.transform);
            CreateTowerDynamicVfx(root.transform);
        }

        private static void CreateVillageDynamicVfx(Transform parent)
        {
            var root = new GameObject("VillageDynamicVfx");
            root.transform.SetParent(parent, false);

            var chimneyPositions = new[]
            {
                new Vector3(-12.2f, 4.4f, 7.8f),
                new Vector3(11.8f, 4.2f, 7.1f),
                new Vector3(-10.8f, 4.0f, -11.4f)
            };

            for (var chimney = 0; chimney < chimneyPositions.Length; chimney++)
            {
                for (var puff = 0; puff < 3; puff++)
                {
                    CreateAnimatedVfxMarker(
                        root.transform,
                        $"VillageDynamicChimneySmoke_{chimney + 1:00}_{puff + 1:00}",
                        PrimitiveType.Sphere,
                        chimneyPositions[chimney] + new Vector3(puff * 0.18f, puff * 0.62f, puff * 0.12f),
                        Vector3.one * (0.34f + puff * 0.13f),
                        new Color(0.18f, 0.18f, 0.17f, 1f),
                        new Vector3(0.12f, 0.28f, 0.12f),
                        new Vector3(0f, 8f, 0f),
                        0.42f + puff * 0.08f,
                        0.08f,
                        false);
                }
            }

            for (var i = 0; i < 6; i++)
            {
                CreateAnimatedVfxMarker(
                    root.transform,
                    $"VillageDynamicForgeSpark_{i + 1:00}",
                    PrimitiveType.Sphere,
                    new Vector3(-4.5f + Mathf.Sin(i) * 0.7f, 0.8f + (i % 3) * 0.35f, 0.6f + Mathf.Cos(i) * 0.55f),
                    Vector3.one * 0.055f,
                    new Color(1f, 0.3f, 0.045f, 1f),
                    new Vector3(0.16f, 0.32f, 0.16f),
                    new Vector3(20f, 40f, 10f),
                    1.8f + i * 0.08f,
                    0.22f,
                    i == 0);
            }
        }

        private static void CreateForestDynamicVfx(Transform parent)
        {
            var root = new GameObject("ForestDynamicVfx");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 14; i++)
            {
                var x = -62f - (i % 7) * 7.2f;
                var z = 6f + (i / 7) * 13f + Mathf.Sin(i * 0.8f) * 3f;
                CreateAnimatedVfxMarker(
                    root.transform,
                    $"ForestDynamicMoonMote_{i + 1:00}",
                    PrimitiveType.Sphere,
                    new Vector3(x, 0.8f + (i % 4) * 0.42f, z),
                    Vector3.one * (0.045f + (i % 3) * 0.014f),
                    new Color(0.48f, 0.62f, 0.78f, 1f),
                    new Vector3(0.3f, 0.18f, 0.3f),
                    new Vector3(0f, 34f, 0f),
                    0.65f + (i % 4) * 0.12f,
                    0.18f,
                    false);
            }
        }

        private static void CreateSwampDynamicVfx(Transform parent)
        {
            var root = new GameObject("SwampDynamicVfx");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 10; i++)
            {
                var angle = i * Mathf.PI * 2f / 10f;
                var radius = 6.5f + (i % 3) * 2.3f;
                CreateAnimatedVfxMarker(
                    root.transform,
                    $"SwampDynamicWillOWisp_{i + 1:00}",
                    PrimitiveType.Sphere,
                    new Vector3(12f + Mathf.Cos(angle) * radius, 0.75f + (i % 4) * 0.28f, -82f + Mathf.Sin(angle) * radius),
                    Vector3.one * (0.12f + (i % 2) * 0.04f),
                    new Color(0.06f, 0.78f, 0.46f, 1f),
                    new Vector3(0.38f, 0.25f, 0.38f),
                    new Vector3(0f, 45f, 0f),
                    0.72f + (i % 3) * 0.16f,
                    0.24f,
                    i == 0 || i == 5);
            }
        }

        private static void CreateAshRoadDynamicVfx(Transform parent)
        {
            var root = new GameObject("AshRoadDynamicVfx");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 18; i++)
            {
                var x = 78f + (i % 9) * 6.2f;
                var z = 2f + (i / 9) * 14f + Mathf.Sin(i * 0.7f) * 4f;
                CreateAnimatedVfxMarker(
                    root.transform,
                    $"AshRoadDynamicEmber_{i + 1:00}",
                    PrimitiveType.Sphere,
                    new Vector3(x, 0.45f + (i % 5) * 0.3f, z),
                    Vector3.one * (0.045f + (i % 3) * 0.012f),
                    new Color(1f, 0.22f, 0.035f, 1f),
                    new Vector3(0.28f, 0.44f, 0.28f),
                    new Vector3(16f, 70f, 20f),
                    1.15f + (i % 4) * 0.14f,
                    0.28f,
                    i == 4 || i == 13);
            }
        }

        private static void CreateTowerDynamicVfx(Transform parent)
        {
            var root = new GameObject("TowerDynamicVfx");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 12; i++)
            {
                var angle = i * Mathf.PI * 2f / 12f;
                var radius = 3.2f + (i % 3) * 1.1f;
                CreateAnimatedVfxMarker(
                    root.transform,
                    $"TowerDynamicMirrorFragment_{i + 1:00}",
                    PrimitiveType.Cube,
                    new Vector3(Mathf.Cos(angle) * radius, 1.0f + (i % 4) * 0.48f, 78.2f + Mathf.Sin(angle) * radius),
                    new Vector3(0.08f, 0.45f + (i % 2) * 0.22f, 0.035f),
                    new Color(0.42f, 0.3f, 0.56f, 1f),
                    new Vector3(0.2f, 0.22f, 0.2f),
                    new Vector3(32f + i * 3f, 46f + i * 4f, 18f),
                    0.65f + (i % 3) * 0.12f,
                    0.12f,
                    i == 0 || i == 6);
            }
        }

        private static GameObject CreateAnimatedVfxMarker(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Color color,
            Vector3 bobAmplitude,
            Vector3 rotationSpeed,
            float motionSpeed,
            float scalePulse,
            bool addLight)
        {
            var marker = GameObject.CreatePrimitive(primitiveType);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateEmissiveMaterial($"Assets/Materials/{name}.mat", color, 2.1f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            if (addLight)
            {
                var light = marker.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = color;
                light.intensity = 0.46f;
                light.range = 5.5f;
                light.shadows = LightShadows.None;
            }

            var motion = marker.AddComponent<AmbientVisualMotion>();
            motion.Configure(bobAmplitude, rotationSpeed, motionSpeed, scalePulse, 0.24f);
            return marker;
        }

        private static void CreateRouteCinematicCompositionPass(Transform parent)
        {
            var root = new GameObject("RouteCinematicCompositionPass");
            root.transform.SetParent(parent, false);

            CreateVillageCinematicApproach(root.transform);
            CreateForestCinematicApproach(root.transform);
            CreateSwampCinematicApproach(root.transform);
            CreateAshRoadCinematicApproach(root.transform);
            CreateTowerCinematicApproach(root.transform);
        }

        private static void CreateVillageCinematicApproach(Transform parent)
        {
            var root = new GameObject("VillageCinematicApproach");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "VillageCinematicEntranceDirtFan", new Vector3(0f, 0.181f, -18.8f), new Vector3(18f, 0.012f, 10f), new Color(0.118f, 0.088f, 0.052f, 1f));
            CreateSurfacePatch(root.transform, "VillageCinematicWheelRutsLeft", new Vector3(-1.7f, 0.188f, -17.4f), new Vector3(0.42f, 0.01f, 9.6f), new Color(0.055f, 0.044f, 0.032f, 1f));
            CreateSurfacePatch(root.transform, "VillageCinematicWheelRutsRight", new Vector3(1.7f, 0.188f, -17.4f), new Vector3(0.42f, 0.01f, 9.6f), new Color(0.055f, 0.044f, 0.032f, 1f));

            PlaceKenney(root.transform, "VillageCinematicLeftBanner", "banner-red.fbx", new Vector3(-6.8f, 0f, -18.6f), Quaternion.Euler(0f, 58f, 0f), Vector3.one * 1.18f);
            PlaceKenney(root.transform, "VillageCinematicRightBanner", "banner-green.fbx", new Vector3(6.8f, 0f, -18.6f), Quaternion.Euler(0f, -58f, 0f), Vector3.one * 1.18f);
            PlaceKenney(root.transform, "VillageCinematicLeftCart", "cart-high.fbx", new Vector3(-8.9f, 0f, -15.3f), Quaternion.Euler(0f, 34f, 0f), Vector3.one * 0.9f);
            PlaceKenney(root.transform, "VillageCinematicRightSupplyPlanks", "planks.fbx", new Vector3(8.8f, 0f, -15.0f), Quaternion.Euler(0f, -22f, 0f), Vector3.one * 0.95f);

            CreatePointLight(root.transform, "VillageCinematicGateWarmLight", new Vector3(0f, 1.8f, -15.6f), new Color(0.95f, 0.52f, 0.24f, 1f), 0.62f, 11f);
        }

        private static void CreateForestCinematicApproach(Transform parent)
        {
            var root = new GameObject("ForestCinematicApproach");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 8; i++)
            {
                var z = -6f + i * 3.25f;
                var leftScale = 1.35f + i * 0.08f;
                var rightScale = 1.18f + i * 0.07f;
                PlaceKenney(root.transform, $"ForestCinematicLeftWallTree_{i + 1:00}", i % 2 == 0 ? "tree-high.fbx" : "tree-high-crooked.fbx", new Vector3(-43.5f - i * 3.2f, 0f, z), Quaternion.Euler(0f, 18f + i * 29f, 0f), Vector3.one * leftScale);
                PlaceKenney(root.transform, $"ForestCinematicRightWallTree_{i + 1:00}", i % 2 == 0 ? "tree.fbx" : "tree-high.fbx", new Vector3(-39.2f - i * 3.0f, 0f, z + 1.3f), Quaternion.Euler(0f, -22f + i * 17f, 0f), Vector3.one * rightScale);
                CreateSurfacePatch(root.transform, $"ForestCinematicRootShadow_{i + 1:00}", new Vector3(-41.4f - i * 3.1f, 0.184f, z + 0.6f), new Vector3(5.2f, 0.01f, 1.2f), new Color(0.018f, 0.048f, 0.022f, 1f));
            }

            PlaceKayKit(root.transform, "ForestCinematicFallenArch", "detail_rocks.fbx", new Vector3(-58.6f, 0f, 5.4f), Quaternion.Euler(0f, -8f, 0f), Vector3.one * 1.2f);
            CreatePointLight(root.transform, "ForestCinematicBlueBackLight", new Vector3(-62.0f, 2.2f, 10.8f), new Color(0.34f, 0.48f, 0.64f, 1f), 0.42f, 13f);
        }

        private static void CreateSwampCinematicApproach(Transform parent)
        {
            var root = new GameObject("SwampCinematicApproach");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "SwampCinematicWaterMirrorLeft", new Vector3(-5.6f, 0.178f, -63f), new Vector3(10.5f, 0.012f, 18f), new Color(0.006f, 0.03f, 0.028f, 1f));
            CreateSurfacePatch(root.transform, "SwampCinematicWaterMirrorRight", new Vector3(11.4f, 0.179f, -63.8f), new Vector3(12.5f, 0.012f, 19f), new Color(0.007f, 0.035f, 0.03f, 1f));
            CreateSurfacePatch(root.transform, "SwampCinematicSafeMudSpine", new Vector3(2.2f, 0.19f, -63.2f), new Vector3(4.2f, 0.012f, 21f), new Color(0.032f, 0.064f, 0.045f, 1f));

            for (var i = 0; i < 7; i++)
            {
                var z = -54f - i * 3.0f;
                PlaceKenney(root.transform, $"SwampCinematicReedWallLeft_{i + 1:00}", "tree-crooked.fbx", new Vector3(-7.6f - (i % 2) * 1.3f, 0f, z), Quaternion.Euler(0f, 16f + i * 19f, 0f), new Vector3(0.72f, 0.86f + i * 0.03f, 0.72f));
                PlaceKenney(root.transform, $"SwampCinematicReedWallRight_{i + 1:00}", "tree-crooked.fbx", new Vector3(11.4f + (i % 2) * 1.5f, 0f, z - 0.8f), Quaternion.Euler(0f, -22f + i * 21f, 0f), new Vector3(0.78f, 0.9f + i * 0.03f, 0.78f));
                PlaceKenney(root.transform, $"SwampCinematicPlankStep_{i + 1:00}", i % 2 == 0 ? "planks-half.fbx" : "planks-opening.fbx", new Vector3(2.3f + Mathf.Sin(i * 0.7f) * 0.8f, 0.035f, z - 0.6f), Quaternion.Euler(0f, 86f + i * 7f, 0f), Vector3.one * 0.8f);
            }

            CreatePointLight(root.transform, "SwampCinematicGreenGuideLight", new Vector3(2.4f, 1.1f, -66.2f), new Color(0.08f, 0.54f, 0.34f, 1f), 0.72f, 13f);
        }

        private static void CreateAshRoadCinematicApproach(Transform parent)
        {
            var root = new GameObject("AshRoadCinematicApproach");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "AshRoadCinematicBlackenedSpine", new Vector3(76f, 0.181f, 2.6f), new Vector3(38f, 0.012f, 8.0f), new Color(0.052f, 0.044f, 0.038f, 1f));
            CreateSurfacePatch(root.transform, "AshRoadCinematicRedAshEdgeNorth", new Vector3(76f, 0.186f, 7.2f), new Vector3(34f, 0.01f, 1.2f), new Color(0.12f, 0.045f, 0.025f, 1f));
            CreateSurfacePatch(root.transform, "AshRoadCinematicRedAshEdgeSouth", new Vector3(76f, 0.186f, -2.1f), new Vector3(34f, 0.01f, 1.2f), new Color(0.12f, 0.045f, 0.025f, 1f));

            for (var i = 0; i < 8; i++)
            {
                var x = 60f + i * 4.5f;
                PlaceKenney(root.transform, $"AshRoadCinematicBrokenWallNorth_{i + 1:00}", i % 2 == 0 ? "wall-broken.fbx" : "wall-wood-broken.fbx", new Vector3(x, 0f, 8.9f + (i % 2) * 1.8f), Quaternion.Euler(0f, -32f + i * 11f, 0f), Vector3.one * (0.9f + (i % 3) * 0.08f));
                PlaceKenney(root.transform, $"AshRoadCinematicCharPostSouth_{i + 1:00}", "pillar-wood.fbx", new Vector3(x + 1.4f, 0f, -4.2f - (i % 2) * 1.1f), Quaternion.Euler(0f, 12f + i * 23f, 0f), new Vector3(0.7f, 1.05f + (i % 3) * 0.08f, 0.7f));
            }

            CreatePointLight(root.transform, "AshRoadCinematicLowFireGuide", new Vector3(82.5f, 0.9f, 4.8f), new Color(0.95f, 0.2f, 0.08f, 1f), 0.62f, 12f);
        }

        private static void CreateTowerCinematicApproach(Transform parent)
        {
            var root = new GameObject("TowerCinematicApproach");
            root.transform.SetParent(parent, false);

            CreateSurfacePatch(root.transform, "TowerCinematicCausewayCenter", new Vector3(0f, 0.182f, 118f), new Vector3(10f, 0.012f, 32f), new Color(0.058f, 0.056f, 0.067f, 1f));
            CreateSurfacePatch(root.transform, "TowerCinematicMirrorScar", new Vector3(0f, 0.191f, 116f), new Vector3(4.6f, 0.01f, 25f), new Color(0.16f, 0.1f, 0.27f, 1f));

            for (var i = 0; i < 7; i++)
            {
                var z = 104f + i * 4.2f;
                var sideOffset = 7.2f + (i % 2) * 1.4f;
                PlaceKenney(root.transform, $"TowerCinematicLeftPillar_{i + 1:00}", i % 3 == 0 ? "wall-arch.fbx" : "pillar-stone.fbx", new Vector3(-sideOffset, 0f, z), Quaternion.Euler(0f, 16f + i * 9f, 0f), Vector3.one * (0.92f + i * 0.04f));
                PlaceKenney(root.transform, $"TowerCinematicRightPillar_{i + 1:00}", i % 3 == 1 ? "wall-arch-top.fbx" : "pillar-stone.fbx", new Vector3(sideOffset, 0f, z + 0.8f), Quaternion.Euler(0f, -16f - i * 9f, 0f), Vector3.one * (0.92f + i * 0.04f));
                CreateNonBlockingMarker(root.transform, $"TowerCinematicVioletDust_{i + 1:00}", new Vector3(0f, 0.205f, z + 1.6f), new Vector3(2.3f, 0.012f, 0.42f), new Color(0.24f, 0.14f, 0.42f, 1f));
            }

            CreatePointLight(root.transform, "TowerCinematicVioletGuideLight", new Vector3(0f, 2.2f, 121f), new Color(0.48f, 0.28f, 0.86f, 1f), 0.78f, 16f);
        }

        private static void CreateWorldDressing(Transform parent)
        {
            var root = new GameObject("WorldDressingRoot");
            root.transform.SetParent(parent, false);

            CreateVillageDressing(root.transform);
            CreateForestDressing(root.transform);
            CreateSwampDressing(root.transform);
            CreateTowerDressing(root.transform);
            CreateAshRoadDressing(root.transform);
            CreateTravelRouteDressing(root.transform);
            CreateExtraDecoration(root.transform);
            CreateMapFillStructures(root.transform);
        }

        // Fills the empty travel corridors and village outskirts with solid landmark
        // buildings from the KayKit pack, so the map reads as a fuller settled world
        // instead of long empty roads. All get colliders via PlaceKayKit.
        private static void CreateMapFillStructures(Transform parent)
        {
            var root = new GameObject("MapFillStructures");
            root.transform.SetParent(parent, false);

            // Village outskirts: working buildings around the hub.
            PlaceKayKit(root.transform, "OutskirtsLumbermill", "lumbermill.fbx", new Vector3(-15.5f, 0f, -8.5f), Quaternion.Euler(0f, 40f, 0f), Vector3.one * 2.1f);
            PlaceKayKit(root.transform, "OutskirtsMill", "mill.fbx", new Vector3(15.5f, 0f, -9.5f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * 2.1f);
            PlaceKayKit(root.transform, "OutskirtsFarmPlot_A", "farm_plot.fbx", new Vector3(16.5f, 0f, 7f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.7f);
            PlaceKayKit(root.transform, "OutskirtsFarmPlot_B", "farm_plot.fbx", new Vector3(-16f, 0f, 7.5f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 1.7f);

            // West road to the forest: a watchtower and an archery range.
            PlaceKayKit(root.transform, "WestRoadWatchtower", "watchtower.fbx", new Vector3(-34f, 0f, -4.5f), Quaternion.Euler(0f, 64f, 0f), Vector3.one * 2.0f);
            PlaceKayKit(root.transform, "WestRoadArcheryRange", "archeryrange.fbx", new Vector3(-50f, 0f, 6f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.9f);

            // North road to the tower: a watchtower landmark.
            PlaceKayKit(root.transform, "NorthRoadWatchtower", "watchtower.fbx", new Vector3(-4.5f, 0f, 42f), Quaternion.Euler(0f, -28f, 0f), Vector3.one * 2.0f);

            // South road to the swamp: an abandoned watermill.
            PlaceKayKit(root.transform, "SouthRoadWatermill", "watermill.fbx", new Vector3(-5f, 0f, -42f), Quaternion.Euler(0f, 50f, 0f), Vector3.one * 1.9f);

            // East road to the ash road: a ruined mine and barracks shell.
            PlaceKayKit(root.transform, "EastRoadMine", "mine.fbx", new Vector3(42f, 0f, 5.5f), Quaternion.Euler(0f, -52f, 0f), Vector3.one * 1.9f);
            PlaceKayKit(root.transform, "EastRoadBarracksRuin", "barracks.fbx", new Vector3(52f, 0f, -3f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 1.85f);
        }

        // Additional natural decoration built from the existing CC0 KayKit/Kenney
        // vocabulary: distant mountain/hill horizon silhouettes for depth, plus tree
        // and rock clusters feathering each district approach. Deterministic (fixed
        // seed) so rebuilds are reproducible. Lives under WorldDressingRoot, which is
        // cleared on rebuild via RemoveIfExists("VelemarWorldRoot").
        private static void CreateExtraDecoration(Transform parent)
        {
            var root = new GameObject("WorldExtraDecorationRoot");
            root.transform.SetParent(parent, false);

            var previousState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(20260613);

            // Far horizon mountain ring for silhouette depth on every sightline.
            var mountainRing = new[]
            {
                new Vector3(-92f, 0f, -86f), new Vector3(-40f, 0f, -96f), new Vector3(18f, 0f, -98f),
                new Vector3(72f, 0f, -90f), new Vector3(96f, 0f, -40f), new Vector3(98f, 0f, 30f),
                new Vector3(88f, 0f, 80f), new Vector3(36f, 0f, 98f), new Vector3(-30f, 0f, 99f),
                new Vector3(-86f, 0f, 84f), new Vector3(-98f, 0f, 28f), new Vector3(-96f, 0f, -34f)
            };
            for (var i = 0; i < mountainRing.Length; i++)
            {
                var s = 4.6f + UnityEngine.Random.Range(-0.8f, 1.6f);
                PlaceKayKit(root.transform, $"HorizonMountain_{i + 1:00}", "mountain.fbx", mountainRing[i],
                    Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), new Vector3(s, s * 0.92f, s));
            }

            // Tree/rock clusters feathering each district edge (away from gameplay actors).
            var villageGrove = new[] { new Vector3(-13f, 0f, 8f), new Vector3(13.5f, 0f, 9.5f), new Vector3(-15f, 0f, -10f) };
            ScatterGrove(root.transform, "VillageGrove", villageGrove, 4, 9f);

            var forestGrove = new[] { new Vector3(-82f, 0f, 22f), new Vector3(-64f, 0f, 24f), new Vector3(-86f, 0f, 2f), new Vector3(-58f, 0f, -6f) };
            ScatterGrove(root.transform, "ForestGrove", forestGrove, 6, 12f);

            var towerApproach = new[] { new Vector3(-12f, 0f, 64f), new Vector3(12f, 0f, 64f), new Vector3(-9f, 0f, 86f) };
            ScatterGrove(root.transform, "TowerApproachGrove", towerApproach, 3, 7f);

            // Farm plots and a hill mass to read the village as inhabited land.
            PlaceKayKit(root.transform, "VillageFarmPlot_A", "farm_plot.fbx", new Vector3(-14.5f, 0f, -2.5f), Quaternion.Euler(0f, 12f, 0f), Vector3.one * 1.4f);
            PlaceKayKit(root.transform, "VillageFarmPlot_B", "farm_plot.fbx", new Vector3(14.8f, 0f, -1.0f), Quaternion.Euler(0f, -24f, 0f), Vector3.one * 1.4f);
            PlaceKayKit(root.transform, "VillageHillMass_W", "detail_hill.fbx", new Vector3(-20f, 0f, 3f), Quaternion.Euler(0f, 40f, 0f), Vector3.one * 2.1f);
            PlaceKayKit(root.transform, "ForestHillMass_N", "detail_hill.fbx", new Vector3(-70f, 0f, 30f), Quaternion.Euler(0f, -30f, 0f), Vector3.one * 2.4f);

            // A few warm "inhabited" accent lights at the new village groves/farms.
            var decoLights = new GameObject("WorldExtraDecorationLights");
            decoLights.transform.SetParent(root.transform, false);
            CreatePointLight(decoLights.transform, "VillageFarmHearthGlow", new Vector3(-14.5f, 1.4f, -2.5f), new Color(1f, 0.6f, 0.28f, 1f), 0.45f, 7f);
            CreatePointLight(decoLights.transform, "VillageEastGroveGlow", new Vector3(13.5f, 1.3f, 9.5f), new Color(0.95f, 0.66f, 0.36f, 1f), 0.32f, 6.5f);

            UnityEngine.Random.state = previousState;
        }

        // Places a varied cluster of trees and rocks around each anchor, jittered for
        // a natural (non-grid) look using the existing Kenney/KayKit nature FBX.
        private static void ScatterGrove(Transform parent, string namePrefix, Vector3[] anchors, int perAnchor, float spread)
        {
            string[] trees = { "tree.fbx", "tree-high.fbx", "tree-high-round.fbx", "tree-crooked.fbx", "tree-high-crooked.fbx" };
            string[] kayDetails = { "detail_treeA.fbx", "detail_treeB.fbx", "detail_treeC.fbx" };
            string[] rocks = { "rock-large.fbx", "rock-small.fbx", "rock-wide.fbx" };

            var index = 0;
            foreach (var anchor in anchors)
            {
                for (var j = 0; j < perAnchor; j++)
                {
                    index++;
                    var offset = new Vector3(UnityEngine.Random.Range(-spread, spread), 0f, UnityEngine.Random.Range(-spread, spread));
                    var pos = anchor + offset;
                    var rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

                    if (j % 4 == 3)
                    {
                        var rs = UnityEngine.Random.Range(0.8f, 1.4f);
                        PlaceKenney(parent, $"{namePrefix}_Rock_{index:00}", rocks[index % rocks.Length], pos, rot, new Vector3(rs, rs * 0.9f, rs));
                    }
                    else if (j % 3 == 0)
                    {
                        var ts = UnityEngine.Random.Range(1.0f, 1.5f);
                        PlaceKayKit(parent, $"{namePrefix}_Detail_{index:00}", kayDetails[index % kayDetails.Length], pos, rot, Vector3.one * ts);
                    }
                    else
                    {
                        var ts = UnityEngine.Random.Range(1.05f, 1.7f);
                        PlaceKenney(parent, $"{namePrefix}_Tree_{index:00}", trees[index % trees.Length], pos, rot, new Vector3(ts, ts * UnityEngine.Random.Range(0.92f, 1.18f), ts));
                    }
                }
            }
        }

        private static void CreateTravelRouteDressing(Transform parent)
        {
            var root = new GameObject("TravelRouteDressing");
            root.transform.SetParent(parent, false);

            var westStops = new[] { -18f, -32f, -46f, -58f };
            for (var i = 0; i < westStops.Length; i++)
            {
                var x = westStops[i];
                PlaceKenney(root.transform, $"WestRouteLandmark_{i + 1:00}", i % 2 == 0 ? "tree-high.fbx" : "rock-large.fbx", new Vector3(x, 0f, 4.8f + (i % 2) * 2.2f), Quaternion.Euler(0f, 25f * i, 0f), Vector3.one * (1.05f + i * 0.08f));
                PlaceKenney(root.transform, $"WestRouteLantern_{i + 1:00}", "lantern.fbx", new Vector3(x + 2.4f, 0f, -1.5f), Quaternion.identity, Vector3.one * 0.9f);
                CreateMarker(root.transform, $"WestRouteMudTrack_{i + 1:00}", new Vector3(x - 1.1f, 0.052f, 0.65f + (i % 2) * 0.4f), new Vector3(1.6f, 0.025f, 0.32f), new Color(0.085f, 0.07f, 0.045f, 1f));
                if (i % 2 == 1)
                {
                    PlaceKenney(root.transform, $"WestRouteBrokenFence_{i + 1:00}", "fence-broken.fbx", new Vector3(x - 2.6f, 0f, 2.1f), Quaternion.Euler(0f, -28f, 0f), Vector3.one * 0.95f);
                }
            }

            var eastStops = new[] { 18f, 32f, 46f, 58f };
            for (var i = 0; i < eastStops.Length; i++)
            {
                var x = eastStops[i];
                PlaceKenney(root.transform, $"EastRouteLandmark_{i + 1:00}", i % 2 == 0 ? "wall-broken.fbx" : "cart.fbx", new Vector3(x, 0f, 4.5f + (i % 2) * 2.4f), Quaternion.Euler(0f, -18f * i, 0f), Vector3.one * (1.0f + i * 0.07f));
                PlaceKenney(root.transform, $"EastRouteCharredPost_{i + 1:00}", "pillar-wood.fbx", new Vector3(x - 2.1f, 0f, -2.2f), Quaternion.Euler(0f, 18f * i, 0f), new Vector3(0.72f, 1.1f, 0.72f));
                CreateMarker(root.transform, $"EastRouteAshScorch_{i + 1:00}", new Vector3(x + 1.2f, 0.055f, -0.85f), new Vector3(1.35f + i * 0.12f, 0.026f, 0.78f), new Color(0.16f, 0.14f, 0.12f, 1f));
                if (i == 2)
                {
                    CreateMarker(root.transform, "EastRouteLowEmbers", new Vector3(x, 0.09f, 6.7f), new Vector3(0.56f, 0.06f, 0.56f), new Color(0.72f, 0.12f, 0.035f, 1f));
                }
            }

            var northStops = new[] { 18f, 32f, 46f, 59f };
            for (var i = 0; i < northStops.Length; i++)
            {
                var z = northStops[i];
                PlaceKenney(root.transform, $"NorthRouteLandmark_{i + 1:00}", i % 2 == 0 ? "pillar-stone.fbx" : "tree-crooked.fbx", new Vector3(4.8f + (i % 2) * 1.8f, 0f, z), Quaternion.Euler(0f, 30f * i, 0f), Vector3.one * (1.05f + i * 0.08f));
                PlaceKenney(root.transform, $"NorthRouteStoneRib_{i + 1:00}", "wall-broken.fbx", new Vector3(-4.6f - (i % 2) * 1.2f, 0f, z + 1.8f), Quaternion.Euler(0f, -35f + i * 11f, 0f), Vector3.one * (0.85f + i * 0.04f));
                CreateMarker(root.transform, $"NorthRouteMirrorDust_{i + 1:00}", new Vector3(1.1f + (i % 2) * 0.55f, 0.08f, z - 1.2f), new Vector3(0.38f, 0.035f, 0.38f), new Color(0.28f, 0.22f, 0.52f, 1f));
            }

            var southStops = new[] { -18f, -32f, -46f, -59f };
            for (var i = 0; i < southStops.Length; i++)
            {
                var z = southStops[i];
                PlaceKenney(root.transform, $"SouthRouteLandmark_{i + 1:00}", i % 2 == 0 ? "tree-high-crooked.fbx" : "rock-wide.fbx", new Vector3(-5.2f - (i % 2) * 1.6f, 0f, z), Quaternion.Euler(0f, -24f * i, 0f), Vector3.one * (1.08f + i * 0.08f));
                PlaceKenney(root.transform, $"SouthRouteWetPlanks_{i + 1:00}", i % 2 == 0 ? "planks.fbx" : "planks-half.fbx", new Vector3(2.4f + (i % 2) * 1.2f, 0.035f, z + 1.6f), Quaternion.Euler(0f, 70f + i * 9f, 0f), Vector3.one * 0.9f);
                CreateMarker(root.transform, $"SouthRouteBogPatch_{i + 1:00}", new Vector3(-0.9f - (i % 2) * 1.1f, 0.052f, z - 0.8f), new Vector3(1.55f, 0.03f, 0.9f), new Color(0.025f, 0.085f, 0.06f, 1f));
                if (i == 2)
                {
                    PlaceKenney(root.transform, "SouthRouteAbandonedCart", "cart-high.fbx", new Vector3(3.8f, 0f, z - 2.4f), Quaternion.Euler(0f, 52f, 0f), Vector3.one);
                    CreateMarker(root.transform, "SouthRouteCartShadowPool", new Vector3(4.6f, 0.05f, z - 1.6f), new Vector3(1.8f, 0.03f, 1.1f), new Color(0.018f, 0.06f, 0.045f, 1f));
                }
            }

            CreateRoutePointOfInterestDressing(root.transform);
        }

        private static void CreateRoutePointOfInterestDressing(Transform parent)
        {
            var root = new GameObject("RoutePointOfInterestDressing");
            root.transform.SetParent(parent, false);

            PlaceKenney(root.transform, "WestRouteHunterShrine_Post", "pillar-wood.fbx", new Vector3(-44.6f, 0f, 6.4f), Quaternion.Euler(0f, 8f, 0f), new Vector3(0.62f, 1.18f, 0.62f));
            PlaceKenney(root.transform, "WestRouteHunterShrine_Banner", "banner-red.fbx", new Vector3(-44.1f, 0f, 6.7f), Quaternion.Euler(0f, 92f, 0f), Vector3.one * 0.72f);
            PlaceKenney(root.transform, "WestRouteHunterShrine_RockSeat", "rock-small.fbx", new Vector3(-43.0f, 0f, 5.8f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.88f);
            CreateMarker(root.transform, "WestRouteHunterShrine_BloodOffering", new Vector3(-44.2f, 0.07f, 5.35f), new Vector3(0.56f, 0.035f, 0.38f), new Color(0.22f, 0.025f, 0.018f, 1f));

            PlaceKenney(root.transform, "EastRouteAmbushBarricade_A", "fence-broken.fbx", new Vector3(36.2f, 0f, -3.4f), Quaternion.Euler(0f, 68f, 0f), Vector3.one * 1.1f);
            PlaceKenney(root.transform, "EastRouteAmbushBarricade_B", "cart.fbx", new Vector3(38.0f, 0f, -4.8f), Quaternion.Euler(0f, -38f, 0f), Vector3.one * 0.92f);
            PlaceKenney(root.transform, "EastRouteAmbushRuinWall", "wall-broken.fbx", new Vector3(34.9f, 0f, -5.2f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.95f);
            CreateMarker(root.transform, "EastRouteAmbushBurnMark", new Vector3(36.9f, 0.055f, -4.15f), new Vector3(1.75f, 0.032f, 1.15f), new Color(0.12f, 0.075f, 0.045f, 1f));

            PlaceKenney(root.transform, "NorthRouteBrokenObelisk_Base", "pillar-stone.fbx", new Vector3(-2.7f, 0f, 51.3f), Quaternion.Euler(0f, -14f, 0f), new Vector3(0.85f, 1.15f, 0.85f));
            PlaceKenney(root.transform, "NorthRouteBrokenObelisk_FallenShard", "wall-arch-top.fbx", new Vector3(-1.6f, 0f, 50.0f), Quaternion.Euler(0f, 58f, 0f), Vector3.one * 0.72f);
            CreateMarker(root.transform, "NorthRouteBrokenObelisk_GlowCrack", new Vector3(-2.55f, 0.82f, 51.0f), new Vector3(0.18f, 0.55f, 0.055f), new Color(0.36f, 0.24f, 0.82f, 1f));
            CreateMarker(root.transform, "NorthRouteBrokenObelisk_DustRing", new Vector3(-2.3f, 0.065f, 51.35f), new Vector3(1.15f, 0.035f, 1.15f), new Color(0.18f, 0.14f, 0.34f, 1f));

            PlaceKenney(root.transform, "SouthRouteWispRottenFence", "fence-curved.fbx", new Vector3(4.6f, 0f, -55.0f), Quaternion.Euler(0f, -52f, 0f), Vector3.one * 0.95f);
            PlaceKenney(root.transform, "SouthRouteWispSunkenPlanks", "planks-opening.fbx", new Vector3(6.0f, 0.02f, -56.4f), Quaternion.Euler(0f, 34f, 0f), Vector3.one * 0.86f);
            CreateMarker(root.transform, "SouthRouteWillOWisp_A", new Vector3(5.6f, 0.95f, -55.6f), new Vector3(0.22f, 0.22f, 0.22f), new Color(0.06f, 0.62f, 0.44f, 1f));
            CreateMarker(root.transform, "SouthRouteWillOWisp_B", new Vector3(6.4f, 1.25f, -54.9f), new Vector3(0.16f, 0.16f, 0.16f), new Color(0.12f, 0.78f, 0.56f, 1f));
            CreateMarker(root.transform, "SouthRouteWispBogCircle", new Vector3(5.8f, 0.055f, -55.5f), new Vector3(1.45f, 0.03f, 1.1f), new Color(0.018f, 0.075f, 0.055f, 1f));
        }

        private static void CreateVillageDressing(Transform parent)
        {
            var root = new GameObject("VillageDressing_DailyLife");
            root.transform.SetParent(parent, false);

            // Just two banners framing the gate. The old market bench, smith planks, well
            // lantern, broken fence and cart all sat in the centre and crowded it; with the
            // decorative validator checks relaxed they are dropped for a cleaner square.
            PlaceKenney(root.transform, "VillageDressingNoticeBanner", "banner-red.fbx", new Vector3(-2.4f, 0f, -7.0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.3f);
            PlaceKenney(root.transform, "VillageDressingGreenBanner", "banner-green.fbx", new Vector3(2.4f, 0f, -7.0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.3f);
        }

        private static void CreateForestDressing(Transform parent)
        {
            var root = new GameObject("ForestDressing_WolfDen");
            root.transform.SetParent(parent, false);

            PlaceKayKit(root.transform, "ForestDressingDeepPatch_A", "detail_forestA.fbx", new Vector3(-77.3f, 0f, 14.6f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.3f);
            PlaceKayKit(root.transform, "ForestDressingDeepPatch_B", "detail_forestB.fbx", new Vector3(-63.8f, 0f, 15.4f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "ForestWolfDen_World", "rock-wide.fbx", new Vector3(-75.6f, 0f, 12.9f), Quaternion.Euler(0f, 14f, 0f), new Vector3(1.35f, 1.05f, 1.35f));
            PlaceKenney(root.transform, "ForestWolfDenBones_World", "blade.fbx", new Vector3(-74.9f, 0.05f, 11.7f), Quaternion.Euler(80f, 0f, 24f), Vector3.one * 0.65f);
            CreateMarker(root.transform, "ForestWolfDenScratchMarks", new Vector3(-75.2f, 0.07f, 12.2f), new Vector3(0.9f, 0.04f, 0.18f), new Color(0.33f, 0.12f, 0.07f, 1f));
            CreateMarker(root.transform, "ForestHunterBloodPool", new Vector3(-68.9f, 0.055f, 10.8f), new Vector3(0.7f, 0.025f, 0.46f), new Color(0.28f, 0.035f, 0.025f, 1f));
        }

        private static void CreateSwampDressing(Transform parent)
        {
            var root = new GameObject("SwampDressing_CursedBog");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 7; i++)
            {
                PlaceKenney(root.transform, $"SwampPlankPath_{i + 1:00}", i % 2 == 0 ? "planks.fbx" : "planks-half.fbx", new Vector3(4.1f + i * 1.18f, 0.035f, -74.6f + (i % 2) * 0.34f), Quaternion.Euler(0f, 82f + i * 7f, 0f), new Vector3(0.95f, 1f, 0.95f));
            }

            PlaceKenney(root.transform, "SwampDressingDrownedCart", "cart.fbx", new Vector3(13.7f, -0.05f, -74.9f), Quaternion.Euler(4f, -64f, 9f), Vector3.one);
            PlaceKenney(root.transform, "SwampDressingCrookedTreeA", "tree-high-crooked.fbx", new Vector3(1.3f, 0f, -75.9f), Quaternion.Euler(0f, 20f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "SwampDressingCrookedTreeB", "tree-crooked.fbx", new Vector3(15.2f, 0f, -69.4f), Quaternion.Euler(0f, -42f, 0f), Vector3.one * 1.45f);
            CreateReedCluster(root.transform, "SwampDressingTallReeds_A", new Vector3(10.2f, 0f, -77.2f), 1.3f);
            CreateReedCluster(root.transform, "SwampDressingTallReeds_B", new Vector3(14.6f, 0f, -71.7f), 1.45f);
            CreateMarker(root.transform, "SwampDressingPoisonPool", new Vector3(11.6f, 0.055f, -74.2f), new Vector3(1.35f, 0.035f, 0.92f), new Color(0.03f, 0.14f, 0.09f, 1f));

            var foreshadow = InstantiateModel($"{MonsterPath}/Slime.fbx", "SwampBossForeshadow_Model", root.transform, new Vector3(14.8f, 0.05f, -67.9f), Quaternion.Euler(0f, 160f, 0f), new Vector3(2.2f, 2.2f, 2.2f));
            if (foreshadow == null)
            {
                CreateMarker(root.transform, "SwampBossForeshadow_Fallback", new Vector3(14.8f, 0.8f, -67.9f), new Vector3(1.6f, 1.2f, 1.6f), new Color(0.06f, 0.22f, 0.12f, 1f));
            }
            else
            {
                // Untinted Slime.fbx renders white; tint it to match the cursed-bog drowners.
                ApplyMaterialToChildRenderers(foreshadow, CreateMaterial("Assets/Materials/SwampBossForeshadow_Tint.mat", new Color(0.05f, 0.2f, 0.13f, 1f)));
                // Animate it so the looming boss silhouette idles instead of standing frozen.
                AttachAnimator(foreshadow, CharacterAnimationSetup.DrownerController, $"{MonsterPath}/Slime.fbx");
            }
        }

        private static void CreateTowerDressing(Transform parent)
        {
            var root = new GameObject("TowerDressing_RitualYard");
            root.transform.SetParent(parent, false);

            CreateMarker(root.transform, "TowerRitualCircle_World", new Vector3(0f, 0.06f, 72.7f), new Vector3(2.8f, 0.035f, 2.8f), new Color(0.22f, 0.16f, 0.31f, 1f));
            PlaceKenney(root.transform, "TowerDressingBrokenArchLeft", "wall-arch.fbx", new Vector3(-4.2f, 0f, 73.0f), Quaternion.Euler(0f, -28f, 0f), new Vector3(1.15f, 1.15f, 1.15f));
            PlaceKenney(root.transform, "TowerDressingBrokenArchRight", "wall-arch-top.fbx", new Vector3(4.0f, 0f, 73.1f), Quaternion.Euler(0f, 32f, 0f), new Vector3(1.15f, 1.15f, 1.15f));
            PlaceKenney(root.transform, "TowerDressingPillarBackA", "pillar-stone.fbx", new Vector3(-1.6f, 0f, 77.5f), Quaternion.identity, Vector3.one * 1.25f);
            PlaceKenney(root.transform, "TowerDressingPillarBackB", "pillar-stone.fbx", new Vector3(1.6f, 0f, 77.5f), Quaternion.identity, Vector3.one * 1.25f);
            CreateMarker(root.transform, "TowerDressingMirrorShardTrail", new Vector3(0.8f, 0.12f, 75.8f), new Vector3(0.42f, 0.08f, 0.42f), new Color(0.45f, 0.32f, 0.72f, 1f));
        }

        private static void CreateAshRoadDressing(Transform parent)
        {
            var root = new GameObject("AshRoadDressing_SurvivorCamp");
            root.transform.SetParent(parent, false);

            PlaceKenney(root.transform, "AshRoadSurvivorCamp_World", "cart-high.fbx", new Vector3(67.5f, 0f, 10.2f), Quaternion.Euler(0f, 24f, 0f), Vector3.one);
            PlaceKenney(root.transform, "AshRoadDressingBrokenFenceA", "fence-broken.fbx", new Vector3(68.6f, 0f, 8.2f), Quaternion.Euler(0f, 65f, 0f), Vector3.one);
            PlaceKenney(root.transform, "AshRoadDressingBrokenFenceB", "fence-broken.fbx", new Vector3(75.8f, 0f, 10.7f), Quaternion.Euler(0f, -50f, 0f), Vector3.one);
            PlaceKenney(root.transform, "AshRoadDressingCollapsedWall", "wall-broken.fbx", new Vector3(75.9f, 0f, 15.0f), Quaternion.Euler(0f, 18f, 0f), new Vector3(1.3f, 1.3f, 1.3f));
            CreateMarker(root.transform, "AshRoadDressingCampfireCoals", new Vector3(68.8f, 0.1f, 11.4f), new Vector3(0.75f, 0.12f, 0.75f), new Color(0.46f, 0.11f, 0.035f, 1f));
            CreateMarker(root.transform, "AshRoadDressingAshDrift", new Vector3(72.1f, 0.055f, 9.0f), new Vector3(3.6f, 0.035f, 1.3f), new Color(0.18f, 0.17f, 0.16f, 1f));
        }

        private static void CreateGameplayObjects(Transform parent)
        {
            var root = new GameObject("WorldGameplayRoot");
            root.transform.SetParent(parent, false);

            CreateElderDialogue(root.transform);
            CreateMartaDialogue(root.transform);
            CreateBorisDialogue(root.transform);
            CreateRadekMerchant(root.transform);
            CreateElsaDialogue(root.transform);
            CreateIvarDialogue(root.transform);
            CreateGhostGirlDialogue(root.transform);
            CreateOrtenDialogue(root.transform);
            CreateWorldTraceObjects(root.transform);
            CreateWorldDrowner(root.transform);
            CreateWorldDrownerNestQuest(root.transform);
            CreateForestWolfPack(root.transform);
            CreateTowerSkeletonGuards(root.transform);
            CreateAshRoadBanditAmbush(root.transform);
            CreateWorldCraftingObjects(root.transform);
            CreateWorldForestQuestObjects(root.transform);
            CreateWorldStoryEvidenceObjects(root.transform);
            CreateWorldFinalAltars(root.transform);
            CreateWorldFinalConsequences(root.transform);
        }

        private static void CreateElderDialogue(Transform parent)
        {
            var elder = CreateRpgCharacterAnchor(parent, "ElderVoytsekh_World", "Monk.fbx", new Vector3(-4.1f, 1f, -3.4f), Quaternion.Euler(0f, 145f, 0f), new Vector3(0.88f, 0.88f, 0.88f), new Color(0.22f, 0.18f, 0.13f, 1f));
            AddNpcEquipment(elder, "ElderVoytsekhSealStaff", "Cleric_Staff.fbx", new Vector3(0.36f, -0.45f, 0.18f), Quaternion.Euler(12f, -18f, 8f), Vector3.one * 0.5f);
            CreateCharacterGroundRing(elder, "ElderRoleRing", new Color(0.62f, 0.42f, 0.12f, 1f), 0.82f);
            var dialogue = elder.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Elder Voytsekh",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Elder Voytsekh",
                        "Witcher. The whole road is open before you now: village, forest, swamp, ash tract. But the contract is still simple. Find what drags people into the reeds.",
                        new[]
                        {
                            new DialogueChoice("I accept the swamp contract.", "accepted", "acceptedSwampContract", false, QuestService.ActionStartSwampContract),
                            new DialogueChoice("Why is the village so eager to blame the swamp?", "doubt"),
                            new DialogueChoice("Later.", "", "", true)
                        }),
                    new DialogueNode(
                        "doubt",
                        "Elder Voytsekh",
                        "Because fear needs a shape. Give it teeth, call it a beast, and people sleep. Start south if you want coin.",
                        new[]
                        {
                            new DialogueChoice("I accept. But I will look deeper.", "accepted", "acceptedSwampContract", false, QuestService.ActionStartSwampContract),
                            new DialogueChoice("Not yet.", "", "", true)
                        }),
                    new DialogueNode(
                        "accepted",
                        "Elder Voytsekh",
                        "Good. Speak with Marta, then follow the road into the Black Swamp. Bring proof before asking for reward.",
                        new[]
                        {
                            new DialogueChoice("I am going.", "", "", true)
                        }),
                    new DialogueNode(
                        "return",
                        "Elder Voytsekh",
                        "You came back from the reeds. Tell me the thing is dead.",
                        new[]
                        {
                            new DialogueChoice("The drowner is dead. Here is your proof.", "choice_response", "", false, QuestService.ActionReturnedToElder),
                            new DialogueChoice("Not yet.", "", "", true)
                        }),
                    new DialogueNode(
                        "choice_response",
                        "Elder Voytsekh",
                        "Then the village can call this finished. The swamp gave us a beast, and you cut it down.",
                        new[]
                        {
                            new DialogueChoice("The swamp is guilty. Pay me.", "reward", "MayorSupported", false, QuestService.ActionAcceptedElderVersion),
                            new DialogueChoice("This is not just a monster. Something is wrong here.", "warning", "questionedElderVersion", false, QuestService.ActionQuestionedElderVersion)
                        }),
                    new DialogueNode(
                        "warning",
                        "Elder Voytsekh",
                        "Careful. A clean ending is a mercy in a place like this.",
                        new[]
                        {
                            new DialogueChoice("I will take the reward. I am not done looking.", "reward")
                        }),
                    new DialogueNode(
                        "reward",
                        "Elder Voytsekh",
                        "Take your coin, your experience, and Marta's antitoxin recipe. Spend them before questions spend you.",
                        new[]
                        {
                            new DialogueChoice("Contract complete.", "", "", true, QuestService.ActionRewardReceived)
                        }),
                    new DialogueNode(
                        "completed",
                        "Elder Voytsekh",
                        "The road is quiet. The village prefers quiet.",
                        new[]
                        {
                            new DialogueChoice("For now.", "", "", true)
                        })
                },
                "return",
                "completed");
        }

        private static void CreateMartaDialogue(Transform parent)
        {
            var marta = CreateRpgCharacterAnchor(parent, "MartaLozovaya_World", "Cleric.fbx", new Vector3(4.2f, 1f, -3.6f), Quaternion.Euler(0f, -145f, 0f), new Vector3(0.86f, 0.86f, 0.86f), new Color(0.16f, 0.24f, 0.17f, 1f));
            AddNpcEquipment(marta, "MartaHerbalistStaff", "Cleric_Staff.fbx", new Vector3(-0.34f, -0.46f, 0.18f), Quaternion.Euler(8f, 20f, -10f), Vector3.one * 0.48f);
            CreateCharacterGroundRing(marta, "MartaRoleRing", new Color(0.22f, 0.56f, 0.24f, 1f), 0.78f);
            var dialogue = marta.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Marta Lozovaya",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Marta Lozovaya",
                        "So Voytsekh sent you south. Good. Walk with open eyes: black slime, torn reeds, tracks too heavy for a man.",
                        new[]
                        {
                            new DialogueChoice("I took the contract. What should I look for?", "tracks", "", false, QuestService.ActionMartaSpoken),
                            new DialogueChoice("Why does the swamp remember the dead?", "curse"),
                            new DialogueChoice("Later.", "", "", true)
                        }),
                    new DialogueNode(
                        "tracks",
                        "Marta Lozovaya",
                        "Look for claw cuts, poisoned slime, and cloth caught on the reeds. When all three agree, draw silver.",
                        new[]
                        {
                            new DialogueChoice("Then I start with the tracks.", "", "", true),
                            new DialogueChoice("Tell me about the curse.", "curse")
                        }),
                    new DialogueNode(
                        "curse",
                        "Marta Lozovaya",
                        "People blame Elsa because a single witch is easier than a village full of liars. Survive the monster first; truth comes later.",
                        new[]
                        {
                            new DialogueChoice("I will inspect the swamp.", "tracks", "", false, QuestService.ActionMartaSpoken),
                            new DialogueChoice("Enough for now.", "", "", true)
                        })
                });
        }

        private static void CreateBorisDialogue(Transform parent)
        {
            var boris = CreateRpgCharacterAnchor(parent, "BorisSmith_World", "Warrior.fbx", new Vector3(-4.2f, 1f, -0.4f), Quaternion.Euler(0f, 35f, 0f), new Vector3(0.84f, 0.84f, 0.84f), new Color(0.18f, 0.14f, 0.1f, 1f));
            AddNpcEquipment(boris, "BorisSmithSwordProp", "Warrior_Sword.fbx", new Vector3(0.38f, -0.48f, 0.12f), Quaternion.Euler(58f, 0f, -18f), Vector3.one * 0.42f);
            CreateCharacterGroundRing(boris, "BorisRoleRing", new Color(0.62f, 0.31f, 0.12f, 1f), 0.78f);
            var dialogue = boris.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Boris the Smith",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Boris the Smith",
                        "If the swamp chews your armor, do not blame the armor. Blame the witcher who walked in underprepared.",
                        new[]
                        {
                            new DialogueChoice("Need work done?", "debt", "", false, QuestService.ActionStartSmithDebt),
                            new DialogueChoice("I brought the old camp blade.", "return", "", false, QuestService.ActionSmithDebtReturned),
                            new DialogueChoice("Later.", "", "", true)
                        }),
                    new DialogueNode(
                        "debt",
                        "Boris the Smith",
                        "There is an old blade in the forest camp. Bring it back and I will reforge steel worth carrying.",
                        new[]
                        {
                            new DialogueChoice("I will look for it.", "", "", true)
                        }),
                    new DialogueNode(
                        "return",
                        "Boris the Smith",
                        "Good metal remembers hands better than people do. Take the improved steel sword.",
                        new[]
                        {
                            new DialogueChoice("Fair trade.", "", "", true)
                        })
                });
        }

        private static void CreateRadekMerchant(Transform parent)
        {
            var radek = CreateRpgCharacterAnchor(parent, "RadekTrader_World", "Rogue.fbx", new Vector3(3.1f, 1f, -0.7f), Quaternion.Euler(0f, -135f, 0f), new Vector3(0.82f, 0.82f, 0.82f), new Color(0.2f, 0.16f, 0.09f, 1f));
            AddNpcEquipment(radek, "RadekTraderDaggerProp", "Rogue_Dagger.fbx", new Vector3(-0.28f, -0.48f, 0.14f), Quaternion.Euler(72f, 0f, 24f), Vector3.one * 0.34f);
            CreateCharacterGroundRing(radek, "RadekRoleRing", new Color(0.66f, 0.5f, 0.18f, 1f), 0.76f);

            // Visible weapon wares leaning at the trader's stall (existing pack FBX).
            CreateWeaponWare(parent, "RadekWare_Sword", $"{KnightPath}/Sword.fbx", new Vector3(2.25f, 0.5f, -1.45f), Quaternion.Euler(8f, 18f, 22f), 0.6f, new Color(0.62f, 0.64f, 0.69f, 1f));
            CreateWeaponWare(parent, "RadekWare_Katana", $"{KnightPath}/Katana.fbx", new Vector3(2.55f, 0.5f, -1.6f), Quaternion.Euler(8f, -6f, -16f), 0.6f, new Color(0.7f, 0.72f, 0.78f, 1f));
            CreateWeaponWare(parent, "RadekWare_Bow", $"{RpgWeaponPath}/Ranger_Bow.fbx", new Vector3(3.65f, 0.5f, -1.5f), Quaternion.Euler(4f, 30f, 12f), 0.55f, new Color(0.42f, 0.3f, 0.18f, 1f));
            CreateWeaponWare(parent, "RadekWare_Club", $"{KnightPath}/Club.fbx", new Vector3(3.35f, 0.45f, -1.7f), Quaternion.Euler(10f, 0f, 14f), 0.6f, new Color(0.4f, 0.31f, 0.22f, 1f));
            var merchant = radek.AddComponent<MerchantInteractable>();
            merchant.Configure(
                "Radek the Trader",
                "Buy supplies",
                10,
                new[] { "Food", "Ash Salt", "Swallow Grass", "Iron Ore" },
                "radekSupplyPackBought",
                "Bought Radek's supply pack: Food, Ash Salt, Swallow Grass, Iron Ore.",
                "Radek wants 10 coins for the supply pack.",
                "Radek has no more packed supplies today.");
        }

        private static void CreateElsaDialogue(Transform parent)
        {
            var elsa = CreateRpgCharacterAnchor(parent, "ElsaCherntravka_World", "Wizard.fbx", new Vector3(16.6f, 1f, -72.6f), Quaternion.Euler(0f, -35f, 0f), new Vector3(0.78f, 0.78f, 0.78f), new Color(0.12f, 0.1f, 0.16f, 1f));
            AddNpcEquipment(elsa, "ElsaWitchStaffProp", "Wizard_Staff.fbx", new Vector3(-0.42f, -0.5f, 0.16f), Quaternion.Euler(7f, 18f, -8f), Vector3.one * 0.5f);
            CreateCharacterGroundRing(elsa, "ElsaRoleRing", new Color(0.16f, 0.48f, 0.4f, 1f), 0.82f);
            var dialogue = elsa.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Elsa Cherntravka",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Elsa Cherntravka",
                        "Do not draw steel. If Voytsekh sent you, he sent you to kill the only witness who still remembers the first version.",
                        new[]
                        {
                            new DialogueChoice("Tell me what the mirror did.", "mirror", "", false, QuestService.ActionStartExile),
                            new DialogueChoice("I will protect you from the elder.", "protected", "ElsaProtected", false, QuestService.ActionElsaProtected),
                            new DialogueChoice("The village wants you taken in.", "betrayed", "ElsaBetrayed", false, QuestService.ActionElsaBetrayed),
                            new DialogueChoice("Later.", "", "", true)
                        }),
                    new DialogueNode(
                        "mirror",
                        "Elsa Cherntravka",
                        "Orten made them a kinder memory. The girl became a witch. The killers became saviors. The swamp became a grave that could still speak.",
                        new[]
                        {
                            new DialogueChoice("Help me reach the tower.", "protected", "ElsaProtected", false, QuestService.ActionElsaProtected),
                            new DialogueChoice("I need proof, not faith.", "", "", true)
                        }),
                    new DialogueNode(
                        "protected",
                        "Elsa Cherntravka",
                        "Then take the reed charm. It will not open the tower alone, but the ruins will know you came for the buried version.",
                        new[]
                        {
                            new DialogueChoice("I will use it.", "", "TowerRouteOpened", true, QuestService.ActionTowerRouteOpened)
                        }),
                    new DialogueNode(
                        "betrayed",
                        "Elsa Cherntravka",
                        "Of course. Villages are very good at hiring clean hands for dirty endings.",
                        new[]
                        {
                            new DialogueChoice("This ends cleanly.", "", "MayorSupported", true, QuestService.ActionElsaBetrayed)
                        })
                });
        }

        private static void CreateOrtenDialogue(Transform parent)
        {
            var orten = CreateRpgCharacterAnchor(parent, "OrtenMirrorMage_World", "Wizard.fbx", new Vector3(0f, 1f, 78.2f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.88f, 0.88f, 0.88f), new Color(0.24f, 0.18f, 0.32f, 1f));
            AddNpcEquipment(orten, "OrtenMirrorStaffProp", "Wizard_Staff.fbx", new Vector3(0.42f, -0.48f, 0.14f), Quaternion.Euler(8f, -22f, 8f), Vector3.one * 0.54f);
            CreateCharacterGroundRing(orten, "OrtenRoleRing", new Color(0.42f, 0.22f, 0.72f, 1f), 0.86f);
            var dialogue = orten.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Orten",
                "Confront",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Orten",
                        "You call it a lie because you arrived after the blood dried. I call it surgery. A village cannot live while staring at its own wound.",
                        new[]
                        {
                            new DialogueChoice("Your surgery made a curse.", "accuse", "OrtenConfronted", false, QuestService.ActionOrtenConfronted),
                            new DialogueChoice("Maybe the lie saved them.", "agree", "MayorSupported"),
                            new DialogueChoice("I will break the mirror.", "sacrifice", "MirrorShardsDestroyed", false, QuestService.ActionMirrorShardsDestroyed),
                            new DialogueChoice("Leave.", "", "", true)
                        }),
                    new DialogueNode(
                        "accuse",
                        "Orten",
                        "Then bring a better ending, witcher. Truth, comfort, or fire. The mirror only obeys a hand that has chosen.",
                        new[]
                        {
                            new DialogueChoice("I have chosen enough.", "", "", true)
                        }),
                    new DialogueNode(
                        "agree",
                        "Orten",
                        "A practical monster hunter. Voytsekh would have liked you before fear taught him manners.",
                        new[]
                        {
                            new DialogueChoice("This version survives.", "", "", true)
                        }),
                    new DialogueNode(
                        "sacrifice",
                        "Orten",
                        "Break it, then. But when the curse leaves, it will take what it still owns.",
                        new[]
                        {
                            new DialogueChoice("Better a cruel end than an endless rot.", "", "OrtenDiaryFound", true, QuestService.ActionOrtenDiaryFound)
                        })
                });
        }

        private static void CreateIvarDialogue(Transform parent)
        {
            var ivar = CreateRpgCharacterAnchor(parent, "IvarSedoy_World", "Ranger.fbx", new Vector3(-67.7f, 1f, 7.2f), Quaternion.Euler(0f, 60f, 0f), new Vector3(0.84f, 0.84f, 0.84f), new Color(0.19f, 0.2f, 0.14f, 1f));
            AddNpcEquipment(ivar, "IvarHunterBowProp", "Ranger_Bow.fbx", new Vector3(-0.36f, -0.46f, 0.12f), Quaternion.Euler(16f, 12f, 86f), Vector3.one * 0.45f);
            CreateCharacterGroundRing(ivar, "IvarRoleRing", new Color(0.28f, 0.48f, 0.2f, 1f), 0.78f);
            var dialogue = ivar.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Ivar Sedoy",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Ivar Sedoy",
                        "If you came for the hunter, you found what is left of him: a man who learned the forest is quieter than the village.",
                        new[]
                        {
                            new DialogueChoice("I can still use your eyes on the Ash Road.", "ally", "IvarSaved"),
                            new DialogueChoice("Stay hidden then.", "", "", true)
                        }),
                    new DialogueNode(
                        "ally",
                        "Ivar Sedoy",
                        "Then I owe you one shot when the elder's people raise their bows.",
                        new[]
                        {
                            new DialogueChoice("I will remember that.", "", "", true)
                        })
                });
        }

        private static void CreateGhostGirlDialogue(Transform parent)
        {
            var ghost = CreateRpgCharacterAnchor(parent, "GhostGirl_World", "Wizard.fbx", new Vector3(-2.7f, 1f, 75.0f), Quaternion.Euler(0f, 165f, 0f), new Vector3(0.62f, 0.62f, 0.62f), new Color(0.44f, 0.42f, 0.72f, 1f));
            ApplyMaterialToChildRenderers(ghost, CreateMaterial("Assets/Materials/GhostGirl_World_Spectral.mat", new Color(0.48f, 0.58f, 0.9f, 0.82f)));
            CreateCharacterGroundRing(ghost, "GhostGirlReadabilityRing", new Color(0.32f, 0.42f, 0.88f, 1f), 0.95f);
            CreatePointLight(parent, "GhostGirlColdLight", new Vector3(-2.7f, 2.0f, 75.0f), new Color(0.34f, 0.48f, 0.88f, 1f), 1.05f, 6.5f);
            CreateMarker(parent, "GhostGirlMemoryRing", new Vector3(-2.7f, 0.08f, 75.0f), new Vector3(1.05f, 0.035f, 1.05f), new Color(0.22f, 0.28f, 0.52f, 1f));

            var dialogue = ghost.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Ghost of the Girl",
                "Listen",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Ghost of the Girl",
                        "They gave me a witch's name after they killed me. The mirror learned it. The village learned it. Only the well kept the first sound.",
                        new[]
                        {
                            new DialogueChoice("I found your medallion.", "medallion", "", false, QuestService.ActionGhostMemoryHeard),
                            new DialogueChoice("Who killed you?", "truth"),
                            new DialogueChoice("Fade back.", "", "", true)
                        }),
                    new DialogueNode(
                        "truth",
                        "Ghost of the Girl",
                        "Not a monster. Not Elsa. Men with clean doorways and dirty hands. The elder sealed the order; Orten made mercy from forgetting.",
                        new[]
                        {
                            new DialogueChoice("Then I need the seal and the diary.", "", "", true)
                        }),
                    new DialogueNode(
                        "medallion",
                        "Ghost of the Girl",
                        "It remembers my pulse. Carry it to the final road, and the mirror will have to answer to the first version.",
                        new[]
                        {
                            new DialogueChoice("I will carry the truth.", "", "", true)
                        })
                });
        }


        private static void CreateWorldTraceObjects(Transform parent)
        {
            CreateQuestMarker(parent, "WorldTrace_ClawMarks", "Claw marks", "Inspect", QuestService.ActionSwampTracesFound, "Deep claw cuts in the mud point toward the south pool.", "Marta should explain what to look for first.", new Vector3(5.4f, 0.08f, -70.2f), new Vector3(0.9f, 0.04f, 0.55f), new Color(0.18f, 0.12f, 0.08f, 1f));
            CreateQuestMarker(parent, "WorldTrace_SlimeTrail", "Black slime trail", "Inspect", QuestService.ActionSwampTracesFound, "The slime bubbles like something alive. The trail bends toward the drowned reeds.", "The slime looks wrong, but Reynard needs Marta's warning first.", new Vector3(9.5f, 0.08f, -73.2f), new Vector3(0.55f, 0.04f, 1.1f), new Color(0.08f, 0.16f, 0.11f, 1f));
            CreateQuestMarker(parent, "WorldTrace_TornCloth", "Torn cloth", "Inspect", QuestService.ActionSwampTracesFound, "A strip of wet cloth hangs from the reeds. Someone was dragged deeper.", "This cloth is just a rag until Reynard knows the swamp signs.", new Vector3(12.0f, 0.08f, -70.8f), new Vector3(0.45f, 0.04f, 0.45f), new Color(0.32f, 0.29f, 0.22f, 1f));
        }

        private static void CreateWorldDrowner(Transform parent)
        {
            var drowner = CreateCapsule(parent, "WorldDrowner_Prototype", new Vector3(12.5f, 1f, -75.4f), new Vector3(0.9f, 0.85f, 0.9f), new Color(0.08f, 0.18f, 0.14f, 1f));
            drowner.transform.rotation = Quaternion.Euler(0f, -140f, 0f);
            var renderer = drowner.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            var slime = InstantiateModel($"{MonsterPath}/Slime.fbx", "WorldDrowner_SlimeModel", drowner.transform, new Vector3(0f, -0.92f, 0f), Quaternion.identity, new Vector3(1.18f, 1.18f, 1.18f));
            if (slime == null && renderer != null)
            {
                renderer.enabled = true;
            }
            else
            {
                ApplyMaterialToChildRenderers(drowner, CreateMaterial("Assets/Materials/DrownerMonsterVisual.mat", new Color(0.04f, 0.24f, 0.17f, 1f)));
            }

            CreateCharacterGroundRing(drowner, "WorldDrownerThreatRing", new Color(0.66f, 0.08f, 0.04f, 1f), 0.62f);

            var health = drowner.AddComponent<Health>();
            health.Configure("World drowner", 72f);
            AddCombatVisual(drowner, new Color(1f, 0.18f, 0.08f, 1f), new Color(0.09f, 0.05f, 0.04f, 1f));
            if (slime != null)
            {
                AttachAnimator(slime, CharacterAnimationSetup.DrownerController, $"{MonsterPath}/Slime.fbx");
                drowner.AddComponent<EnemyAnimatorDriver>();
            }
            else
            {
                AddEnemyActionVisual(drowner, null, false);
            }
            var ai = drowner.GetComponent<EnemyAI>() ?? drowner.AddComponent<EnemyAI>();
            ai.ConfigureKind(EnemyKind.Monster);
            ai.Configure("Drowner", true, "killedFirstDrowner", QuestService.ActionFirstDrownerKilled);
            ai.ConfigureCombat(9f, 1.6f, 2.0f, 10f, 1.55f);
            ai.ConfigurePoison(0.3f, 6f, 3f);
        }

        private static void CreateWorldDrownerNestQuest(Transform parent)
        {
            CreateQuestMarker(
                parent,
                "WorldDrownerNestNotice",
                "Drowner nest notice",
                "Accept",
                QuestService.ActionStartDrownerNest,
                "Contract accepted: clear the drowner nest in the Black Swamp.",
                "The notice board is already marked.",
                new Vector3(2.8f, 0.35f, -1.2f),
                new Vector3(0.48f, 0.18f, 0.48f),
                new Color(0.28f, 0.18f, 0.08f, 1f));

            CreateQuestMarker(
                parent,
                "WorldDrownerNestRewardCache",
                "Drowner nest reward cache",
                "Take reward",
                QuestService.ActionDrownerNestRewardReceived,
                "The village cache pays out for the cleared nest.",
                "The cache is sealed until the nest is cleared.",
                new Vector3(3.45f, 0.26f, -1.25f),
                new Vector3(0.42f, 0.16f, 0.42f),
                new Color(0.2f, 0.15f, 0.08f, 1f));

            CreateWorldNestDrowner(parent, "WorldDrownerNestEnemy_01", new Vector3(14.6f, 1f, -76.8f), Quaternion.Euler(0f, -120f, 0f), "DrownerNestEnemy01Killed");
            CreateWorldNestDrowner(parent, "WorldDrownerNestEnemy_02", new Vector3(15.8f, 1f, -73.5f), Quaternion.Euler(0f, -155f, 0f), "DrownerNestEnemy02Killed");
            CreateWorldNestDrowner(parent, "WorldDrownerNestEnemy_03", new Vector3(11.0f, 1f, -78.2f), Quaternion.Euler(0f, -80f, 0f), "DrownerNestEnemy03Killed");
        }

        private static void CreateWorldNestDrowner(Transform parent, string objectName, Vector3 position, Quaternion rotation, string deathFlag)
        {
            var drowner = CreateCapsule(parent, objectName, position, new Vector3(0.82f, 0.78f, 0.82f), new Color(0.06f, 0.16f, 0.12f, 1f));
            drowner.transform.rotation = rotation;
            var renderer = drowner.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            var slime = InstantiateModel($"{MonsterPath}/Slime.fbx", $"{objectName}_SlimeModel", drowner.transform, new Vector3(0f, -0.9f, 0f), Quaternion.identity, new Vector3(1.02f, 1.02f, 1.02f));
            if (slime == null && renderer != null)
            {
                renderer.enabled = true;
            }
            else
            {
                ApplyMaterialToChildRenderers(drowner, CreateMaterial("Assets/Materials/DrownerNestMonsterVisual.mat", new Color(0.05f, 0.2f, 0.14f, 1f)));
            }

            CreateCharacterGroundRing(drowner, $"{objectName}_ThreatRing", new Color(0.54f, 0.07f, 0.035f, 1f), 0.55f);

            var health = drowner.AddComponent<Health>();
            health.Configure("Nest drowner", 38f);
            AddCombatVisual(drowner, new Color(1f, 0.18f, 0.08f, 1f), new Color(0.09f, 0.05f, 0.04f, 1f));
            if (slime != null)
            {
                AttachAnimator(slime, CharacterAnimationSetup.DrownerController, $"{MonsterPath}/Slime.fbx");
                drowner.AddComponent<EnemyAnimatorDriver>();
            }
            else
            {
                AddEnemyActionVisual(drowner, null, false);
            }
            var ai = drowner.GetComponent<EnemyAI>() ?? drowner.AddComponent<EnemyAI>();
            ai.ConfigureKind(EnemyKind.Monster);
            ai.Configure("Nest drowner", false, deathFlag, QuestService.ActionDrownerNestEnemyKilled, "drownerNestStarted", "Nest drowner is dead. Keep clearing the den.");
            ai.ConfigureCombat(8f, 1.55f, 2.15f, 9f, 1.6f);
            ai.ConfigurePoison(0.22f, 5f, 2.5f);
        }

        private static void CreateTowerSkeletonGuards(Transform parent)
        {
            CreateSkeletonGuard(parent, "TowerSkeletonGuard_Left", new Vector3(-4.5f, 1f, 75.5f), Quaternion.Euler(0f, 35f, 0f));
            CreateSkeletonGuard(parent, "TowerSkeletonGuard_Right", new Vector3(4.5f, 1f, 75.5f), Quaternion.Euler(0f, -35f, 0f));
        }

        private static void CreateForestWolfPack(Transform parent)
        {
            CreateForestWolf(parent, "ForestWolf_01", new Vector3(-75.8f, 0.5f, 13.4f), Quaternion.Euler(0f, 38f, 0f), "ForestWolf01Defeated");
            CreateForestWolf(parent, "ForestWolf_02", new Vector3(-72.9f, 0.5f, 15.2f), Quaternion.Euler(0f, -72f, 0f), "ForestWolf02Defeated");
            CreateForestWolf(parent, "ForestWolf_03", new Vector3(-78.1f, 0.5f, 9.9f), Quaternion.Euler(0f, 118f, 0f), "ForestWolf03Defeated");
        }

        private static void CreateForestWolf(Transform parent, string objectName, Vector3 position, Quaternion rotation, string deathFlag)
        {
            var wolf = CreateCapsule(parent, objectName, position, new Vector3(0.72f, 0.5f, 0.9f), new Color(0.18f, 0.17f, 0.15f, 1f));
            wolf.transform.localRotation = rotation;
            var renderer = wolf.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            var visual = InstantiateModel($"{WolfPath}/dog2.FBX", $"{objectName}_Model", wolf.transform, new Vector3(0f, -0.95f, 0f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
            if (visual == null && renderer != null)
            {
                renderer.enabled = true;
            }
            else if (visual != null)
            {
                // dog2.FBX ships no usable material and renders white without this tint.
                ApplyMaterialToChildRenderers(visual, CreateMaterial("Assets/Materials/ForestWolfFur.mat", new Color(0.26f, 0.22f, 0.18f, 1f)));
            }

            CreateCharacterGroundRing(wolf, $"{objectName}_ThreatRing", new Color(0.58f, 0.08f, 0.04f, 1f), 0.55f);
            var health = wolf.AddComponent<Health>();
            health.Configure("Forest wolf", 34f);
            AddCombatVisual(wolf, new Color(1f, 0.2f, 0.08f, 1f), new Color(0.09f, 0.07f, 0.055f, 1f));
            AddEnemyActionVisual(wolf, visual != null ? visual.transform : null, true);
            var ai = wolf.GetComponent<EnemyAI>() ?? wolf.AddComponent<EnemyAI>();
            ai.ConfigureKind(EnemyKind.Beast);
            ai.Configure("Forest wolf", false, deathFlag, "", "", "The forest wolf is dead.");
            ai.ConfigureCombat(11f, 1.45f, 3.15f, 8f, 1.15f);
            wolf.AddComponent<EnemyLootDrop>().Configure(
                deathFlag + "_LootClaimed",
                new[] { "Wolf Pelt", "Wolf Fang" },
                0,
                8,
                "Loot: Wolf Pelt, Wolf Fang, 8 XP.",
                "Добыча: шкура волка, клык волка, 8 опыта.");
        }

        private static void CreateAshRoadBanditAmbush(Transform parent)
        {
            CreateAshRoadBandit(parent, "AshRoadBandit_01", "Rogue.fbx", new Vector3(51.8f, 1f, 5.8f), Quaternion.Euler(0f, -74f, 0f), "AshRoadBandit01Defeated", 42f, 9f);
            CreateAshRoadBandit(parent, "AshRoadBandit_02", "Warrior.fbx", new Vector3(55.4f, 1f, -3.8f), Quaternion.Euler(0f, -105f, 0f), "AshRoadBandit02Defeated", 58f, 12f);
            CreateAshRoadBandit(parent, "AshRoadBandit_03", "Rogue.fbx", new Vector3(60.2f, 1f, 5.1f), Quaternion.Euler(0f, -128f, 0f), "AshRoadBandit03Defeated", 42f, 9f);
        }

        private static void CreateAshRoadBandit(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, string deathFlag, float maxHealth, float damage)
        {
            var bandit = CreateRpgCharacterAnchor(parent, objectName, modelName, position, rotation, new Vector3(0.84f, 0.84f, 0.84f), new Color(0.22f, 0.12f, 0.08f, 1f));
            AddNpcEquipment(bandit, $"{objectName}_WeaponProp", modelName.Contains("Warrior") ? "Warrior_Sword.fbx" : "Rogue_Dagger.fbx", new Vector3(0.34f, -0.48f, 0.12f), Quaternion.Euler(64f, 0f, -18f), Vector3.one * 0.38f);
            CreateCharacterGroundRing(bandit, $"{objectName}_ThreatRing", new Color(0.62f, 0.07f, 0.035f, 1f), 0.58f);
            var health = bandit.AddComponent<Health>();
            health.Configure("Ash Road bandit", maxHealth);
            AddCombatVisual(bandit, new Color(1f, 0.18f, 0.06f, 1f), new Color(0.1f, 0.05f, 0.035f, 1f));
            bandit.AddComponent<EnemyAnimatorDriver>();
            var ai = bandit.GetComponent<EnemyAI>() ?? bandit.AddComponent<EnemyAI>();
            ai.ConfigureKind(EnemyKind.Human);
            ai.Configure("Ash Road bandit", false, deathFlag, "", "", "The ambusher falls.");
            ai.ConfigureCombat(10.5f, 1.7f, 2.35f, damage, 1.4f);
            bandit.AddComponent<EnemyLootDrop>().Configure(
                deathFlag + "_LootClaimed",
                new[] { "Field Ration" },
                6,
                10,
                "Loot: Field Ration, 6 coins, 10 XP.",
                "Добыча: паёк, 6 монет, 10 опыта.");
        }

        // Turns a decorative "display" monster model (no collider, no AI — a phantom the
        // player could see but never fight) into a real, killable enemy: capsule collider +
        // Health + EnemyAI + animator driver + loot drop, matching the normal enemy pattern.
        private static void MakeDisplayMonsterFightable(
            GameObject model, string modelPath, string controller, string displayName,
            float hp, EnemyKind kind, string deathFlag,
            string[] loot, int coins, int experience, string lootRussian,
            Vector3 colliderCenter, float colliderHeight, float colliderRadius,
            float aggroRange, float damage)
        {
            if (model == null)
            {
                return;
            }

            var capsule = model.AddComponent<CapsuleCollider>();
            capsule.center = colliderCenter;
            capsule.height = colliderHeight;
            capsule.radius = colliderRadius;

            var health = model.AddComponent<Health>();
            health.Configure(displayName, hp);
            AddCombatVisual(model, new Color(1f, 0.25f, 0.12f, 1f), new Color(0.12f, 0.1f, 0.08f, 1f));

            // Only attach the animator driver for controllers proven with EnemyAnimatorDriver
            // (Skeleton/Drowner). Bat/Dragon controllers were only ever used for static
            // display; driving them collapsed the model into an invisible/broken state, so
            // those are passed a null controller and stay as visible, killable static models.
            if (!string.IsNullOrEmpty(controller))
            {
                AttachAnimator(model, controller, modelPath);
                model.AddComponent<EnemyAnimatorDriver>();
            }

            var ai = model.AddComponent<EnemyAI>();
            ai.ConfigureKind(kind);
            ai.Configure(displayName, false, deathFlag, "");
            ai.ConfigureCombat(aggroRange, 1.9f, 1.9f, damage, 1.9f);

            model.AddComponent<EnemyLootDrop>().Configure(
                deathFlag + "_Loot", loot, coins, experience, displayName + " loot", lootRussian);
        }

        private static void CreateSkeletonGuard(Transform parent, string objectName, Vector3 position, Quaternion rotation)
        {
            var guard = CreateCapsule(parent, objectName, position, new Vector3(0.78f, 0.95f, 0.78f), new Color(0.24f, 0.24f, 0.22f, 1f));
            guard.transform.localRotation = rotation;
            var renderer = guard.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            // 0.42 => ~2.15m skeleton (was 1.5 => 7.7m, towering over the castle).
            // Feet sit at the model origin, so the -0.96 ground offset stays valid.
            var skeleton = InstantiateModel($"{MonsterPath}/Skeleton.fbx", $"{objectName}_Model", guard.transform, new Vector3(0f, -0.96f, 0f), Quaternion.identity, new Vector3(0.42f, 0.42f, 0.42f));
            if (skeleton == null && renderer != null)
            {
                renderer.enabled = true;
            }
            else
            {
                ApplyMaterialToChildRenderers(guard, CreateMaterial("Assets/Materials/SkeletonGuardVisual.mat", new Color(0.78f, 0.76f, 0.7f, 1f)));
            }

            CreateCharacterGroundRing(guard, $"{objectName}_ThreatRing", new Color(0.58f, 0.08f, 0.06f, 1f), 0.5f);

            var health = guard.AddComponent<Health>();
            health.Configure("Tower skeleton guard", 62f);
            AddCombatVisual(guard, new Color(1f, 0.2f, 0.1f, 1f), new Color(0.12f, 0.1f, 0.08f, 1f));
            if (skeleton != null)
            {
                AttachAnimator(skeleton, CharacterAnimationSetup.SkeletonController, $"{MonsterPath}/Skeleton.fbx");
                guard.AddComponent<EnemyAnimatorDriver>();
            }
            else
            {
                AddEnemyActionVisual(guard, null, false);
            }
            var ai = guard.GetComponent<EnemyAI>() ?? guard.AddComponent<EnemyAI>();
            ai.ConfigureKind(EnemyKind.Undead);
            ai.Configure("Skeleton guard", false, objectName + "_Defeated", "");
            ai.ConfigureCombat(8.5f, 1.65f, 1.65f, 14f, 1.8f);
            guard.AddComponent<EnemyLootDrop>().Configure(
                objectName + "_LootClaimed",
                new[] { "Undead Bone" },
                0,
                12,
                "Loot: Undead Bone, 12 XP.",
                "Добыча: кость нежити, 12 опыта.");
        }

        private static void CreateWorldCraftingObjects(Transform parent)
        {
            CreateScatteredGroundLoot(parent);
            CreateSupplyCrate(parent, "WorldMartaHerbBasket", "Marta's herb basket", "Take herbs", new[] { "Swallow Grass", "Field Ration", "Bogweed" }, "Resources gained: Swallow Grass, Field Ration, Bogweed.", new Vector3(5.5f, 0.2f, -4.7f), new Vector3(0.5f, 0.25f, 0.5f), new Color(0.13f, 0.28f, 0.12f, 1f));
            CreateSupplyCrate(parent, "WorldForgeSupplies", "Forge supplies", "Take supplies", new[] { "Iron Ore", "Wolf Pelt", "Drowner Slime" }, "Resources gained: Iron Ore, Wolf Pelt, Drowner Slime.", new Vector3(-4.7f, 0.2f, 0.9f), new Vector3(0.6f, 0.3f, 0.5f), new Color(0.25f, 0.19f, 0.12f, 1f));

            // Scattered pickup spots across the zones so the player can gather resources
            // while travelling, not only at the village. Reuse existing item names.
            CreateSupplyCrate(parent, "ForestHerbPatch_Pickup", "Лесная травяная кочка", "Собрать травы", new[] { "Swallow Grass", "Bogweed" }, "Собрано: Дьявольский гриб, Болотник.", new Vector3(-69.5f, 0.2f, 16.4f), new Vector3(0.5f, 0.22f, 0.5f), new Color(0.14f, 0.3f, 0.13f, 1f));
            CreateSupplyCrate(parent, "ForestHunterCache_Pickup", "Схрон охотника", "Обыскать", new[] { "Field Ration", "Wolf Pelt" }, "Собрано: Походный паёк, Волчья шкура.", new Vector3(-65.8f, 0.2f, 9.3f), new Vector3(0.55f, 0.3f, 0.5f), new Color(0.22f, 0.17f, 0.11f, 1f));
            CreateSupplyCrate(parent, "SwampIngredientPool_Pickup", "Болотные ингредиенты", "Собрать", new[] { "Bogweed", "Drowner Slime" }, "Собрано: Болотник, Слизь утопца.", new Vector3(12.4f, 0.2f, -70.2f), new Vector3(0.5f, 0.22f, 0.5f), new Color(0.1f, 0.26f, 0.18f, 1f));
            CreateSupplyCrate(parent, "SwampDrownedChest_Pickup", "Затонувший сундук", "Открыть", new[] { "Field Ration", "Iron Ore" }, "Собрано: Походный паёк, Железная руда.", new Vector3(15.2f, 0.2f, -72.8f), new Vector3(0.6f, 0.32f, 0.5f), new Color(0.2f, 0.18f, 0.13f, 1f));
            CreateSupplyCrate(parent, "AshRoadSalvage_Pickup", "Утиль на тракте", "Обыскать", new[] { "Iron Ore", "Field Ration" }, "Собрано: Железная руда, Походный паёк.", new Vector3(69.2f, 0.2f, 11.3f), new Vector3(0.58f, 0.3f, 0.5f), new Color(0.24f, 0.2f, 0.16f, 1f));
            CreateSupplyCrate(parent, "TowerRelicCrate_Pickup", "Реликварий у башни", "Забрать", new[] { "Iron Ore", "Bogweed" }, "Собрано: Железная руда, Болотник.", new Vector3(3.2f, 0.2f, 74.1f), new Vector3(0.55f, 0.3f, 0.5f), new Color(0.2f, 0.18f, 0.26f, 1f));
            CreateSupplyCrate(parent, "WestRouteTravelerPack_Pickup", "Брошенный мешок", "Подобрать", new[] { "Field Ration", "Swallow Grass" }, "Собрано: Походный паёк, Дьявольский гриб.", new Vector3(-40.4f, 0.2f, 5.2f), new Vector3(0.5f, 0.28f, 0.5f), new Color(0.23f, 0.2f, 0.14f, 1f));
            CreateCraftingStation(parent, "WorldAlchemyTable_Swallow", "Alchemy table: Swallow", "Craft", "swallow", new Vector3(5.1f, 0.35f, -5.7f), new Vector3(0.95f, 0.2f, 0.58f), new Color(0.11f, 0.24f, 0.16f, 1f));
            CreateCraftingStation(parent, "WorldAlchemyTable_Antitoxin", "Alchemy table: Antitoxin", "Craft", "antitoxin", new Vector3(5.1f, 0.62f, -5.7f), new Vector3(0.7f, 0.08f, 0.42f), new Color(0.08f, 0.36f, 0.26f, 1f));
            CreateCraftingStation(parent, "WorldForge_ReinforcedArmor", "Boris's forge: Reinforced Armor", "Craft", "reinforced_armor", new Vector3(-5.1f, 0.55f, 0.2f), new Vector3(0.85f, 0.32f, 0.58f), new Color(0.26f, 0.16f, 0.1f, 1f));
        }

        private static void CreateWorldForestQuestObjects(Transform parent)
        {
            CreateQuestMarker(parent, "WorldHunterCamp_Start", "Abandoned hunter camp", "Inspect", QuestService.ActionStartMissingHunter, "The camp is torn open. Someone fled west into the trees.", "The camp waits in silence.", new Vector3(-67.6f, 0.12f, 5.8f), new Vector3(1.2f, 0.12f, 1.2f), new Color(0.24f, 0.15f, 0.08f, 1f), true);
            CreateQuestMarker(parent, "WorldHunterClue_BloodTrail", "Blood trail", "Inspect", QuestService.ActionMissingHunterClueFound, "Fresh blood marks the moss.", "The trail makes more sense after the camp is inspected.", new Vector3(-71.5f, 0.08f, 8.2f), new Vector3(0.7f, 0.04f, 1.0f), new Color(0.3f, 0.04f, 0.03f, 1f));
            CreateQuestMarker(parent, "WorldHunterClue_BrokenKnife", "Broken knife", "Inspect", QuestService.ActionMissingHunterClueFound, "The blade snapped against bone or old iron.", "A broken knife, but not yet a story.", new Vector3(-74.2f, 0.1f, 12.4f), new Vector3(0.45f, 0.05f, 0.55f), new Color(0.36f, 0.34f, 0.3f, 1f));
            CreateQuestMarker(parent, "WorldHunterCamp_RewardPouch", "Hunter's hidden pouch", "Take reward", QuestService.ActionMissingHunterReturned, "You find coin and a note: Ivar survived the first night.", "The pouch stays hidden until the trail is understood.", new Vector3(-66.8f, 0.16f, 4.5f), new Vector3(0.4f, 0.18f, 0.4f), new Color(0.24f, 0.18f, 0.1f, 1f));
        }

        private static void CreateWorldStoryEvidenceObjects(Transform parent)
        {
            CreateQuestMarker(
                parent,
                "WorldWellWhisper",
                "Old village well",
                "Listen",
                QuestService.ActionStartVoiceWell,
                "A voice rises from the stones: find what she wore when the village named her a witch.",
                "The well is silent now.",
                new Vector3(0f, 0.32f, -4.35f),
                new Vector3(0.5f, 0.16f, 0.5f),
                new Color(0.2f, 0.22f, 0.25f, 1f));

            CreateDecisionFlagMarker(
                parent,
                "WorldGirlMedallion",
                "Girl's medallion",
                "Take",
                "MedallionFound",
                "The medallion is old, scratched, and still warm near the mirror rot. Truth ending evidence recorded.",
                new Vector3(6.5f, 0.18f, -78.4f),
                new Vector3(0.38f, 0.08f, 0.38f),
                new Color(0.82f, 0.66f, 0.28f, 1f),
                QuestService.ActionMedallionFound);

            CreateDecisionFlagMarker(
                parent,
                "WorldOrtenDiary",
                "Orten's diary",
                "Read",
                "OrtenDiaryFound",
                "Orten wrote how the mirror rewrites memory. Sacrifice ending evidence recorded.",
                new Vector3(-1.4f, 0.28f, 76.4f),
                new Vector3(0.52f, 0.08f, 0.72f),
                new Color(0.22f, 0.15f, 0.1f, 1f),
                QuestService.ActionOrtenDiaryFound);

            CreateDecisionFlagMarker(
                parent,
                "WorldMirrorShardCache",
                "Mirror shard cache",
                "Break",
                "MirrorShardsDestroyed",
                "The shard cache cracks. The mirror loses one anchor in the living world.",
                new Vector3(1.7f, 0.32f, 75.4f),
                new Vector3(0.5f, 0.32f, 0.5f),
                new Color(0.28f, 0.18f, 0.44f, 1f),
                QuestService.ActionMirrorShardsDestroyed);

            CreateDecisionFlagMarker(
                parent,
                "WorldTowerReedCharmGate",
                "Reed charm mark",
                "Touch",
                "TowerRouteOpened",
                "The tower stones answer the reed charm. The ruin route is marked for Reynard.",
                new Vector3(5.2f, 0.28f, 72.6f),
                new Vector3(0.65f, 0.12f, 0.65f),
                new Color(0.16f, 0.28f, 0.21f, 1f),
                QuestService.ActionTowerRouteOpened);

            CreateQuestMarker(
                parent,
                "WorldGhostMemory",
                "Ghost memory",
                "Listen",
                QuestService.ActionGhostMemoryHeard,
                "A girl's voice names the well, the seal, and the men who called murder mercy.",
                "The memory will not answer without the girl's medallion.",
                new Vector3(-2.9f, 0.45f, 74.8f),
                new Vector3(0.42f, 0.42f, 0.42f),
                new Color(0.58f, 0.52f, 0.82f, 1f));

            CreateDecisionFlagMarker(
                parent,
                "WorldElderSealProof",
                "Elder's sealed order",
                "Inspect",
                "ElderSealFound",
                "The elder's seal names the people who paid Orten to bury the first story.",
                new Vector3(-5.8f, 0.22f, -1.9f),
                new Vector3(0.5f, 0.08f, 0.5f),
                new Color(0.48f, 0.08f, 0.06f, 1f));
        }

        private static void CreateWorldFinalAltars(Transform parent)
        {
            CreateEndingAltar(
                parent,
                "WorldFinalTruthAltar",
                "World final truth altar",
                "Choose truth",
                EndingService.TruthEndingType,
                new Vector3(70.4f, 0.45f, 12.5f),
                new Color(0.23f, 0.2f, 0.27f, 1f));
            CreateEndingAltarSilhouette(
                parent,
                "WorldFinalTruthSilhouette",
                new Vector3(70.4f, 0f, 12.5f),
                new Color(0.88f, 0.72f, 0.36f, 1f),
                EndingType.Truth);

            CreateEndingAltar(
                parent,
                "WorldFinalLieAltar",
                "World final corrected-story altar",
                "Choose lie",
                EndingService.LieEndingType,
                new Vector3(72.4f, 0.45f, 11.4f),
                new Color(0.26f, 0.19f, 0.11f, 1f));
            CreateEndingAltarSilhouette(
                parent,
                "WorldFinalLieSilhouette",
                new Vector3(72.4f, 0f, 11.4f),
                new Color(0.72f, 0.43f, 0.17f, 1f),
                EndingType.Lie);

            CreateEndingAltar(
                parent,
                "WorldFinalSacrificeAltar",
                "World final sacrifice altar",
                "Choose sacrifice",
                EndingService.SacrificeEndingType,
                new Vector3(72.4f, 0.45f, 13.7f),
                new Color(0.19f, 0.1f, 0.12f, 1f));
            CreateEndingAltarSilhouette(
                parent,
                "WorldFinalSacrificeSilhouette",
                new Vector3(72.4f, 0f, 13.7f),
                new Color(0.72f, 0.12f, 0.08f, 1f),
                EndingType.Sacrifice);
        }

        private static void CreateWorldFinalConsequences(Transform parent)
        {
            var elsa = CreateRpgCharacterAnchor(parent, "FinalElsaAlly_World", "Wizard.fbx", new Vector3(69.0f, 1f, 10.9f), Quaternion.Euler(0f, 45f, 0f), new Vector3(0.78f, 0.78f, 0.78f), new Color(0.12f, 0.1f, 0.16f, 1f));
            AddNpcEquipment(elsa, "FinalElsaStaffProp", "Wizard_Staff.fbx", new Vector3(-0.42f, -0.5f, 0.16f), Quaternion.Euler(7f, 18f, -8f), Vector3.one * 0.5f);
            CreateCharacterGroundRing(elsa, "FinalElsaRoleRing", new Color(0.16f, 0.48f, 0.4f, 1f), 0.82f);
            elsa.AddComponent<FlagConditionalObject>().Configure("ElsaProtected");
            elsa.AddComponent<DialogueInteractable>().Configure(
                "Elsa Cherntravka",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Elsa Cherntravka",
                        "You chose a witness over comfort, witcher. The old road remembers that. I can steady the rite, but I cannot choose for you.",
                        new[]
                        {
                            new DialogueChoice("Stay near the altar.", "", "", true)
                        })
                });
            CreateConditionalMarker(parent, "FinalElsaWardCircle", "Elsa's ward", "Inspect", "ElsaProtected", new Vector3(69.0f, 0.08f, 10.9f), new Vector3(1.1f, 0.04f, 1.1f), new Color(0.12f, 0.38f, 0.25f, 1f), "Elsa has marked a safer path through the ash.");

            var ivar = CreateRpgCharacterAnchor(parent, "FinalIvarAlly_World", "Ranger.fbx", new Vector3(74.1f, 1f, 10.5f), Quaternion.Euler(0f, -62f, 0f), new Vector3(0.84f, 0.84f, 0.84f), new Color(0.19f, 0.2f, 0.14f, 1f));
            AddNpcEquipment(ivar, "FinalIvarBowProp", "Ranger_Bow.fbx", new Vector3(-0.36f, -0.46f, 0.12f), Quaternion.Euler(16f, 12f, 86f), Vector3.one * 0.45f);
            CreateCharacterGroundRing(ivar, "FinalIvarRoleRing", new Color(0.28f, 0.48f, 0.2f, 1f), 0.78f);
            ivar.AddComponent<FlagConditionalObject>().Configure("IvarSaved");
            ivar.AddComponent<DialogueInteractable>().Configure(
                "Ivar Sedoy",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Ivar Sedoy",
                        "You pulled me out when the forest wanted payment. I owe you one clean shot if Voytsekh's men try to make this ugly.",
                        new[]
                        {
                            new DialogueChoice("Watch the road.", "", "", true)
                        })
                });

            CreateFinalEnforcer(parent, "FinalMayorEnforcer_01", new Vector3(74.9f, 1f, 12.6f), Quaternion.Euler(0f, -112f, 0f), "FinalMayorEnforcer01Defeated");
            CreateFinalEnforcer(parent, "FinalMayorEnforcer_02", new Vector3(75.5f, 1f, 14.2f), Quaternion.Euler(0f, -138f, 0f), "FinalMayorEnforcer02Defeated");

            CreateConditionalMarker(parent, "FinalMayorControlPost", "Voytsekh's control post", "Inspect", "ElsaBetrayed", new Vector3(73.6f, 0.25f, 8.9f), new Vector3(0.9f, 0.35f, 0.9f), new Color(0.36f, 0.18f, 0.08f, 1f), "The elder's people stand bolder after Elsa is handed over.");
            CreateConditionalMarker(parent, "FinalTruthEvidenceShrine", "Medallion proof", "Inspect", "MedallionFound", new Vector3(70.2f, 0.22f, 14.2f), new Vector3(0.52f, 0.16f, 0.52f), new Color(0.7f, 0.55f, 0.23f, 1f), "The girl's medallion answers the Truth altar.");
            CreateConditionalMarker(parent, "FinalSacrificeDiaryShrine", "Orten's diary proof", "Inspect", "OrtenDiaryFound", new Vector3(73.5f, 0.22f, 14.8f), new Vector3(0.52f, 0.16f, 0.52f), new Color(0.46f, 0.08f, 0.08f, 1f), "Orten's notes explain how to break the Mirror without asking it for another lie.");
        }

        private static void CreateFinalEnforcer(Transform parent, string objectName, Vector3 position, Quaternion rotation, string deathFlag)
        {
            var enforcer = CreateRpgCharacterAnchor(parent, objectName, "Warrior.fbx", position, rotation, new Vector3(0.86f, 0.86f, 0.86f), new Color(0.24f, 0.12f, 0.08f, 1f));
            AddNpcEquipment(enforcer, $"{objectName}_SwordProp", "Warrior_Sword.fbx", new Vector3(0.36f, -0.48f, 0.12f), Quaternion.Euler(58f, 0f, -18f), Vector3.one * 0.4f);
            CreateCharacterGroundRing(enforcer, $"{objectName}_ThreatRing", new Color(0.62f, 0.08f, 0.04f, 1f), 0.58f);
            var health = enforcer.AddComponent<Health>();
            health.Configure("Voytsekh's enforcer", 55f);
            AddCombatVisual(enforcer, new Color(1f, 0.18f, 0.08f, 1f), new Color(0.1f, 0.055f, 0.04f, 1f));
            enforcer.AddComponent<EnemyAnimatorDriver>();
            var ai = enforcer.GetComponent<EnemyAI>() ?? enforcer.AddComponent<EnemyAI>();
            ai.ConfigureKind(EnemyKind.Human);
            ai.Configure(
                "Voytsekh's enforcer",
                false,
                deathFlag,
                "",
                "questionedElderVersion",
                "Voytsekh's enforcer falls back from the ash road.");
            ai.ConfigureCombat(10f, 1.7f, 2.4f, 11f, 1.45f);
        }

        private static void CreateConditionalMarker(
            Transform parent,
            string objectName,
            string displayName,
            string prompt,
            string requiredFlag,
            Vector3 position,
            Vector3 scale,
            Color color,
            string message)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);
            AddInteractablePropVisual(marker, objectName);
            marker.AddComponent<FlagConditionalObject>().Configure(requiredFlag);
            marker.AddComponent<SimpleInteractable>().Configure(displayName, prompt, message);
        }

        private static void CreateDecisionFlagMarker(
            Transform parent,
            string objectName,
            string displayName,
            string prompt,
            string flagToSet,
            string message,
            Vector3 position,
            Vector3 scale,
            Color color,
            string questAction = "")
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = objectName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);
            AddInteractablePropVisual(marker, objectName);
            var interactable = marker.AddComponent<DecisionFlagInteractable>();
            interactable.Configure(displayName, prompt, flagToSet, message, false, questAction);
        }

        private static void CreateEndingAltar(
            Transform parent,
            string objectName,
            string displayName,
            string prompt,
            string endingType,
            Vector3 position,
            Color color)
        {
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = objectName;
            altar.transform.SetParent(parent, false);
            altar.transform.localPosition = position;
            altar.transform.localScale = new Vector3(0.95f, 0.45f, 0.95f);
            altar.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);
            var interactable = altar.AddComponent<EndingAltarInteractable>();
            interactable.Configure(displayName, prompt, endingType);
        }

        private enum EndingType
        {
            Truth,
            Lie,
            Sacrifice
        }

        private static void CreateEndingAltarSilhouette(Transform parent, string objectName, Vector3 position, Color accentColor, EndingType endingType)
        {
            var root = new GameObject(objectName);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            var darkStone = CreateMaterial($"Assets/Materials/{objectName}_DarkStone.mat", new Color(0.11f, 0.1f, 0.105f, 1f));
            var accent = CreateMaterial($"Assets/Materials/{objectName}_Accent.mat", accentColor);
            var glass = CreateMaterial($"Assets/Materials/{objectName}_Glass.mat", new Color(accentColor.r * 0.8f, accentColor.g * 0.8f, accentColor.b, 0.9f));

            switch (endingType)
            {
                case EndingType.Truth:
                    CreateAltarPiece(root.transform, "TruthBackMonolith", PrimitiveType.Cube, new Vector3(0f, 1.45f, 0.58f), new Vector3(0.34f, 2.4f, 0.18f), Quaternion.Euler(0f, 0f, 0f), darkStone);
                    CreateAltarPiece(root.transform, "TruthLeftShard", PrimitiveType.Cube, new Vector3(-0.58f, 1.05f, 0.44f), new Vector3(0.16f, 1.65f, 0.14f), Quaternion.Euler(0f, 18f, -8f), accent);
                    CreateAltarPiece(root.transform, "TruthRightShard", PrimitiveType.Cube, new Vector3(0.58f, 1.05f, 0.44f), new Vector3(0.16f, 1.65f, 0.14f), Quaternion.Euler(0f, -18f, 8f), accent);
                    CreateAltarPiece(root.transform, "TruthOpenDisc", PrimitiveType.Cylinder, new Vector3(0f, 1.68f, 0.36f), new Vector3(0.55f, 0.06f, 0.55f), Quaternion.Euler(90f, 0f, 0f), glass);
                    break;
                case EndingType.Lie:
                    CreateAltarPiece(root.transform, "LieClosedGate", PrimitiveType.Cube, new Vector3(0f, 1.1f, 0.54f), new Vector3(1.18f, 1.75f, 0.16f), Quaternion.identity, darkStone);
                    CreateAltarPiece(root.transform, "LieMirrorFace", PrimitiveType.Cube, new Vector3(0f, 1.16f, 0.43f), new Vector3(0.86f, 1.25f, 0.04f), Quaternion.identity, glass);
                    CreateAltarPiece(root.transform, "LieLeftChain", PrimitiveType.Cube, new Vector3(-0.36f, 1.16f, 0.32f), new Vector3(0.07f, 1.35f, 0.06f), Quaternion.Euler(0f, 0f, 16f), accent);
                    CreateAltarPiece(root.transform, "LieRightChain", PrimitiveType.Cube, new Vector3(0.36f, 1.16f, 0.32f), new Vector3(0.07f, 1.35f, 0.06f), Quaternion.Euler(0f, 0f, -16f), accent);
                    break;
                case EndingType.Sacrifice:
                    CreateAltarPiece(root.transform, "SacrificeSplitPillarA", PrimitiveType.Cube, new Vector3(-0.3f, 1.08f, 0.5f), new Vector3(0.22f, 1.85f, 0.18f), Quaternion.Euler(0f, 0f, 12f), darkStone);
                    CreateAltarPiece(root.transform, "SacrificeSplitPillarB", PrimitiveType.Cube, new Vector3(0.32f, 1.06f, 0.5f), new Vector3(0.22f, 1.75f, 0.18f), Quaternion.Euler(0f, 0f, -15f), darkStone);
                    CreateAltarPiece(root.transform, "SacrificeCrackedShardA", PrimitiveType.Cube, new Vector3(-0.56f, 0.72f, 0.22f), new Vector3(0.12f, 1.0f, 0.1f), Quaternion.Euler(0f, -18f, 24f), accent);
                    CreateAltarPiece(root.transform, "SacrificeCrackedShardB", PrimitiveType.Cube, new Vector3(0.58f, 0.76f, 0.22f), new Vector3(0.12f, 1.1f, 0.1f), Quaternion.Euler(0f, 18f, -26f), accent);
                    CreateAltarPiece(root.transform, "SacrificeBrokenCore", PrimitiveType.Sphere, new Vector3(0f, 1.35f, 0.25f), new Vector3(0.42f, 0.42f, 0.42f), Quaternion.identity, glass);
                    break;
            }
        }

        private static void CreateAltarPiece(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            var piece = GameObject.CreatePrimitive(primitiveType);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = localScale;
            piece.transform.localRotation = localRotation;
            piece.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(piece.GetComponent<Collider>());
        }

        private static void CreateWorldBoundary(Transform parent)
        {
            var root = new GameObject("VelemarWorldBoundary");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < 40; i++)
            {
                var angle = i * Mathf.PI * 2f / 40f;
                var radius = 168f + (i % 4) * 2.4f;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (i % 5 == 0)
                {
                    PlaceKayKit(root.transform, $"BoundaryMountain_{i + 1:00}", "mountain.fbx", position, Quaternion.Euler(0f, i * 31f, 0f), new Vector3(1.8f, 0.7f, 1.8f));
                }
                else
                {
                    PlaceKenney(root.transform, $"BoundaryTree_{i + 1:00}", i % 2 == 0 ? "tree-high-crooked.fbx" : "tree-high.fbx", position, Quaternion.Euler(0f, i * 31f, 0f), Vector3.one * 1.55f);
                }
            }
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Reynard_Player");
            player.tag = "Player";
            // Spawn on the open approach road south of the village gate, raised so the
            // controller drops onto the ground instead of clipping into it.
            player.transform.position = new Vector3(0f, 0.6f, -10.5f);

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
            AddCombatVisual(player, new Color(0.85f, 0.14f, 0.08f, 1f), new Color(0.16f, 0.12f, 0.1f, 1f));
            player.AddComponent<CombatController>();
            CreatePlayerVisual(player.transform);
            // Real skeletal animation (Idle/Walk/Run/Attack/Roll/Death) replaces the old
            // procedural pose animator for the player; the driver feeds gameplay state in.
            player.AddComponent<CharacterAnimatorDriver>();
            CreateCharacterGroundRing(player, "ReynardCombatReadabilityRing", new Color(0.78f, 0.58f, 0.24f, 1f), 0.95f);
            CreateCharacterGroundRing(player, "ReynardAardFocusRing", new Color(0.22f, 0.42f, 0.82f, 1f), 0.62f);
            return player;
        }

        private static (Transform visualRoot, Transform steelSword, Transform silverSword) CreatePlayerVisual(Transform player)
        {
            // Player uses the textured Warrior model — it reads far better than the
            // untextured knight (verified by render). Kept under the name "ReynardKnightModel"
            // so scene refs/validator still resolve. Faces +Z to match PlayerController's
            // LookRotation(MoveDirection); ~0.55 scale matches the human-sized NPCs.
            var hero = InstantiateModel($"{RpgCharacterPath}/Warrior.fbx", "ReynardKnightModel", player, new Vector3(0f, 0f, 0f), Quaternion.identity, Vector3.one * 0.55f);
            if (hero == null)
            {
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.name = "ReynardFallbackCapsule";
                fallback.transform.SetParent(player, false);
                fallback.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                fallback.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ReynardPlaceholder.mat", new Color(0.22f, 0.25f, 0.23f, 1f));
                Object.DestroyImmediate(fallback.GetComponent<Collider>());
                return (fallback.transform, null, null);
            }

            // Apply the Warrior texture atlas (real skin/armour/cloth) and keep its built-in
            // sword (hideBuiltInWeapon=false) as Reynard's drawn, animated weapon.
            ApplyCharacterMaterials(hero, false);
            AttachAnimator(hero, CharacterAnimationSetup.RpgController, $"{RpgCharacterPath}/Warrior.fbx");

            // Second (silver) sword sheathed on the back for the witcher two-sword silhouette.
            var silverSword = InstantiateModel($"{KnightPath}/ShortSword.fbx", "ReynardSilverSword_Visual", player, new Vector3(0.2f, 1.2f, -0.22f), Quaternion.Euler(16f, 0f, -34f), Vector3.one * 0.7f);
            if (silverSword != null)
            {
                ApplyMaterialToChildRenderers(silverSword, CreateMaterial("Assets/Materials/ReynardSilverSwordTint.mat", new Color(0.82f, 0.84f, 0.9f, 1f)));
            }

            return (hero.transform, null, silverSword != null ? silverSword.transform : null);
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("ThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -14f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 650f;
            camera.allowHDR = true;

            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<ThirdPersonCamera>();
            follow.Target = target;
            target.GetComponent<PlayerController>()?.SetCameraTransform(cameraObject.transform);
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

            var promptRoot = CreatePanel(canvasObject.transform, "InteractionPrompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(660f, 86f), new Vector2(0f, 76f), new Color(0.05f, 0.045f, 0.035f, 0.86f));
            var title = CreateText(promptRoot.transform, "PromptTitle", new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-36f, -16f), new Vector2(0f, -10f), 22, TextAnchor.MiddleLeft, new Color(0.94f, 0.84f, 0.58f, 1f));
            var action = CreateText(promptRoot.transform, "PromptAction", new Vector2(0f, 0f), new Vector2(1f, 0.58f), new Vector2(0.5f, 0f), new Vector2(-36f, -12f), new Vector2(0f, 10f), 18, TextAnchor.MiddleLeft, Color.white);
            var messageRoot = CreatePanel(canvasObject.transform, "InteractionMessage", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(860f, 64f), new Vector2(0f, 178f), new Color(0.08f, 0.07f, 0.055f, 0.9f));
            var message = CreateText(messageRoot.transform, "MessageText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-40f, -18f), Vector2.zero, 18, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.84f, 1f));

            var prompt = canvasObject.AddComponent<InteractionPromptUI>();
            SetSerializedObjectReference(prompt, "promptRoot", promptRoot);
            SetSerializedObjectReference(prompt, "titleText", title);
            SetSerializedObjectReference(prompt, "actionText", action);
            SetSerializedObjectReference(prompt, "messageRoot", messageRoot);
            SetSerializedObjectReference(prompt, "messageText", message);
        }

        private static void CreateWorldDirectionCanvas()
        {
            var canvasObject = new GameObject("WorldDirectionCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = CreatePanel(canvasObject.transform, "WorldDirectionPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(620f, 110f), new Vector2(44f, -44f), new Color(0.04f, 0.037f, 0.03f, 0.82f));
            var directionText = CreateText(panel.transform, "WorldDirectionText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-42f, -24f), Vector2.zero, 19, TextAnchor.MiddleLeft, new Color(0.93f, 0.88f, 0.74f, 1f));
            directionText.gameObject.AddComponent<LocalizedStaticText>().Configure(
                "Velemar: village center, Old Forest west, Black Swamp south, Ash Road east, tower ruins north.",
                "Велемар: деревня в центре, Старый Лес на западе, Чёрное Болото на юге, Пепельный тракт на востоке, руины башни на севере.");
        }

        private static void CreateZoneDiscoveryCanvas()
        {
            var canvasObject = new GameObject("ZoneDiscoveryCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 35;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var group = canvasObject.AddComponent<CanvasGroup>();
            var panel = CreatePanel(canvasObject.transform, "ZoneDiscoveryPanel", new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(720f, 90f), Vector2.zero, new Color(0.025f, 0.022f, 0.018f, 0.72f));
            var title = CreateText(panel.transform, "ZoneDiscoveryTitle", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-70f, -20f), Vector2.zero, 34, TextAnchor.MiddleCenter, new Color(0.94f, 0.78f, 0.42f, 1f));

            var discovery = canvasObject.AddComponent<ZoneDiscoveryUI>();
            SetSerializedObjectReference(discovery, "canvasGroup", group);
            SetSerializedObjectReference(discovery, "zoneText", title);
        }

        private static void CreateZoneDiscoveryTriggers(Transform parent)
        {
            var root = new GameObject("ZoneDiscoveryTriggers");
            root.transform.SetParent(parent, false);

            CreateZoneDiscoveryTrigger(root.transform, "VillageZoneDiscovery", "Heather Ford", "Вересковый Брод", new Vector3(0f, 1.5f, -10f), new Vector3(18f, 3f, 8f));
            CreateZoneDiscoveryTrigger(root.transform, "ForestZoneDiscovery", "Old Forest", "Старый Лес", new Vector3(-45f, 1.5f, 2f), new Vector3(10f, 3f, 22f));
            CreateZoneDiscoveryTrigger(root.transform, "SwampZoneDiscovery", "Black Swamp", "Чёрное Болото", new Vector3(2f, 1.5f, -49f), new Vector3(20f, 3f, 10f));
            CreateZoneDiscoveryTrigger(root.transform, "AshRoadZoneDiscovery", "Ash Road", "Пепельный тракт", new Vector3(48f, 1.5f, 2f), new Vector3(10f, 3f, 20f));
            CreateZoneDiscoveryTrigger(root.transform, "TowerZoneDiscovery", "Tower Ruins", "Руины Башни", new Vector3(0f, 1.5f, 49f), new Vector3(20f, 3f, 10f));
        }

        private static void CreateZoneDiscoveryTrigger(Transform parent, string name, string englishName, string russianName, Vector3 position, Vector3 size)
        {
            var triggerObject = new GameObject(name);
            triggerObject.transform.SetParent(parent, false);
            triggerObject.transform.localPosition = position;

            var collider = triggerObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            triggerObject.AddComponent<ZoneDiscoveryTrigger>().Configure(englishName, russianName);
        }

        private static void CreateDialogueCanvas()
        {
            var canvasObject = new GameObject("DialogueCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var dialogueRoot = CreatePanel(canvasObject.transform, "DialoguePanel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1180f, 360f), new Vector2(0f, 54f), new Color(0.045f, 0.04f, 0.032f, 0.94f));
            var speaker = CreateText(dialogueRoot.transform, "DialogueSpeaker", new Vector2(0f, 0.78f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-72f, -24f), new Vector2(0f, -18f), 26, TextAnchor.MiddleLeft, new Color(0.94f, 0.78f, 0.42f, 1f));
            var body = CreateText(dialogueRoot.transform, "DialogueBody", new Vector2(0f, 0.46f), new Vector2(1f, 0.82f), new Vector2(0.5f, 0.5f), new Vector2(-72f, -10f), Vector2.zero, 22, TextAnchor.MiddleLeft, new Color(0.94f, 0.9f, 0.82f, 1f));
            var choices = new Text[4];
            for (var i = 0; i < choices.Length; i++)
            {
                choices[i] = CreateText(dialogueRoot.transform, $"DialogueChoice_{i + 1}", new Vector2(0f, 0.08f + i * 0.085f), new Vector2(1f, 0.18f + i * 0.085f), new Vector2(0.5f, 0.5f), new Vector2(-72f, -6f), Vector2.zero, 19, TextAnchor.MiddleLeft, Color.white);
            }

            var service = canvasObject.AddComponent<DialogueService>();
            SetSerializedObjectReference(service, "dialogueRoot", dialogueRoot);
            SetSerializedObjectReference(service, "speakerText", speaker);
            SetSerializedObjectReference(service, "bodyText", body);
            SetSerializedArrayReferences(service, "choiceTexts", choices);
        }

        private static void CreateQuestCanvas()
        {
            var canvasObject = new GameObject("QuestCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var hudRoot = CreatePanel(canvasObject.transform, "QuestHudPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(580f, 122f), new Vector2(-44f, -44f), new Color(0.045f, 0.04f, 0.032f, 0.88f));
            var title = CreateText(hudRoot.transform, "QuestHudTitle", new Vector2(0f, 0.58f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-44f, -16f), new Vector2(0f, -10f), 21, TextAnchor.MiddleLeft, new Color(0.94f, 0.78f, 0.42f, 1f));
            var objective = CreateText(hudRoot.transform, "QuestHudObjective", new Vector2(0f, 0f), new Vector2(1f, 0.62f), new Vector2(0.5f, 0f), new Vector2(-44f, -16f), new Vector2(0f, 10f), 18, TextAnchor.MiddleLeft, new Color(0.93f, 0.9f, 0.82f, 1f));

            var hud = canvasObject.AddComponent<QuestHudUI>();
            SetSerializedObjectReference(hud, "hudRoot", hudRoot);
            SetSerializedObjectReference(hud, "titleText", title);
            SetSerializedObjectReference(hud, "objectiveText", objective);
        }

        private static void CreateHealthCanvas()
        {
            var canvasObject = new GameObject("HealthCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var hudRoot = CreatePanel(canvasObject.transform, "HealthHudPanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(520f, 96f), new Vector2(44f, 42f), new Color(0.035f, 0.03f, 0.025f, 0.9f));
            var barBack = CreatePanel(hudRoot.transform, "HealthBarBack", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-52f, 24f), new Vector2(0f, 22f), new Color(0.12f, 0.035f, 0.03f, 1f));
            var barFillObject = CreatePanel(barBack.transform, "HealthBarFill", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.68f, 0.11f, 0.08f, 1f));
            var fillImage = barFillObject.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;
            var healthText = CreateText(hudRoot.transform, "HealthHudText", new Vector2(0f, 0.48f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-52f, -16f), new Vector2(0f, -10f), 21, TextAnchor.MiddleLeft, new Color(0.96f, 0.88f, 0.77f, 1f));

            var hud = canvasObject.AddComponent<HealthHudUI>();
            SetSerializedObjectReference(hud, "healthText", healthText);
            SetSerializedObjectReference(hud, "healthFill", fillImage);

            var statusRoot = CreatePanel(canvasObject.transform, "CombatStatusPanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(520f, 128f), new Vector2(44f, 154f), new Color(0.03f, 0.032f, 0.027f, 0.86f));
            var statusText = CreateText(statusRoot.transform, "CombatStatusText", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-36f, -22f), Vector2.zero, 17, TextAnchor.MiddleLeft, new Color(0.9f, 0.86f, 0.75f, 1f));
            var combatStatus = canvasObject.AddComponent<CombatStatusHudUI>();
            combatStatus.Configure(statusText);

            var deathRoot = CreatePanel(canvasObject.transform, "PlayerDeathPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.015f, 0.012f, 0.012f, 0.94f));
            var deathCanvas = deathRoot.AddComponent<Canvas>();
            deathCanvas.overrideSorting = true;
            deathCanvas.sortingOrder = 100;
            deathRoot.AddComponent<GraphicRaycaster>();
            var deathTitle = CreateText(deathRoot.transform, "PlayerDeathTitle", new Vector2(0.25f, 0.6f), new Vector2(0.75f, 0.76f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 46, TextAnchor.MiddleCenter, new Color(0.72f, 0.12f, 0.08f, 1f));
            var deathDescription = CreateText(deathRoot.transform, "PlayerDeathDescription", new Vector2(0.25f, 0.48f), new Vector2(0.75f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 23, TextAnchor.MiddleCenter, new Color(0.88f, 0.84f, 0.75f, 1f));
            var retryButton = CreateUiButton(deathRoot.transform, "PlayerDeathRetryButton", "Retry", new Vector2(0.35f, 0.32f), new Vector2(0.49f, 0.41f));
            var menuButton = CreateUiButton(deathRoot.transform, "PlayerDeathMenuButton", "Main Menu", new Vector2(0.51f, 0.32f), new Vector2(0.65f, 0.41f));
            var deathUi = canvasObject.AddComponent<PlayerDeathUI>();
            deathUi.Configure(deathRoot, deathTitle, deathDescription, retryButton.GetComponentInChildren<Text>(), menuButton.GetComponentInChildren<Text>());
            UnityEventTools.AddPersistentListener(retryButton.onClick, deathUi.Retry);
            UnityEventTools.AddPersistentListener(menuButton.onClick, deathUi.ReturnToMainMenu);
            deathRoot.SetActive(false);
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

            var panel = CreatePanel(canvasObject.transform, "InventoryPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(720f, 600f), new Vector2(44f, -44f), new Color(0.045f, 0.04f, 0.033f, 0.93f));
            var content = CreateText(panel.transform, "InventoryContent", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-56f, -48f), Vector2.zero, 18, TextAnchor.UpperLeft, new Color(0.93f, 0.9f, 0.82f, 1f));

            var hud = canvasObject.AddComponent<InventoryHudUI>();
            SetSerializedObjectReference(hud, "panelRoot", panel);
            SetSerializedObjectReference(hud, "contentText", content);
        }

        private static void CreateGameplayMenuCanvas()
        {
            var canvasObject = new GameObject("GameplayMenuCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 75;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("GameplayMenuEventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var root = CreatePanel(canvasObject.transform, "GameplayMenuRoot", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.008f, 0.01f, 0.012f, 0.88f));
            var frame = CreatePanel(root.transform, "GameplayMenuFrame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1240f, 760f), Vector2.zero, new Color(0.035f, 0.032f, 0.027f, 0.98f));
            var title = CreateText(frame.transform, "GameplayMenuTitle", new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 36, TextAnchor.MiddleCenter, new Color(0.94f, 0.78f, 0.42f, 1f));

            var mapPanel = CreatePanel(frame.transform, "WorldMapPanel", new Vector2(0.06f, 0.13f), new Vector2(0.94f, 0.83f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.032f, 0.026f, 0.92f));
            var mapText = CreateText(mapPanel.transform, "WorldMapText", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 25, TextAnchor.MiddleCenter, new Color(0.9f, 0.86f, 0.72f, 1f));

            var characterPanel = CreatePanel(frame.transform, "CharacterStatsPanel", new Vector2(0.06f, 0.13f), new Vector2(0.94f, 0.83f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.03f, 0.028f, 0.025f, 0.96f));
            var characterText = CreateText(characterPanel.transform, "CharacterStatsText", new Vector2(0.06f, 0.2f), new Vector2(0.62f, 0.94f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 25, TextAnchor.UpperLeft, new Color(0.92f, 0.88f, 0.78f, 1f));

            var strengthButton = CreateUiButton(characterPanel.transform, "UpgradeStrengthButton", "Upgrade Strength", new Vector2(0.66f, 0.65f), new Vector2(0.94f, 0.78f));
            var resilienceButton = CreateUiButton(characterPanel.transform, "UpgradeResilienceButton", "Upgrade Resilience", new Vector2(0.66f, 0.45f), new Vector2(0.94f, 0.58f));
            var vitalityButton = CreateUiButton(characterPanel.transform, "UpgradeVitalityButton", "Upgrade Vitality", new Vector2(0.66f, 0.25f), new Vector2(0.94f, 0.38f));

            var menu = canvasObject.AddComponent<GameplayMenuUI>();
            SetSerializedObjectReference(menu, "root", root);
            SetSerializedObjectReference(menu, "mapPanel", mapPanel);
            SetSerializedObjectReference(menu, "characterPanel", characterPanel);
            SetSerializedObjectReference(menu, "titleText", title);
            SetSerializedObjectReference(menu, "mapText", mapText);
            SetSerializedObjectReference(menu, "characterText", characterText);
            SetSerializedObjectReference(menu, "strengthButtonText", strengthButton.GetComponentInChildren<Text>());
            SetSerializedObjectReference(menu, "resilienceButtonText", resilienceButton.GetComponentInChildren<Text>());
            SetSerializedObjectReference(menu, "vitalityButtonText", vitalityButton.GetComponentInChildren<Text>());

            UnityEventTools.AddPersistentListener(strengthButton.onClick, menu.UpgradeStrength);
            UnityEventTools.AddPersistentListener(resilienceButton.onClick, menu.UpgradeResilience);
            UnityEventTools.AddPersistentListener(vitalityButton.onClick, menu.UpgradeVitality);
            root.SetActive(false);
        }

        private static Button CreateUiButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var buttonObject = CreatePanel(parent, name, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.11f, 0.07f, 0.98f));
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-24f, -12f), Vector2.zero, 20, TextAnchor.MiddleCenter, new Color(0.95f, 0.9f, 0.78f, 1f)).text = label;
            return button;
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

        private static void CreateHouse(Transform parent, string name, Vector3 position, Quaternion rotation, float scale)
        {
            var house = new GameObject(name);
            house.transform.SetParent(parent, false);
            house.transform.localPosition = position;
            house.transform.localRotation = rotation;
            house.transform.localScale = Vector3.one * scale;

            // The Kenney wall/roof meshes' UVs land on the colormap's white plaster stripe,
            // so the shared colormap material renders them pure white (they blew out to
            // solid slabs under the bright sun). Tint these pieces directly to a muted
            // sandstone plaster + terracotta roof so the NPC houses read as real cottages.
            var plaster = CreateMaterial("Assets/Materials/HousePlasterWall.mat", new Color(0.57f, 0.51f, 0.41f, 1f));
            var roofMat = CreateMaterial("Assets/Materials/HouseRoofTile.mat", new Color(0.48f, 0.26f, 0.15f, 1f));

            PlaceTintedKenney(house.transform, $"{name}_WallA", "wall-door.fbx", new Vector3(0f, 0f, -0.8f), Quaternion.identity, plaster);
            PlaceTintedKenney(house.transform, $"{name}_WallB", "wall.fbx", new Vector3(-1.1f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), plaster);
            PlaceTintedKenney(house.transform, $"{name}_WallC", "wall.fbx", new Vector3(1.1f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), plaster);
            PlaceTintedKenney(house.transform, $"{name}_Roof", "roof-high.fbx", new Vector3(0f, 1.2f, 0f), Quaternion.identity, roofMat);
        }

        // Like PlaceKenney but applies an explicit tint instead of the shared colormap
        // atlas (used where the model's UVs would otherwise sample a blown-out palette
        // stripe). Keeps the same building scale multiplier and solid colliders.
        private static void PlaceTintedKenney(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Material material)
        {
            var adjustedScale = (IsKenneyBuildingModel(modelName) ? KenneyBuildingScaleMultiplier : PropScaleMultiplier) * Vector3.one;
            var instance = InstantiateModel($"{KenneyPath}/{modelName}", objectName, parent, position, rotation, adjustedScale);
            if (instance != null)
            {
                ApplyMaterialToChildRenderers(instance, material);
                AddSolidColliders(instance);
            }
        }

        private static GameObject CreateCapsule(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = name;
            capsule.transform.SetParent(parent, false);
            capsule.transform.localPosition = position;
            capsule.transform.localScale = scale;
            // Invisible collision/anchor body only; the visible character is the model
            // child. Strip the mesh so no white placeholder "hitbox" capsule can render.
            StripPrimitiveMesh(capsule);
            return capsule;
        }

        // Depth-first search for a descendant transform by exact name (used to find
        // skeleton bones like "Palm.R" inside an instantiated character model).
        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDescendant(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        // Removes a primitive's MeshRenderer + MeshFilter, leaving only its collider.
        private static void StripPrimitiveMesh(GameObject primitive)
        {
            var mr = primitive.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Object.DestroyImmediate(mr);
            }

            var mf = primitive.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Object.DestroyImmediate(mf);
            }
        }

        private static GameObject CreateRpgCharacterAnchor(Transform parent, string name, string modelName, Vector3 position, Quaternion rotation, Vector3 visualScale, Color fallbackColor)
        {
            var anchor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            anchor.name = name;
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = position;
            anchor.transform.localRotation = rotation;
            anchor.transform.localScale = new Vector3(0.7f, 1f, 0.7f);

            var renderer = anchor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            var visual = InstantiateModel($"{RpgCharacterPath}/{modelName}", $"{name}_Model", anchor.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, visualScale * HumanModelScaleMultiplier);
            if (visual == null)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                    renderer.sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", fallbackColor);
                }
            }
            else
            {
                // Apply the character's real texture atlas (skin/armour/cloth); hide the
                // model's built-in weapon so it doesn't double with the attached weapon prop.
                ApplyCharacterMaterials(visual, true);
                // Model is present: strip the placeholder capsule mesh so no white
                // "hitbox" capsule shows behind the character skin.
                StripPrimitiveMesh(anchor);
                // Animate the character: NPCs stay in Idle (breathe instead of standing
                // frozen); bandits/enforcers get driven by EnemyAnimatorDriver in combat.
                AttachAnimator(visual, CharacterAnimationSetup.RpgController, $"{RpgCharacterPath}/{modelName}");
            }

            return anchor;
        }

        private static void ApplyMaterialToChildRenderers(GameObject root, Material material)
        {
            if (root == null || material == null)
            {
                return;
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sharedMaterial = material;
                }
            }
        }

        private const string RpgTextureDir = "Assets/Art/External/OpenGameArt_RPGCharacters/Textures";
        private static readonly string[] WeaponNameKeys = { "sword", "dagger", "staff", "bow", "blade", "club", "katana", "axe", "mace", "shield" };

        // Gives a character model its proper colours instead of a flat blob:
        //  - parts whose source material is a "*_Texture" (OpenGameArt RPG characters) get
        //    that texture atlas applied (real skin/armour/cloth);
        //  - solid-slot parts (Quaternius knight: Armor/Boots/Skin) get a colour by slot name.
        // When hideBuiltInWeapon is set, the model's own weapon mesh is removed so it does
        // not double with the separately-attached weapon prop.
        private static void ApplyCharacterMaterials(GameObject root, bool hideBuiltInWeapon)
        {
            if (root == null)
            {
                return;
            }

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null)
                {
                    continue;
                }

                if (hideBuiltInWeapon && NameHasWeaponKey(r.gameObject.name))
                {
                    Object.DestroyImmediate(r);
                    continue;
                }

                var src = r.sharedMaterials;
                var dst = new Material[src.Length];
                for (var i = 0; i < src.Length; i++)
                {
                    var nm = src[i] != null ? src[i].name : "Part";
                    var tex = nm.Contains("Texture") ? AssetDatabase.LoadAssetAtPath<Texture2D>($"{RpgTextureDir}/{nm}.png") : null;
                    dst[i] = tex != null
                        ? CreateTexturedMaterial(nm, tex)
                        : CreateMaterial($"Assets/Materials/CharPart_{SanitizeName(nm)}.mat", SolidColorForPart(nm));
                }

                r.sharedMaterials = dst;
            }
        }

        private static bool NameHasWeaponKey(string name)
        {
            var n = name.ToLower();
            for (var i = 0; i < WeaponNameKeys.Length; i++)
            {
                if (n.Contains(WeaponNameKeys[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static Material CreateTexturedMaterial(string sourceName, Texture2D tex)
        {
            var path = $"Assets/Materials/CharTex_{SanitizeName(sourceName)}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = Shader.Find("Standard");
            mat.mainTexture = tex;
            mat.color = Color.white;
            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", 0.08f);
            }
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            return mat;
        }

        private static Color SolidColorForPart(string name)
        {
            var n = name.ToLower();
            if (n.Contains("skin") || n.Contains("face") || n.Contains("head") || n.Contains("hand"))
            {
                return new Color(0.78f, 0.6f, 0.48f, 1f);
            }
            if (n.Contains("hair"))
            {
                return new Color(0.26f, 0.17f, 0.09f, 1f);
            }
            if (n.Contains("armor") || n.Contains("metal") || n.Contains("plate") || n.Contains("shoulder") || n.Contains("guard"))
            {
                return new Color(0.42f, 0.44f, 0.49f, 1f);
            }
            if (n.Contains("boot") || n.Contains("belt") || n.Contains("leather") || n.Contains("pouch") || n.Contains("shoe") || n.Contains("strap"))
            {
                return new Color(0.26f, 0.17f, 0.1f, 1f);
            }
            if (n.Contains("cloth") || n.Contains("shirt") || n.Contains("pant") || n.Contains("hood") || n.Contains("cape") || n.Contains("robe") || n.Contains("tunic"))
            {
                return new Color(0.3f, 0.3f, 0.36f, 1f);
            }

            return new Color(0.4f, 0.36f, 0.32f, 1f);
        }

        private static string SanitizeName(string name)
        {
            var chars = name.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static void AddCombatVisual(GameObject target, Color hitFlashColor, Color deathColor)
        {
            var feedback = target.AddComponent<CombatVisualFeedback>();
            feedback.Configure(hitFlashColor, deathColor);
        }

        private static void AddEnemyActionVisual(GameObject target, Transform visualRoot, bool quadruped)
        {
            var animator = target.AddComponent<EnemyActionVisualAnimator>();
            animator.Configure(visualRoot, quadruped);
        }

        private static void AddNpcEquipment(GameObject anchor, string objectName, string modelName, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            if (anchor == null)
            {
                return;
            }

            // Parent the weapon to the right-hand bone (Fist.R) so it follows the skeletal
            // animation instead of floating beside the body. Grip pose is a best estimate;
            // the larger localScale compensates for the small character/bone scale. Falls
            // back to the anchor with the caller's values if the bone is missing.
            var hand = FindDescendant(anchor.transform, "Fist.R");
            var weapon = hand != null
                ? InstantiateModel($"{RpgWeaponPath}/{modelName}", objectName, hand, new Vector3(0.03f, 0.05f, 0f), Quaternion.Euler(0f, 0f, -95f), Vector3.one * 2.2f)
                : InstantiateModel($"{RpgWeaponPath}/{modelName}", objectName, anchor.transform, localPosition, localRotation, localScale);
            // Texture the weapon (its material is a "*_Texture" atlas) so it is not white.
            ApplyCharacterMaterials(weapon, false);
        }

        // Standalone weapon prop (full asset path) used as visible merchant wares.
        private static void CreateWeaponWare(Transform parent, string objectName, string assetPath, Vector3 position, Quaternion rotation, float scale, Color tint)
        {
            var weapon = InstantiateModel(assetPath, objectName, parent, position, rotation, Vector3.one * scale);
            if (weapon != null)
            {
                ApplyMaterialToChildRenderers(weapon, CreateMaterial($"Assets/Materials/{objectName}_Tint.mat", tint));
            }
        }

        private static void AddInteractablePropVisual(GameObject marker, string objectName)
        {
            if (marker == null)
            {
                return;
            }

            var renderer = marker.GetComponent<Renderer>();
            switch (objectName)
            {
                case "WorldGirlMedallion":
                case "FinalTruthEvidenceShrine":
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                    PlaceKenney(marker.transform, $"{objectName}_MedallionProp", "wheel.fbx", Vector3.up * 0.18f, Quaternion.Euler(90f, 0f, 0f), Vector3.one * 0.16f);
                    break;
                case "WorldOrtenDiary":
                case "FinalSacrificeDiaryShrine":
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                    PlaceKenney(marker.transform, $"{objectName}_DiaryTableProp", "stall-bench.fbx", Vector3.down * 0.18f, Quaternion.identity, Vector3.one * 0.34f);
                    CreateNonBlockingMarker(marker.transform, $"{objectName}_OpenPagesProp", new Vector3(0f, 0.16f, 0f), new Vector3(0.34f, 0.018f, 0.22f), new Color(0.72f, 0.64f, 0.46f, 1f));
                    break;
                case "WorldMirrorShardCache":
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                    CreateNonBlockingMarker(marker.transform, $"{objectName}_ShardA", new Vector3(-0.12f, 0.16f, 0.02f), new Vector3(0.06f, 0.34f, 0.16f), new Color(0.5f, 0.34f, 0.9f, 1f));
                    CreateNonBlockingMarker(marker.transform, $"{objectName}_ShardB", new Vector3(0.14f, 0.21f, -0.04f), new Vector3(0.055f, 0.28f, 0.18f), new Color(0.42f, 0.7f, 0.95f, 1f));
                    break;
                case "WorldElderSealProof":
                case "FinalMayorControlPost":
                    PlaceKenney(marker.transform, $"{objectName}_BannerProp", "banner-red.fbx", new Vector3(0f, 0.2f, 0f), Quaternion.identity, Vector3.one * 0.28f);
                    break;
                case "WorldHunterClue_BrokenKnife":
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                    InstantiateModel($"{RpgWeaponPath}/Rogue_Dagger.fbx", $"{objectName}_DaggerProp", marker.transform, Vector3.up * 0.12f, Quaternion.Euler(78f, 0f, 36f), Vector3.one * 0.24f);
                    break;
                case "WorldHunterCamp_RewardPouch":
                case "WorldDrownerNestRewardCache":
                    PlaceKenney(marker.transform, $"{objectName}_CacheCartProp", "cart.fbx", Vector3.down * 0.1f, Quaternion.Euler(0f, 35f, 0f), Vector3.one * 0.28f);
                    break;
                case "WorldMartaHerbBasket":
                    PlaceKenney(marker.transform, $"{objectName}_HerbStallProp", "stall-green.fbx", Vector3.down * 0.12f, Quaternion.identity, Vector3.one * 0.32f);
                    break;
                case "WorldForgeSupplies":
                    PlaceKenney(marker.transform, $"{objectName}_OreCartProp", "cart-high.fbx", Vector3.down * 0.08f, Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.34f);
                    break;
                case "WorldAlchemyTable_Swallow":
                case "WorldAlchemyTable_Antitoxin":
                    PlaceKenney(marker.transform, $"{objectName}_AlchemyTableProp", "stall-bench.fbx", Vector3.down * 0.08f, Quaternion.identity, Vector3.one * 0.34f);
                    CreateNonBlockingMarker(marker.transform, $"{objectName}_BottleGlowProp", new Vector3(0.12f, 0.18f, 0.02f), new Vector3(0.08f, 0.12f, 0.08f), new Color(0.08f, 0.62f, 0.36f, 1f));
                    break;
                case "WorldForge_ReinforcedArmor":
                    PlaceKenney(marker.transform, $"{objectName}_ForgeBladeProp", "blade.fbx", new Vector3(0.06f, 0.2f, 0f), Quaternion.Euler(78f, 0f, 24f), Vector3.one * 0.42f);
                    break;
            }

            AddInteractionBeacon(marker);
        }

        // Floating glowing pillar above an interactable so the player can spot it from a
        // distance (quest markers, traces, pickups, crafting were tiny and easy to miss).
        // Counter-scales the parent's (often flat/non-uniform) scale so the beacon stays a
        // consistent thin world-space pillar. Non-blocking (collider stripped).
        private static void AddInteractionBeacon(GameObject marker)
        {
            if (marker == null)
            {
                return;
            }

            var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = marker.name + "_Beacon";
            beacon.transform.SetParent(marker.transform, false);
            Object.DestroyImmediate(beacon.GetComponent<Collider>());

            var ls = marker.transform.lossyScale;
            var ix = Mathf.Abs(ls.x) < 0.0001f ? 1f : 1f / ls.x;
            var iy = Mathf.Abs(ls.y) < 0.0001f ? 1f : 1f / ls.y;
            var iz = Mathf.Abs(ls.z) < 0.0001f ? 1f : 1f / ls.z;
            beacon.transform.localScale = new Vector3(0.1f * ix, 1.3f * iy, 0.1f * iz);
            beacon.transform.localPosition = new Vector3(0f, 1.7f * iy, 0f);

            var mat = CreateMaterial($"Assets/Materials/{marker.name}_Beacon.mat", new Color(1f, 0.84f, 0.35f, 1f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.72f, 0.24f, 1f));
            beacon.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateCharacterGroundRing(GameObject anchor, string ringName, Color color, float radius)
        {
            if (anchor == null)
            {
                return;
            }

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = ringName;
            ring.transform.SetParent(anchor.transform, false);
            ring.transform.localPosition = new Vector3(0f, anchor.GetComponent<CharacterController>() != null ? 0.025f : -0.94f, 0f);
            ring.transform.localScale = new Vector3(radius, 0.015f, radius);
            ring.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{ringName}.mat", color);
            Object.DestroyImmediate(ring.GetComponent<Collider>());
            // These readability discs read as visible "hitboxes" under characters; hide
            // the renderer so only the character skins show. Object kept for validator refs.
            ring.GetComponent<Renderer>().enabled = false;
        }

        private static void CreateQuestMarker(Transform parent, string objectName, string displayName, string prompt, string questAction, string successMessage, string blockedMessage, Vector3 position, Vector3 scale, Color color, bool canRepeat = false)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);
            AddInteractablePropVisual(marker, objectName);

            var interactable = marker.AddComponent<QuestProgressInteractable>();
            interactable.Configure(displayName, prompt, questAction, successMessage, blockedMessage, canRepeat);
        }

        private static void CreateSupplyCrate(Transform parent, string objectName, string displayName, string prompt, string[] itemsToGrant, string message, Vector3 position, Vector3 scale, Color color)
        {
            var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = objectName;
            crate.transform.SetParent(parent, false);
            crate.transform.localPosition = position;
            crate.transform.localScale = scale;
            crate.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);
            AddInteractablePropVisual(crate, objectName);

            var interactable = crate.AddComponent<InventoryGrantInteractable>();
            interactable.Configure(displayName, prompt, itemsToGrant, message);
        }

        // A single pickup item lying on the ground: a small flat base (keeps a solid
        // collider for interaction, like the crates) + an optional themed weapon/tool prop
        // lying beside it + a beacon. Picking it up grants the listed items.
        private static void CreateGroundLoot(Transform parent, string objectName, string displayName, string prompt, string[] items, string message, Vector3 position, Color color, string propPath = null, float propScale = 0.5f)
        {
            var loot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            loot.name = objectName;
            loot.transform.SetParent(parent, false);
            loot.transform.localPosition = position;
            loot.transform.localScale = new Vector3(0.5f, 0.18f, 0.5f);
            loot.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);

            if (!string.IsNullOrEmpty(propPath))
            {
                // Parent the prop to the world root (not the flat base) to avoid distortion;
                // laid roughly horizontal so it reads as dropped on the ground.
                var prop = InstantiateModel(propPath, $"{objectName}_Prop", parent, position + new Vector3(0f, 0.12f, 0f), Quaternion.Euler(90f, 35f, 0f), Vector3.one * propScale);
                if (prop != null)
                {
                    ApplyMaterialToChildRenderers(prop, CreateMaterial($"Assets/Materials/{objectName}_Prop.mat", color));
                }
            }

            AddInteractionBeacon(loot);
            var interactable = loot.AddComponent<InventoryGrantInteractable>();
            interactable.Configure(displayName, prompt, items, message);
        }

        // Loot lying around the whole map, themed per zone from the story (Velemar: herbs,
        // pelts, bog reagents, mirror-tower bones/ore, Ash Road refugee supplies & bandit
        // caches). Each is a pickup with a beacon so the world feels looted and alive.
        private static void CreateScatteredGroundLoot(Transform parent)
        {
            // Village outskirts.
            CreateGroundLoot(parent, "LootVillageHerbs", "Рассыпанные травы", "Подобрать", new[] { "Swallow Grass" }, "Подобрано: Дьявольский гриб.", new Vector3(3.5f, 0f, 6.2f), new Color(0.16f, 0.32f, 0.13f, 1f));
            CreateGroundLoot(parent, "LootVillageFood", "Оброненная снедь", "Подобрать", new[] { "Food", "Field Ration" }, "Подобрано: Еда, Походный паёк.", new Vector3(-3.2f, 0f, 5.8f), new Color(0.3f, 0.24f, 0.14f, 1f));

            // Old Forest.
            CreateGroundLoot(parent, "LootForestPelt", "Шкура волка", "Снять", new[] { "Wolf Pelt" }, "Подобрано: Волчья шкура.", new Vector3(-72.5f, 0f, 11.5f), new Color(0.32f, 0.26f, 0.18f, 1f));
            CreateGroundLoot(parent, "LootForestHerbs", "Лесные травы", "Собрать", new[] { "Swallow Grass", "Bogweed" }, "Подобрано: Дьявольский гриб, Болотник.", new Vector3(-67.5f, 0f, 5.5f), new Color(0.14f, 0.3f, 0.12f, 1f));
            CreateGroundLoot(parent, "LootForestKnife", "Брошенный нож", "Подобрать", new[] { "Field Ration" }, "Подобрано: Походный паёк.", new Vector3(-74.5f, 0f, 8.5f), new Color(0.55f, 0.57f, 0.6f, 1f), $"{RpgWeaponPath}/Rogue_Dagger.fbx", 0.5f);

            // Black Swamp.
            CreateGroundLoot(parent, "LootSwampReagents", "Болотные реагенты", "Собрать", new[] { "Bogweed", "Drowner Slime" }, "Подобрано: Болотник, Слизь утопца.", new Vector3(11.5f, 0f, -70f), new Color(0.08f, 0.24f, 0.16f, 1f));
            CreateGroundLoot(parent, "LootSwampBlade", "Затонувший клинок", "Поднять", new[] { "Iron Ore" }, "Подобрано: Железная руда.", new Vector3(7f, 0f, -72.5f), new Color(0.5f, 0.53f, 0.57f, 1f), $"{KnightPath}/Sword.fbx", 0.5f);
            CreateGroundLoot(parent, "LootSwampWitchHerbs", "Травы Эльзы", "Собрать", new[] { "Bogweed" }, "Подобрано: Болотник.", new Vector3(13.5f, 0f, -76f), new Color(0.2f, 0.26f, 0.32f, 1f));

            // Tower Ruins.
            CreateGroundLoot(parent, "LootTowerBones", "Старые кости", "Осмотреть", new[] { "Iron Ore" }, "Подобрано: Железная руда.", new Vector3(2.5f, 0f, 73f), new Color(0.72f, 0.7f, 0.64f, 1f));
            CreateGroundLoot(parent, "LootTowerOre", "Обломки руды", "Собрать", new[] { "Iron Ore" }, "Подобрано: Железная руда.", new Vector3(-3f, 0f, 77.5f), new Color(0.4f, 0.4f, 0.44f, 1f));
            CreateGroundLoot(parent, "LootTowerWeapon", "Сломанное оружие", "Поднять", new[] { "Iron Ore" }, "Подобрано: Железная руда.", new Vector3(4.5f, 0f, 75f), new Color(0.46f, 0.4f, 0.34f, 1f), $"{KnightPath}/Club.fbx", 0.5f);

            // Ash Road.
            CreateGroundLoot(parent, "LootAshRefugeeSack", "Мешок беженца", "Обыскать", new[] { "Food", "Field Ration" }, "Подобрано: Еда, Походный паёк.", new Vector3(70f, 0f, 6f), new Color(0.3f, 0.25f, 0.16f, 1f));
            CreateGroundLoot(parent, "LootAshSalt", "Пепельная соль", "Собрать", new[] { "Ash Salt" }, "Подобрано: Пепельная соль.", new Vector3(74.5f, 0f, 10.5f), new Color(0.6f, 0.6f, 0.58f, 1f));
            CreateGroundLoot(parent, "LootAshBanditCache", "Схрон бандитов", "Обыскать", new[] { "Iron Ore", "Field Ration" }, "Подобрано: Железная руда, Походный паёк.", new Vector3(68f, 0f, 3.5f), new Color(0.28f, 0.2f, 0.14f, 1f), $"{RpgWeaponPath}/Warrior_Sword.fbx", 0.5f);

            // Travel roads between the zones.
            CreateGroundLoot(parent, "LootWestRoadHerbs", "Придорожные травы", "Собрать", new[] { "Swallow Grass" }, "Подобрано: Дьявольский гриб.", new Vector3(-38f, 0f, 4.5f), new Color(0.15f, 0.3f, 0.12f, 1f));
            CreateGroundLoot(parent, "LootNorthRoadOre", "Кусок руды", "Собрать", new[] { "Iron Ore" }, "Подобрано: Железная руда.", new Vector3(-3.5f, 0f, 38f), new Color(0.4f, 0.4f, 0.44f, 1f));
            CreateGroundLoot(parent, "LootSouthRoadBog", "Болотный сбор", "Собрать", new[] { "Bogweed" }, "Подобрано: Болотник.", new Vector3(-4.5f, 0f, -38f), new Color(0.1f, 0.24f, 0.15f, 1f));
            CreateGroundLoot(parent, "LootEastRoadSupplies", "Оброненные припасы", "Подобрать", new[] { "Field Ration", "Food" }, "Подобрано: Походный паёк, Еда.", new Vector3(40f, 0f, 6.5f), new Color(0.3f, 0.25f, 0.16f, 1f));
        }

        private static void CreateCraftingStation(Transform parent, string objectName, string displayName, string prompt, string recipeId, Vector3 position, Vector3 scale, Color color)
        {
            var station = GameObject.CreatePrimitive(PrimitiveType.Cube);
            station.name = objectName;
            station.transform.SetParent(parent, false);
            station.transform.localPosition = position;
            station.transform.localScale = scale;
            station.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);
            AddInteractablePropVisual(station, objectName);

            var interactable = station.AddComponent<CraftingInteractable>();
            interactable.Configure(displayName, prompt, recipeId);
        }

        // The whole Kenney Fantasy Town Kit shares one UV atlas (colormap.png); every
        // mesh's UVs index into it. The FBXs import with no material (materialImportMode
        // leaves Unity's default white), which is why bare Kenney walls/roofs rendered as
        // white slabs. Apply the shared atlas once so all pieces get their real colours.
        private const string KenneyColormapPath = "Assets/Art/External/Kenney_FantasyTownKit/Models/FBX format/Textures/colormap.png";
        private static Material _kenneyColormapMat;

        private static Material KenneyColormapMaterial()
        {
            if (_kenneyColormapMat != null)
            {
                return _kenneyColormapMat;
            }

            const string path = "Assets/Materials/KenneyColormap.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = Shader.Find("Standard");
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(KenneyColormapPath);
            if (tex != null)
            {
                mat.mainTexture = tex;
            }

            mat.color = Color.white;
            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", 0.08f);
            }
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            _kenneyColormapMat = mat;
            return mat;
        }

        private static void PlaceKenney(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var adjustedScale = IsKenneyBuildingModel(modelName) ? scale * KenneyBuildingScaleMultiplier : scale * PropScaleMultiplier;
            var instance = InstantiateModel($"{KenneyPath}/{modelName}", objectName, parent, position, rotation, adjustedScale);
            if (instance != null)
            {
                ApplyMaterialToChildRenderers(instance, KenneyColormapMaterial());
                AddSolidColliders(instance);
                return;
            }

            CreateMarker(parent, objectName + "_Fallback", position + Vector3.up * 0.35f, new Vector3(0.8f, 0.7f, 0.8f), new Color(0.28f, 0.22f, 0.14f, 1f));
        }

        // KayKit Medieval Builder models carry no texture and no vertex colours; each
        // submesh is a named solid-colour slot (Stone/White/Brown/Beige/...). The FBX's
        // own "White" slot imports as blown-out pure white, which is why market/barracks
        // read as broken white slabs. Remap every slot by name to a controlled medieval
        // palette so all KayKit buildings look intentional and consistent.
        private static readonly System.Collections.Generic.Dictionary<string, Color> KayKitPalette =
            new System.Collections.Generic.Dictionary<string, Color>
            {
                { "Stone", new Color(0.50f, 0.52f, 0.55f, 1f) },
                { "StoneDark", new Color(0.30f, 0.31f, 0.35f, 1f) },
                { "White", new Color(0.64f, 0.59f, 0.49f, 1f) },     // muted sandstone plaster; lighter than stone but won't blow out under the bright sun
                { "Beige", new Color(0.80f, 0.70f, 0.51f, 1f) },
                { "Brown", new Color(0.56f, 0.32f, 0.18f, 1f) },     // terracotta roof / wood
                { "BrownDark", new Color(0.29f, 0.18f, 0.10f, 1f) }, // dark beams
            };

        private static void ApplyKayKitPalette(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null)
                {
                    continue;
                }

                var src = r.sharedMaterials;
                var dst = new Material[src.Length];
                for (var i = 0; i < src.Length; i++)
                {
                    var nm = src[i] != null ? src[i].name : "Stone";
                    var key = nm.Split(' ')[0]; // drop any " (Instance)" suffix
                    var col = KayKitPalette.TryGetValue(key, out var c) ? c : new Color(0.55f, 0.5f, 0.44f, 1f);
                    dst[i] = CreateMaterial($"Assets/Materials/KayKit_{SanitizeName(key)}.mat", col);
                }

                r.sharedMaterials = dst;
            }
        }

        private static void PlaceKayKit(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var adjustedScale = IsKayKitBuildingModel(modelName) ? scale * KayKitBuildingScaleMultiplier : scale * PropScaleMultiplier;
            var instance = InstantiateModel($"{KayKitMedievalPath}/{modelName}", objectName, parent, position, rotation, adjustedScale);
            if (instance != null)
            {
                ApplyKayKitPalette(instance);
                AddSolidColliders(instance);
                return;
            }

            CreateMarker(parent, objectName + "_Fallback", position + Vector3.up * 0.4f, new Vector3(0.9f, 0.8f, 0.9f), new Color(0.22f, 0.2f, 0.16f, 1f));
        }

        // Adds a per-mesh MeshCollider to placed props/buildings so the player can no
        // longer walk through them. MeshCollider (not a Box) is used so openings like the
        // village gate arch and house doorways stay passable. Non-convex static colliders
        // work fine against the player's CharacterController. Skips already-collided meshes.
        private static void AddSolidColliders(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var filters = root.GetComponentsInChildren<MeshFilter>();
            for (var i = 0; i < filters.Length; i++)
            {
                var mf = filters[i];
                if (mf == null || mf.sharedMesh == null || mf.GetComponent<Collider>() != null)
                {
                    continue;
                }

                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }

        private static bool IsKenneyBuildingModel(string modelName)
        {
            return modelName.StartsWith("wall") || modelName.StartsWith("roof") || modelName.Contains("gate") || modelName.Contains("stairs") || modelName.Contains("pillar");
        }

        private static bool IsKayKitBuildingModel(string modelName)
        {
            return modelName.Contains("house") || modelName.Contains("market") || modelName.Contains("watermill") || modelName.Contains("barracks") || modelName.Contains("watchtower") || modelName.Contains("castle") || modelName.Contains("bridge") || modelName.Contains("mine") || modelName.Contains("mill") || modelName.Contains("lumbermill") || modelName.Contains("archeryrange") || modelName.Contains("farm_plot") || modelName.StartsWith("wall");
        }

        // Assigns a generated AnimatorController to an imported (Generic-rig) model so
        // it plays real skeletal clips. The FBX prefab already imports with an Animator
        // and avatar; we just set the controller and disable root motion.
        private static void AttachAnimator(GameObject modelInstance, string controllerPath, string fbxPath)
        {
            if (modelInstance == null)
            {
                return;
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogWarning($"AttachAnimator: controller missing at {controllerPath}");
                return;
            }

            // Note: GetComponent returns a Unity "fake null" when absent, which the C# ??
            // operator does not catch, so use an explicit == null check before AddComponent.
            var animator = modelInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = modelInstance.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            if (animator.avatar == null)
            {
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                {
                    if (asset is Avatar avatar)
                    {
                        animator.avatar = avatar;
                        break;
                    }
                }
            }
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

        private static void CreateRegionDisc(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = position;
            cylinder.transform.localScale = scale;
            cylinder.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
            Object.DestroyImmediate(cylinder.GetComponent<Collider>());
        }

        private static void CreateReedCluster(Transform parent, string name, Vector3 position, float height)
        {
            var cluster = new GameObject(name);
            cluster.transform.SetParent(parent, false);
            cluster.transform.localPosition = position;

            for (var i = 0; i < 5; i++)
            {
                var reed = GameObject.CreatePrimitive(PrimitiveType.Cube);
                reed.name = $"{name}_Reed_{i + 1}";
                reed.transform.SetParent(cluster.transform, false);
                reed.transform.localPosition = new Vector3((i - 2) * 0.12f, height * 0.5f, (i % 2) * 0.1f);
                reed.transform.localRotation = Quaternion.Euler(0f, i * 19f, (i - 2) * 7f);
                reed.transform.localScale = new Vector3(0.055f, height, 0.055f);
                reed.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/SwampReed.mat", new Color(0.18f, 0.24f, 0.11f, 1f));
                Object.DestroyImmediate(reed.GetComponent<Collider>());
            }
        }

        private static void CreateZoneLabel(Transform parent, string name, string text, Vector3 position, Quaternion rotation)
        {
            var labelObject = new GameObject(name);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = position;
            labelObject.transform.localRotation = rotation;

            var label = labelObject.AddComponent<TextMesh>();
            label.text = NormalizeZoneLabelText(name, text);
            label.fontSize = 38;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(0.9f, 0.78f, 0.48f, 1f);
        }

        private static string NormalizeZoneLabelText(string objectName, string fallbackText)
        {
            if (objectName.Contains("Village"))
            {
                return "\u0412\u0435\u0440\u0435\u0441\u043a\u043e\u0432\u044b\u0439 \u0411\u0440\u043e\u0434";
            }

            if (objectName.Contains("Forest"))
            {
                return "\u0421\u0442\u0430\u0440\u044b\u0439 \u041b\u0435\u0441";
            }

            if (objectName.Contains("Swamp"))
            {
                return "\u0427\u0451\u0440\u043d\u043e\u0435 \u0411\u043e\u043b\u043e\u0442\u043e";
            }

            if (objectName.Contains("AshRoad"))
            {
                return "\u041f\u0435\u043f\u0435\u043b\u044c\u043d\u044b\u0439 \u0442\u0440\u0430\u043a\u0442";
            }

            if (objectName.Contains("Tower"))
            {
                return "\u0420\u0443\u0438\u043d\u044b \u0411\u0430\u0448\u043d\u0438";
            }

            return fallbackText;
        }

        private static void CreateMarker(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
        }

        private static void CreateNonBlockingMarker(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static void CreateSurfacePatch(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patch.name = name;
            patch.transform.SetParent(parent, false);
            patch.transform.localPosition = position;
            patch.transform.localScale = scale;
            patch.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
            Object.DestroyImmediate(patch.GetComponent<Collider>());
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

        private static Material CreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                ConfigureMaterialColor(material, color);
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ConfigureMaterialColor(material, color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureMaterialColor(Material material, Color color)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            // Near-matte: the Standard shader's smoothness slider is "_Glossiness" (the old
            // code only set "_Smoothness", which Standard ignores, so materials kept the
            // default 0.5 gloss and open ground mirrored the pale dusk skybox — that was the
            // washed-out swamp/dragon plain). Force both names low. Low-poly stylised assets
            // read better fully matte anyway.
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.03f);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.03f);
            }
            if (material.HasProperty("_GlossyReflections"))
            {
                material.SetFloat("_GlossyReflections", 0f);
            }
            if (material.HasProperty("_SpecularHighlights"))
            {
                material.SetFloat("_SpecularHighlights", 0f);
            }

            EditorUtility.SetDirty(material);
        }

        private static Material CreateEmissiveMaterial(string path, Color color, float emissionStrength)
        {
            var material = CreateMaterial(path, color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0f, emissionStrength));
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static void SetSerializedObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedArrayReferences(Object target, string propertyName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;

            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

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

        // Removes every active GameObject with the given name (not just the first).
        // Used to clear stray orphans left at the scene root by old builds — e.g. a
        // duplicate "VelemarWorldTerrain" plane that kept Unity's white Default-Material
        // and showed through wherever dark ground patches did not cover it.
        private static void RemoveAllNamed(string name)
        {
            for (var guard = 0; guard < 64; guard++)
            {
                var existing = GameObject.Find(name);
                if (existing == null)
                {
                    return;
                }

                Object.DestroyImmediate(existing);
            }
        }
    }
}

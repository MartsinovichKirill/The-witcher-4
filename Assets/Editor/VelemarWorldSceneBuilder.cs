using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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

namespace WitcherRightVersion.Editor
{
    public static class VelemarWorldSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/VelemarWorldScene.unity";
        private const string KenneyPath = "Assets/Art/External/Kenney_FantasyTownKit/Models/FBX format";
        private const string KnightPath = "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX";
        private const string RpgCharacterPath = "Assets/Art/External/OpenGameArt_RPGCharacters/FBX";
        private const string MonsterPath = "Assets/Art/External/Quaternius_AnimatedMonsters/FBX";
        private const string KayKitMedievalPath = "Assets/Art/External/KayKit_MedievalBuilder/FBX";

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
            RemoveIfExists("VelemarAtmosphereLights");
            RemoveIfExists("VelemarWorldRoot");
            RemoveIfExists("InteractionCanvas");
            RemoveIfExists("DialogueCanvas");
            RemoveIfExists("QuestCanvas");
            RemoveIfExists("HealthCanvas");
            RemoveIfExists("InventoryCanvas");
            RemoveIfExists("EndingCanvas");
            RemoveIfExists("WorldDirectionCanvas");

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
            light.color = new Color(0.92f, 0.78f, 0.58f, 1f);
            light.intensity = 0.82f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(44f, -34f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.18f, 0.17f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.21f, 0.27f, 0.24f, 1f);
            RenderSettings.fogDensity = 0.0105f;

            var lightsRoot = new GameObject("VelemarAtmosphereLights");
            CreatePointLight(lightsRoot.transform, "VillageWarmLanternLight", new Vector3(0f, 3.1f, -4.8f), new Color(1f, 0.58f, 0.28f, 1f), 1.15f, 10f);
            CreatePointLight(lightsRoot.transform, "SwampColdMiasmaLight", new Vector3(5.4f, 2.2f, -25.8f), new Color(0.16f, 0.48f, 0.36f, 1f), 0.85f, 12f);
            CreatePointLight(lightsRoot.transform, "TowerMirrorVioletLight", new Vector3(0f, 3.6f, 28.6f), new Color(0.48f, 0.28f, 0.86f, 1f), 1.35f, 13f);
            CreatePointLight(lightsRoot.transform, "AshRoadEmberLight", new Vector3(24f, 2.5f, 5.5f), new Color(0.9f, 0.25f, 0.12f, 1f), 0.95f, 11f);
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

        private static void CreateWorldRoot()
        {
            var root = new GameObject("VelemarWorldRoot");
            root.transform.position = Vector3.zero;

            CreateGround(root.transform);
            CreateRoadNetwork(root.transform);
            CreateVillageDistrict(root.transform);
            CreateForestDistrict(root.transform);
            CreateSwampDistrict(root.transform);
            CreateAshRoadDistrict(root.transform);
            CreateTowerVistaDistrict(root.transform);
            CreateGameplayObjects(root.transform);
            CreateWorldBoundary(root.transform);
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "VelemarWorldTerrain";
            ground.transform.SetParent(parent, true);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(42f, 1f, 42f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/VelemarWorldTerrain.mat", new Color(0.13f, 0.2f, 0.13f, 1f));
        }

        private static void CreateRoadNetwork(Transform parent)
        {
            var root = new GameObject("VelemarRoadNetwork");
            root.transform.SetParent(parent, false);

            for (var z = -30; z <= 30; z += 6)
            {
                PlaceKenney(root.transform, $"VelemarNorthSouthRoad_{z}", "road.fbx", new Vector3(0f, 0.035f, z), Quaternion.identity, new Vector3(1.25f, 1f, 1.25f));
            }

            for (var x = -30; x <= 30; x += 6)
            {
                PlaceKenney(root.transform, $"VelemarEastWestRoad_{x}", "road.fbx", new Vector3(x, 0.04f, 0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.25f, 1f, 1.25f));
            }

            CreateZoneLabel(root.transform, "RoadSign_Village", "Vereskovy Brod", new Vector3(-4.5f, 1.2f, -5.5f), Quaternion.Euler(0f, 28f, 0f));
            CreateZoneLabel(root.transform, "RoadSign_Forest", "Old Forest", new Vector3(-18f, 1.2f, 3.5f), Quaternion.Euler(0f, 70f, 0f));
            CreateZoneLabel(root.transform, "RoadSign_Swamp", "Black Swamp", new Vector3(4.6f, 1.2f, -18f), Quaternion.Euler(0f, -28f, 0f));
            CreateZoneLabel(root.transform, "RoadSign_AshRoad", "Ash Road", new Vector3(20f, 1.2f, 3.5f), Quaternion.Euler(0f, -70f, 0f));
        }

        private static void CreateVillageDistrict(Transform parent)
        {
            var root = new GameObject("VillageDistrict_VereskovyBrod");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 0f, -3f);

            CreateRegionDisc(root.transform, "VillageDistrictGround", Vector3.zero, new Vector3(9f, 0.035f, 7f), new Color(0.17f, 0.23f, 0.14f, 1f));

            CreateHouse(root.transform, "ElderHouse_World", new Vector3(-3.8f, 0f, -0.2f), Quaternion.Euler(0f, 18f, 0f), 1.15f);
            CreateHouse(root.transform, "MartaHouse_World", new Vector3(3.8f, 0f, -0.4f), Quaternion.Euler(0f, -18f, 0f), 1.05f);
            CreateHouse(root.transform, "Smithy_World", new Vector3(-2.2f, 0f, 3.4f), Quaternion.Euler(0f, -25f, 0f), 0.95f);
            PlaceKayKit(root.transform, "VillageKayKitHouse_A", "house.fbx", new Vector3(-6.4f, 0f, 1.9f), Quaternion.Euler(0f, 58f, 0f), new Vector3(1.35f, 1.35f, 1.35f));
            PlaceKayKit(root.transform, "VillageKayKitMarket_World", "market.fbx", new Vector3(2.2f, 0f, 2.0f), Quaternion.Euler(0f, -22f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
            PlaceKayKit(root.transform, "VillageKayKitWell_World", "well.fbx", new Vector3(0f, 0f, -1.35f), Quaternion.identity, new Vector3(0.95f, 0.95f, 0.95f));
            PlaceKayKit(root.transform, "VillageKayKitWatermill_World", "watermill.fbx", new Vector3(6.6f, 0f, 2.7f), Quaternion.Euler(0f, -50f, 0f), new Vector3(1.05f, 1.05f, 1.05f));
            PlaceKayKit(root.transform, "VillageKayKitBarracks_World", "barracks.fbx", new Vector3(-6.8f, 0f, -2.8f), Quaternion.Euler(0f, 28f, 0f), new Vector3(0.95f, 0.95f, 0.95f));
            PlaceKenney(root.transform, "VillageCart_World", "cart.fbx", new Vector3(1.8f, 0f, 2.8f), Quaternion.Euler(0f, 35f, 0f), Vector3.one);
            PlaceKenney(root.transform, "VillageLantern_World", "lantern.fbx", new Vector3(-0.9f, 0f, 1.7f), Quaternion.identity, Vector3.one);

            for (var i = 0; i < 7; i++)
            {
                PlaceKenney(root.transform, $"VillageFence_World_{i + 1}", i % 3 == 0 ? "fence-broken.fbx" : "fence.fbx", new Vector3(-6f + i * 2f, 0f, -4.2f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            }

            for (var i = 0; i < 5; i++)
            {
                PlaceKayKit(root.transform, $"VillageWallSegment_World_{i + 1}", "wall_straight.fbx", new Vector3(-5.6f + i * 2.8f, 0f, -5.5f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.1f);
            }

            PlaceKayKit(root.transform, "VillageGate_World", "wall_gate.fbx", new Vector3(0f, 0f, -5.55f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.2f);
        }

        private static void CreateForestDistrict(Transform parent)
        {
            var root = new GameObject("ForestDistrict_OldForest");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(-24f, 0f, 4f);

            CreateRegionDisc(root.transform, "ForestDistrictGround", Vector3.zero, new Vector3(13f, 0.035f, 11f), new Color(0.08f, 0.16f, 0.08f, 1f));

            for (var i = 0; i < 32; i++)
            {
                var x = -8f + (i % 8) * 2.15f + (i % 2) * 0.45f;
                var z = -5f + (i / 8) * 3.0f + (i % 3) * 0.35f;
                var tree = i % 4 == 0 ? "tree-crooked.fbx" : i % 3 == 0 ? "tree-high.fbx" : "tree.fbx";
                PlaceKenney(root.transform, $"OldForestTree_{i + 1:00}", tree, new Vector3(x, 0f, z), Quaternion.Euler(0f, i * 23f, 0f), Vector3.one * (0.95f + (i % 3) * 0.12f));
            }

            PlaceKenney(root.transform, "HunterCampCart_World", "cart-high.fbx", new Vector3(2.6f, 0f, -2.4f), Quaternion.Euler(0f, -42f, 0f), Vector3.one);
            PlaceKenney(root.transform, "HunterCampRock_World", "rock-large.fbx", new Vector3(4.5f, 0f, -1f), Quaternion.Euler(0f, 16f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "OldForestKayKitForestPatch_A", "forest.fbx", new Vector3(-4.5f, 0f, 5.5f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 1.15f);
            PlaceKayKit(root.transform, "OldForestKayKitRocks_A", "detail_rocks.fbx", new Vector3(5.9f, 0f, 3.1f), Quaternion.Euler(0f, -18f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "OldForestKayKitHill_A", "detail_hill.fbx", new Vector3(-7.2f, 0f, -4.6f), Quaternion.Euler(0f, 42f, 0f), Vector3.one);
            CreateMarker(root.transform, "HunterCampFire_World", new Vector3(1.2f, 0.12f, -1.2f), new Vector3(0.9f, 0.18f, 0.9f), new Color(0.42f, 0.18f, 0.08f, 1f));
        }

        private static void CreateSwampDistrict(Transform parent)
        {
            var root = new GameObject("SwampDistrict_BlackSwamp");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(4f, 0f, -25f);

            CreateRegionDisc(root.transform, "SwampDistrictBog", Vector3.zero, new Vector3(13f, 0.035f, 10f), new Color(0.06f, 0.12f, 0.09f, 1f));
            CreateRegionDisc(root.transform, "SwampWaterPool_01", new Vector3(-3.5f, 0.035f, -1.4f), new Vector3(3.6f, 0.022f, 1.7f), new Color(0.025f, 0.055f, 0.05f, 1f));
            CreateRegionDisc(root.transform, "SwampWaterPool_02", new Vector3(2.8f, 0.035f, 2.1f), new Vector3(3.1f, 0.022f, 1.45f), new Color(0.02f, 0.05f, 0.045f, 1f));

            PlaceKenney(root.transform, "ElsaHutPlaceholder_World", "stall.fbx", new Vector3(5.6f, 0f, -1.6f), Quaternion.Euler(0f, -22f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
            PlaceKayKit(root.transform, "SwampKayKitRoofedBridge_World", "bridge_roofed.fbx", new Vector3(-0.4f, 0f, 5.6f), Quaternion.Euler(0f, 8f, 0f), new Vector3(1.35f, 1.35f, 1.35f));
            PlaceKayKit(root.transform, "SwampKayKitMine_World", "mine.fbx", new Vector3(7.4f, 0f, 3.0f), Quaternion.Euler(0f, -60f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "SwampKayKitRocks_A", "detail_rocks_small.fbx", new Vector3(-6.6f, 0f, -0.4f), Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.25f);
            PlaceKenney(root.transform, "DeadSwampTree_World", "tree-crooked.fbx", new Vector3(-5.2f, 0f, 2.8f), Quaternion.Euler(0f, 36f, 0f), new Vector3(1.3f, 1.1f, 1.3f));

            for (var i = 0; i < 12; i++)
            {
                CreateReedCluster(root.transform, $"SwampReedCluster_World_{i + 1:00}", new Vector3(-6f + i * 1.1f, 0f, -4.2f + (i % 4) * 1.8f), 0.8f + (i % 3) * 0.16f);
            }
        }

        private static void CreateAshRoadDistrict(Transform parent)
        {
            var root = new GameObject("AshRoadDistrict_PepelnyTrakt");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(24f, 0f, 4f);

            CreateRegionDisc(root.transform, "AshRoadDistrictGround", Vector3.zero, new Vector3(12f, 0.035f, 7f), new Color(0.15f, 0.13f, 0.12f, 1f));
            CreateMarker(root.transform, "AshRoadBurnedTrack_World", new Vector3(0f, 0.08f, 0f), new Vector3(2.5f, 0.08f, 9.5f), new Color(0.07f, 0.065f, 0.06f, 1f));

            for (var i = 0; i < 8; i++)
            {
                CreateMarker(root.transform, $"AshRoadBrokenPost_World_{i + 1}", new Vector3(-3.8f + i * 1.1f, 0.65f, 3f + (i % 2) * 1.0f), new Vector3(0.18f, 1.15f, 0.18f), new Color(0.16f, 0.09f, 0.055f, 1f));
            }

            PlaceKenney(root.transform, "AshRoadBrokenWall_World", "wall-broken.fbx", new Vector3(3.6f, 0f, -1.8f), Quaternion.Euler(0f, 24f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "AshRoadKayKitWatchtower_Burned", "watchtower.fbx", new Vector3(-5.4f, 0f, -1.4f), Quaternion.Euler(0f, 34f, 0f), Vector3.one * 0.95f);
            PlaceKayKit(root.transform, "AshRoadKayKitGateRuins", "wall_gate_closed.fbx", new Vector3(3.1f, 0f, 2.6f), Quaternion.Euler(0f, -35f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "AshRoadKayKitMountain_Backdrop", "mountain.fbx", new Vector3(7.8f, 0f, 0.4f), Quaternion.Euler(0f, 74f, 0f), new Vector3(1.6f, 1.0f, 1.6f));
        }

        private static void CreateTowerVistaDistrict(Transform parent)
        {
            var root = new GameObject("TowerVistaDistrict_Ruins");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 0f, 26f);

            CreateRegionDisc(root.transform, "TowerVistaGround", Vector3.zero, new Vector3(9f, 0.035f, 7f), new Color(0.12f, 0.12f, 0.13f, 1f));
            PlaceKayKit(root.transform, "TowerKayKitCastleCore_World", "castle.fbx", new Vector3(0f, 0f, 1.2f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.35f, 1.35f, 1.35f));
            PlaceKayKit(root.transform, "TowerKayKitWatchtowerLeft_World", "watchtower.fbx", new Vector3(-5.2f, 0f, 1.3f), Quaternion.Euler(0f, -30f, 0f), Vector3.one);
            PlaceKayKit(root.transform, "TowerKayKitWatchtowerRight_World", "watchtower.fbx", new Vector3(5.2f, 0f, 1.3f), Quaternion.Euler(0f, 30f, 0f), Vector3.one);
            PlaceKenney(root.transform, "TowerVistaStairs_World", "stairs-stone.fbx", new Vector3(0f, 0f, -2.5f), Quaternion.identity, new Vector3(1.3f, 1.3f, 1.3f));
            PlaceKenney(root.transform, "TowerVistaPillarLeft_World", "pillar-stone.fbx", new Vector3(-2.8f, 0f, 0.3f), Quaternion.identity, new Vector3(1.4f, 1.4f, 1.4f));
            PlaceKenney(root.transform, "TowerVistaPillarRight_World", "pillar-stone.fbx", new Vector3(2.8f, 0f, 0.3f), Quaternion.identity, new Vector3(1.4f, 1.4f, 1.4f));
            PlaceKenney(root.transform, "TowerVistaBrokenWall_World", "wall-broken.fbx", new Vector3(0f, 0f, 1.8f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.4f, 1.4f, 1.4f));
            CreateMarker(root.transform, "TowerMirrorGlow_World", new Vector3(0f, 1.4f, 2.6f), new Vector3(0.28f, 2.1f, 0.9f), new Color(0.34f, 0.24f, 0.48f, 1f));
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
            CreateTowerSkeletonGuards(root.transform);
            CreateWorldCraftingObjects(root.transform);
            CreateWorldForestQuestObjects(root.transform);
            CreateWorldStoryEvidenceObjects(root.transform);
            CreateWorldFinalAltars(root.transform);
        }

        private static void CreateElderDialogue(Transform parent)
        {
            var elder = CreateRpgCharacterAnchor(parent, "ElderVoytsekh_World", "Monk.fbx", new Vector3(-4.1f, 1f, -3.4f), Quaternion.Euler(0f, 145f, 0f), new Vector3(0.88f, 0.88f, 0.88f), new Color(0.22f, 0.18f, 0.13f, 1f));
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
            var boris = CreateRpgCharacterAnchor(parent, "BorisSmith_World", "Warrior.fbx", new Vector3(-3.1f, 1f, 0.7f), Quaternion.Euler(0f, 55f, 0f), new Vector3(0.84f, 0.84f, 0.84f), new Color(0.18f, 0.14f, 0.1f, 1f));
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
            var radek = CreateRpgCharacterAnchor(parent, "RadekTrader_World", "Rogue.fbx", new Vector3(2.4f, 1f, -0.1f), Quaternion.Euler(0f, -120f, 0f), new Vector3(0.82f, 0.82f, 0.82f), new Color(0.2f, 0.16f, 0.09f, 1f));
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
            var elsa = CreateRpgCharacterAnchor(parent, "ElsaCherntravka_World", "Wizard.fbx", new Vector3(12.6f, 1f, -25.6f), Quaternion.Euler(0f, -35f, 0f), new Vector3(0.78f, 0.78f, 0.78f), new Color(0.12f, 0.1f, 0.16f, 1f));
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
            var orten = CreateRpgCharacterAnchor(parent, "OrtenMirrorMage_World", "Wizard.fbx", new Vector3(0f, 1f, 30.2f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.88f, 0.88f, 0.88f), new Color(0.24f, 0.18f, 0.32f, 1f));
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
            var ivar = CreateRpgCharacterAnchor(parent, "IvarSedoy_World", "Ranger.fbx", new Vector3(-21.7f, 1f, 3.2f), Quaternion.Euler(0f, 60f, 0f), new Vector3(0.84f, 0.84f, 0.84f), new Color(0.19f, 0.2f, 0.14f, 1f));
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
            var ghost = CreateRpgCharacterAnchor(parent, "GhostGirl_World", "Wizard.fbx", new Vector3(-2.7f, 1f, 27.0f), Quaternion.Euler(0f, 165f, 0f), new Vector3(0.62f, 0.62f, 0.62f), new Color(0.44f, 0.42f, 0.72f, 1f));
            ApplyMaterialToChildRenderers(ghost, CreateMaterial("Assets/Materials/GhostGirl_World_Spectral.mat", new Color(0.48f, 0.58f, 0.9f, 0.82f)));
            CreatePointLight(parent, "GhostGirlColdLight", new Vector3(-2.7f, 2.0f, 27.0f), new Color(0.34f, 0.48f, 0.88f, 1f), 1.05f, 6.5f);
            CreateMarker(parent, "GhostGirlMemoryRing", new Vector3(-2.7f, 0.08f, 27.0f), new Vector3(1.05f, 0.035f, 1.05f), new Color(0.22f, 0.28f, 0.52f, 1f));

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
            CreateQuestMarker(parent, "WorldTrace_ClawMarks", "Claw marks", "Inspect", QuestService.ActionSwampTracesFound, "Deep claw cuts in the mud point toward the south pool.", "Marta should explain what to look for first.", new Vector3(1.4f, 0.08f, -23.2f), new Vector3(0.9f, 0.04f, 0.55f), new Color(0.18f, 0.12f, 0.08f, 1f));
            CreateQuestMarker(parent, "WorldTrace_SlimeTrail", "Black slime trail", "Inspect", QuestService.ActionSwampTracesFound, "The slime bubbles like something alive. The trail bends toward the drowned reeds.", "The slime looks wrong, but Reynard needs Marta's warning first.", new Vector3(5.5f, 0.08f, -26.2f), new Vector3(0.55f, 0.04f, 1.1f), new Color(0.08f, 0.16f, 0.11f, 1f));
            CreateQuestMarker(parent, "WorldTrace_TornCloth", "Torn cloth", "Inspect", QuestService.ActionSwampTracesFound, "A strip of wet cloth hangs from the reeds. Someone was dragged deeper.", "This cloth is just a rag until Reynard knows the swamp signs.", new Vector3(8.0f, 0.08f, -23.8f), new Vector3(0.45f, 0.04f, 0.45f), new Color(0.32f, 0.29f, 0.22f, 1f));
        }

        private static void CreateWorldDrowner(Transform parent)
        {
            var drowner = CreateCapsule(parent, "WorldDrowner_Prototype", new Vector3(8.5f, 1f, -28.4f), new Vector3(0.9f, 0.85f, 0.9f), new Color(0.08f, 0.18f, 0.14f, 1f));
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

            var health = drowner.AddComponent<Health>();
            health.Configure("World drowner", 72f);
            var ai = drowner.AddComponent<EnemyAI>();
            ai.Configure("Drowner", true, "killedFirstDrowner", QuestService.ActionFirstDrownerKilled);
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

            CreateWorldNestDrowner(parent, "WorldDrownerNestEnemy_01", new Vector3(10.6f, 1f, -29.8f), Quaternion.Euler(0f, -120f, 0f), "DrownerNestEnemy01Killed");
            CreateWorldNestDrowner(parent, "WorldDrownerNestEnemy_02", new Vector3(11.8f, 1f, -26.5f), Quaternion.Euler(0f, -155f, 0f), "DrownerNestEnemy02Killed");
            CreateWorldNestDrowner(parent, "WorldDrownerNestEnemy_03", new Vector3(7.0f, 1f, -31.2f), Quaternion.Euler(0f, -80f, 0f), "DrownerNestEnemy03Killed");
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

            var health = drowner.AddComponent<Health>();
            health.Configure("Nest drowner", 38f);
            var ai = drowner.AddComponent<EnemyAI>();
            ai.Configure("Nest drowner", false, deathFlag, QuestService.ActionDrownerNestEnemyKilled, "drownerNestStarted", "Nest drowner is dead. Keep clearing the den.");
        }

        private static void CreateTowerSkeletonGuards(Transform parent)
        {
            CreateSkeletonGuard(parent, "TowerSkeletonGuard_Left", new Vector3(-4.5f, 1f, 27.5f), Quaternion.Euler(0f, 35f, 0f));
            CreateSkeletonGuard(parent, "TowerSkeletonGuard_Right", new Vector3(4.5f, 1f, 27.5f), Quaternion.Euler(0f, -35f, 0f));
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

            var skeleton = InstantiateModel($"{MonsterPath}/Skeleton.fbx", $"{objectName}_Model", guard.transform, new Vector3(0f, -0.92f, 0f), Quaternion.identity, new Vector3(0.92f, 0.92f, 0.92f));
            if (skeleton == null && renderer != null)
            {
                renderer.enabled = true;
            }

            var health = guard.AddComponent<Health>();
            health.Configure("Tower skeleton guard", 62f);
            var ai = guard.AddComponent<EnemyAI>();
            ai.Configure("Skeleton guard", false, objectName + "_Defeated", "");
        }

        private static void CreateWorldCraftingObjects(Transform parent)
        {
            CreateSupplyCrate(parent, "WorldMartaHerbBasket", "Marta's herb basket", "Take herbs", new[] { "Swallow Grass", "Field Ration", "Bogweed" }, "Resources gained: Swallow Grass, Field Ration, Bogweed.", new Vector3(5.5f, 0.2f, -4.7f), new Vector3(0.5f, 0.25f, 0.5f), new Color(0.13f, 0.28f, 0.12f, 1f));
            CreateSupplyCrate(parent, "WorldForgeSupplies", "Forge supplies", "Take supplies", new[] { "Iron Ore", "Wolf Pelt", "Drowner Slime" }, "Resources gained: Iron Ore, Wolf Pelt, Drowner Slime.", new Vector3(-2.1f, 0.2f, 1.1f), new Vector3(0.6f, 0.3f, 0.5f), new Color(0.25f, 0.19f, 0.12f, 1f));
            CreateCraftingStation(parent, "WorldAlchemyTable_Swallow", "Alchemy table: Swallow", "Craft", "swallow", new Vector3(5.1f, 0.35f, -5.7f), new Vector3(0.95f, 0.2f, 0.58f), new Color(0.11f, 0.24f, 0.16f, 1f));
            CreateCraftingStation(parent, "WorldAlchemyTable_Antitoxin", "Alchemy table: Antitoxin", "Craft", "antitoxin", new Vector3(5.1f, 0.62f, -5.7f), new Vector3(0.7f, 0.08f, 0.42f), new Color(0.08f, 0.36f, 0.26f, 1f));
            CreateCraftingStation(parent, "WorldForge_ReinforcedArmor", "Boris's forge: Reinforced Armor", "Craft", "reinforced_armor", new Vector3(-2.5f, 0.55f, 0.55f), new Vector3(0.85f, 0.32f, 0.58f), new Color(0.26f, 0.16f, 0.1f, 1f));
        }

        private static void CreateWorldForestQuestObjects(Transform parent)
        {
            CreateQuestMarker(parent, "WorldHunterCamp_Start", "Abandoned hunter camp", "Inspect", QuestService.ActionStartMissingHunter, "The camp is torn open. Someone fled west into the trees.", "The camp waits in silence.", new Vector3(-21.6f, 0.12f, 1.8f), new Vector3(1.2f, 0.12f, 1.2f), new Color(0.24f, 0.15f, 0.08f, 1f), true);
            CreateQuestMarker(parent, "WorldHunterClue_BloodTrail", "Blood trail", "Inspect", QuestService.ActionMissingHunterClueFound, "Fresh blood marks the moss.", "The trail makes more sense after the camp is inspected.", new Vector3(-25.5f, 0.08f, 4.2f), new Vector3(0.7f, 0.04f, 1.0f), new Color(0.3f, 0.04f, 0.03f, 1f));
            CreateQuestMarker(parent, "WorldHunterClue_BrokenKnife", "Broken knife", "Inspect", QuestService.ActionMissingHunterClueFound, "The blade snapped against bone or old iron.", "A broken knife, but not yet a story.", new Vector3(-28.2f, 0.1f, 8.4f), new Vector3(0.45f, 0.05f, 0.55f), new Color(0.36f, 0.34f, 0.3f, 1f));
            CreateQuestMarker(parent, "WorldHunterCamp_RewardPouch", "Hunter's hidden pouch", "Take reward", QuestService.ActionMissingHunterReturned, "You find coin and a note: Ivar survived the first night.", "The pouch stays hidden until the trail is understood.", new Vector3(-20.8f, 0.16f, 0.5f), new Vector3(0.4f, 0.18f, 0.4f), new Color(0.24f, 0.18f, 0.1f, 1f));
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
                new Vector3(2.5f, 0.18f, -31.4f),
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
                new Vector3(-1.4f, 0.28f, 28.4f),
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
                new Vector3(1.7f, 0.32f, 27.4f),
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
                new Vector3(5.2f, 0.28f, 24.6f),
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
                new Vector3(-2.9f, 0.45f, 26.8f),
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
                new Vector3(22.4f, 0.45f, 8.5f),
                new Color(0.23f, 0.2f, 0.27f, 1f));
            CreateEndingAltarSilhouette(
                parent,
                "WorldFinalTruthSilhouette",
                new Vector3(22.4f, 0f, 8.5f),
                new Color(0.88f, 0.72f, 0.36f, 1f),
                EndingType.Truth);

            CreateEndingAltar(
                parent,
                "WorldFinalLieAltar",
                "World final corrected-story altar",
                "Choose lie",
                EndingService.LieEndingType,
                new Vector3(24.4f, 0.45f, 7.4f),
                new Color(0.26f, 0.19f, 0.11f, 1f));
            CreateEndingAltarSilhouette(
                parent,
                "WorldFinalLieSilhouette",
                new Vector3(24.4f, 0f, 7.4f),
                new Color(0.72f, 0.43f, 0.17f, 1f),
                EndingType.Lie);

            CreateEndingAltar(
                parent,
                "WorldFinalSacrificeAltar",
                "World final sacrifice altar",
                "Choose sacrifice",
                EndingService.SacrificeEndingType,
                new Vector3(24.4f, 0.45f, 9.7f),
                new Color(0.19f, 0.1f, 0.12f, 1f));
            CreateEndingAltarSilhouette(
                parent,
                "WorldFinalSacrificeSilhouette",
                new Vector3(24.4f, 0f, 9.7f),
                new Color(0.72f, 0.12f, 0.08f, 1f),
                EndingType.Sacrifice);
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

            for (var i = 0; i < 24; i++)
            {
                var angle = i * Mathf.PI * 2f / 24f;
                var radius = 38f + (i % 3) * 1.2f;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                PlaceKenney(root.transform, $"BoundaryTree_{i + 1:00}", i % 2 == 0 ? "tree-high-crooked.fbx" : "tree-high.fbx", position, Quaternion.Euler(0f, i * 31f, 0f), Vector3.one * 1.35f);
            }
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Reynard_Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.08f, -8f);

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
            CreatePlayerVisual(player.transform);
            return player;
        }

        private static void CreatePlayerVisual(Transform player)
        {
            var knight = InstantiateModel($"{KnightPath}/KnightCharacter.fbx", "ReynardKnightModel", player, new Vector3(0f, -0.02f, 0f), Quaternion.Euler(0f, 180f, 0f), Vector3.one);
            if (knight == null)
            {
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.name = "ReynardFallbackCapsule";
                fallback.transform.SetParent(player, false);
                fallback.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                fallback.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ReynardPlaceholder.mat", new Color(0.22f, 0.25f, 0.23f, 1f));
                Object.DestroyImmediate(fallback.GetComponent<Collider>());
                return;
            }

            InstantiateModel($"{KnightPath}/Sword.fbx", "ReynardSteelSword_Visual", player, new Vector3(-0.32f, 1.2f, -0.18f), Quaternion.Euler(65f, 0f, 25f), Vector3.one * 0.9f);
            InstantiateModel($"{KnightPath}/ShortSword.fbx", "ReynardSilverSword_Visual", player, new Vector3(0.32f, 1.12f, -0.18f), Quaternion.Euler(65f, 0f, -25f), Vector3.one * 0.9f);
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("ThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -14f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 650f;

            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<ThirdPersonCamera>();
            follow.Target = target;
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
            CreateText(panel.transform, "WorldDirectionText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-42f, -24f), Vector2.zero, 19, TextAnchor.MiddleLeft, new Color(0.93f, 0.88f, 0.74f, 1f))
                .text = "Semi-open Velemar prototype: village center, forest west, swamp south, ash road east, tower ruins north.";
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

            var hudRoot = CreatePanel(canvasObject.transform, "HealthHudPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 92f), new Vector2(44f, -44f), new Color(0.045f, 0.04f, 0.032f, 0.88f));
            var barBack = CreatePanel(hudRoot.transform, "HealthBarBack", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-44f, 22f), new Vector2(0f, 20f), new Color(0.14f, 0.05f, 0.04f, 1f));
            var barFillObject = CreatePanel(barBack.transform, "HealthBarFill", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.68f, 0.11f, 0.08f, 1f));
            var fillImage = barFillObject.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;
            var healthText = CreateText(hudRoot.transform, "HealthHudText", new Vector2(0f, 0.48f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-44f, -14f), new Vector2(0f, -10f), 21, TextAnchor.MiddleLeft, new Color(0.96f, 0.88f, 0.77f, 1f));

            var hud = canvasObject.AddComponent<HealthHudUI>();
            SetSerializedObjectReference(hud, "healthText", healthText);
            SetSerializedObjectReference(hud, "healthFill", fillImage);
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

            PlaceKenney(house.transform, $"{name}_WallA", "wall-door.fbx", new Vector3(0f, 0f, -0.8f), Quaternion.identity, Vector3.one);
            PlaceKenney(house.transform, $"{name}_WallB", "wall.fbx", new Vector3(-1.1f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceKenney(house.transform, $"{name}_WallC", "wall.fbx", new Vector3(1.1f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceKenney(house.transform, $"{name}_Roof", "roof-high.fbx", new Vector3(0f, 1.2f, 0f), Quaternion.identity, Vector3.one);
        }

        private static GameObject CreateCapsule(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = name;
            capsule.transform.SetParent(parent, false);
            capsule.transform.localPosition = position;
            capsule.transform.localScale = scale;
            capsule.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
            return capsule;
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

            var visual = InstantiateModel($"{RpgCharacterPath}/{modelName}", $"{name}_Model", anchor.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, visualScale);
            if (visual == null)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                    renderer.sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", fallbackColor);
                }
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

        private static void CreateQuestMarker(Transform parent, string objectName, string displayName, string prompt, string questAction, string successMessage, string blockedMessage, Vector3 position, Vector3 scale, Color color, bool canRepeat = false)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = scale;
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);

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

            var interactable = crate.AddComponent<InventoryGrantInteractable>();
            interactable.Configure(displayName, prompt, itemsToGrant, message);
        }

        private static void CreateCraftingStation(Transform parent, string objectName, string displayName, string prompt, string recipeId, Vector3 position, Vector3 scale, Color color)
        {
            var station = GameObject.CreatePrimitive(PrimitiveType.Cube);
            station.name = objectName;
            station.transform.SetParent(parent, false);
            station.transform.localPosition = position;
            station.transform.localScale = scale;
            station.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);

            var interactable = station.AddComponent<CraftingInteractable>();
            interactable.Configure(displayName, prompt, recipeId);
        }

        private static void PlaceKenney(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var instance = InstantiateModel($"{KenneyPath}/{modelName}", objectName, parent, position, rotation, scale);
            if (instance != null)
            {
                return;
            }

            CreateMarker(parent, objectName + "_Fallback", position + Vector3.up * 0.35f, new Vector3(0.8f, 0.7f, 0.8f), new Color(0.28f, 0.22f, 0.14f, 1f));
        }

        private static void PlaceKayKit(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var instance = InstantiateModel($"{KayKitMedievalPath}/{modelName}", objectName, parent, position, rotation, scale);
            if (instance != null)
            {
                return;
            }

            CreateMarker(parent, objectName + "_Fallback", position + Vector3.up * 0.4f, new Vector3(0.9f, 0.8f, 0.9f), new Color(0.22f, 0.2f, 0.16f, 1f));
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
            label.text = text;
            label.fontSize = 38;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(0.9f, 0.78f, 0.48f, 1f);
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
    }
}

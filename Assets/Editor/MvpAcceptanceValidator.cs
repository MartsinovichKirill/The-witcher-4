using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    public static class MvpAcceptanceValidator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string VillageScenePath = "Assets/Scenes/VillageScene.unity";
        private const string ForestScenePath = "Assets/Scenes/ForestScene.unity";
        private const string AshRoadScenePath = "Assets/Scenes/AshRoadScene.unity";
        private const string VelemarWorldScenePath = "Assets/Scenes/VelemarWorldScene.unity";

        [MenuItem("Tools/Witcher Right Version/Validate MVP Acceptance")]
        public static void Validate()
        {
            VerticalSliceValidator.Validate();

            var failures = new List<string>();
            ValidateSceneAssets(failures);
            ValidateMainMenuAcceptance(failures);
            ValidateVillageAcceptance(failures);
            ValidateForestAcceptance(failures);
            ValidateAshRoadAcceptance(failures);
            ValidateVelemarWorldAcceptance(failures);
            ValidateEditorBuildPipeline(failures);

            if (failures.Count > 0)
            {
                var message = "MVP acceptance validation failed:\n- " + string.Join("\n- ", failures);
                Debug.LogError(message);
                throw new InvalidOperationException(message);
            }

            Debug.Log("MVP acceptance validation passed.");
            Debug.Log("MVP checklist: main menu, 2 playable zones, 3 quests, combat, dialogue, inventory, crafting, XP, save/load, scene transfer, Truth ending, ending UI, and Windows build pipeline are present.");
        }

        private static void ValidateSceneAssets(List<string> failures)
        {
            Require(File.Exists(MainMenuScenePath), failures, "MainMenuScene asset must exist.");
            Require(File.Exists(VillageScenePath), failures, "VillageScene asset must exist.");
            Require(File.Exists(ForestScenePath), failures, "ForestScene asset must exist.");
            Require(File.Exists(AshRoadScenePath), failures, "AshRoadScene asset must exist.");
            Require(File.Exists(VelemarWorldScenePath), failures, "VelemarWorldScene asset must exist.");

            RequireBuildScene(MainMenuScenePath, failures);
            RequireBuildScene(VillageScenePath, failures);
            RequireBuildScene(ForestScenePath, failures);
            RequireBuildScene(AshRoadScenePath, failures);
            RequireBuildScene(VelemarWorldScenePath, failures);
        }

        private static void ValidateMainMenuAcceptance(List<string> failures)
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            RequireObject<MainMenuController>("MainMenuController", failures);
            RequireObject("NewGameButton", failures);
            RequireObject("ContinueButton", failures);
            RequireObject("SettingsButton", failures);
            RequireObject("LanguageDropdown", failures);
        }

        private static void ValidateVillageAcceptance(List<string> failures)
        {
            EditorSceneManager.OpenScene(VillageScenePath, OpenSceneMode.Single);

            ValidatePlayer("Reynard_Player", failures);
            ValidateRuntimeServices("RuntimeServices", failures);

            RequireObject<DialogueService>("DialogueCanvas", failures);
            RequireObject<QuestHudUI>("QuestCanvas", failures);
            RequireObject<HealthHudUI>("HealthCanvas", failures);
            RequireObject<InventoryHudUI>("InventoryCanvas", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);

            RequireObject<DialogueInteractable>("ElderVoytsekh_Prototype", failures);
            RequireObject<DialogueInteractable>("MartaLozovaya_Prototype", failures);
            RequireObject<QuestProgressInteractable>("BorisSmithQuest_Start", failures);
            RequireObject<QuestProgressInteractable>("BorisSmithQuest_Return", failures);
            RequireObject<CraftingInteractable>("AlchemyTable_Swallow", failures);
            RequireObject<CraftingInteractable>("AlchemyTable_Antitoxin", failures);
            RequireObject<CraftingInteractable>("Forge_ReinforcedArmor", failures);
            RequireObject<EndingGateInteractable>("AshRoadFinalPath", failures);

            var drowner = RequireObject("FirstDrowner_Prototype", failures);
            if (drowner != null)
            {
                RequireComponent<Health>(drowner, failures, "FirstDrowner_Prototype");
                RequireComponent<EnemyAI>(drowner, failures, "FirstDrowner_Prototype");
            }
        }

        private static void ValidateForestAcceptance(List<string> failures)
        {
            EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Single);

            ValidatePlayer("Reynard_Player", failures);
            ValidateRuntimeServices("RuntimeServices", failures);
            RequireObject<SceneTransitionInteractable>("VillagePathTransition", failures);
            RequireObject<QuestProgressInteractable>("HunterCamp_Start", failures);
            RequireObject<QuestProgressInteractable>("HunterClue_BloodTrail", failures);
            RequireObject<QuestProgressInteractable>("HunterClue_BrokenKnife", failures);
            RequireObject<QuestProgressInteractable>("HunterCamp_RewardPouch", failures);
            RequireObject<QuestProgressInteractable>("OldCampBlade", failures);
            RequireObject<InventoryHudUI>("InventoryCanvas", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
        }

        private static void ValidateAshRoadAcceptance(List<string> failures)
        {
            EditorSceneManager.OpenScene(AshRoadScenePath, OpenSceneMode.Single);

            ValidatePlayer("Reynard_Player", failures);
            ValidateRuntimeServices("RuntimeServices", failures);
            RequireObject<EndingAltarInteractable>("FinalTruthAltar", failures);
            RequireObject<EndingHudUI>("EndingCanvas", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
        }

        private static void ValidateVelemarWorldAcceptance(List<string> failures)
        {
            EditorSceneManager.OpenScene(VelemarWorldScenePath, OpenSceneMode.Single);

            ValidatePlayer("Reynard_Player", failures);
            ValidateRuntimeServices("RuntimeServices", failures);
            Require(File.ReadAllText(VelemarWorldScenePath).Contains(QuestService.ActionStartExile), failures, "Elsa dialogue must start the Exile quest.");
            RequireObject("VelemarWorldRoot", failures);
            RequireObject("VelemarWorldTerrain", failures);
            RequireObject("VelemarTerrainVisualLayers", failures);
            RequireObject("VelemarAtmosphereLights", failures);
            RequireObject("VelemarRoadNetwork", failures);
            RequireObject("VelemarNorthSouthRoad_Main", failures);
            RequireObject("VelemarEastWestRoad_Main", failures);
            RequireObject("VelemarCrossroadsPackedMud", failures);
            RequireObject("VillagePackedEarthBlend", failures);
            RequireObject("VillageGateTrampledGround", failures);
            RequireObject("InternetAssetMapExtensions_CC0", failures);
            RequireObject("OuterVillageRing_KayKit", failures);
            RequireObject("DeepForestMapExtension_KayKit", failures);
            RequireObject("DeepSwampMapExtension_Kenney", failures);
            RequireObject("AshRoadMapExtension_KayKit", failures);
            RequireObject("TowerMapExtension_KayKit", failures);
            RequireObject("OuterRoadSetPieces_InternetAssets", failures);
            RequireObject("MapDensityAndScalePass", failures);
            RequireObject("StructuredVillageBlocks", failures);
            RequireObject("ForestApproachDensity", failures);
            RequireObject("SwampApproachDensity", failures);
            RequireObject("AshRoadRuinedSettlement", failures);
            RequireObject("TowerApproachDensity", failures);
            RequireObject("VillageMainSquarePackedMud", failures);
            RequireObject("OuterVillageNorthStreetSpine", failures);
            RequireObject("DeepForestOutpostClearing", failures);
            RequireObject("DeepSwampMainCauseway", failures);
            RequireObject("FarAshRoadFortYard", failures);
            RequireObject("FarTowerProcessionalRoad", failures);
            RequireObject("GameplayCompositionPass", failures);
            RequireObject("VillageGameplayCompositions", failures);
            RequireObject("ForestGameplayCompositions", failures);
            RequireObject("SwampGameplayCompositions", failures);
            RequireObject("TowerGameplayCompositions", failures);
            RequireObject("FinalRoadGameplayCompositions", failures);
            RequireObject("ElderCourtyardCompositionGround", failures);
            RequireObject("MartaHerbGardenCompositionGround", failures);
            RequireObject("BorisForgeCompositionGround", failures);
            RequireObject("RadekMarketCompositionGround", failures);
            RequireObject("HunterCampCompositionClearing", failures);
            RequireObject("ElsaHutCompositionGround", failures);
            RequireObject("TowerEvidenceCompositionFloor", failures);
            RequireObject("FinalChoiceTriangleGround", failures);
            RequireObject("LandmarkAndTraversalPass", failures);
            RequireObject("VillageCrossroadsLandmarks", failures);
            RequireObject("ForestTraversalGate", failures);
            RequireObject("SwampTraversalGate", failures);
            RequireObject("AshRoadTraversalGate", failures);
            RequireObject("TowerTraversalGate", failures);
            RequireObject("OuterHorizonAnchors", failures);
            RequireObject("VillageCrossroadsStoneRing", failures);
            RequireObject("ForestGateThresholdMud", failures);
            RequireObject("SwampGateRoofedBridgeAnchor", failures);
            RequireObject("AshRoadGateCollapsedGate", failures);
            RequireObject("TowerGateStoneArch", failures);
            RequireObject("HorizonNorthCastleSilhouette", failures);
            RequireObject("WorldCompositionPolishPass", failures);
            RequireObject("VillageEdgeComposition", failures);
            RequireObject("VillageOrchardGround", failures);
            RequireObject("VillageCemeteryGround", failures);
            RequireObject("ForestCanopyComposition", failures);
            RequireObject("ForestCanopyWall", failures);
            RequireObject("ForestLoggingCamp", failures);
            RequireObject("SwampBoardwalkComposition", failures);
            RequireObject("SwampBoardwalkMainLine_01", failures);
            RequireObject("SwampBoardwalkLowGreenLight", failures);
            RequireObject("AshRoadBurnedHamletComposition", failures);
            RequireObject("AshRoadBurnedHamletStreet", failures);
            RequireObject("AshRoadHamletEmberLight", failures);
            RequireObject("TowerOuterRuinsComposition", failures);
            RequireObject("TowerOuterRuinRing_A", failures);
            RequireObject("TowerOuterRuinVioletLight", failures);
            RequireObject("FullMapVisualOverhaulPass", failures);
            RequireObject("VillageStreetOverhaul", failures);
            RequireObject("VillageOverhaulGateHouseLeft", failures);
            RequireObject("ForestDepthOverhaul", failures);
            RequireObject("ForestOverhaulTreeWall_01", failures);
            RequireObject("ForestOverhaulDeepMineMouth", failures);
            RequireObject("SwampDepthOverhaul", failures);
            RequireObject("SwampOverhaulBrokenBoardwalk_01", failures);
            RequireObject("SwampOverhaulGreenFogLight", failures);
            RequireObject("AshRoadDepthOverhaul", failures);
            RequireObject("AshRoadOverhaulFortGate", failures);
            RequireObject("TowerDepthOverhaul", failures);
            RequireObject("TowerOverhaulCastleMassBack", failures);
            RequireObject("CreatureModelShowcase", failures);
            RequireObject("TowerOverhaulSkeletonDisplay", failures);
            RequireObject("SwampOverhaulBossBackdrop", failures);
            RequireObject("TerrainDepthAndSilhouettePass", failures);
            RequireObject("VillageReliefSilhouette", failures);
            RequireObject("ForestRidgeSilhouette", failures);
            RequireObject("SwampWaterMazeSilhouette", failures);
            RequireObject("AshRoadBattlefieldSilhouette", failures);
            RequireObject("TowerCliffSilhouette", failures);
            RequireObject("VillageReliefOuterDitchNorth", failures);
            RequireObject("ForestRidgeRockLine_01", failures);
            RequireObject("SwampMazeWaterPocket_01", failures);
            RequireObject("AshBattlefieldCrater_01", failures);
            RequireObject("TowerCliffRaisedNorthShelf", failures);
            RequireObject("AmbientCharacterPopulationPass", failures);
            RequireObject("VillageAmbientPopulation", failures);
            RequireObject("AmbientVillager_Market_01", failures);
            RequireObject("ForestAmbientPopulation", failures);
            RequireObject("AmbientForestHunter_01", failures);
            RequireObject("AshRoadAmbientPopulation", failures);
            RequireObject("AmbientAshRoadRefugee_01", failures);
            RequireObject("TowerAmbientPopulation", failures);
            RequireObject("AmbientTowerMemory_01", failures);
            RequireObject("VisualAtmospherePolishPass", failures);
            RequireObject("VillageCameraCompositionPolish", failures);
            RequireObject("VillagePolishMainStreetWetMud", failures);
            RequireObject("VillagePolishDistantMillSilhouette", failures);
            RequireObject("ForestDepthFogPolish", failures);
            RequireObject("ForestPolishLayeredCanopy_01", failures);
            RequireObject("ForestPolishLowMistBand_01", failures);
            RequireObject("SwampWaterAndReedPolish", failures);
            RequireObject("SwampPolishMirrorWater_Main", failures);
            RequireObject("SwampPolishTallReedCluster_01", failures);
            RequireObject("AshRoadSmokeAndRuinPolish", failures);
            RequireObject("AshRoadPolishSmokeColumn_01", failures);
            RequireObject("AshRoadPolishDeepFireGlow", failures);
            RequireObject("TowerRitualSkylinePolish", failures);
            RequireObject("TowerPolishFloatingMirrorShard_01", failures);
            RequireObject("TowerPolishFarCastleSilhouette", failures);
            RequireObject("CharacterPresentationPolishPass", failures);
            RequireObject("VillageNpcPresentationPolish", failures);
            RequireObject("ElderPresentationAuthorityMat", failures);
            RequireObject("MartaPresentationHerbCircle", failures);
            RequireObject("BorisPresentationForgeGlow", failures);
            RequireObject("RadekPresentationTradeMat", failures);
            RequireObject("SwampCharacterPresentationPolish", failures);
            RequireObject("ElsaPresentationWardMat", failures);
            RequireObject("GhostPresentationColdAura", failures);
            RequireObject("ForestCharacterPresentationPolish", failures);
            RequireObject("IvarPresentationHunterMat", failures);
            RequireObject("TowerCharacterPresentationPolish", failures);
            RequireObject("OrtenPresentationMirrorStage", failures);
            RequireObject("EnemyPresentationPolish", failures);
            RequireObject("DrownerPresentationThreatWater", failures);
            RequireObject("SkeletonPresentationGraveDust", failures);
            RequireObject("BanditPresentationAmbushDirt", failures);
            RequireObject("DynamicAmbientVfxPass", failures);
            RequireObject("VillageDynamicVfx", failures);
            RequireObject("VillageDynamicChimneySmoke_01_01", failures);
            RequireObject<AmbientVisualMotion>("VillageDynamicForgeSpark_01", failures);
            RequireObject("ForestDynamicVfx", failures);
            RequireObject<AmbientVisualMotion>("ForestDynamicMoonMote_01", failures);
            RequireObject("SwampDynamicVfx", failures);
            RequireObject<AmbientVisualMotion>("SwampDynamicWillOWisp_01", failures);
            RequireObject("AshRoadDynamicVfx", failures);
            RequireObject<AmbientVisualMotion>("AshRoadDynamicEmber_01", failures);
            RequireObject("TowerDynamicVfx", failures);
            RequireObject<AmbientVisualMotion>("TowerDynamicMirrorFragment_01", failures);
            RequireObject("OuterVillageLumbermill", failures);
            RequireObject("OuterVillageWindmill", failures);
            RequireObject("DeepForestRangerOutpost", failures);
            RequireObject("DeepSwampBossSilhouette_InternetAsset", failures);
            RequireObject("FarAshRoadGateFort", failures);
            RequireObject("FarTowerCastleBackdrop", failures);
            RequireObject("VillageDistrict_VereskovyBrod", failures);
            var forestDistrict = RequireObject("ForestDistrict_OldForest", failures);
            var swampDistrict = RequireObject("SwampDistrict_BlackSwamp", failures);
            var ashRoadDistrict = RequireObject("AshRoadDistrict_PepelnyTrakt", failures);
            var towerDistrict = RequireObject("TowerVistaDistrict_Ruins", failures);
            Require(forestDistrict == null || forestDistrict.transform.position.magnitude >= 60f, failures, "Forest district must be placed on the expanded world perimeter.");
            Require(swampDistrict == null || swampDistrict.transform.position.magnitude >= 60f, failures, "Swamp district must be placed on the expanded world perimeter.");
            Require(ashRoadDistrict == null || ashRoadDistrict.transform.position.magnitude >= 60f, failures, "Ash Road district must be placed on the expanded world perimeter.");
            Require(towerDistrict == null || towerDistrict.transform.position.magnitude >= 60f, failures, "Tower district must be placed on the expanded world perimeter.");
            RequireObject("WorldDressingRoot", failures);
            RequireObject("TravelRouteDressing", failures);
            RequireObject("WestRouteLandmark_01", failures);
            RequireObject("EastRouteLandmark_01", failures);
            RequireObject("NorthRouteLandmark_01", failures);
            RequireObject("SouthRouteLandmark_01", failures);
            RequireObject("WestRouteMudTrack_02", failures);
            RequireObject("EastRouteAshScorch_03", failures);
            RequireObject("NorthRouteMirrorDust_02", failures);
            RequireObject("SouthRouteBogPatch_03", failures);
            RequireObject("SouthRouteWetPlanks_02", failures);
            RequireObject("WestRouteLanternLight_A", failures);
            RequireObject("NorthRouteMirrorGlimmer_A", failures);
            RequireObject("SouthRouteBogGlow_A", failures);
            RequireObject("EastRouteAshGlow_A", failures);
            RequireObject("RoutePointOfInterestDressing", failures);
            RequireObject("WestRouteHunterShrine_Post", failures);
            RequireObject("EastRouteAmbushBarricade_A", failures);
            RequireObject("NorthRouteBrokenObelisk_Base", failures);
            RequireObject("SouthRouteWillOWisp_A", failures);
            RequireObject("WestRouteShrineLanternLight", failures);
            RequireObject("NorthRouteObeliskVioletLight", failures);
            RequireObject("SouthRouteWillOWispLight", failures);
            RequireObject("VillageDressingNoticeBanner", failures);
            RequireObject("ForestWolfDen_World", failures);
            RequireObject("SwampPlankPath_01", failures);
            RequireObject("SwampBossForeshadow_Model", failures);
            RequireObject("TowerRitualCircle_World", failures);
            RequireObject("AshRoadSurvivorCamp_World", failures);
            RequireObject<CombatVisualFeedback>("Reynard_Player", failures);
            RequireObject<PlayerActionVisualAnimator>("Reynard_Player", failures);
            RequireObject("ReynardCombatReadabilityRing", failures);
            RequireObject("ReynardAardFocusRing", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
            RequireObject<DialogueService>("DialogueCanvas", failures);
            RequireObject<QuestHudUI>("QuestCanvas", failures);
            RequireObject<HealthHudUI>("HealthCanvas", failures);
            RequireObject<InventoryHudUI>("InventoryCanvas", failures);
            RequireObject<EndingHudUI>("EndingCanvas", failures);
            RequireObject("WorldDirectionCanvas", failures);
            RequireObject("WorldGameplayRoot", failures);
            RequireObject("VillageKayKitMarket_World", failures);
            RequireObject("VillageKayKitWell_World", failures);
            RequireObject("VillageGate_World", failures);
            RequireObject("SwampKayKitRoofedBridge_World", failures);
            RequireObject("TowerKayKitCastleCore_World", failures);
            RequireObject<DialogueInteractable>("ElderVoytsekh_World", failures);
            RequireObject("ElderRoleRing", failures);
            RequireObject<DialogueInteractable>("MartaLozovaya_World", failures);
            RequireObject<DialogueInteractable>("BorisSmith_World", failures);
            RequireObject<MerchantInteractable>("RadekTrader_World", failures);
            RequireObject<DialogueInteractable>("ElsaCherntravka_World", failures);
            RequireObject<DialogueInteractable>("IvarSedoy_World", failures);
            RequireObject<DialogueInteractable>("GhostGirl_World", failures);
            RequireObject("GhostGirlReadabilityRing", failures);
            RequireObject("GhostGirlColdLight", failures);
            RequireObject("GhostGirlMemoryRing", failures);
            RequireObject<DialogueInteractable>("OrtenMirrorMage_World", failures);
            RequireObject<QuestProgressInteractable>("WorldTrace_ClawMarks", failures);
            RequireObject<QuestProgressInteractable>("WorldTrace_SlimeTrail", failures);
            RequireObject<QuestProgressInteractable>("WorldTrace_TornCloth", failures);
            RequireObject<QuestProgressInteractable>("WorldHunterCamp_Start", failures);
            RequireObject<QuestProgressInteractable>("WorldWellWhisper", failures);
            RequireObject<QuestProgressInteractable>("WorldDrownerNestNotice", failures);
            RequireObject<QuestProgressInteractable>("WorldDrownerNestRewardCache", failures);
            RequireObject<CraftingInteractable>("WorldAlchemyTable_Swallow", failures);
            RequireObject<CraftingInteractable>("WorldAlchemyTable_Antitoxin", failures);
            RequireObject<CraftingInteractable>("WorldForge_ReinforcedArmor", failures);
            RequireObject<DecisionFlagInteractable>("WorldGirlMedallion", failures);
            RequireObject<DecisionFlagInteractable>("WorldOrtenDiary", failures);
            RequireObject<DecisionFlagInteractable>("WorldMirrorShardCache", failures);
            RequireObject<DecisionFlagInteractable>("WorldTowerReedCharmGate", failures);
            RequireObject<QuestProgressInteractable>("WorldGhostMemory", failures);
            RequireObject<DecisionFlagInteractable>("WorldElderSealProof", failures);
            RequireObject<EndingAltarInteractable>("WorldFinalTruthAltar", failures);
            RequireObject<EndingAltarInteractable>("WorldFinalLieAltar", failures);
            RequireObject<EndingAltarInteractable>("WorldFinalSacrificeAltar", failures);
            RequireObject("WorldFinalTruthSilhouette", failures);
            RequireObject("WorldFinalLieSilhouette", failures);
            RequireObject("WorldFinalSacrificeSilhouette", failures);
            RequireObject<DialogueInteractable>("FinalElsaAlly_World", failures);
            RequireObject<FlagConditionalObject>("FinalElsaWardCircle", failures);
            RequireObject<DialogueInteractable>("FinalIvarAlly_World", failures);
            RequireObject<FlagConditionalObject>("FinalMayorControlPost", failures);
            RequireObject<FlagConditionalObject>("FinalTruthEvidenceShrine", failures);
            RequireObject<FlagConditionalObject>("FinalSacrificeDiaryShrine", failures);

            var drowner = RequireObject("WorldDrowner_Prototype", failures);
            if (drowner != null)
            {
                RequireComponent<Health>(drowner, failures, "WorldDrowner_Prototype");
                RequireComponent<EnemyAI>(drowner, failures, "WorldDrowner_Prototype");
                RequireComponent<CombatVisualFeedback>(drowner, failures, "WorldDrowner_Prototype");
            }
            RequireObject("WorldDrownerThreatRing", failures);

            ValidateEnemy("TowerSkeletonGuard_Left", failures);
            ValidateEnemy("TowerSkeletonGuard_Right", failures);
            ValidateEnemy("WorldDrownerNestEnemy_01", failures);
            ValidateEnemy("WorldDrownerNestEnemy_02", failures);
            ValidateEnemy("WorldDrownerNestEnemy_03", failures);
            ValidateLootEnemy("ForestWolf_01", failures);
            ValidateLootEnemy("ForestWolf_02", failures);
            ValidateLootEnemy("ForestWolf_03", failures);
            RequireObject("ForestWolf_01_Model", failures);
            ValidateLootEnemy("AshRoadBandit_01", failures);
            ValidateLootEnemy("AshRoadBandit_02", failures);
            ValidateLootEnemy("AshRoadBandit_03", failures);
            ValidateEnemy("FinalMayorEnforcer_01", failures);
            ValidateEnemy("FinalMayorEnforcer_02", failures);
        }

        private static void ValidateEnemy(string objectName, List<string> failures)
        {
            var enemy = RequireObject(objectName, failures);
            if (enemy == null)
            {
                return;
            }

            RequireComponent<Health>(enemy, failures, objectName);
            RequireComponent<EnemyAI>(enemy, failures, objectName);
            RequireComponent<CombatVisualFeedback>(enemy, failures, objectName);
        }

        private static void ValidateLootEnemy(string objectName, List<string> failures)
        {
            ValidateEnemy(objectName, failures);
            var enemy = FindSceneObject(objectName);
            if (enemy != null)
            {
                RequireComponent<EnemyLootDrop>(enemy, failures, objectName);
            }
        }

        private static void ValidateEditorBuildPipeline(List<string> failures)
        {
            Require(typeof(WindowsBuildBuilder) != null, failures, "WindowsBuildBuilder must exist.");
        }

        private static void ValidatePlayer(string objectName, List<string> failures)
        {
            var player = RequireObject(objectName, failures);
            if (player == null)
            {
                return;
            }

            RequireComponent<CharacterController>(player, failures, objectName);
            RequireComponent<PlayerController>(player, failures, objectName);
            RequireComponent<ThirdPersonCamera>(FindSceneObject("ThirdPersonCamera"), failures, "ThirdPersonCamera");
            RequireComponent<InteractionController>(player, failures, objectName);
            RequireComponent<Health>(player, failures, objectName);
            RequireComponent<CombatController>(player, failures, objectName);
        }

        private static void ValidateRuntimeServices(string objectName, List<string> failures)
        {
            var services = RequireObject(objectName, failures);
            if (services == null)
            {
                return;
            }

            RequireComponent<AudioFeedbackService>(services, failures, objectName);
            RequireComponent<DecisionFlagService>(services, failures, objectName);
            RequireComponent<EndingService>(services, failures, objectName);
            RequireComponent<PlayerRewardService>(services, failures, objectName);
            RequireComponent<InventoryService>(services, failures, objectName);
            RequireComponent<CraftingService>(services, failures, objectName);
            RequireComponent<QuestService>(services, failures, objectName);
            RequireComponent<SaveService>(services, failures, objectName);
        }

        private static void RequireBuildScene(string scenePath, List<string> failures)
        {
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (scene.enabled && scene.path == scenePath)
                {
                    return;
                }
            }

            failures.Add($"Build Settings must include enabled scene: {scenePath}");
        }

        private static GameObject RequireObject(string objectName, List<string> failures)
        {
            var target = FindSceneObject(objectName);
            Require(target != null, failures, $"Missing GameObject: {objectName}");
            return target;
        }

        private static void RequireObject<T>(string objectName, List<string> failures) where T : Component
        {
            var target = RequireObject(objectName, failures);
            if (target != null)
            {
                RequireComponent<T>(target, failures, objectName);
            }
        }

        private static void RequireComponent<T>(GameObject target, List<string> failures, string ownerName) where T : Component
        {
            Require(target != null && target.GetComponent<T>() != null, failures, $"{ownerName} must have {typeof(T).Name}.");
        }

        private static GameObject FindSceneObject(string objectName)
        {
            var activeObject = GameObject.Find(objectName);
            if (activeObject != null)
            {
                return activeObject;
            }

            var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (var i = 0; i < allTransforms.Length; i++)
            {
                var transform = allTransforms[i];
                if (transform != null
                    && transform.name == objectName
                    && transform.gameObject.scene.IsValid()
                    && transform.gameObject.scene.isLoaded)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static void Require(bool condition, List<string> failures, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }
    }
}

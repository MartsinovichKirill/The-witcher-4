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
            RequireObject("ClassicSliceButton", failures);
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
            RequireObject("VelemarWorldRoot", failures);
            RequireObject("VelemarWorldTerrain", failures);
            RequireObject("VelemarAtmosphereLights", failures);
            RequireObject("VelemarRoadNetwork", failures);
            RequireObject("VillageDistrict_VereskovyBrod", failures);
            RequireObject("ForestDistrict_OldForest", failures);
            RequireObject("SwampDistrict_BlackSwamp", failures);
            RequireObject("AshRoadDistrict_PepelnyTrakt", failures);
            RequireObject("TowerVistaDistrict_Ruins", failures);
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
            RequireObject<DialogueInteractable>("MartaLozovaya_World", failures);
            RequireObject<DialogueInteractable>("ElsaCherntravka_World", failures);
            RequireObject<DialogueInteractable>("IvarSedoy_World", failures);
            RequireObject<DialogueInteractable>("OrtenMirrorMage_World", failures);
            RequireObject<QuestProgressInteractable>("WorldTrace_ClawMarks", failures);
            RequireObject<QuestProgressInteractable>("WorldTrace_SlimeTrail", failures);
            RequireObject<QuestProgressInteractable>("WorldTrace_TornCloth", failures);
            RequireObject<QuestProgressInteractable>("WorldHunterCamp_Start", failures);
            RequireObject<CraftingInteractable>("WorldAlchemyTable_Swallow", failures);
            RequireObject<CraftingInteractable>("WorldAlchemyTable_Antitoxin", failures);
            RequireObject<CraftingInteractable>("WorldForge_ReinforcedArmor", failures);
            RequireObject<DecisionFlagInteractable>("WorldGirlMedallion", failures);
            RequireObject<DecisionFlagInteractable>("WorldOrtenDiary", failures);
            RequireObject<DecisionFlagInteractable>("WorldMirrorShardCache", failures);
            RequireObject<DecisionFlagInteractable>("WorldTowerReedCharmGate", failures);
            RequireObject<DecisionFlagInteractable>("WorldGhostMemory", failures);
            RequireObject<DecisionFlagInteractable>("WorldElderSealProof", failures);
            RequireObject<EndingAltarInteractable>("WorldFinalTruthAltar", failures);
            RequireObject<EndingAltarInteractable>("WorldFinalLieAltar", failures);
            RequireObject<EndingAltarInteractable>("WorldFinalSacrificeAltar", failures);

            var drowner = RequireObject("WorldDrowner_Prototype", failures);
            if (drowner != null)
            {
                RequireComponent<Health>(drowner, failures, "WorldDrowner_Prototype");
                RequireComponent<EnemyAI>(drowner, failures, "WorldDrowner_Prototype");
            }

            ValidateEnemy("TowerSkeletonGuard_Left", failures);
            ValidateEnemy("TowerSkeletonGuard_Right", failures);
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

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Core;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Player;
using WitcherRightVersion.Quest;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Editor
{
    public static class VerticalSliceValidator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string VillageScenePath = "Assets/Scenes/VillageScene.unity";
        private const string ForestScenePath = "Assets/Scenes/ForestScene.unity";

        [MenuItem("Tools/Witcher Right Version/Validate Vertical Slice")]
        public static void Validate()
        {
            var failures = new List<string>();

            ValidateBuildSettings(failures);
            ValidateMainMenuScene(failures);
            ValidateVillageScene(failures);
            ValidateForestScene(failures);
            ValidateQuestFlowSimulation(failures);

            if (failures.Count > 0)
            {
                var message = "Vertical slice validation failed:\n- " + string.Join("\n- ", failures);
                Debug.LogError(message);
                throw new InvalidOperationException(message);
            }

            Debug.Log("Vertical slice validation passed.");
        }

        private static void ValidateBuildSettings(List<string> failures)
        {
            var hasMainMenu = false;
            var hasVillage = false;
            var hasForest = false;

            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (!scene.enabled)
                {
                    continue;
                }

                hasMainMenu |= scene.path == MainMenuScenePath;
                hasVillage |= scene.path == VillageScenePath;
                hasForest |= scene.path == ForestScenePath;
            }

            Require(hasMainMenu, failures, "Build Settings must include enabled MainMenuScene.");
            Require(hasVillage, failures, "Build Settings must include enabled VillageScene.");
            Require(hasForest, failures, "Build Settings must include enabled ForestScene.");
        }

        private static void ValidateMainMenuScene(List<string> failures)
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            RequireObject<MainMenuController>("MainMenuController", failures);
            RequireObject("MainMenuCanvas", failures);
            RequireObject("MainMenuEventSystem", failures);
            RequireObject("NewGameButton", failures);
            RequireObject("ContinueButton", failures);
            RequireObject("SettingsButton", failures);
            RequireObject("ExitButton", failures);
            RequireObject("StatusText", failures);
        }

        private static void ValidateVillageScene(List<string> failures)
        {
            EditorSceneManager.OpenScene(VillageScenePath, OpenSceneMode.Single);

            var player = RequireObject("Reynard_Player", failures);
            if (player != null)
            {
                RequireComponent<CharacterController>(player, failures, "Reynard_Player");
                RequireComponent<PlayerController>(player, failures, "Reynard_Player");
                RequireComponent<InteractionController>(player, failures, "Reynard_Player");
                RequireComponent<Health>(player, failures, "Reynard_Player");
                RequireComponent<CombatController>(player, failures, "Reynard_Player");
            }

            var services = RequireObject("RuntimeServices", failures);
            if (services != null)
            {
                RequireComponent<AudioFeedbackService>(services, failures, "RuntimeServices");
                RequireComponent<DecisionFlagService>(services, failures, "RuntimeServices");
                RequireComponent<PlayerRewardService>(services, failures, "RuntimeServices");
                RequireComponent<InventoryService>(services, failures, "RuntimeServices");
                RequireComponent<QuestService>(services, failures, "RuntimeServices");
                RequireComponent<SaveService>(services, failures, "RuntimeServices");
            }

            RequireObject<DialogueInteractable>("ElderVoytsekh_Prototype", failures);
            RequireObject<DialogueInteractable>("MartaLozovaya_Prototype", failures);
            RequireObject<SceneTransitionInteractable>("ForestPathTransition", failures);

            RequireObject("SwampMoodRoot", failures);
            RequireObject("SwampBogGround", failures);
            RequireObject("SwampBlackWaterPool_01", failures);
            RequireObject("SwampReeds_01", failures);

            ValidateTrace("SwampTrace_ClawMarks", failures);
            ValidateTrace("SwampTrace_SlimeTrail", failures);
            ValidateTrace("SwampTrace_TornCloth", failures);

            var drowner = RequireObject("FirstDrowner_Prototype", failures);
            if (drowner != null)
            {
                RequireComponent<Health>(drowner, failures, "FirstDrowner_Prototype");
                RequireComponent<EnemyAI>(drowner, failures, "FirstDrowner_Prototype");
            }

            RequireObject<DialogueService>("DialogueCanvas", failures);
            RequireObject<QuestHudUI>("QuestCanvas", failures);
            RequireObject<HealthHudUI>("HealthCanvas", failures);
            RequireObject<ControlsHintUI>("ControlsCanvas", failures);
            RequireObject<InventoryHudUI>("InventoryCanvas", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
            RequireObject("ThirdPersonCamera", failures);
            RequireObject("VillageBlockoutGround", failures);
        }

        private static void ValidateForestScene(List<string> failures)
        {
            EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Single);

            var player = RequireObject("Reynard_Player", failures);
            if (player != null)
            {
                RequireComponent<CharacterController>(player, failures, "Forest Reynard_Player");
                RequireComponent<PlayerController>(player, failures, "Forest Reynard_Player");
                RequireComponent<InteractionController>(player, failures, "Forest Reynard_Player");
                RequireComponent<Health>(player, failures, "Forest Reynard_Player");
                RequireComponent<CombatController>(player, failures, "Forest Reynard_Player");
            }

            var services = RequireObject("RuntimeServices", failures);
            if (services != null)
            {
                RequireComponent<AudioFeedbackService>(services, failures, "Forest RuntimeServices");
                RequireComponent<DecisionFlagService>(services, failures, "Forest RuntimeServices");
                RequireComponent<PlayerRewardService>(services, failures, "Forest RuntimeServices");
                RequireComponent<InventoryService>(services, failures, "Forest RuntimeServices");
                RequireComponent<QuestService>(services, failures, "Forest RuntimeServices");
                RequireComponent<SaveService>(services, failures, "Forest RuntimeServices");
            }

            RequireObject("ForestBlockoutGround", failures);
            RequireObject("ForestMoodRoot", failures);
            RequireObject("ForestTree_01", failures);
            RequireObject("ForestRock_01", failures);
            RequireObject<SceneTransitionInteractable>("VillagePathTransition", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
            RequireObject("ThirdPersonCamera", failures);
        }

        private static void ValidateTrace(string objectName, List<string> failures)
        {
            var trace = RequireObject(objectName, failures);
            if (trace == null)
            {
                return;
            }

            var interactable = RequireComponent<QuestProgressInteractable>(trace, failures, objectName);
            if (interactable != null)
            {
                Require(!string.IsNullOrWhiteSpace(interactable.PersistentId), failures, $"{objectName} must have a persistent id.");
            }
        }

        private static void ValidateQuestFlowSimulation(List<string> failures)
        {
            ResetSingleton<DecisionFlagService>();
            ResetSingleton<PlayerRewardService>();
            ResetSingleton<QuestService>();

            var root = new GameObject("VerticalSliceQuestFlowSimulation");
            try
            {
                var flags = root.AddComponent<DecisionFlagService>();
                var rewards = root.AddComponent<PlayerRewardService>();
                var quest = root.AddComponent<QuestService>();

                InvokeAwake(flags);
                InvokeAwake(rewards);
                InvokeAwake(quest);

                flags.SetFlag("acceptedSwampContract");
                Require(quest.RunAction(QuestService.ActionStartSwampContract), failures, "Quest flow must start swamp contract.");
                Require(quest.SwampContractState == QuestState.Active, failures, "Quest flow must become active after contract accept.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.SpeakWithMarta, failures, "Quest flow must move to Marta after contract accept.");

                Require(quest.RunAction(QuestService.ActionMartaSpoken), failures, "Quest flow must accept Marta conversation.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.FindSwampTraces, failures, "Quest flow must move to trace investigation after Marta.");

                Require(quest.RunAction(QuestService.ActionSwampTracesFound), failures, "Quest flow must accept first trace.");
                Require(quest.RunAction(QuestService.ActionSwampTracesFound), failures, "Quest flow must accept second trace.");
                Require(quest.RunAction(QuestService.ActionSwampTracesFound), failures, "Quest flow must accept third trace.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.KillDrowner, failures, "Quest flow must unlock drowner after three traces.");

                flags.SetFlag("killedFirstDrowner");
                Require(quest.RunAction(QuestService.ActionFirstDrownerKilled), failures, "Quest flow must accept first drowner kill.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.ReturnToElder, failures, "Quest flow must return to elder after drowner kill.");

                Require(quest.RunAction(QuestService.ActionReturnedToElder), failures, "Quest flow must accept elder return.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.ChooseResponse, failures, "Quest flow must reach response choice.");

                flags.SetFlag("questionedElderVersion");
                Require(quest.RunAction(QuestService.ActionQuestionedElderVersion), failures, "Quest flow must accept questioned elder version choice.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.ReceiveReward, failures, "Quest flow must reach reward stage after choice.");

                Require(quest.RunAction(QuestService.ActionRewardReceived), failures, "Quest flow must accept reward.");
                Require(quest.SwampContractState == QuestState.Completed, failures, "Quest flow must complete swamp contract after reward.");
                Require(quest.CurrentSwampContractStage == SwampContractStage.Completed, failures, "Quest flow stage must be completed after reward.");
                Require(rewards.Experience == 50, failures, "Quest reward must grant exactly 50 XP.");
                Require(rewards.Coins == 20, failures, "Quest reward must grant exactly 20 coins.");
                Require(rewards.HasRecipe("antitoxin"), failures, "Quest reward must unlock Antitoxin recipe.");
                Require(flags.HasFlag("acceptedSwampContract"), failures, "Quest flow must keep acceptedSwampContract flag.");
                Require(flags.HasFlag("questionedElderVersion"), failures, "Quest flow must keep questionedElderVersion flag.");
                Require(flags.HasFlag("killedFirstDrowner"), failures, "Quest flow must keep killedFirstDrowner flag.");
                Require(flags.HasFlag("receivedAntitoxinRecipe"), failures, "Quest flow must set receivedAntitoxinRecipe flag.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                ResetSingleton<DecisionFlagService>();
                ResetSingleton<PlayerRewardService>();
                ResetSingleton<QuestService>();
            }
        }

        private static GameObject RequireObject(string objectName, List<string> failures)
        {
            var target = GameObject.Find(objectName);
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

        private static T RequireComponent<T>(GameObject target, List<string> failures, string objectName) where T : Component
        {
            var component = target.GetComponent<T>();
            Require(component != null, failures, $"{objectName} must have {typeof(T).Name}.");
            return component;
        }

        private static void Require(bool condition, List<string> failures, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        private static void InvokeAwake(MonoBehaviour component)
        {
            var method = component.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(component, null);
        }

        private static void ResetSingleton<T>() where T : Component
        {
            var backingField = typeof(T).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            backingField?.SetValue(null, null);
        }
    }
}

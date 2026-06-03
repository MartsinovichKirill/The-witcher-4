using System;
using System.Collections.Generic;
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

        [MenuItem("Tools/Witcher Right Version/Validate Vertical Slice")]
        public static void Validate()
        {
            var failures = new List<string>();

            ValidateBuildSettings(failures);
            ValidateMainMenuScene(failures);
            ValidateVillageScene(failures);

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

            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (!scene.enabled)
                {
                    continue;
                }

                hasMainMenu |= scene.path == MainMenuScenePath;
                hasVillage |= scene.path == VillageScenePath;
            }

            Require(hasMainMenu, failures, "Build Settings must include enabled MainMenuScene.");
            Require(hasVillage, failures, "Build Settings must include enabled VillageScene.");
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
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
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
    public static class VerticalSliceValidator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string VillageScenePath = "Assets/Scenes/VillageScene.unity";
        private const string ForestScenePath = "Assets/Scenes/ForestScene.unity";
        private const string AshRoadScenePath = "Assets/Scenes/AshRoadScene.unity";

        [MenuItem("Tools/Witcher Right Version/Validate Vertical Slice")]
        public static void Validate()
        {
            var failures = new List<string>();

            ValidateBuildSettings(failures);
            ValidateMainMenuScene(failures);
            ValidateVillageScene(failures);
            ValidateForestScene(failures);
            ValidateAshRoadScene(failures);
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
            var hasAshRoad = false;

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
                hasAshRoad |= scene.path == AshRoadScenePath;
            }

            Require(hasMainMenu, failures, "Build Settings must include enabled MainMenuScene.");
            Require(hasVillage, failures, "Build Settings must include enabled VillageScene.");
            Require(hasForest, failures, "Build Settings must include enabled ForestScene.");
            Require(hasAshRoad, failures, "Build Settings must include enabled AshRoadScene.");
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
            RequireObject("LanguageLabel", failures);
            RequireObject("LanguageDropdown", failures);
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
                RequireComponent<EndingService>(services, failures, "RuntimeServices");
                RequireComponent<PlayerRewardService>(services, failures, "RuntimeServices");
                RequireComponent<InventoryService>(services, failures, "RuntimeServices");
                RequireComponent<CraftingService>(services, failures, "RuntimeServices");
                RequireComponent<QuestService>(services, failures, "RuntimeServices");
                RequireComponent<SaveService>(services, failures, "RuntimeServices");
            }

            RequireObject<DialogueInteractable>("ElderVoytsekh_Prototype", failures);
            RequireObject<DialogueInteractable>("MartaLozovaya_Prototype", failures);
            RequireObject<SceneTransitionInteractable>("ForestPathTransition", failures);
            RequireObject<EndingGateInteractable>("AshRoadFinalPath", failures);
            ValidateForestQuestObject("BorisSmithQuest_Start", failures);
            ValidateForestQuestObject("BorisSmithQuest_Return", failures);
            ValidateInventoryGrantObject("MartaHerbBasket", failures);
            ValidateInventoryGrantObject("VillageForgeSupplies", failures);
            ValidateCraftingObject("AlchemyTable_Swallow", failures);
            ValidateCraftingObject("AlchemyTable_Antitoxin", failures);
            ValidateCraftingObject("Forge_ReinforcedArmor", failures);

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
                RequireComponent<EndingService>(services, failures, "Forest RuntimeServices");
                RequireComponent<PlayerRewardService>(services, failures, "Forest RuntimeServices");
                RequireComponent<InventoryService>(services, failures, "Forest RuntimeServices");
                RequireComponent<CraftingService>(services, failures, "Forest RuntimeServices");
                RequireComponent<QuestService>(services, failures, "Forest RuntimeServices");
                RequireComponent<SaveService>(services, failures, "Forest RuntimeServices");
            }

            RequireObject("ForestBlockoutGround", failures);
            RequireObject("ForestMoodRoot", failures);
            RequireObject("ForestTree_01", failures);
            RequireObject("ForestRock_01", failures);
            RequireObject<SceneTransitionInteractable>("VillagePathTransition", failures);
            ValidateForestQuestObject("HunterCamp_Start", failures);
            ValidateForestQuestObject("HunterClue_BloodTrail", failures);
            ValidateForestQuestObject("HunterClue_BrokenKnife", failures);
            ValidateForestQuestObject("HunterCamp_RewardPouch", failures);
            ValidateForestQuestObject("OldCampBlade", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
            RequireObject<InventoryHudUI>("InventoryCanvas", failures);
            RequireObject("ThirdPersonCamera", failures);
        }

        private static void ValidateAshRoadScene(List<string> failures)
        {
            EditorSceneManager.OpenScene(AshRoadScenePath, OpenSceneMode.Single);

            var player = RequireObject("Reynard_Player", failures);
            if (player != null)
            {
                RequireComponent<CharacterController>(player, failures, "Ash Road Reynard_Player");
                RequireComponent<PlayerController>(player, failures, "Ash Road Reynard_Player");
                RequireComponent<InteractionController>(player, failures, "Ash Road Reynard_Player");
                RequireComponent<Health>(player, failures, "Ash Road Reynard_Player");
                RequireComponent<CombatController>(player, failures, "Ash Road Reynard_Player");
            }

            var services = RequireObject("RuntimeServices", failures);
            if (services != null)
            {
                RequireComponent<AudioFeedbackService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<DecisionFlagService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<EndingService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<PlayerRewardService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<InventoryService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<CraftingService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<QuestService>(services, failures, "Ash Road RuntimeServices");
                RequireComponent<SaveService>(services, failures, "Ash Road RuntimeServices");
            }

            RequireObject("AshRoadBlockoutGround", failures);
            RequireObject("AshRoadMoodRoot", failures);
            RequireObject("BurnedRoadStrip", failures);
            RequireObject<EndingAltarInteractable>("FinalTruthAltar", failures);
            RequireObject<InteractionPromptUI>("InteractionCanvas", failures);
            RequireObject<EndingHudUI>("EndingCanvas", failures);
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

        private static void ValidateForestQuestObject(string objectName, List<string> failures)
        {
            var target = RequireObject(objectName, failures);
            if (target == null)
            {
                return;
            }

            var interactable = RequireComponent<QuestProgressInteractable>(target, failures, objectName);
            if (interactable != null)
            {
                Require(!string.IsNullOrWhiteSpace(interactable.PersistentId), failures, $"{objectName} must have a persistent id.");
            }
        }

        private static void ValidateCraftingObject(string objectName, List<string> failures)
        {
            var target = RequireObject(objectName, failures);
            if (target != null)
            {
                RequireComponent<CraftingInteractable>(target, failures, objectName);
            }
        }

        private static void ValidateInventoryGrantObject(string objectName, List<string> failures)
        {
            var target = RequireObject(objectName, failures);
            if (target != null)
            {
                RequireComponent<InventoryGrantInteractable>(target, failures, objectName);
            }
        }

        private static void ValidateQuestFlowSimulation(List<string> failures)
        {
            ResetSingleton<DecisionFlagService>();
            ResetSingleton<EndingService>();
            ResetSingleton<PlayerRewardService>();
            ResetSingleton<InventoryService>();
            ResetSingleton<CraftingService>();
            ResetSingleton<QuestService>();
            ResetSingleton<SaveService>();

            var root = new GameObject("VerticalSliceQuestFlowSimulation");
            try
            {
                PlayerPrefs.DeleteKey(EndingService.PendingTruthRouteKey);
                PlayerPrefs.DeleteKey(EndingService.CompletedEndingKey);
                PlayerPrefs.Save();

                var flags = root.AddComponent<DecisionFlagService>();
                var endings = root.AddComponent<EndingService>();
                var rewards = root.AddComponent<PlayerRewardService>();
                var inventory = root.AddComponent<InventoryService>();
                var crafting = root.AddComponent<CraftingService>();
                var quest = root.AddComponent<QuestService>();
                var save = root.AddComponent<SaveService>();

                InvokeAwake(flags);
                InvokeAwake(endings);
                InvokeAwake(rewards);
                InvokeAwake(inventory);
                InvokeAwake(crafting);
                InvokeAwake(quest);
                InvokeAwake(save);

                Require(!endings.CanCompleteTruthEnding(), failures, "Truth ending must be locked before route requirements are met.");
                EndingService.UnlockTruthRoute();
                Require(endings.CanCompleteTruthEnding(), failures, "Truth ending must unlock after entering Ash Road route.");
                PlayerPrefs.DeleteKey(EndingService.PendingTruthRouteKey);
                PlayerPrefs.Save();
                Require(!endings.CanCompleteTruthEnding(), failures, "Truth ending must lock again after clearing temporary route unlock.");

                Require(crafting.Recipes.Count == 3, failures, "Crafting must expose three MVP recipes.");

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
                Require(rewards.Level == 1, failures, "50 XP must keep the player at level 1.");
                Require(rewards.SkillPoints == 0, failures, "Level 1 must grant 0 skill points.");
                Require(rewards.HasRecipe("antitoxin"), failures, "Quest reward must unlock Antitoxin recipe.");
                Require(flags.HasFlag("acceptedSwampContract"), failures, "Quest flow must keep acceptedSwampContract flag.");
                Require(flags.HasFlag("questionedElderVersion"), failures, "Quest flow must keep questionedElderVersion flag.");
                Require(flags.HasFlag("killedFirstDrowner"), failures, "Quest flow must keep killedFirstDrowner flag.");
                Require(flags.HasFlag("receivedAntitoxinRecipe"), failures, "Quest flow must set receivedAntitoxinRecipe flag.");

                inventory.AddItem("Swallow Grass");
                inventory.AddItem("Field Ration");
                Require(crafting.Craft("swallow"), failures, "Crafting must create Swallow from grass and ration.");
                Require(inventory.HasItem("Swallow"), failures, "Crafting must add Swallow to inventory.");
                Require(!inventory.HasItem("Swallow Grass"), failures, "Crafting must consume Swallow Grass.");
                Require(flags.HasFlag("crafted_swallow"), failures, "Crafting must set crafted_swallow flag.");

                inventory.AddItem("Bogweed");
                inventory.AddItem("Drowner Slime");
                Require(crafting.Craft("antitoxin"), failures, "Crafting must create Antitoxin after recipe unlock.");
                Require(inventory.HasItem("Antitoxin"), failures, "Crafting must add Antitoxin to inventory.");
                Require(!inventory.HasItem("Bogweed"), failures, "Crafting must consume Bogweed.");
                Require(flags.HasFlag("crafted_antitoxin"), failures, "Crafting must set crafted_antitoxin flag.");

                inventory.AddItem("Wolf Pelt");
                inventory.AddItem("Iron Ore");
                Require(crafting.Craft("reinforced_armor"), failures, "Forge crafting must create Reinforced Armor.");
                Require(inventory.HasItem("Reinforced Armor"), failures, "Forge crafting must add Reinforced Armor.");
                Require(!inventory.HasItem("Leather Witcher Armor"), failures, "Forge crafting must consume Leather Witcher Armor.");
                Require(flags.HasFlag("crafted_reinforced_armor"), failures, "Forge crafting must set crafted_reinforced_armor flag.");

                var xpAfterSwamp = rewards.Experience;
                var coinsAfterSwamp = rewards.Coins;

                Require(quest.RunAction(QuestService.ActionStartMissingHunter), failures, "Missing Hunter must start.");
                Require(quest.MissingHunterState == QuestState.Active, failures, "Missing Hunter must become active.");
                Require(quest.CurrentMissingHunterStage == MissingHunterStage.FindClues, failures, "Missing Hunter must start at FindClues.");
                Require(quest.RunAction(QuestService.ActionMissingHunterClueFound), failures, "Missing Hunter must accept first clue.");
                Require(quest.RunAction(QuestService.ActionMissingHunterClueFound), failures, "Missing Hunter must accept second clue.");
                Require(quest.CurrentMissingHunterStage == MissingHunterStage.ReturnToCamp, failures, "Missing Hunter must return to camp after two clues.");
                Require(quest.RunAction(QuestService.ActionMissingHunterReturned), failures, "Missing Hunter must complete from reward pouch.");
                Require(quest.MissingHunterState == QuestState.Completed, failures, "Missing Hunter must be completed.");
                Require(quest.CurrentMissingHunterStage == MissingHunterStage.Completed, failures, "Missing Hunter stage must be completed.");
                Require(rewards.Experience == xpAfterSwamp + 25, failures, "Missing Hunter must grant 25 XP.");
                Require(rewards.Coins == coinsAfterSwamp + 10, failures, "Missing Hunter must grant 10 coins.");
                Require(flags.HasFlag("missingHunterStarted"), failures, "Missing Hunter must set missingHunterStarted flag.");
                Require(flags.HasFlag("missingHunterCompleted"), failures, "Missing Hunter must set missingHunterCompleted flag.");

                var xpAfterHunter = rewards.Experience;

                Require(quest.RunAction(QuestService.ActionStartSmithDebt), failures, "Smith's Debt must start.");
                Require(quest.SmithDebtState == QuestState.Active, failures, "Smith's Debt must become active.");
                Require(quest.CurrentSmithDebtStage == SmithDebtStage.FindOldCampBlade, failures, "Smith's Debt must start at FindOldCampBlade.");
                Require(quest.RunAction(QuestService.ActionOldCampBladeFound), failures, "Smith's Debt must accept old camp blade.");
                Require(quest.CurrentSmithDebtStage == SmithDebtStage.ReturnToSmith, failures, "Smith's Debt must move to ReturnToSmith after blade.");
                Require(inventory.HasItem("Old Camp Blade"), failures, "Smith's Debt must add Old Camp Blade to inventory.");
                Require(quest.RunAction(QuestService.ActionSmithDebtReturned), failures, "Smith's Debt must complete at Boris's anvil.");
                Require(quest.SmithDebtState == QuestState.Completed, failures, "Smith's Debt must be completed.");
                Require(quest.CurrentSmithDebtStage == SmithDebtStage.Completed, failures, "Smith's Debt stage must be completed.");
                Require(rewards.Experience == xpAfterHunter + 30, failures, "Smith's Debt must grant 30 XP.");
                Require(rewards.Experience == 105, failures, "Three MVP quests must total 105 XP.");
                Require(rewards.Level == 2, failures, "105 XP must raise the player to level 2.");
                Require(rewards.SkillPoints == 1, failures, "Level 2 must grant 1 skill point.");
                Require(inventory.HasWeapon("Improved Steel Sword"), failures, "Smith's Debt must add Improved Steel Sword.");
                Require(flags.HasFlag("smithDebtStarted"), failures, "Smith's Debt must set smithDebtStarted flag.");
                Require(flags.HasFlag("oldCampBladeFound"), failures, "Smith's Debt must set oldCampBladeFound flag.");
                Require(flags.HasFlag("smithDebtCompleted"), failures, "Smith's Debt must set smithDebtCompleted flag.");

                Require(quest.RunAction(QuestService.ActionStartRightVersion), failures, "Right Version must start.");
                Require(quest.RightVersionState == QuestState.Active, failures, "Right Version must become active.");
                Require(quest.CurrentRightVersionStage == RightVersionStage.FindElsa, failures, "Right Version must start at FindElsa.");
                Require(quest.RunAction(QuestService.ActionElsaProtected), failures, "Right Version must accept Elsa protection.");
                Require(quest.CurrentRightVersionStage == RightVersionStage.FindMedallion, failures, "Right Version must move to FindMedallion after Elsa.");
                Require(flags.HasFlag("ElsaProtected"), failures, "Right Version must set ElsaProtected flag.");
                Require(quest.RunAction(QuestService.ActionMedallionFound), failures, "Right Version must accept medallion evidence.");
                Require(quest.CurrentRightVersionStage == RightVersionStage.OpenTowerRoute, failures, "Right Version must move to OpenTowerRoute after medallion.");
                Require(flags.HasFlag("MedallionFound"), failures, "Right Version must set MedallionFound flag.");
                Require(quest.RunAction(QuestService.ActionTowerRouteOpened), failures, "Tower route must complete Right Version and start Mirror of Truth.");
                Require(quest.RightVersionState == QuestState.Completed, failures, "Right Version must complete when tower route opens.");
                Require(quest.MirrorTruthState == QuestState.Active, failures, "Mirror of Truth must start when tower route opens.");
                Require(quest.CurrentMirrorTruthStage == MirrorTruthStage.ReadOrtenDiary, failures, "Mirror of Truth must start at ReadOrtenDiary.");
                Require(quest.RunAction(QuestService.ActionOrtenDiaryFound), failures, "Mirror of Truth must accept Orten diary.");
                Require(quest.CurrentMirrorTruthStage == MirrorTruthStage.ConfrontOrten, failures, "Mirror of Truth must move to ConfrontOrten.");
                Require(flags.HasFlag("OrtenDiaryFound"), failures, "Mirror of Truth must set OrtenDiaryFound flag.");
                Require(quest.RunAction(QuestService.ActionOrtenConfronted), failures, "Mirror of Truth must accept Orten confrontation.");
                Require(quest.CurrentMirrorTruthStage == MirrorTruthStage.ChooseEnding, failures, "Mirror of Truth must move to ChooseEnding.");
                Require(flags.HasFlag("OrtenConfronted"), failures, "Mirror of Truth must set OrtenConfronted flag.");

                Require(SaveService.PrepareSceneTransfer(), failures, "Scene transfer must capture current session state.");
                ValidateSceneTransferRestore(failures);
                SetSingleton(flags);
                SetSingleton(endings);
                SetSingleton(rewards);
                SetSingleton(inventory);
                SetSingleton(crafting);
                SetSingleton(quest);
                SetSingleton(save);

                flags.SetFlag("MedallionFound");
                flags.SetFlag("MayorSupported");
                flags.SetFlag("OrtenDiaryFound");
                Require(endings.CanCompleteTruthEnding(), failures, "World truth ending must unlock from completed contract and questioned elder flag.");
                Require(endings.CanCompleteLieEnding(), failures, "Lie ending must unlock from mayor support.");
                Require(endings.CanCompleteSacrificeEnding(), failures, "Sacrifice ending must unlock from Orten diary.");
                Require(endings.CompleteLieEnding(), failures, "Lie ending must complete from final altar.");
                Require(endings.CompletedEnding == EndingService.LieEndingType, failures, "Lie ending type must be stored.");
                Require(flags.HasFlag(EndingService.LieEndingFlag), failures, "Lie ending must set ending flag.");
                Require(endings.CompleteSacrificeEnding(), failures, "Sacrifice ending must complete from final altar.");
                Require(endings.CompletedEnding == EndingService.SacrificeEndingType, failures, "Sacrifice ending type must be stored.");
                Require(flags.HasFlag(EndingService.SacrificeEndingFlag), failures, "Sacrifice ending must set ending flag.");
                Require(endings.CompleteTruthEnding(), failures, "Truth ending must complete from final altar.");
                Require(endings.CompletedEnding == EndingService.TruthEndingType, failures, "Truth ending type must be stored.");
                Require(flags.HasFlag(EndingService.TruthEndingFlag), failures, "Truth ending must set MVP ending flag.");
                Require(flags.HasFlag("VillageTruthExposed"), failures, "Truth ending must set VillageTruthExposed flag.");
            }
            finally
            {
                PlayerPrefs.DeleteKey(EndingService.PendingTruthRouteKey);
                PlayerPrefs.DeleteKey(EndingService.CompletedEndingKey);
                PlayerPrefs.Save();

                UnityEngine.Object.DestroyImmediate(root);
                ResetSingleton<DecisionFlagService>();
                ResetSingleton<EndingService>();
                ResetSingleton<PlayerRewardService>();
                ResetSingleton<InventoryService>();
                ResetSingleton<CraftingService>();
                ResetSingleton<QuestService>();
                ResetSingleton<SaveService>();
            }
        }

        private static void ValidateSceneTransferRestore(List<string> failures)
        {
            ResetSingleton<DecisionFlagService>();
            ResetSingleton<EndingService>();
            ResetSingleton<PlayerRewardService>();
            ResetSingleton<InventoryService>();
            ResetSingleton<CraftingService>();
            ResetSingleton<QuestService>();
            ResetSingleton<SaveService>();

            var restoreRoot = new GameObject("SceneTransferRestoreSimulation");
            try
            {
                var restoreFlags = restoreRoot.AddComponent<DecisionFlagService>();
                var restoreEndings = restoreRoot.AddComponent<EndingService>();
                var restoreRewards = restoreRoot.AddComponent<PlayerRewardService>();
                var restoreInventory = restoreRoot.AddComponent<InventoryService>();
                var restoreCrafting = restoreRoot.AddComponent<CraftingService>();
                var restoreQuest = restoreRoot.AddComponent<QuestService>();
                var restoreSave = restoreRoot.AddComponent<SaveService>();

                InvokeAwake(restoreFlags);
                InvokeAwake(restoreEndings);
                InvokeAwake(restoreRewards);
                InvokeAwake(restoreInventory);
                InvokeAwake(restoreCrafting);
                InvokeAwake(restoreQuest);
                InvokeAwake(restoreSave);
                InvokeStart(restoreSave);

                Require(restoreQuest.SwampContractState == QuestState.Completed, failures, "Scene transfer must restore completed swamp contract.");
                Require(restoreQuest.RightVersionState == QuestState.Completed, failures, "Scene transfer must restore completed Right Version.");
                Require(restoreQuest.MirrorTruthState == QuestState.Active, failures, "Scene transfer must restore active Mirror of Truth.");
                Require(restoreQuest.CurrentMirrorTruthStage == MirrorTruthStage.ChooseEnding, failures, "Scene transfer must restore Mirror of Truth ending stage.");
                Require(restoreQuest.MissingHunterState == QuestState.Completed, failures, "Scene transfer must restore completed Missing Hunter.");
                Require(restoreQuest.SmithDebtState == QuestState.Completed, failures, "Scene transfer must restore completed Smith's Debt.");
                Require(restoreRewards.Experience == 105, failures, "Scene transfer must restore XP.");
                Require(restoreRewards.Level == 2, failures, "Scene transfer must restore level from XP.");
                Require(restoreRewards.Coins == 30, failures, "Scene transfer must restore coins.");
                Require(restoreInventory.HasWeapon("Improved Steel Sword"), failures, "Scene transfer must restore improved steel sword.");
                Require(restoreInventory.HasItem("Antitoxin"), failures, "Scene transfer must restore crafted Antitoxin.");
                Require(restoreFlags.HasFlag("questionedElderVersion"), failures, "Scene transfer must restore questionedElderVersion flag.");
                Require(restoreFlags.HasFlag("ElsaProtected"), failures, "Scene transfer must restore ElsaProtected flag.");
                Require(restoreFlags.HasFlag("MedallionFound"), failures, "Scene transfer must restore MedallionFound flag.");
                Require(restoreFlags.HasFlag("OrtenConfronted"), failures, "Scene transfer must restore OrtenConfronted flag.");
                Require(restoreFlags.HasFlag("smithDebtCompleted"), failures, "Scene transfer must restore smithDebtCompleted flag.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(restoreRoot);
                ResetSingleton<DecisionFlagService>();
                ResetSingleton<EndingService>();
                ResetSingleton<PlayerRewardService>();
                ResetSingleton<InventoryService>();
                ResetSingleton<CraftingService>();
                ResetSingleton<QuestService>();
                ResetSingleton<SaveService>();
            }
        }

        private static GameObject RequireObject(string objectName, List<string> failures)
        {
            var target = FindSceneObject(objectName);
            Require(target != null, failures, $"Missing GameObject: {objectName}");
            return target;
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

        private static void InvokeStart(MonoBehaviour component)
        {
            var method = component.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(component, null);
        }

        private static void ResetSingleton<T>() where T : Component
        {
            var backingField = typeof(T).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            backingField?.SetValue(null, null);
        }

        private static void SetSingleton<T>(T component) where T : Component
        {
            var backingField = typeof(T).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            backingField?.SetValue(null, component);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Core
{
    public sealed class PlayerRewardService : MonoBehaviour
    {
        public const int ExperiencePerLevel = 100;

        private readonly HashSet<string> unlockedRecipes = new HashSet<string>();

        public static PlayerRewardService Instance { get; private set; }

        public int Experience { get; private set; }
        public int Coins { get; private set; }
        public int Level { get; private set; } = 1;
        public int SkillPoints { get; private set; }
        public int ExperienceIntoLevel => Experience % ExperiencePerLevel;
        public int ExperienceToNextLevel => ExperiencePerLevel - ExperienceIntoLevel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Experience += amount;
            RecalculateLevel();
            Debug.Log($"XP received: {amount}. Total XP: {Experience}", this);
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Coins += amount;
            Debug.Log($"Coins received: {amount}. Total coins: {Coins}", this);
        }

        public void UnlockRecipe(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return;
            }

            if (unlockedRecipes.Add(recipeId))
            {
                Debug.Log($"Recipe unlocked: {recipeId}", this);
            }
        }

        public bool HasRecipe(string recipeId)
        {
            return !string.IsNullOrWhiteSpace(recipeId) && unlockedRecipes.Contains(recipeId);
        }

        public void GrantSwampContractReward()
        {
            AddExperience(50);
            AddCoins(20);
            UnlockRecipe("antitoxin");
            DecisionFlagService.Instance?.SetFlag("receivedAntitoxinRecipe");
            InteractionPromptUI.Instance?.ShowMessage("Reward: 50 XP, 20 coins, Antitoxin recipe.");
        }

        public void GrantMissingHunterReward()
        {
            AddExperience(25);
            AddCoins(10);
            DecisionFlagService.Instance?.SetFlag("missingHunterCompleted");
            InteractionPromptUI.Instance?.ShowMessage("Reward: 25 XP and 10 coins.");
        }

        public void GrantSmithDebtReward()
        {
            AddExperience(30);
            InventoryService.Instance?.AddWeapon("Improved Steel Sword");
            DecisionFlagService.Instance?.SetFlag("smithDebtCompleted");
            InteractionPromptUI.Instance?.ShowMessage("Reward: 30 XP and Improved Steel Sword.");
        }

        public void GrantVoiceWellReward()
        {
            AddExperience(20);
            DecisionFlagService.Instance?.SetFlag("voiceWellCompleted");
            InteractionPromptUI.Instance?.ShowMessage("Reward: 20 XP. Truth evidence strengthened.");
        }

        public PlayerRewardSnapshot CaptureSnapshot()
        {
            return new PlayerRewardSnapshot
            {
                experience = Experience,
                coins = Coins,
                level = Level,
                skillPoints = SkillPoints,
                unlockedRecipes = new List<string>(unlockedRecipes).ToArray()
            };
        }

        public void RestoreSnapshot(PlayerRewardSnapshot snapshot)
        {
            Experience = snapshot != null ? Mathf.Max(0, snapshot.experience) : 0;
            Coins = snapshot != null ? Mathf.Max(0, snapshot.coins) : 0;
            RecalculateLevel();

            unlockedRecipes.Clear();
            if (snapshot?.unlockedRecipes != null)
            {
                for (var i = 0; i < snapshot.unlockedRecipes.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(snapshot.unlockedRecipes[i]))
                    {
                        unlockedRecipes.Add(snapshot.unlockedRecipes[i]);
                    }
                }
            }

            Debug.Log($"Rewards restored. XP: {Experience}, Coins: {Coins}", this);
        }

        private void RecalculateLevel()
        {
            Level = Mathf.Max(1, Experience / ExperiencePerLevel + 1);
            SkillPoints = Mathf.Max(0, Level - 1);
        }
    }

    [System.Serializable]
    public sealed class PlayerRewardSnapshot
    {
        public int experience;
        public int coins;
        public int level;
        public int skillPoints;
        public string[] unlockedRecipes;
    }
}

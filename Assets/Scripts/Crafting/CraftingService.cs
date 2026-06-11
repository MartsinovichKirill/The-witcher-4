using System;
using System.Collections.Generic;
using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Localization;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Crafting
{
    public sealed class CraftingService : MonoBehaviour
    {
        private readonly Dictionary<string, CraftingRecipe> recipes = new Dictionary<string, CraftingRecipe>();
        private readonly List<CraftingRecipe> recipeOrder = new List<CraftingRecipe>();

        public static CraftingService Instance { get; private set; }
        public IReadOnlyList<CraftingRecipe> Recipes => recipeOrder;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RegisterDefaultRecipes();
        }

        public bool CanCraft(string recipeId, out string reason)
        {
            reason = string.Empty;

            if (!recipes.TryGetValue(recipeId, out var recipe))
            {
                reason = GameLocalization.Select("Unknown recipe.", "Неизвестный рецепт.");
                return false;
            }

            var flags = DecisionFlagService.Instance;
            if (flags != null && flags.HasFlag(recipe.CraftedFlag))
            {
                reason = GameLocalization.Select($"{recipe.ResultItem} is already crafted.", $"{GameLocalization.Text(recipe.ResultItem)} уже создано.");
                return false;
            }

            var rewards = PlayerRewardService.Instance;
            if (!string.IsNullOrWhiteSpace(recipe.RequiredUnlockedRecipe)
                && (rewards == null || !rewards.HasRecipe(recipe.RequiredUnlockedRecipe)))
            {
                reason = GameLocalization.Select($"{recipe.DisplayName} recipe is locked.", $"Рецепт «{GameLocalization.Text(recipe.DisplayName)}» ещё закрыт.");
                return false;
            }

            var inventory = InventoryService.Instance;
            if (inventory == null)
            {
                reason = GameLocalization.Select("Inventory is not ready.", "Инвентарь ещё не готов.");
                return false;
            }

            for (var i = 0; i < recipe.Ingredients.Length; i++)
            {
                if (!inventory.HasItem(recipe.Ingredients[i]) && !inventory.HasWeapon(recipe.Ingredients[i]))
                {
                    reason = GameLocalization.Select($"Missing ingredient: {recipe.Ingredients[i]}.", $"Не хватает ингредиента: {GameLocalization.Text(recipe.Ingredients[i])}.");
                    return false;
                }
            }

            return true;
        }

        public bool Craft(string recipeId)
        {
            if (!CanCraft(recipeId, out var reason))
            {
                InteractionPromptUI.Instance?.ShowMessage(reason);
                Debug.Log($"Craft failed: {recipeId}. {reason}", this);
                return false;
            }

            var recipe = recipes[recipeId];
            var inventory = InventoryService.Instance;
            for (var i = 0; i < recipe.Ingredients.Length; i++)
            {
                if (!inventory.RemoveItem(recipe.Ingredients[i]))
                {
                    inventory.RemoveWeapon(recipe.Ingredients[i]);
                }
            }

            if (recipe.ResultIsWeapon)
            {
                inventory.AddWeapon(recipe.ResultItem);
            }
            else
            {
                inventory.AddItem(recipe.ResultItem);
            }

            DecisionFlagService.Instance?.SetFlag(recipe.CraftedFlag);
            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"Crafted: {recipe.ResultItem}.", $"Создано: {GameLocalization.Text(recipe.ResultItem)}."));
            Debug.Log($"Crafted recipe: {recipe.Id} -> {recipe.ResultItem}", this);
            return true;
        }

        public bool HasRecipe(string recipeId)
        {
            return recipes.ContainsKey(recipeId);
        }

        private void RegisterDefaultRecipes()
        {
            recipes.Clear();
            recipeOrder.Clear();
            Register(new CraftingRecipe(
                "swallow",
                "Swallow",
                "Swallow",
                false,
                string.Empty,
                new[] { "Swallow Grass", "Field Ration" }));
            Register(new CraftingRecipe(
                "antitoxin",
                "Antitoxin",
                "Antitoxin",
                false,
                "antitoxin",
                new[] { "Bogweed", "Drowner Slime" }));
            Register(new CraftingRecipe(
                "reinforced_armor",
                "Reinforced Armor",
                "Reinforced Armor",
                false,
                string.Empty,
                new[] { "Leather Witcher Armor", "Wolf Pelt", "Iron Ore" }));
            Register(new CraftingRecipe(
                "thunder",
                "Thunder",
                "Thunder",
                false,
                string.Empty,
                new[] { "Wolf Fang", "Ash Salt" }));
            Register(new CraftingRecipe(
                "cat",
                "Cat",
                "Cat",
                false,
                string.Empty,
                new[] { "Bogweed", "Mirror Shard" }));
            Register(new CraftingRecipe(
                "ash_bomb",
                "Ash Bomb",
                "Ash Bomb",
                false,
                string.Empty,
                new[] { "Ash Salt", "Iron Ore" }));
            Register(new CraftingRecipe(
                "light_bomb",
                "Light Bomb",
                "Light Bomb",
                false,
                string.Empty,
                new[] { "Ash Salt", "Undead Bone" }));
            Register(new CraftingRecipe(
                "undead_oil",
                "Undead Oil",
                "Undead Oil",
                false,
                string.Empty,
                new[] { "Undead Bone", "Bogweed" }));
            Register(new CraftingRecipe(
                "swamp_oil",
                "Bog Creature Oil",
                "Bog Creature Oil",
                false,
                "swamp_oil",
                new[] { "Drowner Slime", "Bogweed" }));
            Register(new CraftingRecipe(
                "hanged_man_oil",
                "Hanged Man Oil",
                "Hanged Man Oil",
                false,
                string.Empty,
                new[] { "Wolf Fang", "Iron Ore" }));
            Register(new CraftingRecipe(
                "swamp_cloak",
                "Swamp Cloak",
                "Swamp Cloak",
                false,
                string.Empty,
                new[] { "Wolf Pelt", "Bogweed", "Drowner Slime" }));
            Register(new CraftingRecipe(
                "improved_silver_sword",
                "Improved Silver Sword",
                "Improved Silver Sword",
                true,
                string.Empty,
                new[] { "Witcher Silver Sword", "Mirror Shard", "Undead Bone" }));
        }

        private void Register(CraftingRecipe recipe)
        {
            recipes[recipe.Id] = recipe;
            recipeOrder.Add(recipe);
        }
    }

    public sealed class CraftingRecipe
    {
        public CraftingRecipe(string id, string displayName, string resultItem, bool resultIsWeapon, string requiredUnlockedRecipe, string[] ingredients)
        {
            Id = id;
            DisplayName = displayName;
            ResultItem = resultItem;
            ResultIsWeapon = resultIsWeapon;
            RequiredUnlockedRecipe = requiredUnlockedRecipe;
            Ingredients = ingredients ?? Array.Empty<string>();
            CraftedFlag = "crafted_" + id;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string ResultItem { get; }
        public bool ResultIsWeapon { get; }
        public string RequiredUnlockedRecipe { get; }
        public string[] Ingredients { get; }
        public string CraftedFlag { get; }
    }
}

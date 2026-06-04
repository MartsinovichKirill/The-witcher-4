using System;
using System.Collections.Generic;
using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;
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
                reason = "Unknown recipe.";
                return false;
            }

            var flags = DecisionFlagService.Instance;
            if (flags != null && flags.HasFlag(recipe.CraftedFlag))
            {
                reason = $"{recipe.ResultItem} is already crafted.";
                return false;
            }

            var rewards = PlayerRewardService.Instance;
            if (!string.IsNullOrWhiteSpace(recipe.RequiredUnlockedRecipe)
                && (rewards == null || !rewards.HasRecipe(recipe.RequiredUnlockedRecipe)))
            {
                reason = $"{recipe.DisplayName} recipe is locked.";
                return false;
            }

            var inventory = InventoryService.Instance;
            if (inventory == null)
            {
                reason = "Inventory is not ready.";
                return false;
            }

            for (var i = 0; i < recipe.Ingredients.Length; i++)
            {
                if (!inventory.HasItem(recipe.Ingredients[i]))
                {
                    reason = $"Missing ingredient: {recipe.Ingredients[i]}.";
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
                inventory.RemoveItem(recipe.Ingredients[i]);
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
            InteractionPromptUI.Instance?.ShowMessage($"Crafted: {recipe.ResultItem}.");
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

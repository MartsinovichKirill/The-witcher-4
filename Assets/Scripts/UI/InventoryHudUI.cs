using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Crafting;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Localization;
using WitcherRightVersion.Quest;

namespace WitcherRightVersion.UI
{
    public sealed class InventoryHudUI : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private KeyCode nextPageKey = KeyCode.Tab;
        [SerializeField] private KeyCode craftSelectedKey = KeyCode.Return;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text contentText;

        private static readonly HashSet<string> ArmorItems = new HashSet<string>
        {
            "Leather Witcher Armor",
            "Reinforced Armor",
            "Swamp Cloak"
        };

        private static readonly HashSet<string> ConsumableItems = new HashSet<string>
        {
            "Swallow",
            "Antitoxin",
            "Thunder",
            "Cat",
            "Food",
            "Field Ration",
            "Ash Bomb",
            "Light Bomb",
            "Undead Oil",
            "Bog Creature Oil",
            "Hanged Man Oil"
        };

        private static readonly HashSet<string> QuestItems = new HashSet<string>
        {
            "Old Camp Blade",
            "Mirror Shard",
            "Messenger Note",
            "Ritual Key",
            "Orten Diary",
            "Girl's Medallion",
            "Reed Charm",
            "Elder's Seal"
        };

        private readonly StringBuilder builder = new StringBuilder(1024);
        private InventoryPage currentPage;
        private int selectedRecipeIndex;

        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            Hide();
        }

        private void OnDisable()
        {
            IsOpen = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }

            if (panelRoot != null && panelRoot.activeSelf)
            {
                if (Input.GetKeyDown(nextPageKey) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    CyclePage(1);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    CyclePage(-1);
                }

                if (currentPage == InventoryPage.Crafting)
                {
                    HandleCraftingInput();
                }

                Refresh();
            }
        }

        private void Toggle()
        {
            if (panelRoot == null)
            {
                return;
            }

            var nextState = !panelRoot.activeSelf;
            panelRoot.SetActive(nextState);
            IsOpen = nextState;
            if (nextState)
            {
                Refresh();
            }
        }

        private void CyclePage(int direction)
        {
            var pageCount = System.Enum.GetValues(typeof(InventoryPage)).Length;
            var next = ((int)currentPage + direction) % pageCount;
            if (next < 0)
            {
                next += pageCount;
            }

            currentPage = (InventoryPage)next;
            selectedRecipeIndex = 0;
        }

        private void HandleCraftingInput()
        {
            var crafting = CraftingService.Instance;
            if (crafting == null || crafting.Recipes.Count == 0)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedRecipeIndex = (selectedRecipeIndex + 1) % crafting.Recipes.Count;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedRecipeIndex--;
                if (selectedRecipeIndex < 0)
                {
                    selectedRecipeIndex = crafting.Recipes.Count - 1;
                }
            }
            else if (Input.GetKeyDown(craftSelectedKey) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                selectedRecipeIndex = Mathf.Clamp(selectedRecipeIndex, 0, crafting.Recipes.Count - 1);
                crafting.Craft(crafting.Recipes[selectedRecipeIndex].Id);
            }
        }

        private void Refresh()
        {
            if (contentText == null)
            {
                return;
            }

            builder.Clear();
            AppendHeader();

            switch (currentPage)
            {
                case InventoryPage.Overview:
                    AppendOverview();
                    break;
                case InventoryPage.Gear:
                    AppendGear();
                    break;
                case InventoryPage.Items:
                    AppendItems();
                    break;
                case InventoryPage.Crafting:
                    AppendCrafting();
                    break;
            }

            contentText.text = builder.ToString();
        }

        private void AppendHeader()
        {
            builder.Append(L("Inventory")).Append(" - ").Append(GetPageName(currentPage)).Append(" ");
            builder.Append((int)currentPage + 1).Append("/").Append(System.Enum.GetValues(typeof(InventoryPage)).Length).AppendLine();
            builder.AppendLine("------------------------");
        }

        private void AppendOverview()
        {
            var rewards = PlayerRewardService.Instance;
            var inventory = InventoryService.Instance;
            var quest = QuestService.Instance;

            builder.Append(L("Level")).Append(": ").Append(rewards != null ? rewards.Level : 1).Append("    ");
            builder.Append(L("Skill points")).Append(": ").Append(rewards != null ? rewards.SkillPoints : 0).AppendLine();
            builder.Append(L("XP")).Append(": ").Append(rewards != null ? rewards.Experience : 0);
            builder.Append(" (").Append(rewards != null ? rewards.ExperienceIntoLevel : 0).Append("/");
            builder.Append(PlayerRewardService.ExperiencePerLevel).Append(")").AppendLine();
            builder.Append(L("Coins")).Append(": ").Append(rewards != null ? rewards.Coins : 0).AppendLine();
            builder.Append(L("Equipped")).Append(": ").Append(L(inventory != null ? inventory.EquippedWeapon : "None")).AppendLine();
            builder.Append(L("Active quest")).Append(": ").Append(L(quest != null && quest.HasActiveQuest ? quest.ActiveQuestTitle : "None")).AppendLine();
            builder.AppendLine();

            builder.Append(L("Weapons")).Append(": ").Append(inventory != null ? inventory.Weapons.Count : 0).AppendLine();
            builder.Append(L("Items")).Append(": ").Append(inventory != null ? inventory.Items.Count : 0).AppendLine();
            builder.Append(L("Known recipes")).Append(": ").Append(GetKnownRecipeCount()).AppendLine();
        }

        private void AppendGear()
        {
            var inventory = InventoryService.Instance;
            builder.Append(L("Equipped weapon")).Append(": ").Append(L(inventory != null ? inventory.EquippedWeapon : "None")).AppendLine();
            builder.AppendLine();
            builder.Append(L("Weapons")).AppendLine(":");

            if (inventory == null || inventory.Weapons.Count == 0)
            {
                builder.Append("- ").AppendLine(L("None"));
            }
            else
            {
                for (var i = 0; i < inventory.Weapons.Count; i++)
                {
                    builder.Append("- ").Append(L(inventory.Weapons[i])).AppendLine();
                }
            }

            builder.AppendLine();
            builder.Append(L("Armor")).AppendLine(":");
            AppendFilteredItems(item => ArmorItems.Contains(item), "None");
        }

        private void AppendItems()
        {
            builder.Append(L("Consumables")).AppendLine(":");
            AppendFilteredItems(item => ConsumableItems.Contains(item), "None");
            builder.AppendLine();
            builder.Append(L("Resources")).AppendLine(":");
            AppendFilteredItems(item => !ArmorItems.Contains(item) && !ConsumableItems.Contains(item) && !QuestItems.Contains(item), "None");
            builder.AppendLine();
            builder.Append(L("Quest items")).AppendLine(":");
            AppendFilteredItems(item => QuestItems.Contains(item), "None");
        }

        private void AppendCrafting()
        {
            var crafting = CraftingService.Instance;
            if (crafting == null || crafting.Recipes.Count == 0)
            {
                builder.AppendLine(L("No recipes available."));
                return;
            }

            selectedRecipeIndex = Mathf.Clamp(selectedRecipeIndex, 0, crafting.Recipes.Count - 1);

            for (var i = 0; i < crafting.Recipes.Count; i++)
            {
                var recipe = crafting.Recipes[i];
                var canCraft = crafting.CanCraft(recipe.Id, out var reason);

                builder.Append(i == selectedRecipeIndex ? "> " : "  ");
                builder.Append(canCraft ? L("[READY] ") : L("[LOCKED] "));
                builder.Append(L(recipe.DisplayName)).Append(" -> ").Append(L(recipe.ResultItem)).AppendLine();
                builder.Append("  ").Append(L("Needs")).Append(": ");

                for (var ingredientIndex = 0; ingredientIndex < recipe.Ingredients.Length; ingredientIndex++)
                {
                    if (ingredientIndex > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(L(recipe.Ingredients[ingredientIndex]));
                }

                builder.AppendLine();
                if (!canCraft)
                {
                    builder.Append("  ").Append(L(reason)).AppendLine();
                }

                if (i < crafting.Recipes.Count - 1)
                {
                    builder.AppendLine();
                }
            }

            builder.AppendLine();
            builder.Append(L("Use Up/Down to select. Enter crafts selected recipe.")).AppendLine();
        }

        private void AppendFilteredItems(System.Func<string, bool> predicate, string emptyText)
        {
            var inventory = InventoryService.Instance;
            var count = 0;

            if (inventory == null || inventory.Items.Count == 0)
            {
                builder.Append("- ").Append(L(emptyText)).AppendLine();
                return;
            }

            for (var i = 0; i < inventory.Items.Count; i++)
            {
                var item = inventory.Items[i];
                if (!predicate(item))
                {
                    continue;
                }

                builder.Append("- ").Append(L(item)).AppendLine();
                count++;
            }

            if (count == 0)
            {
                builder.Append("- ").Append(L(emptyText)).AppendLine();
            }
        }

        private static string L(string english)
        {
            return GameLocalization.Text(english);
        }

        private static string GetPageName(InventoryPage page)
        {
            switch (page)
            {
                case InventoryPage.Gear:
                    return L("Gear");
                case InventoryPage.Items:
                    return L("Items");
                case InventoryPage.Crafting:
                    return L("Crafting");
                default:
                    return L("Overview");
            }
        }

        private int GetKnownRecipeCount()
        {
            var crafting = CraftingService.Instance;
            if (crafting == null)
            {
                return 0;
            }

            var known = 0;
            for (var i = 0; i < crafting.Recipes.Count; i++)
            {
                var recipe = crafting.Recipes[i];
                if (string.IsNullOrWhiteSpace(recipe.RequiredUnlockedRecipe)
                    || (PlayerRewardService.Instance != null && PlayerRewardService.Instance.HasRecipe(recipe.RequiredUnlockedRecipe)))
                {
                    known++;
                }
            }

            return known;
        }

        private void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            IsOpen = false;
        }

        private enum InventoryPage
        {
            Overview,
            Gear,
            Items,
            Crafting
        }
    }
}

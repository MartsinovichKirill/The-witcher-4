using UnityEngine;
using WitcherRightVersion.Interaction;

namespace WitcherRightVersion.Crafting
{
    public sealed class CraftingInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Crafting station";
        [SerializeField] private string interactionPrompt = "Craft";
        [SerializeField] private string recipeId = "swallow";

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => true;

        public void Configure(string newDisplayName, string newPrompt, string newRecipeId)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            recipeId = newRecipeId;
        }

        public void Interact(InteractionController interactor)
        {
            CraftingService.Instance?.Craft(recipeId);
        }
    }
}

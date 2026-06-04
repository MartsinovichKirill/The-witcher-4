using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class EndingAltarInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Final truth altar";
        [SerializeField] private string interactionPrompt = "Choose truth";

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => true;

        public void Configure(string newDisplayName, string newPrompt)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
        }

        public void Interact(InteractionController interactor)
        {
            if (EndingService.Instance == null)
            {
                InteractionPromptUI.Instance?.ShowMessage("Nothing answers.");
                return;
            }

            EndingService.Instance.CompleteTruthEnding();
        }
    }
}

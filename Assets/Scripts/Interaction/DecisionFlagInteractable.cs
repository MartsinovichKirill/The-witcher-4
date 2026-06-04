using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class DecisionFlagInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Evidence";
        [SerializeField] private string interactionPrompt = "Inspect";
        [SerializeField] private string flagToSet = "EvidenceFound";
        [SerializeField] private string interactionMessage = "Evidence recorded.";
        [SerializeField] private bool canRepeat;
        [SerializeField] private bool completed;

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => canRepeat || !completed;

        public void Configure(string newDisplayName, string newPrompt, string newFlagToSet, string newMessage, bool newCanRepeat = false)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            flagToSet = newFlagToSet;
            interactionMessage = newMessage;
            canRepeat = newCanRepeat;
        }

        public void Interact(InteractionController interactor)
        {
            if (!CanInteract)
            {
                return;
            }

            DecisionFlagService.Instance?.SetFlag(flagToSet);
            completed = true;
            InteractionPromptUI.Instance?.ShowMessage(interactionMessage);
            Debug.Log($"Decision evidence recorded: {flagToSet}", this);
        }
    }
}

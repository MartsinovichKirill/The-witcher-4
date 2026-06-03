using UnityEngine;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Quest
{
    public sealed class QuestProgressInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Quest object";
        [SerializeField] private string interactionPrompt = "Inspect";
        [SerializeField] private string questAction;
        [SerializeField] private string successMessage = "Quest updated.";
        [SerializeField] private string blockedMessage = "Nothing useful yet.";
        [SerializeField] private bool canRepeat;

        private bool completed;

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => canRepeat || !completed;

        public void Configure(string newDisplayName, string newPrompt, string newQuestAction, string newSuccessMessage, string newBlockedMessage, bool newCanRepeat = false)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            questAction = newQuestAction;
            successMessage = newSuccessMessage;
            blockedMessage = newBlockedMessage;
            canRepeat = newCanRepeat;
        }

        public void Interact(InteractionController interactor)
        {
            if (!CanInteract)
            {
                return;
            }

            var advanced = QuestService.Instance != null && QuestService.Instance.RunAction(questAction);
            if (advanced && !canRepeat)
            {
                completed = true;
            }

            InteractionPromptUI.Instance?.ShowMessage(advanced ? successMessage : blockedMessage);
        }
    }
}

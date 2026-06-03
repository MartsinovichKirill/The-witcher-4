using UnityEngine;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.Quest;

namespace WitcherRightVersion.Dialogue
{
    public sealed class DialogueInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Speaker";
        [SerializeField] private string interactionPrompt = "Talk";
        [SerializeField] private string startNodeId = "start";
        [SerializeField] private string returnToElderNodeId;
        [SerializeField] private string completedQuestNodeId;
        [SerializeField] private DialogueNode[] dialogueNodes;

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => DialogueService.Instance != null && !DialogueService.Instance.IsDialogueOpen;

        public void Configure(string newDisplayName, string newPrompt, string newStartNodeId, DialogueNode[] newDialogueNodes, string newReturnToElderNodeId = "", string newCompletedQuestNodeId = "")
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            startNodeId = newStartNodeId;
            dialogueNodes = newDialogueNodes;
            returnToElderNodeId = newReturnToElderNodeId;
            completedQuestNodeId = newCompletedQuestNodeId;
        }

        public void Interact(InteractionController interactor)
        {
            if (DialogueService.Instance == null)
            {
                Debug.LogWarning("DialogueService is missing from the scene.", this);
                return;
            }

            DialogueService.Instance.StartDialogue(dialogueNodes, ResolveStartNodeId());
        }

        private string ResolveStartNodeId()
        {
            var quest = QuestService.Instance;
            if (quest == null)
            {
                return startNodeId;
            }

            if (!string.IsNullOrWhiteSpace(completedQuestNodeId) && quest.SwampContractState == QuestState.Completed)
            {
                return completedQuestNodeId;
            }

            if (!string.IsNullOrWhiteSpace(returnToElderNodeId)
                && quest.SwampContractState == QuestState.Active
                && (quest.CurrentSwampContractStage == SwampContractStage.ReturnToElder
                    || quest.CurrentSwampContractStage == SwampContractStage.ChooseResponse
                    || quest.CurrentSwampContractStage == SwampContractStage.ReceiveReward))
            {
                return returnToElderNodeId;
            }

            return startNodeId;
        }
    }
}

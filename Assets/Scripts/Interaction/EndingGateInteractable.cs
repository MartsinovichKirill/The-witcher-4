using UnityEngine;
using UnityEngine.SceneManagement;
using WitcherRightVersion.Core;
using WitcherRightVersion.Localization;
using WitcherRightVersion.Quest;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class EndingGateInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Ash Road";
        [SerializeField] private string interactionPrompt = "Travel";
        [SerializeField] private string targetSceneName = "AshRoadScene";

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => true;

        public void Configure(string newDisplayName, string newPrompt, string newTargetSceneName)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            targetSceneName = newTargetSceneName;
        }

        public void Interact(InteractionController interactor)
        {
            if (!HasTruthRouteRequirements())
            {
                InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("The road is not ready. Finish the swamp contract and question the elder's story first.", "Дорога ещё закрыта. Заверши болотный контракт и сначала усомнись в версии старосты."));
                return;
            }

            EndingService.UnlockTruthRoute();
            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("The ash road opens. The village will have to hear the truth.", "Пепельный тракт открыт. Деревне придётся услышать правду."));
            SaveService.PrepareSceneTransfer();
            SceneManager.LoadScene(targetSceneName);
        }

        private static bool HasTruthRouteRequirements()
        {
            var quest = QuestService.Instance;
            var flags = DecisionFlagService.Instance;

            return quest != null
                && flags != null
                && quest.SwampContractState == QuestState.Completed
                && flags.HasFlag("questionedElderVersion");
        }
    }
}

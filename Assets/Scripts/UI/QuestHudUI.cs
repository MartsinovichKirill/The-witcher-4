using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Quest;

namespace WitcherRightVersion.UI
{
    public sealed class QuestHudUI : MonoBehaviour
    {
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text objectiveText;

        private string lastTitle;
        private string lastObjective;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            var service = QuestService.Instance;
            if (service == null || !service.HasActiveQuest)
            {
                Hide();
                return;
            }

            if (hudRoot != null && !hudRoot.activeSelf)
            {
                hudRoot.SetActive(true);
            }

            UpdateText(service.ActiveQuestTitle, service.CurrentObjective);
        }

        private void UpdateText(string title, string objective)
        {
            if (title == lastTitle && objective == lastObjective)
            {
                return;
            }

            lastTitle = title;
            lastObjective = objective;

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (objectiveText != null)
            {
                objectiveText.text = objective;
            }
        }

        private void Hide()
        {
            lastTitle = string.Empty;
            lastObjective = string.Empty;

            if (hudRoot != null)
            {
                hudRoot.SetActive(false);
            }
        }
    }
}

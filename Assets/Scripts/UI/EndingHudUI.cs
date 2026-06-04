using UnityEngine;
using UnityEngine.UI;

namespace WitcherRightVersion.UI
{
    public sealed class EndingHudUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        public static EndingHudUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Hide();
        }

        public void ShowEnding(string endingType)
        {
            if (panelRoot == null)
            {
                return;
            }

            panelRoot.SetActive(true);

            if (titleText != null)
            {
                titleText.text = endingType == "Truth" ? "Truth Ending" : "Ending Reached";
            }

            if (bodyText != null)
            {
                bodyText.text = endingType == "Truth"
                    ? "Reynard exposes the first lie buried under Velemar. The curse begins to break, but Heather Ford must now survive the truth it tried to erase."
                    : "Reynard's choice changes Velemar. The final version of the story is now written.";
            }
        }

        private void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}

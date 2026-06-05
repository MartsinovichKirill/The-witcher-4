using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Localization;

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
                titleText.text = GameLocalization.Text(GetTitle(endingType));
            }

            if (bodyText != null)
            {
                bodyText.text = GameLocalization.Text(GetBody(endingType));
            }
        }

        private static string GetTitle(string endingType)
        {
            switch (endingType)
            {
                case EndingService.TruthEndingType:
                    return "Truth Ending";
                case EndingService.LieEndingType:
                    return "Corrected Story Ending";
                case EndingService.SacrificeEndingType:
                    return "Sacrifice Ending";
                default:
                    return "Ending Reached";
            }
        }

        private static string GetBody(string endingType)
        {
            switch (endingType)
            {
                case EndingService.TruthEndingType:
                    return "Reynard exposes the first lie buried under Velemar. The curse begins to break, but Heather Ford must now survive the truth it tried to erase.";
                case EndingService.LieEndingType:
                    return "Reynard lets the elder seal the useful version of history. The village survives, but the mirror keeps one more polished lie.";
                case EndingService.SacrificeEndingType:
                    return "Reynard destroys the mirror's heart. The curse breaks hard and fast, taking the weakest infected with it before silence returns.";
                default:
                    return "Reynard's choice changes Velemar. The final version of the story is now written.";
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

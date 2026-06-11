using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Localization;

namespace WitcherRightVersion.UI
{
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedStaticText : MonoBehaviour
    {
        [SerializeField] private string englishText;
        [SerializeField] private string russianText;

        private Text target;

        public void Configure(string english, string russian)
        {
            englishText = english;
            russianText = russian;
            Refresh();
        }

        private void Awake()
        {
            target = GetComponent<Text>();
            GameLocalization.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            GameLocalization.LanguageChanged -= Refresh;
        }

        private void Refresh()
        {
            if (target == null)
            {
                target = GetComponent<Text>();
            }

            if (target != null)
            {
                target.text = GameLocalization.Select(englishText, russianText);
            }
        }
    }
}

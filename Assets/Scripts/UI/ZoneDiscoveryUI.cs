using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Localization;

namespace WitcherRightVersion.UI
{
    public sealed class ZoneDiscoveryUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text zoneText;
        [SerializeField] private float visibleSeconds = 2.8f;
        [SerializeField] private float fadeSpeed = 2.6f;

        private string englishZoneName;
        private string russianZoneName;
        private float hideTime;

        public static ZoneDiscoveryUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            SetAlpha(0f);
            GameLocalization.LanguageChanged += RefreshLanguage;
        }

        private void OnDestroy()
        {
            GameLocalization.LanguageChanged -= RefreshLanguage;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            var targetAlpha = Time.unscaledTime < hideTime ? 1f : 0f;
            SetAlpha(Mathf.MoveTowards(canvasGroup != null ? canvasGroup.alpha : 0f, targetAlpha, fadeSpeed * Time.unscaledDeltaTime));
        }

        public void Show(string englishName, string russianName)
        {
            englishZoneName = englishName;
            russianZoneName = russianName;
            RefreshLanguage();
            hideTime = Time.unscaledTime + visibleSeconds;
            SetAlpha(1f);
        }

        private void RefreshLanguage()
        {
            if (zoneText != null)
            {
                zoneText.text = GameLocalization.Select(englishZoneName, russianZoneName);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}

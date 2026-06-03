using UnityEngine;
using UnityEngine.UI;

namespace WitcherRightVersion.UI
{
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text actionText;
        [SerializeField] private GameObject messageRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private float messageDuration = 2.5f;

        private float messageHideTime;

        public static InteractionPromptUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            HidePrompt();
            HideMessage();
        }

        private void Update()
        {
            if (messageRoot != null && messageRoot.activeSelf && Time.time >= messageHideTime)
            {
                HideMessage();
            }
        }

        public void ShowPrompt(string title, string action, KeyCode keyCode)
        {
            if (promptRoot == null)
            {
                return;
            }

            promptRoot.SetActive(true);

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (actionText != null)
            {
                actionText.text = $"{keyCode}: {action}";
            }
        }

        public void HidePrompt()
        {
            if (promptRoot != null)
            {
                promptRoot.SetActive(false);
            }
        }

        public void ShowMessage(string message)
        {
            if (messageRoot == null)
            {
                return;
            }

            messageRoot.SetActive(true);
            messageHideTime = Time.time + messageDuration;

            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        private void HideMessage()
        {
            if (messageRoot != null)
            {
                messageRoot.SetActive(false);
            }
        }
    }
}

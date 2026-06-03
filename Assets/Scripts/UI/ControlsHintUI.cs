using UnityEngine;

namespace WitcherRightVersion.UI
{
    public sealed class ControlsHintUI : MonoBehaviour
    {
        [SerializeField] private GameObject hintRoot;
        [SerializeField] private float initialVisibleSeconds = 14f;

        private float autoHideTime;
        private bool manuallyPinned;

        private void Awake()
        {
            if (hintRoot != null)
            {
                hintRoot.SetActive(true);
            }

            autoHideTime = Time.time + initialVisibleSeconds;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleHint();
                return;
            }

            if (!manuallyPinned && hintRoot != null && hintRoot.activeSelf && Time.time >= autoHideTime)
            {
                hintRoot.SetActive(false);
            }
        }

        private void ToggleHint()
        {
            if (hintRoot == null)
            {
                return;
            }

            var nextState = !hintRoot.activeSelf;
            hintRoot.SetActive(nextState);
            manuallyPinned = nextState;
        }
    }
}

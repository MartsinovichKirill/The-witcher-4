using UnityEngine;

namespace WitcherRightVersion.Core
{
    public sealed class FlagConditionalObject : MonoBehaviour
    {
        [SerializeField] private string requiredFlag;
        [SerializeField] private bool invertCondition;

        private Renderer[] renderers;
        private Collider[] colliders;
        private Light[] lights;
        private bool lastVisibleState = true;

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            RefreshVisibility(true);
        }

        private void Update()
        {
            RefreshVisibility(false);
        }

        public void Configure(string newRequiredFlag, bool newInvertCondition = false)
        {
            requiredFlag = newRequiredFlag;
            invertCondition = newInvertCondition;
            CacheComponents();
            RefreshVisibility(true);
        }

        private void CacheComponents()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            colliders = GetComponentsInChildren<Collider>(true);
            lights = GetComponentsInChildren<Light>(true);
        }

        private void RefreshVisibility(bool force)
        {
            var shouldBeVisible = ShouldBeVisible();
            if (!force && shouldBeVisible == lastVisibleState)
            {
                return;
            }

            lastVisibleState = shouldBeVisible;
            SetVisible(shouldBeVisible);
        }

        private bool ShouldBeVisible()
        {
            var hasFlag = string.IsNullOrWhiteSpace(requiredFlag)
                || DecisionFlagService.Instance != null && DecisionFlagService.Instance.HasFlag(requiredFlag);
            return invertCondition ? !hasFlag : hasFlag;
        }

        private void SetVisible(bool isVisible)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = isVisible;
                }
            }

            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = isVisible;
                }
            }

            for (var i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = isVisible;
                }
            }
        }
    }
}

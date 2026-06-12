using UnityEngine;
using WitcherRightVersion.Core;

namespace WitcherRightVersion.UI
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ZoneDiscoveryTrigger : MonoBehaviour
    {
        [SerializeField] private string englishZoneName;
        [SerializeField] private string russianZoneName;

        private bool shown;

        public void Configure(string englishName, string russianName)
        {
            englishZoneName = englishName;
            russianZoneName = russianName;

            var trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (shown || !other.CompareTag("Player"))
            {
                return;
            }

            shown = true;
            DecisionFlagService.Instance?.SetFlag("visited_" + ToLocationId(englishZoneName));
            ZoneDiscoveryUI.Instance?.Show(englishZoneName, russianZoneName);
        }

        private static string ToLocationId(string zoneName)
        {
            return string.IsNullOrWhiteSpace(zoneName)
                ? "unknown_zone"
                : zoneName.Trim().ToLowerInvariant().Replace(' ', '_');
        }
    }
}

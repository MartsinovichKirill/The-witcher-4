using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Localization;

namespace WitcherRightVersion.UI
{
    public sealed class CombatStatusHudUI : MonoBehaviour
    {
        [SerializeField] private Text statusText;

        private CombatController combat;

        private void Update()
        {
            if (combat == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                combat = player != null ? player.GetComponent<CombatController>() : null;
            }

            if (statusText == null)
            {
                return;
            }

            var inventory = InventoryService.Instance;
            var weapon = inventory != null ? GameLocalization.Text(inventory.EquippedWeapon) : GameLocalization.Text("None");
            var armor = inventory != null ? GameLocalization.Text(inventory.EquippedArmor) : GameLocalization.Text("None");
            var effects = BuildEffects();

            statusText.text = GameLocalization.Select(
                $"Weapon: {weapon}  [T]\nArmor: {armor}  [X]\nEffects: {effects}\nQ Aard | R Igni | 1-7 quick items",
                $"Меч: {weapon}  [T]\nБроня: {armor}  [X]\nЭффекты: {effects}\nQ Аард | R Игни | 1-7 быстрые предметы");
        }

        public void Configure(Text newStatusText)
        {
            statusText = newStatusText;
        }

        private string BuildEffects()
        {
            if (combat == null)
            {
                return GameLocalization.Text("None");
            }

            var result = string.Empty;
            AppendEffect(ref result, combat.IsPoisoned, GameLocalization.Select("Poison", "Яд"));
            AppendEffect(ref result, combat.IsThunderActive, GameLocalization.Text("Thunder"));
            AppendEffect(ref result, combat.IsCatActive, GameLocalization.Text("Cat"));
            AppendEffect(ref result, !string.IsNullOrWhiteSpace(combat.ActiveOil), GameLocalization.Text(combat.ActiveOil));
            return string.IsNullOrWhiteSpace(result) ? GameLocalization.Text("None") : result;
        }

        private static void AppendEffect(ref string value, bool active, string label)
        {
            if (!active)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                value += ", ";
            }

            value += label;
        }
    }
}

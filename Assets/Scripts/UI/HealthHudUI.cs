using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Localization;

namespace WitcherRightVersion.UI
{
    public sealed class HealthHudUI : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthFill;

        private Health playerHealth;

        private void Update()
        {
            if (playerHealth == null)
            {
                FindPlayerHealth();
            }

            if (playerHealth == null)
            {
                UpdateHealth(0f, 1f);
                return;
            }

            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void FindPlayerHealth()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player != null ? player.GetComponent<Health>() : null;
        }

        private void UpdateHealth(float current, float max)
        {
            var safeMax = Mathf.Max(1f, max);
            var normalized = Mathf.Clamp01(current / safeMax);

            if (healthFill != null)
            {
                healthFill.fillAmount = normalized;
            }

            if (healthText != null)
            {
                healthText.text = GameLocalization.Select(
                    $"Reynard HP {current:0}/{safeMax:0}",
                    $"Рейнард: здоровье {current:0}/{safeMax:0}");
            }
        }
    }
}

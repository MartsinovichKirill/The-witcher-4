using System;
using UnityEngine;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Localization;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Combat
{
    public sealed class CombatController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode lightAttackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode heavyAttackKey = KeyCode.F;
        [SerializeField] private KeyCode blockKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode dodgeKey = KeyCode.Space;
        [SerializeField] private KeyCode aardKey = KeyCode.Q;
        [SerializeField] private KeyCode igniKey = KeyCode.R;
        [SerializeField] private KeyCode switchSwordKey = KeyCode.T;
        [SerializeField] private KeyCode switchArmorKey = KeyCode.X;
        [SerializeField] private KeyCode swallowKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode thunderKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode antitoxinKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode bombKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode oilKey = KeyCode.Alpha5;
        [SerializeField] private KeyCode catKey = KeyCode.Alpha6;
        [SerializeField] private KeyCode foodKey = KeyCode.Alpha7;

        [Header("Attacks")]
        [SerializeField] private float lightAttackDamage = 24f;
        [SerializeField] private float lightAttackRange = 2.45f;
        [SerializeField] private float lightAttackRadius = 1.25f;
        [SerializeField] private float lightAttackCooldown = 0.65f;
        [SerializeField] private float heavyAttackDamage = 42f;
        [SerializeField] private float heavyAttackRange = 2.35f;
        [SerializeField] private float heavyAttackRadius = 1.2f;
        [SerializeField] private float heavyAttackCooldown = 1.35f;
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Block")]
        [SerializeField] private float blockDamageMultiplier = 0.45f;

        [Header("Dodge")]
        [SerializeField] private float dodgeDistance = 2.35f;
        [SerializeField] private float dodgeDuration = 0.18f;
        [SerializeField] private float dodgeCooldown = 1.2f;
        [SerializeField] private Transform cameraTransform;

        [Header("Aard")]
        [SerializeField] private float aardDamage = 8f;
        [SerializeField] private float aardRange = 3.65f;
        [SerializeField] private float aardRadius = 1.55f;
        [SerializeField] private float aardCooldown = 4f;
        [SerializeField] private float aardKnockbackDistance = 1.4f;
        [SerializeField] private float aardStunDuration = 0.8f;

        [Header("Igni")]
        [SerializeField] private float igniDamage = 28f;
        [SerializeField] private float igniRange = 3.4f;
        [SerializeField] private float igniRadius = 1.55f;
        [SerializeField] private float igniCooldown = 5f;

        [Header("Consumables")]
        [SerializeField] private float swallowHealAmount = 35f;
        [SerializeField] private float foodHealAmount = 15f;
        [SerializeField] private float thunderDuration = 35f;
        [SerializeField] private float thunderDamageMultiplier = 1.25f;
        [SerializeField] private float oilDuration = 55f;
        [SerializeField] private float oilDamageMultiplier = 1.3f;
        [SerializeField] private float catDuration = 60f;
        [SerializeField] private float bombRadius = 3.1f;
        [SerializeField] private float ashBombDamage = 34f;
        [SerializeField] private float lightBombDamage = 48f;

        [Header("Status Effects")]
        [SerializeField] private float poisonTickInterval = 1f;

        private readonly Collider[] hitResults = new Collider[12];
        private CharacterController characterController;
        private Health ownHealth;
        private float nextLightAttackTime;
        private float nextHeavyAttackTime;
        private float nextDodgeTime;
        private float dodgeEndTime;
        private float nextAardTime;
        private float nextIgniTime;
        private Vector3 dodgeVelocity;
        private float thunderEndTime;
        private float oilEndTime;
        private string activeOilItem;
        private float poisonEndTime;
        private float poisonDamagePerTick;
        private float nextPoisonTickTime;
        private float catEndTime;
        private Light catLight;

        public bool IsBlocking { get; private set; }
        public bool IsDodging { get; private set; }
        public bool IsPoisoned => Time.time < poisonEndTime;
        public bool IsThunderActive => Time.time < thunderEndTime;
        public bool IsCatActive => Time.time < catEndTime;
        public string ActiveOil => Time.time < oilEndTime ? activeOilItem : string.Empty;

        public event Action LightAttackStarted;
        public event Action HeavyAttackStarted;
        public event Action DodgeStarted;
        public event Action AardStarted;
        public event Action IgniStarted;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            ownHealth = GetComponent<Health>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            UpdateStatusEffects();

            if (ownHealth != null && ownHealth.IsDead)
            {
                IsBlocking = false;
                return;
            }

            if (DialogueService.Instance != null && DialogueService.Instance.IsDialogueOpen)
            {
                IsBlocking = false;
                return;
            }

            if (InventoryHudUI.IsOpen || GameplayMenuUI.IsOpen)
            {
                IsBlocking = false;
                return;
            }

            if (IsDodging)
            {
                ContinueDodge();
                return;
            }

            IsBlocking = Input.GetKey(blockKey);

            if (Input.GetKeyDown(dodgeKey) && Time.time >= nextDodgeTime)
            {
                StartDodge();
                return;
            }

            if (IsBlocking)
            {
                return;
            }

            if (Input.GetKeyDown(lightAttackKey) && Time.time >= nextLightAttackTime)
            {
                LightAttackStarted?.Invoke();
                Attack(lightAttackDamage, lightAttackRange, lightAttackRadius, "Light attack missed.");
                nextLightAttackTime = Time.time + lightAttackCooldown;
                return;
            }

            if (Input.GetKeyDown(heavyAttackKey) && Time.time >= nextHeavyAttackTime)
            {
                HeavyAttackStarted?.Invoke();
                Attack(heavyAttackDamage, heavyAttackRange, heavyAttackRadius, "Heavy attack missed.");
                nextHeavyAttackTime = Time.time + heavyAttackCooldown;
                return;
            }

            if (Input.GetKeyDown(aardKey) && Time.time >= nextAardTime)
            {
                AardStarted?.Invoke();
                CastAard();
                nextAardTime = Time.time + aardCooldown;
                return;
            }

            if (Input.GetKeyDown(igniKey) && Time.time >= nextIgniTime)
            {
                IgniStarted?.Invoke();
                CastIgni();
                nextIgniTime = Time.time + igniCooldown;
                return;
            }

            if (Input.GetKeyDown(switchSwordKey))
            {
                CycleWeapon();
                return;
            }

            if (Input.GetKeyDown(switchArmorKey))
            {
                CycleArmor();
                return;
            }

            HandleConsumableInput();
        }

        public void ReceiveEnemyAttack(float amount, GameObject source)
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            var progressionMultiplier = WitcherRightVersion.Core.PlayerRewardService.Instance != null
                ? WitcherRightVersion.Core.PlayerRewardService.Instance.IncomingDamageMultiplier
                : 1f;
            var finalDamage = (IsBlocking ? amount * blockDamageMultiplier : amount) * progressionMultiplier * GetArmorDamageMultiplier();
            ownHealth.TakeDamage(finalDamage, source);

            var message = IsBlocking
                ? $"Блок: получено {finalDamage:0} урона."
                : $"Рейнард получил {finalDamage:0} урона.";
            InteractionPromptUI.Instance?.ShowMessage(message);
        }

        private void Attack(float damage, float range, float radius, string missMessage)
        {
            // Sword swoosh on every swing (the impact sound plays separately when a target
            // is actually hit, via Health).
            WitcherRightVersion.Core.AudioFeedbackService.Instance?.PlaySwing();

            var bestTarget = FindBestTarget(range, radius);

            if (bestTarget == null)
            {
                InteractionPromptUI.Instance?.ShowMessage(missMessage);
                return;
            }

            var progressionMultiplier = WitcherRightVersion.Core.PlayerRewardService.Instance != null
                ? WitcherRightVersion.Core.PlayerRewardService.Instance.DamageMultiplier
                : 1f;
            var finalDamage = damage * progressionMultiplier * GetWeaponMultiplier(bestTarget) * GetTimedDamageMultiplier(bestTarget);
            bestTarget.TakeDamage(finalDamage, gameObject);
            InteractionPromptUI.Instance?.ShowMessage($"Удар по цели «{WitcherRightVersion.Localization.GameLocalization.Text(bestTarget.DisplayName)}»: {finalDamage:0} урона.");
        }

        private float GetWeaponMultiplier(Health target)
        {
            var inventory = InventoryService.Instance;
            if (inventory == null || target == null)
            {
                return 1f;
            }

            var enemy = target.GetComponent<EnemyAI>();
            var kind = enemy != null ? enemy.Kind : EnemyKind.Human;
            var equipped = inventory.EquippedWeapon ?? string.Empty;
            var silver = equipped.Contains("Silver");
            var steel = equipped.Contains("Steel");

            if (silver && (kind == EnemyKind.Monster || kind == EnemyKind.Undead || kind == EnemyKind.Specter))
            {
                return 1.25f;
            }

            if (steel && (kind == EnemyKind.Human || kind == EnemyKind.Beast))
            {
                return 1.25f;
            }

            if (silver && (kind == EnemyKind.Human || kind == EnemyKind.Beast))
            {
                return 0.85f;
            }

            if (steel && (kind == EnemyKind.Monster || kind == EnemyKind.Undead || kind == EnemyKind.Specter))
            {
                return 0.85f;
            }

            return 1f;
        }

        private float GetTimedDamageMultiplier(Health target)
        {
            var multiplier = Time.time < thunderEndTime ? thunderDamageMultiplier : 1f;
            if (Time.time >= oilEndTime || string.IsNullOrWhiteSpace(activeOilItem) || target == null)
            {
                return multiplier;
            }

            var enemy = target.GetComponent<EnemyAI>();
            var kind = enemy != null ? enemy.Kind : EnemyKind.Human;
            if ((activeOilItem == "Undead Oil" && (kind == EnemyKind.Undead || kind == EnemyKind.Specter))
                || (activeOilItem == "Bog Creature Oil" && kind == EnemyKind.Monster)
                || (activeOilItem == "Hanged Man Oil" && kind == EnemyKind.Human))
            {
                multiplier *= oilDamageMultiplier;
            }

            return multiplier;
        }

        private void HandleConsumableInput()
        {
            if (Input.GetKeyDown(swallowKey))
            {
                UsePotion("Swallow");
            }
            else if (Input.GetKeyDown(thunderKey))
            {
                UsePotion("Thunder");
            }
            else if (Input.GetKeyDown(antitoxinKey))
            {
                UsePotion("Antitoxin");
            }
            else if (Input.GetKeyDown(bombKey))
            {
                if (!UseBomb("Light Bomb") && !UseBomb("Ash Bomb"))
                {
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("No bomb in inventory.", "В инвентаре нет бомб."));
                }
            }
            else if (Input.GetKeyDown(oilKey))
            {
                if (!UseOil("Bog Creature Oil") && !UseOil("Undead Oil") && !UseOil("Hanged Man Oil"))
                {
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("No oil in inventory.", "В инвентаре нет масел."));
                }
            }
            else if (Input.GetKeyDown(catKey))
            {
                UsePotion("Cat");
            }
            else if (Input.GetKeyDown(foodKey))
            {
                if (!UsePotionSilently("Food") && !UsePotionSilently("Field Ration"))
                {
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("No food in inventory.", "В инвентаре нет еды."));
                }
            }
        }

        private bool UsePotion(string itemName)
        {
            var inventory = InventoryService.Instance;
            if (inventory == null || !inventory.RemoveItem(itemName))
            {
                InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"{itemName} is not in inventory.", $"{GameLocalization.Text(itemName)} нет в инвентаре."));
                return false;
            }

            switch (itemName)
            {
                case "Swallow":
                    ownHealth?.Heal(swallowHealAmount);
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("Swallow restored health.", "Ласточка восстановила здоровье."));
                    break;
                case "Thunder":
                    thunderEndTime = Time.time + thunderDuration;
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("Thunder increases damage.", "Гром временно усилил урон."));
                    break;
                case "Antitoxin":
                    ClearPoison();
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("Antitoxin cleanses poison.", "Противоядие сняло отравление."));
                    break;
                case "Cat":
                    catEndTime = Time.time + catDuration;
                    EnsureCatLight();
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("Cat improves vision in darkness.", "Кошка улучшила зрение в темноте."));
                    break;
                case "Food":
                case "Field Ration":
                    ownHealth?.Heal(foodHealAmount);
                    InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("Food restored some health.", "Еда немного восстановила здоровье."));
                    break;
            }

            return true;
        }

        private bool UsePotionSilently(string itemName)
        {
            var inventory = InventoryService.Instance;
            return inventory != null && inventory.HasItem(itemName) && UsePotion(itemName);
        }

        private bool UseOil(string itemName)
        {
            var inventory = InventoryService.Instance;
            if (inventory == null || !inventory.RemoveItem(itemName))
            {
                return false;
            }

            activeOilItem = itemName;
            oilEndTime = Time.time + oilDuration;
            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"{itemName} applied.", $"{GameLocalization.Text(itemName)} нанесено на клинок."));
            return true;
        }

        private bool UseBomb(string itemName)
        {
            var inventory = InventoryService.Instance;
            if (inventory == null || !inventory.RemoveItem(itemName))
            {
                return false;
            }

            var damage = itemName == "Light Bomb" ? lightBombDamage : ashBombDamage;
            var hitCount = Physics.OverlapSphereNonAlloc(transform.position + transform.forward * 1.6f + Vector3.up, bombRadius, hitResults, targetMask);
            var affected = 0;
            for (var i = 0; i < hitCount; i++)
            {
                var target = hitResults[i] == null ? null : hitResults[i].GetComponentInParent<Health>();
                if (target == null || target == ownHealth || target.IsDead)
                {
                    continue;
                }

                var enemy = target.GetComponent<EnemyAI>();
                var kind = enemy != null ? enemy.Kind : EnemyKind.Human;
                var lightMultiplier = itemName == "Light Bomb" && (kind == EnemyKind.Undead || kind == EnemyKind.Specter) ? 1.4f : 1f;
                var finalDamage = damage * lightMultiplier * GetTimedDamageMultiplier(target);
                target.TakeDamage(finalDamage, gameObject);
                affected++;
            }

            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"{itemName} hit {affected} target(s).", $"{GameLocalization.Text(itemName)} задела целей: {affected}."));
            return true;
        }

        public void ApplyPoison(float duration, float damagePerTick)
        {
            var inventory = InventoryService.Instance;
            var resistance = inventory != null && inventory.EquippedArmor == "Swamp Cloak" ? 0.45f : 1f;
            poisonEndTime = Mathf.Max(poisonEndTime, Time.time + Mathf.Max(0f, duration) * resistance);
            poisonDamagePerTick = Mathf.Max(poisonDamagePerTick, Mathf.Max(0f, damagePerTick) * resistance);
            nextPoisonTickTime = Mathf.Min(nextPoisonTickTime <= 0f ? Time.time + poisonTickInterval : nextPoisonTickTime, Time.time + poisonTickInterval);
            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select("Poisoned.", "Отравление."));
        }

        private void ClearPoison()
        {
            poisonEndTime = 0f;
            poisonDamagePerTick = 0f;
            nextPoisonTickTime = 0f;
        }

        private void UpdateStatusEffects()
        {
            if (IsPoisoned && ownHealth != null && !ownHealth.IsDead && Time.time >= nextPoisonTickTime)
            {
                ownHealth.TakeDamage(poisonDamagePerTick, gameObject);
                nextPoisonTickTime = Time.time + poisonTickInterval;
            }

            if (!IsPoisoned && poisonDamagePerTick > 0f)
            {
                ClearPoison();
            }

            if (catLight != null)
            {
                catLight.enabled = Time.time < catEndTime;
            }
        }

        private float GetArmorDamageMultiplier()
        {
            var armor = InventoryService.Instance != null ? InventoryService.Instance.EquippedArmor : string.Empty;
            switch (armor)
            {
                case "Reinforced Armor":
                    return 0.78f;
                case "Swamp Cloak":
                    return 0.88f;
                case "Leather Witcher Armor":
                    return 0.92f;
                default:
                    return 1f;
            }
        }

        private void CycleWeapon()
        {
            var equipped = InventoryService.Instance?.CycleWeapon();
            if (!string.IsNullOrWhiteSpace(equipped))
            {
                InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"Equipped: {equipped}.", $"Экипировано: {GameLocalization.Text(equipped)}."));
            }
        }

        private void CycleArmor()
        {
            var equipped = InventoryService.Instance?.CycleArmor();
            if (!string.IsNullOrWhiteSpace(equipped))
            {
                InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"Armor equipped: {equipped}.", $"Надета броня: {GameLocalization.Text(equipped)}."));
            }
        }

        private void EnsureCatLight()
        {
            if (catLight != null)
            {
                catLight.enabled = true;
                return;
            }

            var lightObject = new GameObject("ReynardCatVisionLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.8f, 0.35f);
            catLight = lightObject.AddComponent<Light>();
            catLight.type = LightType.Point;
            catLight.range = 15f;
            catLight.intensity = 1.2f;
            catLight.color = new Color(0.68f, 0.82f, 0.52f, 1f);
            catLight.shadows = LightShadows.None;
        }

        private Health FindBestTarget(float range, float radius)
        {
            var center = transform.position + Vector3.up * 1f + transform.forward * (range * 0.5f);
            var hitCount = Physics.OverlapSphereNonAlloc(center, radius, hitResults, targetMask);
            Health bestTarget = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = hitResults[i] == null ? null : hitResults[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == ownHealth || candidate.IsDead)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < bestDistance)
                {
                    bestTarget = candidate;
                    bestDistance = distance;
                }
            }

            return bestTarget;
        }

        private void StartDodge()
        {
            if (characterController == null)
            {
                return;
            }

            var direction = GetDodgeDirection();
            dodgeVelocity = direction * (dodgeDistance / Mathf.Max(0.01f, dodgeDuration));
            dodgeEndTime = Time.time + dodgeDuration;
            nextDodgeTime = Time.time + dodgeCooldown;
            IsBlocking = false;
            IsDodging = true;
            DodgeStarted?.Invoke();
            InteractionPromptUI.Instance?.ShowMessage("Уклонение.");
        }

        private void ContinueDodge()
        {
            if (characterController != null)
            {
                characterController.Move(dodgeVelocity * Time.deltaTime);
            }

            if (Time.time >= dodgeEndTime)
            {
                IsDodging = false;
                dodgeVelocity = Vector3.zero;
            }
        }

        private Vector3 GetDodgeDirection()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            if (input.sqrMagnitude <= 0.01f)
            {
                return -transform.forward;
            }

            if (cameraTransform == null)
            {
                return input.normalized;
            }

            var forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }

        private void CastAard()
        {
            var center = transform.position + Vector3.up * 1f + transform.forward * (aardRange * 0.5f);
            var hitCount = Physics.OverlapSphereNonAlloc(center, aardRadius, hitResults, targetMask);
            var affected = 0;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = hitResults[i] == null ? null : hitResults[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == ownHealth || candidate.IsDead)
                {
                    continue;
                }

                var offset = candidate.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.01f && Vector3.Dot(transform.forward, offset.normalized) < 0.25f)
                {
                    continue;
                }

                candidate.TakeDamage(aardDamage, gameObject);

                var enemy = candidate.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.ApplyAard(offset.sqrMagnitude > 0.01f ? offset.normalized : transform.forward, aardKnockbackDistance, aardStunDuration);
                }

                affected++;
            }

            InteractionPromptUI.Instance?.ShowMessage(affected > 0 ? $"Аард задел целей: {affected}." : "Аард никого не задел.");
        }

        private void CastIgni()
        {
            var center = transform.position + Vector3.up * 1f + transform.forward * (igniRange * 0.5f);
            var hitCount = Physics.OverlapSphereNonAlloc(center, igniRadius, hitResults, targetMask);
            var affected = 0;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = hitResults[i] == null ? null : hitResults[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == ownHealth || candidate.IsDead)
                {
                    continue;
                }

                var offset = candidate.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.01f && Vector3.Dot(transform.forward, offset.normalized) < 0.35f)
                {
                    continue;
                }

                var enemy = candidate.GetComponent<EnemyAI>();
                var kind = enemy != null ? enemy.Kind : EnemyKind.Human;
                var kindMultiplier = kind == EnemyKind.Beast || kind == EnemyKind.Monster ? 1.15f : 1f;
                candidate.TakeDamage(igniDamage * kindMultiplier * GetTimedDamageMultiplier(candidate), gameObject);
                affected++;
            }

            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select(
                affected > 0 ? $"Igni burned {affected} target(s)." : "Igni hit nothing.",
                affected > 0 ? $"Игни задел целей: {affected}." : "Игни никого не задел."));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.15f, 0.1f, 0.35f);
            var center = transform.position + Vector3.up * 1f + transform.forward * (lightAttackRange * 0.5f);
            Gizmos.DrawWireSphere(center, lightAttackRadius);

            Gizmos.color = new Color(0.25f, 0.55f, 1f, 0.35f);
            var aardCenter = transform.position + Vector3.up * 1f + transform.forward * (aardRange * 0.5f);
            Gizmos.DrawWireSphere(aardCenter, aardRadius);

            Gizmos.color = new Color(1f, 0.4f, 0.05f, 0.35f);
            var igniCenter = transform.position + Vector3.up * 1f + transform.forward * (igniRange * 0.5f);
            Gizmos.DrawWireSphere(igniCenter, igniRadius);
        }
    }
}

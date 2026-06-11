using System.Collections.Generic;
using UnityEngine;

namespace WitcherRightVersion.Inventory
{
    public sealed class InventoryService : MonoBehaviour
    {
        private readonly List<string> weapons = new List<string>();
        private readonly List<string> items = new List<string>();

        public static InventoryService Instance { get; private set; }

        public IReadOnlyList<string> Weapons => weapons;
        public IReadOnlyList<string> Items => items;
        public string EquippedWeapon { get; private set; }
        public string EquippedArmor { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializeStartingInventory();
        }

        private void InitializeStartingInventory()
        {
            weapons.Clear();
            items.Clear();

            weapons.Add("Old Steel Sword");
            weapons.Add("Witcher Silver Sword");
            items.Add("Leather Witcher Armor");

            EquippedWeapon = "Witcher Silver Sword";
            EquippedArmor = "Leather Witcher Armor";
        }

        public bool HasWeapon(string weaponName)
        {
            return !string.IsNullOrWhiteSpace(weaponName) && weapons.Contains(weaponName);
        }

        public bool HasItem(string itemName)
        {
            return !string.IsNullOrWhiteSpace(itemName) && items.Contains(itemName);
        }

        public void AddWeapon(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName) || weapons.Contains(weaponName))
            {
                return;
            }

            weapons.Add(weaponName);
            Debug.Log($"Weapon added: {weaponName}", this);
        }

        public void AddItem(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName) || items.Contains(itemName))
            {
                return;
            }

            items.Add(itemName);
            if (IsArmor(itemName) && string.IsNullOrWhiteSpace(EquippedArmor))
            {
                EquippedArmor = itemName;
            }
            Debug.Log($"Item added: {itemName}", this);
        }

        public bool RemoveItem(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return false;
            }

            var removed = items.Remove(itemName);
            if (removed)
            {
                if (EquippedArmor == itemName)
                {
                    EquippedArmor = items.Find(IsArmor) ?? string.Empty;
                }

                Debug.Log($"Item removed: {itemName}", this);
            }

            return removed;
        }

        public bool RemoveWeapon(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName) || weapons.Count <= 1)
            {
                return false;
            }

            var removed = weapons.Remove(weaponName);
            if (!removed)
            {
                return false;
            }

            if (EquippedWeapon == weaponName)
            {
                EquippedWeapon = weapons[0];
            }

            Debug.Log($"Weapon removed: {weaponName}", this);
            return true;
        }

        public void EquipWeapon(string weaponName)
        {
            if (!HasWeapon(weaponName))
            {
                return;
            }

            EquippedWeapon = weaponName;
            Debug.Log($"Equipped weapon: {EquippedWeapon}", this);
        }

        public bool EquipArmor(string armorName)
        {
            if (!HasItem(armorName) || !IsArmor(armorName))
            {
                return false;
            }

            EquippedArmor = armorName;
            Debug.Log($"Equipped armor: {EquippedArmor}", this);
            return true;
        }

        public string CycleWeapon()
        {
            if (weapons.Count == 0)
            {
                return string.Empty;
            }

            var currentIndex = weapons.IndexOf(EquippedWeapon);
            EquippedWeapon = weapons[(currentIndex + 1 + weapons.Count) % weapons.Count];
            return EquippedWeapon;
        }

        public string CycleArmor()
        {
            var armor = new List<string>();
            for (var i = 0; i < items.Count; i++)
            {
                if (IsArmor(items[i]))
                {
                    armor.Add(items[i]);
                }
            }

            if (armor.Count == 0)
            {
                EquippedArmor = string.Empty;
                return EquippedArmor;
            }

            var currentIndex = armor.IndexOf(EquippedArmor);
            EquippedArmor = armor[(currentIndex + 1 + armor.Count) % armor.Count];
            return EquippedArmor;
        }

        public static bool IsArmor(string itemName)
        {
            return itemName == "Leather Witcher Armor"
                || itemName == "Reinforced Armor"
                || itemName == "Swamp Cloak";
        }

        public InventorySnapshot CaptureSnapshot()
        {
            return new InventorySnapshot
            {
                weapons = weapons.ToArray(),
                items = items.ToArray(),
                equippedWeapon = EquippedWeapon,
                equippedArmor = EquippedArmor
            };
        }

        public void RestoreSnapshot(InventorySnapshot snapshot)
        {
            weapons.Clear();
            items.Clear();

            if (snapshot?.weapons != null)
            {
                weapons.AddRange(snapshot.weapons);
            }

            if (snapshot?.items != null)
            {
                items.AddRange(snapshot.items);
            }

            if (weapons.Count == 0)
            {
                weapons.Add("Old Steel Sword");
                weapons.Add("Witcher Silver Sword");
            }

            if (items.Count == 0)
            {
                items.Add("Leather Witcher Armor");
            }

            EquippedWeapon = !string.IsNullOrWhiteSpace(snapshot?.equippedWeapon) && weapons.Contains(snapshot.equippedWeapon)
                ? snapshot.equippedWeapon
                : weapons[0];
            EquippedArmor = !string.IsNullOrWhiteSpace(snapshot?.equippedArmor) && items.Contains(snapshot.equippedArmor) && IsArmor(snapshot.equippedArmor)
                ? snapshot.equippedArmor
                : items.Find(IsArmor) ?? string.Empty;

            Debug.Log($"Inventory restored. Weapon: {EquippedWeapon}, armor: {EquippedArmor}", this);
        }
    }

    [System.Serializable]
    public sealed class InventorySnapshot
    {
        public string[] weapons;
        public string[] items;
        public string equippedWeapon;
        public string equippedArmor;
    }
}

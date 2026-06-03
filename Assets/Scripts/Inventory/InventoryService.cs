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
            Debug.Log($"Item added: {itemName}", this);
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

        public InventorySnapshot CaptureSnapshot()
        {
            return new InventorySnapshot
            {
                weapons = weapons.ToArray(),
                items = items.ToArray(),
                equippedWeapon = EquippedWeapon
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

            Debug.Log($"Inventory restored. Equipped: {EquippedWeapon}", this);
        }
    }

    [System.Serializable]
    public sealed class InventorySnapshot
    {
        public string[] weapons;
        public string[] items;
        public string equippedWeapon;
    }
}

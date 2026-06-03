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

        public void EquipWeapon(string weaponName)
        {
            if (!HasWeapon(weaponName))
            {
                return;
            }

            EquippedWeapon = weaponName;
            Debug.Log($"Equipped weapon: {EquippedWeapon}", this);
        }
    }
}

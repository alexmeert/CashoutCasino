using Godot;
using System;
using System.Collections.Generic;
using CashoutCasino.Character;

namespace CashoutCasino.Weapon
{
    /// <summary>
    /// Holds the player's/AI's weapon inventory, handles switching, grenades and firing delegation.
    /// Designed so adding a new weapon requires no changes to the manager logic.
    /// </summary>
    public partial class WeaponManager : Node3D
    {
        [Export] public NodePath[] weaponSlots = new NodePath[4]; // designer assigns scenes
        protected List<Weapon> weapons = new List<Weapon>(4);
        public int currentWeaponIndex = 0;
        public int grenadeCount = 0;

        public override void _Ready()
        {
            // Weapons should be instantiated by the scene & assigned to slots (factory pattern recommended)
        }

        public Weapon GetCurrentWeapon()
        {
            if (weapons.Count == 0) return null;
            return weapons[Math.Clamp(currentWeaponIndex, 0, weapons.Count - 1)];
        }

        public void SwitchWeapon(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= weapons.Count) return;
            currentWeaponIndex = slotIndex;
            // Trigger animation/event for weapon switch
        }

        public void FireCurrentWeapon(Vector3 direction, Character owner)
        {
            var w = GetCurrentWeapon();
            if (w == null) return;
            if (!owner.CanAffordAction(Economy.CurrencyEconomy.CostType.Shoot)) return;
            var proj = w.Fire(direction, owner);
            Economy.CurrencyEconomy.ApplyCurrencyCost(owner, Economy.CurrencyEconomy.CostType.Shoot);
        }

        public void ReloadCurrentWeapon()
        {
            GetCurrentWeapon()?.Reload();
        }

        public void ModifyGrenadeCount(int delta)
        {
            grenadeCount += delta;
        }

        public void UseGrenade(Vector3 direction, Character owner)
        {
            if (grenadeCount <= 0) return;
            grenadeCount--;
            // Find throwable weapon or use factory to spawn grenade projectile
        }
    }
}

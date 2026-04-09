using Godot;
using System;
using System.Collections.Generic;

namespace CashoutCasino.Weapon
{
	public partial class WeaponManager : Node3D
	{
		protected List<Weapon> weapons = new List<Weapon>();
		public int currentWeaponIndex = 0;
		public int grenadeCount = 0;

		public Camera3D PlayerCamera;

		public override void _Ready()
		{
			foreach (Node child in GetChildren())
			{
				if (child is Weapon w)
				{
					weapons.Add(w);
					w.Visible = false;
				}
			}
		}

		public void Setup()
		{
			foreach (var w in weapons)
				w.Reparent(PlayerCamera, false);

			if (weapons.Count > 0)
				EquipWeapon(0);
		}

		public Weapon GetCurrentWeapon()
		{
			if (weapons.Count == 0) return null;
			return weapons[Math.Clamp(currentWeaponIndex, 0, weapons.Count - 1)];
		}

		public void SwitchWeapon(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= weapons.Count) return;
			weapons[currentWeaponIndex].Visible = false;
			EquipWeapon(slotIndex);
		}

		private void EquipWeapon(int index)
		{
			currentWeaponIndex = index;
			var w = weapons[index];
			w.Visible = true;

			if (w is HitscanWeapon hw)
				hw.FireCamera = PlayerCamera;
		}

		public void FireCurrentWeapon(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			var w = GetCurrentWeapon();
			if (w == null) return;

			// Currency is the ammo — check it can afford before firing
			if (!owner.CanAffordAction(Economy.CurrencyEconomy.CostType.Shoot)) return;

			w.Fire(direction, owner);

			// Deduct currency once per shot
			Economy.CurrencyEconomy.ApplyCurrencyCost(owner, Economy.CurrencyEconomy.CostType.Shoot);

			// Mirror player currency into all weapons so HUD stays accurate
			SyncAmmoToAllWeapons(owner.GetCurrency());
		}

		// Called by Player after currency changes (kills, pickups, etc.)
		// so the HUD ammo count always matches currency
		public void SyncAmmoToAllWeapons(int currency)
		{
			foreach (var w in weapons)
				w.currentAmmo = currency;
		}

		public void ReloadCurrentWeapon()
		{
			// No-op: ammo is currency, can't reload
		}

		public void ModifyGrenadeCount(int delta)
		{
			grenadeCount += delta;
		}

		public int GetCurrentAmmo()
		{
			return GetCurrentWeapon()?.GetAmmoCount() ?? 0;
		}

		public string GetCurrentWeaponName()
		{
			return GetCurrentWeapon()?.Name ?? "None";
		}
	}
}

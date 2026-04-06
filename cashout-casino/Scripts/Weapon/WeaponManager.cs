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
			// Only collect weapons here. Do NOT reparent yet — PlayerCamera
			// is assigned by Player._Ready which runs after this.
			foreach (Node child in GetChildren())
			{
				if (child is Weapon w)
				{
					weapons.Add(w);
					w.Visible = false;
				}
			}
		}

		// Called by Player._Ready after it has assigned PlayerCamera.
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
			if (!owner.CanAffordAction(Economy.CurrencyEconomy.CostType.Shoot)) return;
			w.Fire(direction, owner);
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

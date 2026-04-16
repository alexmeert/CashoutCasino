using Godot;
using System.Collections.Generic;
using CashoutCasino.Economy;

namespace CashoutCasino.Weapon
{
	public partial class WeaponManager : Node3D
	{
		protected List<Weapon> weapons = new();
		public int currentWeaponIndex;
		public int grenadeCount;

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
			{
				w.Reparent(this, true);
				w.Visible = false;
			}

			if (weapons.Count > 0)
				EquipWeapon(0);
		}

		public Weapon GetCurrentWeapon()
		{
			if (weapons.Count == 0)
				return null;

			return weapons[Mathf.Clamp(currentWeaponIndex, 0, weapons.Count - 1)];
		}

		public void SwitchWeapon(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= weapons.Count)
				return;

			var current = GetCurrentWeapon();
			if (current != null)
				current.Visible = false;

			EquipWeapon(slotIndex);
		}

		private void EquipWeapon(int index)
		{
			currentWeaponIndex = index;

			var w = weapons[index];
			w.Visible = true;
			w.FireCamera = PlayerCamera;
		}

		public bool CurrentWeaponHoldToFire()
		{
			return GetCurrentWeapon()?.HoldToFire ?? false;
		}

		public void FireCurrentWeapon(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			var w = GetCurrentWeapon();
			if (w == null)
				return;

			CurrencyEconomy.CostType costType = w.FireCostType;
			if (costType == CurrencyEconomy.CostType.Other)
				return;

			if (!w.CanFire() || !owner.CanAffordAction(costType))
				return;

			if (!w.Fire(direction, owner))
				return;

			CurrencyEconomy.ApplyCurrencyCost(owner, costType);
			SyncAmmoToAllWeapons(owner.GetCurrency());
		}

		public void SyncAmmoToAllWeapons(int currency)
		{
			foreach (var w in weapons)
				w.SyncAmmoDisplay(currency);
		}

		public void ReloadCurrentWeapon()
		{
		}

		public void ModifyGrenadeCount(int delta)
		{
			grenadeCount += delta;
		}

		public int GetCurrentAmmo()
		{
			return GetCurrentWeapon()?.GetAmmoCount() ?? 0;
		}

		public string GetCurrentWeaponName() => GetCurrentWeapon()?.Name ?? "None";

		public WeaponKind GetCurrentWeaponKind() => GetCurrentWeapon()?.Kind ?? WeaponKind.Other;
	}
}

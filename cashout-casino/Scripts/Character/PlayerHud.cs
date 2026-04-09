using Godot;
using System;

namespace CashoutCasino.UI
{
	/// <summary>
	/// Bottom-left HUD panel showing weapon name, ammo, and currency.
	/// Currency updates via signal. Ammo polls WeaponManager each frame.
	/// </summary>
	public partial class PlayerHud : CanvasLayer
	{
		private Label weaponNameLabel;
		private Label ammoLabel;
		private Label currencyLabel;

		// Set by Player._Ready after both nodes exist
		public Weapon.WeaponManager WeaponManager;

		public override void _Ready()
		{
			weaponNameLabel = GetNode<Label>("Panel/VBox/WeaponName");
			ammoLabel = GetNode<Label>("Panel/VBox/Ammo");
			currencyLabel = GetNode<Label>("Panel/VBox/Currency");
		}

		public override void _Process(double delta)
		{
			if (WeaponManager == null) return;

			string weaponName = WeaponManager.GetCurrentWeaponName();
			int ammo = WeaponManager.GetCurrentAmmo();
			int maxAmmo = WeaponManager.GetCurrentWeapon()?.maxAmmo ?? 0;

			weaponNameLabel.Text = weaponName;
			ammoLabel.Text = $"{ammo} / {maxAmmo}";
		}

		// Connected to Player.CurrencyChanged signal
		public void OnCurrencyChanged(int newAmount)
		{
			currencyLabel.Text = $"${newAmount}";
		}
	}
}

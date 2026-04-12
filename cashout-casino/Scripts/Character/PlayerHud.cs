using Godot;

namespace CashoutCasino.UI
{
	public partial class PlayerHud : CanvasLayer
	{
		private Label weaponNameLabel;
		private Label ammoLabel;
		private Label currencyLabel;
		private Label healthLabel;
		private TextureProgressBar healthBar;

		public Weapon.WeaponManager WeaponManager;

		public override void _Ready()
		{
			weaponNameLabel = GetNode<Label>("Panel/VBox/WeaponName");
			ammoLabel = GetNode<Label>("Panel/VBox/Ammo");
			currencyLabel = GetNode<Label>("Panel/VBox/Currency");
			healthLabel = GetNodeOrNull<Label>("HealthPanel/VBox/HealthLabel");
			healthBar = GetNodeOrNull<TextureProgressBar>("HealthPanel/VBox/HealthBar");
		}

		public override void _Process(double delta)
		{
			if (WeaponManager == null) return;

			weaponNameLabel.Text = WeaponManager.GetCurrentWeaponName();
			var w = WeaponManager.GetCurrentWeapon();
			if (w != null)
				ammoLabel.Text = $"{w.GetAmmoCount()} / {w.maxAmmo}";
		}

		public void OnCurrencyChanged(int newAmount)
		{
			currencyLabel.Text = $"${newAmount}";
		}

		public void OnHealthChanged(float current, float max)
		{
			if (healthLabel != null)
				healthLabel.Text = $"HP  {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

			if (healthBar != null)
			{
				healthBar.MaxValue = max;
				healthBar.Value = current;
			}
		}
	}
}

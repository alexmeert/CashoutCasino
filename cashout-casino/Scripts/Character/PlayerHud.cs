using Godot;

namespace CashoutCasino.UI
{
	public partial class PlayerHud : CanvasLayer
	{
		private Label weaponNameLabel;
		private Label ammoLabel;
		private Label currencyLabel;
		private ProgressBar healthBar;
		private StyleBoxFlat healthFillStyle;

		public Weapon.WeaponManager WeaponManager;

		public override void _Ready()
		{
			weaponNameLabel = GetNode<Label>("Panel/VBox/WeaponName");
			ammoLabel = GetNode<Label>("Panel/VBox/Ammo");
			currencyLabel = GetNode<Label>("Panel/VBox/Currency");
			healthBar = GetNodeOrNull<ProgressBar>("HealthPanel/VBox/HealthBar");

			// Create a unique fill style so we can shift its color at runtime
			if (healthBar != null)
			{
				healthFillStyle = new StyleBoxFlat();
				healthFillStyle.BgColor = new Color(0.2f, 0.85f, 0.1f, 1f);
				healthFillStyle.CornerRadiusTopLeft = 3;
				healthFillStyle.CornerRadiusTopRight = 3;
				healthFillStyle.CornerRadiusBottomLeft = 3;
				healthFillStyle.CornerRadiusBottomRight = 3;
				healthBar.AddThemeStyleboxOverride("fill", healthFillStyle);
			}
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
			if (healthBar == null || healthFillStyle == null) return;

			healthBar.MaxValue = max;
			healthBar.Value = current;

			float ratio = Mathf.Clamp(current / max, 0f, 1f);
			float r = Mathf.Lerp(1f, 0.1f, ratio);
			float g = Mathf.Lerp(0.1f, 0.85f, ratio);
			healthFillStyle.BgColor = new Color(r, g, 0.1f, 1f);
		}
	}
}

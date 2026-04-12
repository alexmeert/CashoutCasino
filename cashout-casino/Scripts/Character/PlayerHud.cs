using Godot;

namespace CashoutCasino.UI
{
	public partial class PlayerHud : CanvasLayer
	{
		private Label weaponNameLabel;
		private Label ammoLabel;
		private Label currencyLabel;
		private Label healthBarLabel;
		private LabelSettings healthBarSettings;

		private const int BAR_WIDTH = 10;

		public Weapon.WeaponManager WeaponManager;

		public override void _Ready()
		{
			weaponNameLabel = GetNode<Label>("Panel/VBox/WeaponName");
			ammoLabel = GetNode<Label>("Panel/VBox/Ammo");
			currencyLabel = GetNode<Label>("Panel/VBox/Currency");
			healthBarLabel = GetNodeOrNull<Label>("HealthPanel/VBox/HealthBar");

			if (healthBarLabel != null)
			{
				healthBarSettings = new LabelSettings();
				healthBarSettings.FontSize = 21;
				healthBarSettings.FontColor = new Color(0.2f, 0.9f, 0.1f, 1f);
				healthBarLabel.LabelSettings = healthBarSettings;
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
			if (healthBarLabel == null || healthBarSettings == null) return;

			float ratio = Mathf.Clamp(current / max, 0f, 1f);
			int filled = Mathf.RoundToInt(ratio * BAR_WIDTH);
			int empty = BAR_WIDTH - filled;

			healthBarLabel.Text = new string('█', filled) + new string('░', empty);

			float r = Mathf.Lerp(1f, 0.1f, ratio);
			float g = Mathf.Lerp(0.1f, 0.9f, ratio);
			healthBarSettings.FontColor = new Color(r, g, 0.1f, 1f);
		}
	}
}

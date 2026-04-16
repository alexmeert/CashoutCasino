using Godot;

namespace CashoutCasino.UI
{
	public partial class PlayerHud : CanvasLayer
	{
		public static PlayerHud LocalInstance { get; private set; }

		[Export] public Texture2D RifleIcon;
		[Export] public Texture2D ShotgunIcon;
		[Export] public Texture2D PistolIcon;

		private Label weaponNameLabel;
		private Label ammoLabel;
		private Label currencyLabel;
		private Label atmDebtLabel;
		private ProgressBar healthBar;
		private StyleBoxFlat healthFillStyle;
		private VBoxContainer killFeedContainer;

		private static readonly Font KillFeedFont =
			GD.Load<Font>("res://Assets/upheaval/upheavtt.ttf");

		public Weapon.WeaponManager WeaponManager;

		public override void _Ready()
		{
			weaponNameLabel = GetNode<Label>("PlayerHUDPanel/VBox/WeaponName");
			ammoLabel       = GetNode<Label>("PlayerHUDPanel/VBox/Ammo");
			currencyLabel   = GetNode<Label>("PlayerHUDPanel/VBox/Currency");
			atmDebtLabel    = GetNodeOrNull<Label>("PlayerHUDPanel/VBox/AtmDebt");
			healthBar       = GetNodeOrNull<ProgressBar>("HealthPanel/VBox/HealthBar");
			killFeedContainer = GetNodeOrNull<VBoxContainer>("KillFeedContainer");

			if (healthBar != null)
			{
				healthFillStyle = new StyleBoxFlat();
				healthFillStyle.BgColor              = new Color(0.2f, 0.85f, 0.1f, 1f);
				healthFillStyle.CornerRadiusTopLeft   = 3;
				healthFillStyle.CornerRadiusTopRight  = 3;
				healthFillStyle.CornerRadiusBottomLeft  = 3;
				healthFillStyle.CornerRadiusBottomRight = 3;
				healthBar.AddThemeStyleboxOverride("fill", healthFillStyle);
			}

			if (atmDebtLabel != null)
				atmDebtLabel.Visible = false;
		}

		public void SetAsLocalInstance()
		{
			LocalInstance = this;
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

		public void OnAtmDebtChanged(int totalDebt)
		{
			if (atmDebtLabel == null) return;

			if (totalDebt <= 0)
			{
				atmDebtLabel.Visible = false;
				return;
			}

			atmDebtLabel.Text    = $"-${totalDebt} (ATM)";
			atmDebtLabel.Visible = true;
		}

		public void OnHealthChanged(float current, float max)
		{
			if (healthBar == null || healthFillStyle == null) return;

			healthBar.MaxValue = max;
			healthBar.Value    = current;

			float ratio = Mathf.Clamp(current / max, 0f, 1f);
			float r = Mathf.Lerp(1f, 0.1f, ratio);
			float g = Mathf.Lerp(0.1f, 0.85f, ratio);
			healthFillStyle.BgColor = new Color(r, g, 0.1f, 1f);
		}

		public void AddKillEntry(string killerName, string weaponKind, string victimName)
		{
			if (killFeedContainer == null) return;

			var row = new HBoxContainer();
			row.Modulate = new Color(1, 1, 1, 1);
			row.AddThemeConstantOverride("separation", 6);

			row.AddChild(MakeNameLabel(killerName, new Color(1f, 1f, 1f, 1f)));
			row.AddChild(MakeWeaponWidget(weaponKind));
			row.AddChild(MakeNameLabel(victimName, new Color(1f, 0.25f, 0.25f, 1f)));

			killFeedContainer.AddChild(row);

			var tween = row.CreateTween();
			tween.TweenInterval(3.5);
			tween.TweenProperty(row, "modulate:a", 0.0, 0.5);
			tween.TweenCallback(Callable.From(row.QueueFree));
		}

		private Label MakeNameLabel(string text, Color color)
		{
			var lbl = new Label();
			lbl.Text = text.Length > 0 ? text : "Unknown";
			var settings = new LabelSettings();
			settings.FontSize = 14;
			settings.FontColor = color;
			if (KillFeedFont != null) settings.Font = KillFeedFont;
			lbl.LabelSettings = settings;
			lbl.VerticalAlignment = VerticalAlignment.Center;
			return lbl;
		}

		private Control MakeWeaponWidget(string weaponKind)
		{
			Texture2D icon = weaponKind switch
			{
				"Rifle"   => RifleIcon,
				"Shotgun" => ShotgunIcon,
				"Pistol"  => PistolIcon,
				_         => null,
			};

			if (icon != null)
			{
				var tex = new TextureRect();
				tex.Texture = icon;
				tex.CustomMinimumSize = new Vector2(24, 24);
				tex.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				tex.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				return tex;
			}

			string tag = weaponKind switch
			{
				"Rifle"   => "[AR]",
				"Shotgun" => "[SG]",
				"Pistol"  => "[DP]",
				_         => "[?]",
			};
			return MakeNameLabel(tag, new Color(1f, 0.85f, 0.3f, 1f));
		}
	}
}

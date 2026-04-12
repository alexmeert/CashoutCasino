using Godot;

namespace CashoutCasino.UI
{
	/// <summary>
	/// World-space health bar that floats above a character's head.
	/// Rendered client-side only — only the player who shot this character
	/// will call ShowFor() to make it visible on their screen.
	/// Automatically faces the local camera each frame.
	/// Fades out after a short duration if not refreshed.
	/// </summary>
	public partial class WorldHealthBar : Node3D
	{
		[Export] public float hideAfterSeconds = 3f;

		private Label3D bar;
		private Label3D nameLabel;
		private float hideTimer = 0f;
		private bool visible3d = false;
		private Camera3D localCamera;

		// Total bar width in characters
		private const int BAR_WIDTH = 10;

		public override void _Ready()
		{
			bar = GetNode<Label3D>("Bar");
			nameLabel = GetNodeOrNull<Label3D>("NameLabel");

			// Start hidden
			bar.Visible = false;
			if (nameLabel != null) nameLabel.Visible = false;
		}

		/// <summary>
		/// Called by the local player when they hit this character.
		/// Shows the bar and resets the hide timer.
		/// </summary>
		public void ShowFor(float currentHealth, float maxHealth)
		{
			visible3d = true;
			hideTimer = hideAfterSeconds;
			bar.Visible = true;
			if (nameLabel != null) nameLabel.Visible = true;
			UpdateBar(currentHealth, maxHealth);
		}

		public void SetLocalCamera(Camera3D camera)
		{
			localCamera = camera;
		}

		private void UpdateBar(float current, float max)
		{
			float ratio = Mathf.Clamp(current / max, 0f, 1f);
			int filled = Mathf.RoundToInt(ratio * BAR_WIDTH);
			int empty = BAR_WIDTH - filled;

			// Use block characters: filled = █, empty = ░
			string fill = new string('█', filled);
			string unfill = new string('░', empty);

			// Color shifts red as health drops
			float r = Mathf.Lerp(1f, 0.1f, ratio);
			float g = Mathf.Lerp(0.1f, 0.9f, ratio);
			bar.Text = fill + unfill;
			bar.Modulate = new Color(r, g, 0.1f, 1f);
		}

		public override void _Process(double delta)
		{
			if (!visible3d) return;

			// Count down hide timer
			hideTimer -= (float)delta;
			if (hideTimer <= 0f)
			{
				visible3d = false;
				bar.Visible = false;
				if (nameLabel != null) nameLabel.Visible = false;
				return;
			}

			// Billboard — rotate to face the local camera
			if (localCamera != null)
			{
				Vector3 camPos = localCamera.GlobalPosition;
				Vector3 myPos = GlobalPosition;
				Vector3 dir = (camPos - myPos).Normalized();

				if (dir.LengthSquared() > 0.001f)
					LookAt(camPos, Vector3.Up);
			}
		}
	}
}

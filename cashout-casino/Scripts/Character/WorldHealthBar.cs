using Godot;

namespace CashoutCasino.UI
{
	public partial class WorldHealthBar : Node3D
	{
		[Export] public float hideAfterSeconds = 3f;

		private Label3D bar;
		private Label3D nameLabel;
		private float hideTimer = 0f;
		private bool visible3d = false;
		private Camera3D localCamera;

		private const int BAR_WIDTH = 10;

		public override void _Ready()
		{
			bar = GetNode<Label3D>("Bar");
			nameLabel = GetNodeOrNull<Label3D>("NameLabel");

			bar.Visible = false;
			if (nameLabel != null) nameLabel.Visible = false;
		}

		public void ShowFor(float currentHealth, float maxHealth)
		{
			visible3d = true;
			hideTimer = hideAfterSeconds;
			bar.Visible = true;
			if (nameLabel != null) nameLabel.Visible = true;
			UpdateBar(currentHealth, maxHealth);
		}

		// Called on respawn to clear all state so ShowFor works fresh
		public void Reset()
		{
			visible3d = false;
			hideTimer = 0f;
			bar.Visible = false;
			if (nameLabel != null) nameLabel.Visible = false;
			// Reset to full green so it looks correct on first hit after respawn
			bar.Text = new string('█', BAR_WIDTH);
			bar.Modulate = new Color(0.2f, 0.9f, 0.1f, 1f);
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

			float r = Mathf.Lerp(1f, 0.1f, ratio);
			float g = Mathf.Lerp(0.1f, 0.9f, ratio);
			bar.Text = new string('█', filled) + new string('░', empty);
			bar.Modulate = new Color(r, g, 0.1f, 1f);
		}

		public override void _Process(double delta)
		{
			if (!visible3d) return;

			hideTimer -= (float)delta;
			if (hideTimer <= 0f)
			{
				visible3d = false;
				bar.Visible = false;
				if (nameLabel != null) nameLabel.Visible = false;
				return;
			}

			if (localCamera != null)
			{
				Vector3 dir = (localCamera.GlobalPosition - GlobalPosition).Normalized();
				if (dir.LengthSquared() > 0.001f)
					LookAt(localCamera.GlobalPosition, Vector3.Up);
			}
		}
	}
}

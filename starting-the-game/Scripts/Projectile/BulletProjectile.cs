using Godot;

namespace CashoutCasino.Projectile
{
	public partial class BulletProjectile : Projectile
	{
		public override void _Ready()
		{
			lifetime = Mathf.Min(lifetime, 4f);
			Monitoring = true;
			Monitorable = false;

			// Use AreaEntered to detect the dedicated hitbox Area3D on characters.
			// This is separate from the CharacterBody3D movement collider so there
			// is no conflict with floor/wall detection.
			AreaEntered += OnAreaEntered;

			// Keep BodyEntered as a fallback for non-character physics bodies (walls etc.)
			BodyEntered += OnBodyEntered;
		}

		private void OnAreaEntered(Area3D area)
		{
			// The hitbox Area3D is a child of the Character — walk up to find it
			Node parent = area.GetParent();
			if (parent is CashoutCasino.Character.Character c)
			{
				if (owner != null && c == owner) return;
				HitCharacter(c);
			}
		}

		private void OnBodyEntered(Node3D body)
		{
			// Ignore the shooter
			if (owner != null && body == owner) return;

			// If somehow a Character body is hit directly, handle it
			if (body is CashoutCasino.Character.Character c)
			{
				HitCharacter(c);
				return;
			}

			// Hit a wall or other static object — just despawn
			Despawn();
		}

		private void HitCharacter(CashoutCasino.Character.Character c)
		{
			ApplyDamage(c);

			if (c.WorldHealthBar != null)
			{
				c.WorldHealthBar.SetLocalCamera(ShooterCamera);
				c.WorldHealthBar.ShowFor(c.GetHealth(), c.GetMaxHealth());
			}

			Despawn();
		}

		public override void OnHit(Node3D hitTarget)
		{
			if (hitTarget is CashoutCasino.Character.Character c)
				HitCharacter(c);
			else
				Despawn();
		}
	}
}

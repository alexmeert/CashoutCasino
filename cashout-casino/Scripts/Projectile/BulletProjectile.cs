using Godot;
using System;

namespace CashoutCasino.Projectile
{
	public partial class BulletProjectile : Projectile
	{
		public override void _Ready()
		{
			lifetime = Mathf.Min(lifetime, 4f);
			Monitoring = true;
			Monitorable = false;
			BodyEntered += OnBodyEntered;
		}

		private void OnBodyEntered(Node3D body)
		{
			if (owner != null && body == owner)
				return;
			OnHit(body);
		}

		public override void OnHit(Node3D hitTarget)
		{
			if (hitTarget is CashoutCasino.Character.Character c)
				ApplyDamage(c);
			Despawn();
		}
	}
}

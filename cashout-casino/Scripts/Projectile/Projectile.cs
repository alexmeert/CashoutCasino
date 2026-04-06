using Godot;
using System;

namespace CashoutCasino.Projectile
{
	/// <summary>
	/// Generic projectile base for moving objects, hitscan wrappers or area effects.
	/// </summary>
	public abstract partial class Projectile : Area3D
	{
		[Export] public float speed = 30f;
		[Export] public float lifetime = 10f;
		[Export] public float baseDamage = 10f;

		protected Vector3 direction = Vector3.Zero;
		protected CashoutCasino.Character.Character owner;
		protected ulong spawnTime = 0;

		public virtual void Launch(Vector3 dir, CashoutCasino.Character.Character projectileOwner)
		{
			direction = dir.Normalized();
			owner = projectileOwner;
			spawnTime = Time.GetTicksMsec();
		}

		public abstract void OnHit(Node3D hitTarget);

		public virtual float ApplyDamage(CashoutCasino.Character.Character target)
		{
			target.TakeDamage(baseDamage, owner);
			return baseDamage;
		}

		public virtual void Despawn()
		{
			QueueFree();
		}

		public override void _PhysicsProcess(double delta)
		{
			Translate(direction * (float)delta * speed);
			if (Time.GetTicksMsec() - spawnTime > (ulong)(lifetime * 1000f)) Despawn();
		}
	}
}

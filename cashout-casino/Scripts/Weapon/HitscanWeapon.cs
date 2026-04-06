using Godot;
using System;

namespace CashoutCasino.Weapon
{
	public abstract partial class HitscanWeapon : Weapon
	{
		[Export] public float range = 100f;

		public Camera3D FireCamera;

		// Subclasses call this inside their Fire override to cast a single ray.
		protected void PerformRaycast(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			var spaceState = owner.GetWorld3D().DirectSpaceState;

			Vector3 origin = FireCamera != null
				? FireCamera.GlobalPosition
				: owner.GlobalPosition + Vector3.Up * 1.6f;

			Vector3 target = origin + direction.Normalized() * range;

			var query = PhysicsRayQueryParameters3D.Create(origin, target);
			query.Exclude = new Godot.Collections.Array<Rid> { owner.GetRid() };

			var result = spaceState.IntersectRay(query);
			if (result.Count == 0) return;

			if (result["collider"].As<Node>() is CashoutCasino.Character.Character hit)
				hit.TakeDamage(damagePerHit, owner);
		}

		public override void _Ready()
		{
			currentAmmo = maxAmmo;
		}
	}
}

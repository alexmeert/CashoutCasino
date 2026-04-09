using Godot;
using System;

namespace CashoutCasino.Weapon
{
	public abstract partial class HitscanWeapon : Weapon
	{
		[Export] public float range = 100f;

		public Color TrailColor = new Color(1f, 0.95f, 0.6f, 1f);

		public Camera3D FireCamera;

		protected void PerformRaycast(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			var spaceState = owner.GetWorld3D().DirectSpaceState;

			// Raycast origin — from camera if available, else above player head
			Vector3 rayOrigin = FireCamera != null
				? FireCamera.GlobalPosition
				: owner.GlobalPosition + Vector3.Up * 1.6f;

			Vector3 normDir = direction.Normalized();
			Vector3 target = rayOrigin + normDir * range;

			var query = PhysicsRayQueryParameters3D.Create(rayOrigin, target);
			query.Exclude = new Godot.Collections.Array<Rid> { owner.GetRid() };

			var result = spaceState.IntersectRay(query);

			Vector3 hitPoint = result.Count > 0
				? (Vector3)result["position"]
				: target;

			if (result.Count > 0)
			{
				if (result["collider"].As<Node>() is CashoutCasino.Character.Character hit)
					hit.TakeDamage(damagePerHit, owner);
			}

			// Start the trail a bit in front of the camera so it clears
			// the near clip plane and isn't invisible from the shooter's view
			Vector3 trailStart = rayOrigin + normDir * 0.5f;

			SpawnTrail(trailStart, hitPoint, owner);
		}

		private void SpawnTrail(Vector3 from, Vector3 to, CashoutCasino.Character.Character owner)
		{
			// Skip if too short to see (e.g. point-blank wall)
			if (from.DistanceTo(to) < 0.6f) return;

			var trail = new BulletTrail();
			trail.TrailColor = TrailColor;
			trail.Init(from, to);

			// Add to the active scene so the trail is world-space and
			// visible to all cameras in the scene
			owner.GetTree().CurrentScene.AddChild(trail);
		}

		public override void _Ready()
		{
			currentAmmo = maxAmmo;
		}
	}
}

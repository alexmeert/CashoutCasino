using Godot;

namespace CashoutCasino.Weapon
{
	public abstract partial class HitscanWeapon : Weapon
	{
		[Export] public float range = 100f;

		public Color TrailColor = new Color(1f, 0.95f, 0.6f, 1f);

		public Camera3D FireCamera;

		protected void PerformRaycast(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			Vector3 rayOrigin = FireCamera != null
				? FireCamera.GlobalPosition
				: owner.GlobalPosition + Vector3.Up * 1.6f;

			PerformRaycastFrom(rayOrigin, direction.Normalized(), owner);
		}

		protected void PerformRaycastFrom(Vector3 origin, Vector3 direction, CashoutCasino.Character.Character owner)
		{
			var spaceState = owner.GetWorld3D().DirectSpaceState;
			Vector3 target = origin + direction * range;

			var query = PhysicsRayQueryParameters3D.Create(origin, target);
			query.Exclude = new Godot.Collections.Array<Rid> { owner.GetRid() };

			var result = spaceState.IntersectRay(query);
			Vector3 hitPoint = result.Count > 0
				? (Vector3)result["position"]
				: target;

			if (result.Count > 0 && result["collider"].As<Node>() is CashoutCasino.Character.Character hit)
			{
				hit.TakeDamage(damagePerHit, owner);

				// Show the health bar only on the shooter's local screen
				if (hit.WorldHealthBar != null)
				{
					hit.WorldHealthBar.SetLocalCamera(FireCamera);
					hit.WorldHealthBar.ShowFor(hit.GetHealth(), hit.GetMaxHealth());
				}
			}

			SpawnTrail(origin + direction * 0.5f, hitPoint, owner);
		}

		public void SpawnTrail(Vector3 from, Vector3 to, CashoutCasino.Character.Character owner)
		{
			if (from.DistanceTo(to) < 0.6f) return;

			var trail = new BulletTrail();
			trail.TrailColor = TrailColor;
			trail.Init(from, to);
			owner.GetTree().CurrentScene.AddChild(trail);
		}

		public override void _Ready()
		{
			currentAmmo = maxAmmo;
		}
	}
}

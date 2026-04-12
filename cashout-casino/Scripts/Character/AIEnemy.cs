using Godot;

namespace CashoutCasino.Character
{
	public partial class AIEnemy : Character
	{
		[Export] public float detectionRadius = 15f;
		[Export] public float fireRate = 1.2f;
		[Export] public float damagePerShot = 10f;
		[Export] public float gravity = 20f;

		private float verticalVelocity = 0f;
		private float fireTimer = 0f;
		private Character target;

		public override void _Ready()
		{
			base._Ready();

			var whb = GetNodeOrNull<UI.WorldHealthBar>("WorldHealthBar");
			if (whb != null)
				WorldHealthBar = whb;
		}

		public override void _PhysicsProcess(double delta)
		{
			float dt = (float)delta;

			if (IsOnFloor())
				verticalVelocity = -0.5f;
			else
				verticalVelocity -= gravity * dt;

			Velocity = new Vector3(0f, verticalVelocity, 0f);
			MoveAndSlide();

			target = FindNearestPlayer();
			if (target == null) return;

			Vector3 dir = target.GlobalPosition - GlobalPosition;
			dir.Y = 0f;
			if (dir.LengthSquared() > 0.01f)
				LookAt(GlobalPosition + dir, Vector3.Up);

			fireTimer -= dt;
			if (fireTimer <= 0f)
			{
				FireAtTarget();
				fireTimer = fireRate;
			}
		}

		private Character FindNearestPlayer()
		{
			float closest = detectionRadius * detectionRadius;
			Character nearest = null;

			foreach (Node node in GetTree().GetNodesInGroup("Player"))
			{
				if (node is Character c)
				{
					float distSq = GlobalPosition.DistanceSquaredTo(c.GlobalPosition);
					if (distSq < closest)
					{
						closest = distSq;
						nearest = c;
					}
				}
			}

			return nearest;
		}

		private void FireAtTarget()
		{
			if (target == null) return;

			Vector3 origin = GlobalPosition + Vector3.Up * 1.4f;
			Vector3 aimPoint = target.GlobalPosition + Vector3.Up * 0.9f;
			Vector3 direction = (aimPoint - origin).Normalized();
			Vector3 targetPos = origin + direction * 100f;

			var spaceState = GetWorld3D().DirectSpaceState;
			var query = PhysicsRayQueryParameters3D.Create(origin, targetPos);
			query.CollisionMask = 0xFFFFFFFF;
			query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

			var result = spaceState.IntersectRay(query);

			if (result.Count == 0)
			{
				GD.Print("[AI] Ray hit nothing. Origin: " + origin + " Target: " + targetPos);
				SpawnTrail(origin, targetPos);
				return;
			}

			Node colliderNode = result["collider"].As<Node>();
			GD.Print("[AI] Ray hit: " + colliderNode?.Name + " type: " + colliderNode?.GetType().Name);

			// Walk up hierarchy to find Character
			Node node = colliderNode;
			bool damaged = false;
			while (node != null)
			{
				GD.Print("[AI]   checking node: " + node.Name + " / " + node.GetType().Name);
				if (node is Character hit && hit != this)
				{
					GD.Print("[AI] Dealing " + damagePerShot + " damage to " + hit.Name);
					hit.TakeDamage(damagePerShot, this);
					damaged = true;
					break;
				}
				node = node.GetParent();
			}

			if (!damaged)
				GD.Print("[AI] Hit something but found no Character in hierarchy: " + colliderNode?.Name);

			SpawnTrail(origin, (Vector3)result["position"]);
		}

		private void SpawnTrail(Vector3 from, Vector3 to)
		{
			if (from.DistanceTo(to) < 0.5f) return;

			var trail = new Weapon.BulletTrail();
			trail.TrailColor = new Color(1f, 0.3f, 0.0f, 1f);
			trail.Init(from, to);
			GetTree().CurrentScene.AddChild(trail);
		}

		public override void RequestAIDecision() { }
		public override void OnInputAction(string action) { }
	}
}

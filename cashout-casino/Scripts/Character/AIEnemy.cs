using Godot;

namespace CashoutCasino.Character
{
	public partial class AIEnemy : Character
	{
		[Export] public float detectionRadius = 15f;
		[Export] public float fireRate = 1.2f;
		[Export] public float damagePerShot = 10f;
		[Export] public float gravity = 20f;
		[Export] public float respawnTime = 15f;

		private float verticalVelocity = 0f;
		private float fireTimer = 0f;
		private Character target;

		private Vector3 spawnPosition;

		public override void _Ready()
		{
			base._Ready();
			spawnPosition = GlobalPosition;

			var whb = GetNodeOrNull<UI.WorldHealthBar>("WorldHealthBar");
			if (whb != null)
				WorldHealthBar = whb;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (isDead) return;

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

		public override void OnDeath(Character killer)
		{
			base.OnDeath(killer);

			// Reward the killer with currency (= ammo) if they are a player
			if (killer != null)
				Economy.CurrencyEconomy.ApplyCurrencyGain(killer, Economy.CurrencyEconomy.ElimType.Body);

			Visible = false;
			SetPhysicsProcess(false);
			SetProcess(false);

			if (WorldHealthBar != null)
				WorldHealthBar.Visible = false;

			GetTree().CreateTimer(respawnTime).Timeout += Respawn;
		}

		private void Respawn()
		{
			isDead = false;
			currentHealth = maxHealth;
			verticalVelocity = 0f;
			fireTimer = 0f;

			GlobalPosition = spawnPosition;
			Visible = true;
			SetPhysicsProcess(true);
			SetProcess(true);

			if (WorldHealthBar != null)
			{
				WorldHealthBar.Visible = true;
				WorldHealthBar.Reset();
			}
		}

		private Character FindNearestPlayer()
		{
			float closest = detectionRadius * detectionRadius;
			Character nearest = null;

			foreach (Node node in GetTree().GetNodesInGroup("Player"))
			{
				if (node is Character c && !c.IsDead)
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
			Vector3 hitPoint = result.Count > 0 ? (Vector3)result["position"] : targetPos;

			if (result.Count > 0)
			{
				Node node = result["collider"].As<Node>();
				while (node != null)
				{
					if (node is Character hit && hit != this)
					{
						hit.TakeDamage(damagePerShot, this);
						break;
					}
					node = node.GetParent();
				}
			}

			SpawnTrail(origin, hitPoint);
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

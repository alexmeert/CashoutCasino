using Godot;

namespace CashoutCasino.Character
{
	public partial class Player : Character
	{
		[Export] public float mouseSensitivity = 0.15f;
		[Export] public NodePath cameraPath = "CameraHolder/Camera3D";
		[Export] public NodePath cameraHolderPath = "CameraHolder";
		[Export] public NodePath collisionShapePath = "CollisionShape3D";
		[Export] public NodePath weaponManagerPath = "CameraHolder/Camera3D/WeaponManager";
		[Export] public NodePath hudPath = "PlayerHud";
		[Export] public NodePath respawnScreenPath = "RespawnScreen";
		[Export] public float respawnTime = 5f;

		[Export] public float jumpForce = 5f;
		[Export] public float gravity = 20f;
		[Export] public float standHeight = 1.8f;
		[Export] public float crouchHeight = 1.0f;

		// Regen settings
		[Export] public float regenDelay = 15f;    // seconds of no damage before regen starts
		[Export] public float regenRate  = 10f;    // HP per second once regen kicks in

		private Camera3D camera;
		private Node3D cameraHolder;
		private CollisionShape3D collisionShape;
		private Weapon.WeaponManager wm;
		private UI.PlayerHud hud;
		private UI.RespawnScreen respawnScreen;
		// PlayerCharacter handles the animated model (replaces capsule MeshInstance3D)
		private PlayerCharacter playerCharacter;

		private float verticalVelocity = 0f;
		private float cameraPitch = 0f;
		private const float MAX_PITCH = 89f;

		private Vector3 spawnPosition;
		private int atmDebt = 0;

		// Regen state
		private float timeSinceLastDamage = 0f;
		private bool regenActive = false;

		public override void _Ready()
		{
			base._Ready();

			spawnPosition  = GlobalPosition;
			camera         = GetNode<Camera3D>(cameraPath);
			cameraHolder   = GetNode<Node3D>(cameraHolderPath);
			collisionShape = GetNode<CollisionShape3D>(collisionShapePath);
			// Grab the animated PlayerCharacter node instead of a raw MeshInstance3D
			playerCharacter = GetNodeOrNull<PlayerCharacter>("PlayerCharacter");

			if (HasNode(weaponManagerPath))
			{
				wm = GetNode<Weapon.WeaponManager>(weaponManagerPath);
				wm.PlayerCamera = camera;
				wm.Setup();
				weaponManager = wm;
			}

			if (HasNode(hudPath))
			{
				hud = GetNode(hudPath) as UI.PlayerHud;
				if (hud != null)
				{
					hud.WeaponManager = wm;
					CurrencyChanged += hud.OnCurrencyChanged;
				}
			}

			if (HasNode(respawnScreenPath))
				respawnScreen = GetNode(respawnScreenPath) as UI.RespawnScreen;

			var whb = GetNodeOrNull<UI.WorldHealthBar>("WorldHealthBar");
			if (whb != null)
			{
				WorldHealthBar = whb;
				whb.Visible = false;
			}

			currentCurrency = Economy.CurrencyEconomy.INITIAL_SPAWN;
			wm?.SyncAmmoToAllWeapons(currentCurrency);
			CurrencyChanged += OnCurrencyChangedSync;

			hud?.OnCurrencyChanged(currentCurrency);
			hud?.OnHealthChanged(currentHealth, maxHealth);
			hud?.OnAtmDebtChanged(atmDebt);

			// Authority setup happens via ClaimAuthority RPC sent by the server.
			// Do NOT call SetupLocalAuthority here — authority isn't set yet at _Ready time.
		}

		[Rpc(MultiplayerApi.RpcMode.Authority,
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		public void ClaimAuthority(long ownerId)
		{
			// All peers run this so every copy of this player has the correct authority set.
			// This is required for MultiplayerSynchronizer to know who sends deltas.
			SetMultiplayerAuthority((int)ownerId);
			GD.Print($"[Player] ClaimAuthority: peer {ownerId} owns {Name} (I am {Multiplayer.GetUniqueId()})");
			if (IsMultiplayerAuthority())
				SetupLocalAuthority();
		}

		private void SetupLocalAuthority()
		{
			if (IsMultiplayerAuthority())
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
				camera.Current = true;
			}
			else
			{
				camera.Current = false;
				if (HasNode(hudPath)) GetNode(hudPath).ProcessMode = ProcessModeEnum.Disabled;
				if (HasNode(respawnScreenPath)) GetNode(respawnScreenPath).ProcessMode = ProcessModeEnum.Disabled;
			}
		}

		public void AddAtmDebt(int amount)
		{
			atmDebt += amount;
			ModifyCurrency(amount);
			hud?.OnAtmDebtChanged(atmDebt);
		}

		public int GetAtmDebt() => atmDebt;
		public int GetFinalScore() => currentCurrency - atmDebt;

		public override void TakeDamage(float damage, Character attacker = null)
		{
			// Reset regen timer on any hit
			timeSinceLastDamage = 0f;
			regenActive = false;
			base.TakeDamage(damage, attacker);
		}

		public override void OnDeath(Character killer)
		{
			base.OnDeath(killer);
			ModifyCurrency(-Economy.CurrencyEconomy.BODY_ELIM);
			SetPhysicsProcess(false);

			timeSinceLastDamage = 0f;
			regenActive = false;

			// Make the player model semi-transparent on death
			if (playerCharacter != null)
			{
				var mat = new StandardMaterial3D();
				mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
				mat.AlbedoColor = new Color(0.6f, 0.6f, 0.8f, 0.35f);
				// Apply to the mesh inside the armature hierarchy
				var mesh = playerCharacter.MyMesh;
				if (mesh != null) mesh.MaterialOverride = mat;
			}

			respawnScreen?.StartCountdown(respawnTime, Respawn);
		}

		private void Respawn()
		{
			isDead = false;
			currentHealth    = maxHealth;
			verticalVelocity = 0f;
			timeSinceLastDamage = 0f;
			regenActive = false;

			GlobalPosition = spawnPosition;
			SetPhysicsProcess(true);

			// Restore default material on respawn
			if (playerCharacter?.MyMesh != null)
				playerCharacter.MyMesh.MaterialOverride = null;

			hud?.OnHealthChanged(currentHealth, maxHealth);
			// Re-capture mouse for the local player on respawn.
			if (IsMultiplayerAuthority())
				Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		protected override void OnHealthChangedInternal(float current, float max)
		{
			hud?.OnHealthChanged(current, max);
		}

		private void OnCurrencyChangedSync(int newAmount)
		{
			wm?.SyncAmmoToAllWeapons(newAmount);
		}

		public override void _Input(InputEvent @event)
		{
			if (!IsMultiplayerAuthority()) return;
			if (isDead) return;

			if (@event is InputEventMouseMotion mouseMotion)
			{
				RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * mouseSensitivity));
				cameraPitch -= mouseMotion.Relative.Y * mouseSensitivity;
				cameraPitch = Mathf.Clamp(cameraPitch, -MAX_PITCH, MAX_PITCH);
				cameraHolder.Rotation = new Vector3(Mathf.DegToRad(cameraPitch), 0f, 0f);
			}

			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				if (keyEvent.Keycode == Key.Escape)
					Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		public override void _Process(double delta)
		{
			if (isDead) return;
			if (currentHealth >= maxHealth) return;

			float dt = (float)delta;
			timeSinceLastDamage += dt;

			if (timeSinceLastDamage >= regenDelay)
			{
				regenActive = true;
				currentHealth = Mathf.Min(currentHealth + regenRate * dt, maxHealth);
				OnHealthChangedInternal(currentHealth, maxHealth);
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!IsMultiplayerAuthority()) return;
			if (isDead) return;

			float dt = (float)delta;

			SetCrouch(Input.IsActionPressed("crouch"));

			Vector3 inputDir = Vector3.Zero;
			if (Input.IsActionPressed("move_forward"))  inputDir -= Transform.Basis.Z;
			if (Input.IsActionPressed("move_backward")) inputDir += Transform.Basis.Z;
			if (Input.IsActionPressed("move_left"))     inputDir -= Transform.Basis.X;
			if (Input.IsActionPressed("move_right"))    inputDir += Transform.Basis.X;

			inputDir = inputDir.Normalized();
			bool sprinting = Input.IsActionPressed("sprint") && !isCrouching;

			if (IsOnFloor())
			{
				verticalVelocity = -0.5f;
				if (Input.IsActionJustPressed("jump") && !isCrouching)
					verticalVelocity = jumpForce;
			}
			else
			{
				verticalVelocity -= gravity * dt;
			}

			float speed = moveSpeed
				* (sprinting ? sprintMultiplier : 1f)
				* (isCrouching ? crouchMultiplier : 1f);

			Velocity = new Vector3(inputDir.X * speed, verticalVelocity, inputDir.Z * speed);
			MoveAndSlide();

			if (wm == null) return;

			bool fireHeld    = Input.IsActionPressed("fire");
			bool firePressed = Input.IsActionJustPressed("fire");
			bool wantFire    = wm.CurrentWeaponHoldToFire() ? fireHeld : firePressed;
			if (wantFire)
				wm.FireCurrentWeapon(-camera.GlobalTransform.Basis.Z, this);

			if (Input.IsActionJustPressed("weapon_1")) wm.SwitchWeapon(0);
			if (Input.IsActionJustPressed("weapon_2")) wm.SwitchWeapon(1);
			if (Input.IsActionJustPressed("weapon_3")) wm.SwitchWeapon(2);
		}

		private void SetCrouch(bool crouch)
		{
			isCrouching = crouch;
			if (collisionShape.Shape is CapsuleShape3D capsule)
			{
				float targetHeight = crouch ? crouchHeight : standHeight;
				capsule.Height = targetHeight;
				collisionShape.Position = new Vector3(0f, targetHeight / 2f, 0f);
				cameraHolder.Position   = new Vector3(0f, targetHeight - 0.15f, 0f);
			}
		}

		public override void RequestAIDecision() { }
		public override void OnInputAction(string action) { }
	}
}

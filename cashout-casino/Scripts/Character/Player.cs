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
		[Export] public NodePath hudPath = "PlayerHUD";

		[Export] public float jumpForce = 5f;
		[Export] public float gravity = 20f;
		[Export] public float standHeight = 1.8f;
		[Export] public float crouchHeight = 1.0f;

		private Camera3D camera;
		private Node3D cameraHolder;
		private CollisionShape3D collisionShape;
		private Weapon.WeaponManager wm;
		private UI.PlayerHud hud;

		private float verticalVelocity = 0f;
		private float cameraPitch = 0f;
		private const float MAX_PITCH = 89f;

		public override void _Ready()
		{
			base._Ready();

			camera = GetNode<Camera3D>(cameraPath);
			cameraHolder = GetNode<Node3D>(cameraHolderPath);
			collisionShape = GetNode<CollisionShape3D>(collisionShapePath);

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
					HealthChanged += hud.OnHealthChanged;
				}
			}

			// Hide own world health bar — you never see it above your own head
			var whb = GetNodeOrNull<UI.WorldHealthBar>("WorldHealthBar");
			if (whb != null)
			{
				WorldHealthBar = whb;
				whb.Visible = false;
			}

			currentCurrency = Economy.CurrencyEconomy.INITIAL_SPAWN;
			wm?.SyncAmmoToAllWeapons(currentCurrency);
			CurrencyChanged += OnCurrencyChangedSync;

			// Fire initial values so HUD shows correct state on spawn
			hud?.OnCurrencyChanged(currentCurrency);
			hud?.OnHealthChanged(currentHealth, maxHealth);

			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		private void OnCurrencyChangedSync(int newAmount)
		{
			wm?.SyncAmmoToAllWeapons(newAmount);
		}

		public override void _Input(InputEvent @event)
		{
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

		public override void _PhysicsProcess(double delta)
		{
			float dt = (float)delta;

			SetCrouch(Input.IsActionPressed("crouch"));

			Vector3 inputDir = Vector3.Zero;
			if (Input.IsActionPressed("move_forward"))
				inputDir -= Transform.Basis.Z;
			if (Input.IsActionPressed("move_backward"))
				inputDir += Transform.Basis.Z;
			if (Input.IsActionPressed("move_left"))
				inputDir -= Transform.Basis.X;
			if (Input.IsActionPressed("move_right"))
				inputDir += Transform.Basis.X;

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

			bool fireHeld = Input.IsActionPressed("fire");
			bool firePressed = Input.IsActionJustPressed("fire");
			bool wantFire = wm.CurrentWeaponHoldToFire() ? fireHeld : firePressed;
			if (wantFire)
				wm.FireCurrentWeapon(-camera.GlobalTransform.Basis.Z, this);

			if (Input.IsActionJustPressed("weapon_1"))
				wm.SwitchWeapon(0);
			if (Input.IsActionJustPressed("weapon_2"))
				wm.SwitchWeapon(1);
			if (Input.IsActionJustPressed("weapon_3"))
				wm.SwitchWeapon(2);
		}

		private void SetCrouch(bool crouch)
		{
			isCrouching = crouch;
			if (collisionShape.Shape is CapsuleShape3D capsule)
			{
				float targetHeight = crouch ? crouchHeight : standHeight;
				capsule.Height = targetHeight;
				collisionShape.Position = new Vector3(0f, targetHeight / 2f, 0f);
				cameraHolder.Position = new Vector3(0f, targetHeight - 0.15f, 0f);
			}
		}

		public override void RequestAIDecision() { }
		public override void OnInputAction(string action) { }
	}
}

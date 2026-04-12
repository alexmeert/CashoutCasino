using Godot;
using System;

namespace CashoutCasino.Character
{
	public abstract partial class Character : CharacterBody3D
	{
		[Export] public float maxHealth = 100f;
		[Export] public float moveSpeed = 7f;
		[Export] public float sprintMultiplier = 1.5f;
		[Export] public float crouchMultiplier = 0.6f;

		protected float currentHealth;
		protected int currentCurrency;
		protected CharacterAnimator animator;
		protected Weapon.WeaponManager weaponManager;

		protected Vector3 moveDirection = Vector3.Zero;
		protected bool isSprintingInput = false;
		protected bool isCrouching = false;

		// Reference to the world-space health bar above this character's head
		// Set in _Ready by subclasses if a WorldHealthBar node exists
		public UI.WorldHealthBar WorldHealthBar;

		[Signal] public delegate void CurrencyChangedEventHandler(int newAmount);
		[Signal] public delegate void DiedEventHandler(Character killer);
		[Signal] public delegate void HealthChangedEventHandler(float current, float max);

		public override void _Ready()
		{
			currentHealth = maxHealth;
		}

		public abstract void OnInputAction(string action);
		public abstract void RequestAIDecision();

		public virtual void TakeDamage(float damage, Character attacker = null)
		{
			currentHealth -= damage;
			currentHealth = Mathf.Max(currentHealth, 0f);
			animator?.PlayTakeDamage();
			EmitSignal(nameof(HealthChanged), currentHealth, maxHealth);

			if (currentHealth <= 0)
				OnDeath(attacker);
		}

		public virtual void OnDeath(Character killer)
		{
			animator?.PlayDeath();
			EmitSignal(nameof(Died), killer);
		}

		public virtual void RequestMovement(Vector3 direction, bool isSprinting)
		{
			moveDirection = direction;
			isSprintingInput = isSprinting;
		}

		public virtual void ModifyCurrency(int amount)
		{
			currentCurrency += amount;
			EmitSignal(nameof(CurrencyChanged), currentCurrency);
		}

		public virtual bool CanAffordAction(Economy.CurrencyEconomy.CostType costType)
		{
			return Economy.CurrencyEconomy.CanAffordAction(this, costType);
		}

		public int GetCurrency() => currentCurrency;
		public float GetHealth() => currentHealth;
		public float GetMaxHealth() => maxHealth;

		public override void _PhysicsProcess(double delta)
		{
			Vector3 velocity = Velocity;
			Vector3 desired = moveDirection * moveSpeed
				* (isSprintingInput ? sprintMultiplier : 1f)
				* (isCrouching ? crouchMultiplier : 1f);
			velocity.X = desired.X;
			velocity.Z = desired.Z;
			Velocity = velocity;
			base._PhysicsProcess(delta);
		}
	}
}

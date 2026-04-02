using Godot;
using System;

namespace CashoutCasino.Character
{
    /// <summary>
    /// Abstract base character used by both players and AI enemies.
    /// Contains shared health, movement basics, currency management and signals.
    /// Override virtual methods to change behavior; prefer composition for complex subsystems.
    /// </summary>
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

        // Movement state
        protected Vector3 moveDirection = Vector3.Zero;
        protected bool isSprintingInput = false;
        protected bool isCrouching = false;

        [Signal] public delegate void CurrencyChangedEventHandler(int newAmount);
        [Signal] public delegate void DiedEventHandler(Character killer);

        public override void _Ready()
        {
            currentHealth = maxHealth;
        }

        // Input/AI hooks
        public abstract void OnInputAction(string action);
        public abstract void RequestAIDecision();

        // Core gameplay hooks
        public virtual void TakeDamage(float damage, Character attacker = null)
        {
            currentHealth -= damage;
            animator?.PlayTakeDamage();
            if (currentHealth <= 0)
            {
                OnDeath(attacker);
            }
        }

        public virtual void OnDeath(Character killer)
        {
            animator?.PlayDeath();
            EmitSignal(nameof(Died), killer);
            // Default: concrete classes or managers handle removal/respawn
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

        public override void _PhysicsProcess(double delta)
        {
            // Basic movement integration. Concrete classes should extend.
            Vector3 velocity = Velocity;
            Vector3 desired = moveDirection * moveSpeed * (isSprintingInput ? sprintMultiplier : 1f) * (isCrouching ? crouchMultiplier : 1f);
            velocity.x = desired.x;
            velocity.z = desired.z;
            Velocity = velocity;
            base._PhysicsProcess(delta);
        }
    }
}

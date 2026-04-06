using Godot;
using System;

namespace CashoutCasino.Characters
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

        // Round/buff multipliers (modified by BuffDebuffSystem)
        protected float moveSpeedMultiplier = 1f;
        protected float damageMultiplier = 1f;
        protected float fireRateMultiplier = 1f;
        protected float ammoCostMultiplier = 1f;

        // Movement state
        protected Vector3 moveDirection = Vector3.Zero;
        protected bool isSprintingInput = false;
        protected bool isCrouching = false;

        [Signal] public delegate void CurrencyChangedEventHandler(int newAmount);
        [Signal] public delegate void DiedEventHandler(Character killer);

        public override void _Ready()
        {
            currentHealth = maxHealth;
            AddToGroup("characters");
        }

        // Input/AI hooks
        public abstract void OnInputAction(string action);
        public abstract void RequestAIDecision();

        // Core gameplay hooks
        public virtual void TakeDamage(float damage, Character attacker = null)
        {
            currentHealth -= damage * damageMultiplier;
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

        public virtual void ApplyRoundModifiers(Managers.BuffDebuffSystem.BuffDebuff modifiers)
        {
            if (modifiers == null) return;
            moveSpeedMultiplier = modifiers.moveSpeedMultiplier;
            damageMultiplier = modifiers.damageMultiplier;
            fireRateMultiplier = modifiers.fireRateMultiplier;
            ammoCostMultiplier = modifiers.ammoCostMultiplier;
        }

        public virtual void ClearRoundModifiers()
        {
            moveSpeedMultiplier = 1f;
            damageMultiplier = 1f;
            fireRateMultiplier = 1f;
            ammoCostMultiplier = 1f;
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
            float appliedSpeed = moveSpeed * moveSpeedMultiplier * (isSprintingInput ? sprintMultiplier : 1f) * (isCrouching ? crouchMultiplier : 1f);
            Vector3 desired = moveDirection * appliedSpeed;
            velocity.X = desired.X;
            velocity.Z = desired.Z;
            Velocity = velocity;
            base._PhysicsProcess(delta);
        }

        [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void ServerReceiveInput(Vector3 direction, bool isSprinting, uint seq)
        {
            if (!Multiplayer.IsServer()) return;
            // Server-side: validate and apply movement
            RequestMovement(direction, isSprinting);
            // TODO: run server-side validation and authoritative physics steps
        }

        [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void ServerApplyDamage(float damage, long attackerNetId)
        {
            if (!Multiplayer.IsServer()) return;
            // Server applies damage and authoritative state changes
            TakeDamage(damage, null);
            // TODO: resolve attacker from net id and award currency/bounty
        }
    }
}

using Godot;
using System;
using CashoutCasino.Character;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
    /// <summary>
    /// Abstract base class for all weapons. Handles ammo bookkeeping, cooldowns and owner binding.
    /// Concrete weapons override Fire/Reload to implement behavior.
    /// </summary>
    public abstract partial class Weapon : Node3D
    {
        [Export] public float fireRate = 0.1f;
        [Export] public int ammoCost = 1;
        [Export] public int maxAmmo = 100;
        [Export] public float damagePerHit = 10f;

        protected int currentAmmo;
        protected double lastFireTime = 0f;
        protected Character owner;

        public virtual Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            throw new NotImplementedException();
        }

        public virtual void Reload()
        {
            // Default reload: refill ammo
            currentAmmo = maxAmmo;
        }

        public bool CanFire()
        {
            double now = OS.GetTicksMsec();
            if (currentAmmo < ammoCost) return false;
            if (now - lastFireTime < fireRate * 1000.0) return false;
            return true;
        }

        public int GetAmmoCount() => currentAmmo;
        public int GetAmmoCost() => ammoCost;

        public virtual void Equip(Character newOwner)
        {
            owner = newOwner;
        }

        public virtual void Unequip()
        {
            owner = null;
        }
    }
}

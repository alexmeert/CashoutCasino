using Godot;
using System;
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
		protected ulong lastFireTime = 0;
		protected CashoutCasino.Character.Character owner;

		public virtual Projectile.Projectile Fire(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			throw new NotImplementedException();
		}

		public virtual void Reload()
		{
			currentAmmo = maxAmmo;
		}

		public bool CanFire()
		{
			ulong now = Time.GetTicksMsec();
			if (currentAmmo < ammoCost) return false;
			if (now - lastFireTime < (ulong)(fireRate * 1000.0)) return false;
			return true;
		}

		public int GetAmmoCount() => currentAmmo;
		public int GetAmmoCost() => ammoCost;

		public virtual void Equip(CashoutCasino.Character.Character newOwner)
		{
			owner = newOwner;
		}

		public virtual void Unequip()
		{
			owner = null;
		}
	}
}

using Godot;
using System;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
	public abstract partial class Weapon : Node3D
	{
		[Export] public float fireRate = 0.1f;
		[Export] public int ammoCost = 1;
		[Export] public int maxAmmo = 100;
		[Export] public float damagePerHit = 10f;

		// Mirrors the player's currency for HUD display — not a separate resource
		public int currentAmmo;
		protected ulong lastFireTime = 0;
		protected CashoutCasino.Character.Character owner;

		public virtual Projectile.Projectile Fire(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			throw new NotImplementedException();
		}

		// Reload not meaningful when ammo = currency, kept as no-op for now
		public virtual void Reload() { }

		// Only checks fire rate — currency affordability is checked by WeaponManager
		public bool CanFire()
		{
			ulong now = Time.GetTicksMsec();
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

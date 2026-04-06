using Godot;
using System;

namespace CashoutCasino.Weapon
{
	public partial class AssaultRifle : HitscanWeapon
	{
		[Export] public float recoil = 1.0f;

		public override void _Ready()
		{
			fireRate = 0.1f;
			ammoCost = 1;
			damagePerHit = 15f;
			maxAmmo = 100;
			base._Ready();
		}

		public override Projectile.Projectile Fire(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			if (!CanFire()) return null;
			lastFireTime = Time.GetTicksMsec();
			currentAmmo -= ammoCost;
			PerformRaycast(direction, owner);
			return null;
		}
	}
}

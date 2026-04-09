using Godot;
using System;

namespace CashoutCasino.Weapon
{
	public partial class DualPistols : HitscanWeapon
	{
		[Export] public float alternateFireDelay = 0.05f;

		private bool leftNext = true;

		public override void _Ready()
		{
			fireRate = 0.2f;
			ammoCost = 1;
			damagePerHit = 10f;
			maxAmmo = 100;
			TrailColor = new Color(1f, 0.6f, 0.1f, 1f);
			base._Ready();
		}

		public override Projectile.Projectile Fire(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			if (!CanFire()) return null;
			lastFireTime = Time.GetTicksMsec();

			float offsetAmount = 0.02f;
			Vector3 offset = leftNext
				? new Vector3(-offsetAmount, 0f, 0f)
				: new Vector3(offsetAmount, 0f, 0f);
			leftNext = !leftNext;

			PerformRaycast((direction + offset).Normalized(), owner);
			return null;
		}
	}
}

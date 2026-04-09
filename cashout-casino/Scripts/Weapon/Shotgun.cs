using Godot;
using System;

namespace CashoutCasino.Weapon
{
	public partial class Shotgun : HitscanWeapon
	{
		[Export] public int pelletCount = 8;
		[Export] public float spreadAngle = 12f;

		private RandomNumberGenerator rng = new RandomNumberGenerator();

		public override void _Ready()
		{
			fireRate = 0.8f;
			ammoCost = 3;
			damagePerHit = 12f;
			maxAmmo = 40;
			TrailColor = new Color(1f, 0.2f, 0.1f, 1f);
			base._Ready();
			rng.Randomize();
		}

		public override Projectile.Projectile Fire(Vector3 direction, CashoutCasino.Character.Character owner)
		{
			if (!CanFire()) return null;
			lastFireTime = Time.GetTicksMsec();

			float spreadRad = Mathf.DegToRad(spreadAngle);

			for (int i = 0; i < pelletCount; i++)
			{
				Vector3 pelletDir = direction
					.Rotated(Vector3.Up, rng.RandfRange(-spreadRad, spreadRad))
					.Rotated(Vector3.Right, rng.RandfRange(-spreadRad, spreadRad))
					.Normalized();

				PerformRaycast(pelletDir, owner);
			}

			return null;
		}
	}
}

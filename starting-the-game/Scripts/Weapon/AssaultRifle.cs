using Godot;
using CashoutCasino.Economy;

namespace CashoutCasino.Weapon
{
	public partial class AssaultRifle : ProjectileWeapon
	{
		private static readonly PackedScene DefaultBulletScene =
			GD.Load<PackedScene>("res://Packed Scenes/Projectile/Bullet.tscn");

		[Export] public PackedScene bulletScene;
		[Export] public float bulletSpeed = 95f;

		public override CurrencyEconomy.CostType FireCostType => CurrencyEconomy.CostType.ShootAR;
		public override bool HoldToFire => true;

		protected override PackedScene ProjectileScene => bulletScene;
		protected override float ProjectileSpeed => bulletSpeed;

		public override void _Ready()
		{
			fireRate = 0.09f;
			ammoCost = 1;
			damagePerHit = 15f;
			maxAmmo = 100;

			bulletScene ??= DefaultBulletScene;
			base._Ready();
		}
	}
}

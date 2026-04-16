using Godot;
using CashoutCasino.Economy;

namespace CashoutCasino.Weapon
{
	public partial class DualPistols : ProjectileWeapon
	{
		private static readonly PackedScene DefaultBulletScene =
			GD.Load<PackedScene>("res://Packed Scenes/Projectile/Bullet.tscn");

		[Export] public Marker3D LeftMuzzle;
		[Export] public Marker3D RightMuzzle;
		[Export] public PackedScene bulletScene;
		[Export] public float bulletSpeed = 88f;

		private bool leftNext = true;

		public override WeaponKind Kind => WeaponKind.Pistol;
		public override CurrencyEconomy.CostType FireCostType => CurrencyEconomy.CostType.ShootPistol;

		protected override PackedScene ProjectileScene => bulletScene;
		protected override float ProjectileSpeed => bulletSpeed;

		public override void _Ready()
		{
			fireRate = 0.18f;
			ammoCost = 1;
			damagePerHit = 10f;
			maxAmmo = 100;

			bulletScene ??= DefaultBulletScene;
			base._Ready();
		}

		protected override Marker3D GetShotMuzzle()
		{
			Marker3D muzzle = leftNext ? LeftMuzzle : RightMuzzle;
			leftNext = !leftNext;
			return muzzle;
		}
	}
}

using Godot;
using System;
using CashoutCasino.Characters;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
    public partial class Shotgun : HitscanWeapon
    {
        [Export] public int pelletCount = 8;
        [Export] public float spreadAngle = 12f;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            base.Fire(direction, owner);
            // Shotgun produces multiple pellets or dice projectiles; implement spread logic.
            throw new NotImplementedException();
        }
    }
}

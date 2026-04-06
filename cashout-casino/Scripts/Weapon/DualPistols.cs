using Godot;
using System;
using CashoutCasino.Characters;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
    public partial class DualPistols : HitscanWeapon
    {
        [Export] public float alternateFireDelay = 0.05f;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            base.Fire(direction, owner);
            // Alternate firing between pistols or fire both depending on design
            throw new NotImplementedException();
        }
    }
}

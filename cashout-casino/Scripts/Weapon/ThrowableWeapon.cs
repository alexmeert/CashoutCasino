using Godot;
using System;
using CashoutCasino.Characters;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
    public abstract partial class ThrowableWeapon : Weapon
    {
        [Export] public PackedScene projectileScene;
        [Export] public float throwForce = 10f;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            if (!CanFire()) return null;
            lastFireTime = OS.GetTicksMsec();
            currentAmmo -= ammoCost;
            throw new NotImplementedException();
        }
    }
}

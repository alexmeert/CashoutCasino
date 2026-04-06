using Godot;
using System;
using CashoutCasino.Characters;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
    public abstract partial class HitscanWeapon : Weapon
    {
        [Export] public PackedScene projectileScene;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            if (!CanFire()) return null;
            lastFireTime = OS.GetTicksMsec();
            currentAmmo -= ammoCost;
            // Hitscan weapons typically apply damage immediately; optionally spawn a visual projectile
            throw new NotImplementedException();
        }
    }
}

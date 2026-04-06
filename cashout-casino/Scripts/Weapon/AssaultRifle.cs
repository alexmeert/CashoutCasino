using Godot;
using System;
using CashoutCasino.Characters;
using CashoutCasino.Projectile;

namespace CashoutCasino.Weapon
{
    public partial class AssaultRifle : HitscanWeapon
    {
        [Export] public float recoil = 1.0f;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            base.Fire(direction, owner);
            // Implement rifle hitscan logic here. Spawn tracer VFX via projectileScene
            throw new NotImplementedException();
        }
    }
}

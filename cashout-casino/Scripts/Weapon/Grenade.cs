using Godot;
using System;
using CashoutCasino.Character;

namespace CashoutCasino.Weapon
{
    public partial class Grenade : ThrowableWeapon
    {
        [Export] public float explosionRadius = 4f;
        [Export] public float fuseTime = 2f;
        [Export] public float explosionDamage = 50f;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            base.Fire(direction, owner);
            // Spawning grenade projectile which will explode on fuse
            throw new NotImplementedException();
        }
    }
}

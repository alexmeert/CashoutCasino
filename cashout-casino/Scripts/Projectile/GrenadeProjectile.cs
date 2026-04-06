using Godot;
using System;
using CashoutCasino.Characters;

namespace CashoutCasino.Projectile
{
    public partial class GrenadeProjectile : Projectile
    {
        [Export] public float explosionRadius = 4f;
        [Export] public float explosionDamage = 50f;

        private float fuseTimer = 0f;

        public override void Launch(Vector3 dir, Character projectileOwner)
        {
            base.Launch(dir, projectileOwner);
            fuseTimer = 0f;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            fuseTimer += (float)delta;
            // Explosion logic based on fuse is left to implementation
        }

        public override void OnHit(Node3D hitTarget)
        {
            // Grenade explodes on impact or fuse expiry; implement area damage
            throw new NotImplementedException();
        }
    }
}

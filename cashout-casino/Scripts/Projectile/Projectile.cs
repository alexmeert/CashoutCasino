using Godot;
using System;
using CashoutCasino.Characters;

namespace CashoutCasino.Projectile
{
    /// <summary>
    /// Generic projectile base for moving objects, hitscan wrappers or area effects.
    /// </summary>
    public abstract partial class Projectile : Area3D
    {
        [Export] public float speed = 30f;
        [Export] public float lifetime = 10f;
        [Export] public float baseDamage = 10f;

        protected Vector3 direction = Vector3.Zero;
        protected Character owner;
        protected float spawnTime = 0f;

        public virtual void Launch(Vector3 dir, Character projectileOwner)
        {
            direction = dir.Normalized();
            owner = projectileOwner;
            spawnTime = OS.GetTicksMsec();
        }

        public abstract void OnHit(Node3D hitTarget);

        public virtual float ApplyDamage(Character target)
        {
            target.TakeDamage(baseDamage, owner);
            return baseDamage;
        }

        public virtual void Despawn()
        {
            QueueFree();
        }

        public override void _PhysicsProcess(double delta)
        {
            // Default kinematic movement
            Translate(direction * (float)delta * speed);
            if (OS.GetTicksMsec() - spawnTime > lifetime * 1000f) Despawn();
        }
    }
}

using Godot;
using System;

namespace CashoutCasino.Projectile
{
    /// <summary>
    /// Visual/tracer projectile for hitscan weapons. In many cases hits are applied immediately without an Area3D.
    /// </summary>
    public partial class HitscanProjectile : Projectile
    {
        public override void OnHit(Node3D hitTarget)
        {
            // Apply direct damage to target if it's a Character
            if (hitTarget is CashoutCasino.Character.Character c)
            {
                ApplyDamage(c);
            }
            Despawn();
        }
    }
}

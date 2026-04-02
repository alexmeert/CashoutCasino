using Godot;
using System;
using CashoutCasino.Character;

namespace CashoutCasino.Weapon
{
    public abstract partial class MeleeWeapon : Weapon
    {
        [Export] public float reach = 2.0f;
        [Export] public float swingTime = 0.3f;

        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            // Melee applies immediate area damage; no projectile by default
            throw new NotImplementedException();
        }
    }
}

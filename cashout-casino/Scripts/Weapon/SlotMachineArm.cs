using Godot;
using System;
using CashoutCasino.Character;

namespace CashoutCasino.Weapon
{
    /// <summary>
    /// Always-available melee arm. Zero cost, used for special interactions (slot machine, melee hits).
    /// </summary>
    public partial class SlotMachineArm : MeleeWeapon
    {
        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            // Slot machine arm: apply zero-cost melee effect or trigger slot interaction
            throw new NotImplementedException();
        }
    }
}

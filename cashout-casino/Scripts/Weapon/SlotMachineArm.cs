using Godot;
using System;
using CashoutCasino.Characters;

namespace CashoutCasino.Weapon
{
    public partial class SlotMachineArm : MeleeWeapon
    {
        public override Projectile.Projectile Fire(Vector3 direction, Character owner)
        {
            // Slot machine arm: apply zero-cost melee effect or trigger slot interaction
            throw new NotImplementedException();
        }
    }
}

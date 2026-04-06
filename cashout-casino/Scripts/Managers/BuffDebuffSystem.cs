using Godot;
using System;
using CashoutCasino.Characters;

namespace CashoutCasino.Managers
{
    /// <summary>
    /// Central system for applying round-wide buffs/debuffs. Make this a single node in the scene and call
    /// SetRoundModifiers at the start of each round.
    /// </summary>
    public partial class BuffDebuffSystem : Node
    {
        public class BuffDebuff
        {
            public float moveSpeedMultiplier = 1f;
            public float damageMultiplier = 1f;
            public float fireRateMultiplier = 1f;
            public float ammoCostMultiplier = 1f;
        }

        public static BuffDebuffSystem Instance { get; private set; }

        public BuffDebuff CurrentModifiers { get; private set; } = new BuffDebuff();

        public override void _EnterTree()
        {
            base._EnterTree();
            Instance = this;
        }

        /// <summary>
        /// Replace current round modifiers and apply to all existing characters.
        /// </summary>
        public void SetRoundModifiers(BuffDebuff modifiers)
        {
            CurrentModifiers = modifiers ?? new BuffDebuff();
            ApplyToAllCharacters();
        }

        public void ClearRoundModifiers()
        {
            CurrentModifiers = new BuffDebuff();
            ApplyToAllCharacters();
        }

        private void ApplyToAllCharacters()
        {
            foreach (var c in GetTree().GetNodesInGroup("characters"))
            {
                if (c is Character ch)
                {
                    ch.ApplyRoundModifiers(CurrentModifiers);
                }
            }
        }
    }
}

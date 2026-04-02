using Godot;
using System;

namespace CashoutCasino.Character
{
    /// <summary>
    /// Player-specific character. Input handling, viewmodels and HUD hooks.
    /// Implement `_Input` and map to `OnInputAction`.
    /// </summary>
    public partial class Player : Character
    {
        private PlayerFirstPersonController fpController;
        private UI.PlayerHUD playerHUD;

        public override void _Ready()
        {
            base._Ready();
            // Acquire references in scene tree in concrete scenes
        }

        public override void OnInputAction(string action)
        {
            // Map actions (move, jump, fire, reload, switch) to behavior.
            throw new NotImplementedException();
        }

        public override void RequestAIDecision()
        {
            // Players do not use AI decisions.
            throw new NotImplementedException();
        }

        public override void _Input(InputEvent @event)
        {
            // Translate Godot InputEvents into OnInputAction calls.
            // Example: if Input.IsActionPressed("fire") call weaponManager.FireCurrentWeapon(...)
        }
    }
}

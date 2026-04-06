using Godot;
using System;

namespace CashoutCasino.Characters
{
    public partial class Player : Character
    {
        private PlayerFirstPersonController fpController;
        private CashoutCasino.UI.PlayerHUD playerHUD;

        public override void _Ready()
        {
            base._Ready();
            // Acquire references in scene tree in concrete scenes
            fpController = GetNodeOrNull<PlayerFirstPersonController>("PlayerFirstPersonController");
            if (fpController == null)
            {
                fpController = new PlayerFirstPersonController();
                AddChild(fpController);
                fpController.ownerCharacter = this;
            }
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
        }
    }
}

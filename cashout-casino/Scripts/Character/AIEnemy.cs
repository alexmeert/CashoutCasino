using Godot;
using System;

namespace CashoutCasino.Characters
{
    public partial class AIEnemy : Character
    {
        private AIController aiController;
        private float decisionCooldown = 0.25f;
        private float decisionTimer = 0f;

        public override void _Ready()
        {
            base._Ready();
        }

        public override void RequestAIDecision()
        {
            throw new NotImplementedException();
        }

        public override void OnInputAction(string action)
        {
            // AI doesn't receive player input.
            throw new NotImplementedException();
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            decisionTimer -= (float)delta;
            if (decisionTimer <= 0f)
            {
                RequestAIDecision();
                decisionTimer = decisionCooldown;
            }
        }
    }
}

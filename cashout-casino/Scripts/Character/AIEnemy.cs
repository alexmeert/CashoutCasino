using Godot;
using System;

namespace CashoutCasino.Character
{
	/// <summary>
	/// AI-controlled enemy. Uses AIController for decisions and pathfinding.
	/// </summary>
	public partial class AIEnemy : Character
	{
		private AIController aiController;
		private float decisionCooldown = 0.25f;
		private float decisionTimer = 0f;

		public override void _Ready()
		{
			base._Ready();
		}

		public override void RequestAIDecision() { }

		public override void OnInputAction(string action) { }

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

		public void OnBulletHit()
		{
			TakeDamage(10); // Example damage value
			GD.Print("AI Enemy hit by bullet! Current health: " + currentHealth);
		}
	}
}

using Godot;
using System;

namespace CashoutCasino.Characters
{
    public partial class CharacterAnimator : Node
    {
        public enum State { Idle, Walk, Run, Crouch, Jump, Land, Shoot, Reload, TakeDamage, Death }

        protected State currentState = State.Idle;

        public virtual void SetState(State s)
        {
            currentState = s;
        }

        public virtual void PlayTakeDamage() { }
        public virtual void PlayDeath() { }
        public virtual void PlayShoot() { }
        public virtual void PlayReload() { }
    }
}

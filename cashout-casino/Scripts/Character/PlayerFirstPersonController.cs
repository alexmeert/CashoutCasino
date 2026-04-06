using Godot;
using System;
using System.Collections.Generic;
using CashoutCasino.Characters;

namespace CashoutCasino.Characters
{
    public partial class PlayerFirstPersonController : Node
    {
        public struct InputState
        {
            public uint seq;
            public Vector3 direction;
            public bool isSprinting;
        }

        public Character ownerCharacter;
        private uint inputSeq = 0;
        private List<InputState> pendingInputs = new List<InputState>();

        [Export] public float inputSendRate = 0.05f; // seconds between sends
        private float sendTimer = 0f;

        public override void _Ready()
        {
            base._Ready();
            if (ownerCharacter == null && GetParent() is Character c)
                ownerCharacter = c;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            // Sample input
            Vector3 dir = new Vector3(
                Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
                0,
                Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward")
            );
            if (dir.Length() > 1f) dir = dir.Normalized();
            bool sprint = Input.IsActionPressed("sprint");

            // Local prediction
            ownerCharacter?.RequestMovement(dir, sprint);

            // Buffer input
            var state = new InputState { seq = inputSeq++, direction = dir, isSprinting = sprint };
            pendingInputs.Add(state);

            // Send periodically (batching)
            sendTimer += (float)delta;
            if (sendTimer >= inputSendRate)
            {
                sendTimer = 0f;
                SendPendingInputs();
            }
        }

        private void SendPendingInputs()
        {
            if (GenericCore.Instance == null) return;
            long serverId = GenericCore.Instance.GetServerNetId();
            foreach (var s in pendingInputs)
            {
                // RPC to server; server method should be implemented on the authoritative Character node
                RpcId(serverId, nameof(Character.ServerReceiveInput), s.direction, s.isSprinting, s.seq);
            }
            // keep pending inputs until server acks (reconciliation not yet implemented)
        }

        // Called when server sends authoritative state to reconcile
        public void OnServerReconcile(uint lastAckSeq, Vector3 serverPosition)
        {
            // TODO: implement reconciliation: remove acked inputs, replay remaining, smooth corrections
            throw new NotImplementedException();
        }
    }
}

using Godot;
using System;

public partial class GameMaster : Node
{
	[Export] public bool GameStarted;
	[Export] public bool GameFinished;
	[Export] public NetworkCore NpmSpawner;
	[Export] public NetworkCore LevelSpawner;


	[Export] public OptionButton LevelDropdown;

	
	[Export] public int SelectedLevel = 0;

	public override void _Ready()
	{
		base._Ready();

		if (LevelDropdown != null)
			LevelDropdown.Visible = false;

		ResolveNpmSpawner();
		// Player Lobby loads after peers are already connected; ClientConnected will not fire again for them.
		if (GenericCore.Instance != null && GenericCore.Instance.IsServer && NpmSpawner != null)
		{
			NpmSpawner.SpawnInexZeroOnConnect = true;
			CallDeferred(new StringName(nameof(SpawnNpmsForAlreadyConnectedPeers)));
		}
	}

	/// <summary>
	/// Finds the scene's <see cref="NetworkCore"/> (MultiplayerSpawner) if not wired in the editor.
	/// </summary>
	private void ResolveNpmSpawner()
	{
		if (NpmSpawner != null)
			return;
		var root = GetParent();
		if (root == null)
			return;
		NpmSpawner = root.FindChild("MultiplayerSpawner", true, false) as NetworkCore;
		if (NpmSpawner == null)
			GD.PushWarning("[GameMaster] No NetworkCore / MultiplayerSpawner found — NPMs will not spawn.");
	}

	/// <summary>
	/// Server-only: spawn one User NPM per connected peer that does not already have one.
	/// </summary>
	private void SpawnNpmsForAlreadyConnectedPeers()
	{
		if (GenericCore.Instance == null || !GenericCore.Instance.IsServer || NpmSpawner == null)
			return;
		foreach (int peerIdInt in Multiplayer.GetPeers())
		{
			long peerId = peerIdInt;
			if (PeerHasNpm(peerId))
				continue;
			NpmSpawner.NetCreateObject(0, Vector3.Zero, Quaternion.Identity, peerId);
		}
	}

	private bool PeerHasNpm(long peerId)
	{
		foreach (var raw in GetTree().GetNodesInGroup("NPM"))
		{
			if (raw is UserNpm npm && npm.MyNetID is NetID netId && netId.OwnerId == peerId)
				return true;
		}
		return false;
	}


	// Called from HostButton.pressed — the server shows the dropdown immediately and starts the game-cycle loop.

	public void OnServerStarted()
	{
		if (!GenericCore.Instance.IsServer) return;

		ResolveNpmSpawner();
		if (NpmSpawner != null)
		{
			NpmSpawner.SpawnInexZeroOnConnect = true;
			CallDeferred(new StringName(nameof(SpawnNpmsForAlreadyConnectedPeers)));
		}

		// Show the level dropdown right away for the host.
		if (LevelDropdown != null)
			LevelDropdown.Visible = true;

		GameCycle();
	}


	// Called on clients (NOT the host) when they successfully connect.
	// The dropdown stays hidden for everyone except the host.

	public void OnClientConnected()
	{
		// Clients never see the level dropdown — nothing to do here for now.
		// (This hook is kept for Phase 2 client-side setup if needed.)
	}


	// Connect the LevelDropdown.ItemSelected signal to this method.
	
	public void OnLevelSelected(int index)
	{
		// Only the server/host player should be able to change this.
		if (!GenericCore.Instance.IsServer) return;
		SelectedLevel = index;
		GD.Print($"[GameMaster] Host selected level: {SelectedLevel}");
	}


	// Main game-cycle loop – runs on the SERVER only.

	public async void GameCycle()
	{
		// Safety guard: only the server should run this logic.
		if (!GenericCore.Instance.IsServer)
		{
			GD.PrintErr("[GameMaster] GameCycle called on a non-server instance aborting.");
			return;
		}

		GD.Print("[GameMaster] GameCycle started waiting for players to ready up...");
		
		while (!GameStarted)
		{
			var npms = GetTree().GetNodesInGroup("NPM");

			if (npms.Count >= 2)
			{
				bool allReady = true;
				foreach (var rawNode in npms)
				{
					if (rawNode is UserNpm npm && !npm.IsReady)
					{
						allReady = false;
						break;
					}
				}

				if (allReady)
				{
					GameStarted = true;
					GD.Print("[GameMaster] All players ready starting game!");
				}
				else
				{
					GD.Print($"[GameMaster] {npms.Count} player(s) connected, waiting for ready...");
				}
			}
			else
			{
				GD.Print($"[GameMaster] Only {npms.Count} player(s) need at least 2.");
			}

			// Poll every 2.5 seconds so we don't spam the log.
			await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
		}


		//Broadcast "GAMESTART" to all clients.

		Rpc(MethodName.ReceiveGameStart, SelectedLevel);
	}

	//Received on ALL peers (server + every client).

	[Rpc(MultiplayerApi.RpcMode.Authority,
		 CallLocal = true,
		 TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReceiveGameStart(int levelIndex)
	{
		GD.Print($"[GameMaster] GAMESTART received! Level index: {levelIndex}");


		//Hide all UserNpm canvas objects on the client.
	
		var npms = GetTree().GetNodesInGroup("NPM");
		foreach (var rawNode in npms)
		{
			if (rawNode is UserNpm npm)
			{
				//UserNpm extends Control, so we can simply hide the whole node.
				npm.Visible = false;
			}
		}

		// Hide the level dropdown too (no longer needed mid-game).
		if (LevelDropdown != null)
			LevelDropdown.Visible = false;

		// Store the level so Phase 2 can use it when spawning.
		SelectedLevel = levelIndex;


		// Kick off Phase 2 from here (or emit a signal for another script).
	

		GD.Print($"[GameMaster] Ready to spawn level {SelectedLevel} and characters.");
	}
}

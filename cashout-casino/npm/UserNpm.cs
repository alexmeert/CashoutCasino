using Godot;
using System;
using System.Collections.Generic;

public partial class UserNpm : Control
{
	[Export] public string PlayerName;
	[Export] public int Team;
	[Export] public int WeaponClass;
	
	[Export] public TextEdit MyName;
	[Export] public ColorRect MyBG;
	[Export] OptionButton TeamOptionButton;
	[Export] OptionButton WeaponClassOptionButton;
	[Export] Label ReadyLabel;
	[Export] CheckBox ReadyCheckBox;
	
	[Export] public NetID MyNetID;
	
	[Export] public bool IsReady;
	[Export] public Color MyColor;
	
	public override void _Ready()
	{
		AddToGroup("NPM");
		base._Ready();
		SlowStart();
	}
	
	public async void SlowStart()
	{
		// Wait longer for all nodes to initialize
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		IsReady = false;
		
		// Set default color to white
		MyColor = new Color(1, 1, 1, 1);
		
		if(!MyNetID.IsLocal)
		{
			MyName.Editable = false;
			TeamOptionButton.Disabled = true;
			WeaponClassOptionButton.Disabled = true;
		}
		else
		{
			// Delay color update RPC
			await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
			UpdateAvailableColors();
		}
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if(!MyNetID.IsLocal)
		{
			// Only update remote player's UI from synced data
			MyName.Text = PlayerName;
			MyBG.Color = MyColor;
			TeamOptionButton.Selected = Team;
			WeaponClassOptionButton.Selected = WeaponClass;
			ReadyCheckBox.ButtonPressed = IsReady;
		}
	}

	//Ask the server to change the team
	public void OnTeamChange(int n)
	{
		GD.Print($"OnTeamChange called! n={n}");
		if(MyNetID.IsLocal)
		{
			Rpc(MethodName.TeamChangeRPC, n);
		}
	}
	
	//Change the team of a player
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void TeamChangeRPC(int n)
	{
		GD.Print($"TeamChangeRPC called with n={n}");
		GD.Print($"IsServer: {GenericCore.Instance.IsServer}");
		
		if(GenericCore.Instance.IsServer)
		{
			// Get the peer ID of who sent this RPC
			int requestingPeerId = Multiplayer.GetRemoteSenderId();
			
			// Check if color is already taken by another player
			if (IsColorTaken(n, requestingPeerId))
			{
				GD.Print($"Color {n} is already taken!");
				return;
			}

			Team = n;
			ApplyTeamColor(n);
			GD.Print($"Server set MyColor to: {MyColor}");
			
			// Tell all clients to update available colors
			Rpc(MethodName.UpdateAvailableColors);
		}
	}

	private bool IsColorTaken(int colorIndex, int requestingPeerId)
	{
		// Get all UserNpm panels in the scene
		var npmPanels = GetTree().GetNodesInGroup("NPM");
		
		foreach (UserNpm npm in npmPanels)
		{
			// Skip the requesting player
			if (npm.MyNetID.OwnerId == requestingPeerId)
				continue;
			
			// Check if another player has this color
			if (npm.Team == colorIndex)
				return true;
		}
		
		return false;
	}
	
	private void ApplyTeamColor(int colorIndex)
	{
		switch(colorIndex)
		{
			case 0: //Red
				MyBG.Color = new Color(1, 0, 0, 1);
				break;
			case 1: //Green
				MyBG.Color = new Color(0, 1, 0, 1);
				break;
			case 2: //Blue
				MyBG.Color = new Color(0, 0, 1, 1);
				break;
			case 3: //Yellow
				MyBG.Color = new Color(1, 1, 0, 1);
				break;
			case 4: //Purple
				MyBG.Color = new Color(1, 0, 1, 1);
				break;
			case 5: //Orange
				MyBG.Color = new Color(1, 0.5f, 0, 1);
				break;
			default:
				break;
		}
		MyColor = MyBG.Color;
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true, TransferMode=MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void UpdateAvailableColors()
	{
		if (TeamOptionButton == null)
			return;

		// Get all taken colors
		var takenColors = new HashSet<int>();
		var npmPanels = GetTree().GetNodesInGroup("NPM");
		
		foreach (UserNpm npm in npmPanels)
		{
			takenColors.Add(npm.Team);
		}

		// Disable taken colors in the dropdown
		for (int i = 0; i < TeamOptionButton.ItemCount; i++)
		{
			bool isTaken = takenColors.Contains(i);
			TeamOptionButton.SetItemDisabled(i, isTaken);
		}
	}
	
	public void OnWeaponClassChange(int n)
	{
		if(MyNetID.IsLocal)
		{
			GD.Print($"Local player changing WeaponClass to: {n}");
			Rpc(MethodName.WeaponClassChangeRPC, n);
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void WeaponClassChangeRPC(int n)
	{
		GD.Print($"WeaponClassChangeRPC called with n={n}");
		
		if(GenericCore.Instance.IsServer)
		{
			GD.Print($"Server setting WeaponClass from {WeaponClass} to {n}");
			WeaponClass = n;
			GD.Print($"WeaponClass is now: {WeaponClass}");
			
			// Broadcast the change to all clients
			Rpc(MethodName.SyncWeaponClass, n);
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal=true, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SyncWeaponClass(int newWeaponClass)
	{
		GD.Print($"SyncWeaponClass: updating WeaponClass to {newWeaponClass}");
		WeaponClass = newWeaponClass;
	}
	
	//Ask the server to change our name
	public void OnNameChange()
	{
		if(MyNetID.IsLocal)
		{
			Rpc(MethodName.NameChangeRPC, MyName.Text);
		}
	}
	
	//Change the name of a client
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void NameChangeRPC(string Text)
	{
		if(GenericCore.Instance.IsServer)
		{
			PlayerName = Text;
			MyName.Text = Text;
		}
	}
	
	//Ask the server to change ready
	public void OnIsReady(bool Change)
	{
		if(MyNetID.IsLocal)
		{
			Rpc(MethodName.IsReadyChange, Change);
		}
	}
	
	//Set the client to be ready
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void IsReadyChange(bool Change)
	{
		if(!GenericCore.Instance.IsServer)
		{
			return;
		}
		IsReady = Change;
	}
}
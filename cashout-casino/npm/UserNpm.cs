using Godot;
using System;

public partial class UserNpm : Control
{
	[Export] public string PlayerName;
	[Export] public int Team;
	[Export] public int Sprite;
	
	[Export] public TextEdit MyName;
	[Export] public ColorRect MyBG;
	[Export] public Color MyColor;
	[Export] public OptionButton PlayerColor;
	[Export] public OptionButton PlayerWeaponClass;
	[Export] Label ReadyLabel;
	[Export] CheckBox ReadyCheckBox;
	
	[Export] public NetID MyNetID;
	
	[Export] public bool IsReady;

	/// <summary>
	/// True when this panel is for the local peer (editable name, color/class, ready).
	/// </summary>
	private bool IsLocalNpm()
	{
		if (MyNetID == null)
			return false;
		return MyNetID.IsLocal;
	}
	
	public override void _Ready()
	{
		AddToGroup("NPM");
		base._Ready();
		SlowStart();
	}
	
	public async void SlowStart()
	{
		if (MyNetID == null)
		{
			GD.PushError("UserNpm: assign MyNetID (NetID / MultiplayerSynchronizer on this instance).");
			return;
		}

		await ToSignal(MyNetID, NetID.SignalName.NetIDReady);

		await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		IsReady = false;
		
		// Set default color to white
		MyColor = new Color(1, 1, 1, 1); // White
		
		if (!IsLocalNpm())
		{
			if (MyName != null)
				MyName.Editable = false;
			if (PlayerColor != null)
				PlayerColor.Disabled = true;
			if (PlayerWeaponClass != null)
				PlayerWeaponClass.Disabled = true;
			if (ReadyCheckBox != null)
				ReadyCheckBox.Disabled = true;
		}
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (!IsLocalNpm())
		{
			if (MyName != null)
				MyName.Text = PlayerName;
			if (MyBG != null)
				MyBG.Color = MyColor;
			if (PlayerColor != null)
				PlayerColor.Selected = Team;
			if (PlayerWeaponClass != null)
				PlayerWeaponClass.Selected = Sprite;
			if (ReadyCheckBox != null)
				ReadyCheckBox.ButtonPressed = IsReady;
		}
	}

	//Ask the server to change the team
	public void OnTeamChange(int n)
	{
		GD.Print($"OnTeamChange called! n={n}");
		//Only the local player should be asking the player to change their team
		//Prevents the local player from controlling other players' teams
		//Solves the issue of why player can control all PCs in a scene
		if (IsLocalNpm())
			Rpc(MethodName.TeamChangeRPC, n);
	}
	
	//Change the team of a player
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void TeamChangeRPC(int n){
		GD.Print($"TeamChangeRPC called with n={n}");
		GD.Print($"IsServer: {GenericCore.Instance.IsServer}");
		
		if(GenericCore.Instance.IsServer)
		{
			//Why not use Multiplayer.IsServer? Because that'll work if it's not connected to anything
			Team = n;
			switch(n){
				case 0: //Red
					MyBG.Color = new Color(1,0,0,1);
					break;
				case 1: //Green
					MyBG.Color = new Color(0,1,0,1);
					break;
				case 2: //Blue
					MyBG.Color = new Color(0,0,1,1);
					break;
				case 3: //Yellow
					MyBG.Color = new Color(1,1,0,1);
					break;
				case 4: //Purple
					MyBG.Color = new Color(1,0,1,1);
					break;
				case 5: //Orange
					MyBG.Color = new Color(1,0.5f,0,1);
					break;
				default:
					break;
			}
			MyColor = MyBG.Color;
			GD.Print($"Server set MyColor to: {MyColor}");
			//Should just synchronize across all players
			//Don't forget to synchronize the color rect!
		}
	}
	
	public void OnSpriteChange(int n)
	{
		if (IsLocalNpm())
			Rpc(MethodName.SpriteChangeRPC, n);
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SpriteChangeRPC(int n){
		if(GenericCore.Instance.IsServer)
		{
			GD.Print($"Server setting Sprite from {Sprite} to {n}");
			Sprite = n;
			GD.Print($"Sprite is now: {Sprite}");
		}
	}
	
	//Ask the server to change our name
	public void OnNameChange()
	{
		if (IsLocalNpm() && MyName != null)
			Rpc(MethodName.NameChangeRPC, MyName.Text);
	}
	
	//Change the name of a client
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void NameChangeRPC(string Text){
		if(GenericCore.Instance.IsServer){
			PlayerName = Text;
			MyName.Text = Text;
		}
	}
	
	//Ask the server to change ready
	public void OnIsReady(bool Change)
	{
		if (IsLocalNpm())
			Rpc(MethodName.IsReadyChange, Change);
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

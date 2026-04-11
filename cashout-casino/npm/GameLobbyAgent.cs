using Godot;
using System;

public partial class GameLobbyAgent : Control
{
    [Export]
    public Button LeaveButton;

    [Export]
    public Label GameInfoLabel;

    [Export]
    public NetID MyNetID;

    private LobbyStreamlined myLobbySystem;
    private bool isDisconnecting = false;
    private bool isHeadless = false;

    public override void _Ready()
    {
        // Check if running headless
        isHeadless = DisplayServer.GetName() == "headless";
        
        GD.Print($"[GameLobbyAgent] Created! Headless: {isHeadless}");
        
        if (isHeadless)
        {
            Visible = false;
            return;
        }
        
        GD.Print("[GameLobbyAgent] Initialized in headful mode");
        base._Ready();
        myLobbySystem = LobbyStreamlined.Instance;
        SlowStart();
    }

    public async void SlowStart()
    {
        if (isHeadless)
            return;
            
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        
        if (LeaveButton != null)
        {
            LeaveButton.Pressed += OnLeavePressed;
            
            if (MyNetID != null && !MyNetID.IsLocal)
            {
                LeaveButton.Disabled = true;
                LeaveButton.Visible = false;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (isHeadless)
            return;
            
        base._Process(delta);
        
        if (GameInfoLabel != null && !isDisconnecting && 
            GenericCore.Instance != null && !GenericCore.Instance.IsQueuedForDeletion())
        {
            try
            {
                int playerCount = GenericCore.Instance._peers.Count;
                GameInfoLabel.Text = $"Players in lobby: {playerCount}";
            }
            catch (ObjectDisposedException)
            {
                isDisconnecting = true;
            }
        }
    }

    public void OnLeavePressed()
    {
        if (isHeadless)
            return;
            
        GD.Print("[GameLobbyAgent] Leaving the game lobby...");
        LeaveLobby();
    }

    private void LeaveLobby()
    {
        if (isDisconnecting)
            return;

        isDisconnecting = true;

        try
        {
            if (GenericCore.Instance != null && !GenericCore.Instance.IsQueuedForDeletion() && 
                GenericCore.Instance.IsServer)
            {
                GD.Print("[GameLobbyAgent] Server initiating shutdown...");
                GenericCore.Instance.ShutdownGame();
                
                GetTree().CreateTimer(1.0f).Timeout += ReturnToLobbyUI;
            }
            else
            {
                if (GenericCore.Instance != null && !GenericCore.Instance.IsQueuedForDeletion() && 
                    GenericCore.Instance.IsGenericCoreConnected)
                {
                    GenericCore.Instance.DisconnectFromGame();
                }
                
                GetTree().CreateTimer(0.5f).Timeout += ReturnToLobbyUI;
            }
        }
        catch (ObjectDisposedException ex)
        {
            GD.PrintErr($"[GameLobbyAgent] Error during shutdown: {ex.Message}");
            ReturnToLobbyUI();
        }
    }

    private void ReturnToLobbyUI()
    {
        GD.Print("[GameLobbyAgent] Returning to lobby menu...");
        
        try
        {
            if (myLobbySystem != null && !myLobbySystem.IsQueuedForDeletion())
            {
                myLobbySystem.DisconnectFromLobbySystem();
            }
        }
        catch (ObjectDisposedException)
        {
            GD.Print("[GameLobbyAgent] LobbySystem already freed");
        }
        
        GetTree().ReloadCurrentScene();
    }
}
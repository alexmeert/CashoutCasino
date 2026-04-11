using Godot;
using System;
using System.Text;
using System.Threading.Tasks;

[GlobalClass]
public partial class LobbyStreamlined : Node
{
    [Export]
    public string PublicIP;
    [Export]
    public string PrivateIP;

    [Export]
    public int PortMinimum;

    [Export]
    private int portOffset = 1;

    [Export]
    private int PortMaximum;

    public string LobbyServerIP;
    private bool UsePublic;
    private bool UsePrivate;
    private bool UseLocal;

    public bool IsWanLobbyConnected;
    public bool IsWanLobbyServer;

    public static LobbyStreamlined Instance;

    [Export]
    private MultiplayerSpawner AgentSpawner;

    private ENetMultiplayerPeer AgentPeer;

    private Godot.MultiplayerApi AgentAPI;

    private NodePath LobbyRootPath;

    [Export]
    public TextEdit GameNameBox;

    public string tempGameName;

    [Export]
    public float MaxGameTime = 30;
    
    public override void _Ready()
    {
        Instance = this;
        AgentAPI = MultiplayerApi.CreateDefaultInterface();
        GetTree().SetMultiplayer(AgentAPI, GetPath());
        LobbyRootPath = GetPath();
        AgentAPI.PeerConnected += OnPeerConnected;
        AgentAPI.PeerDisconnected += OnPeerDisconnected;

        string[] args = OS.GetCmdlineArgs();
        AgentSpawner.SpawnFunction = new Callable(this, nameof(SpawnAgent));
        bool isGameServer = false;
        bool isMasterServer = false;
        
        foreach (string arg in args)
        {
            if (arg == "MASTER")
            {
                GD.Print("[LobbyStreamlined] Creating MASTER server");
                isMasterServer = true;
                CreateMasterServer();
                break;  // Exit loop after finding MASTER
            }
            if(arg.Contains("GAMENAME"))
            {
                tempGameName = arg.Split('#')[1];
                isGameServer = true;
            }
        }

        if (!IsWanLobbyConnected && !isMasterServer)
        {
            GD.Print("[LobbyStreamlined] Connecting the agent to the master!");
            if (!isGameServer)
            {
                GD.Print("[LobbyStreamlined] Connecting agent to master server using IP Ping");
                CheckIPAddresses();
            }
            else
            {
                GD.Print("[LobbyStreamlined] Connecting game server to local master.");
                LobbyServerIP = "127.0.0.1";
                JoinLobbyServer();
            }
        }
        else if (isMasterServer)
        {
            GD.Print("[LobbyStreamlined] Master server created, not connecting as client");
        }
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"[LobbyStreamlined] Peer connected: {id}");
        
        if (IsWanLobbyServer)
        {
            GD.Print($"[LobbyStreamlined] Spawning Agent for peer {id}");
            AgentSpawner.Spawn(id);
        }
        else
        {
            GD.Print($"[LobbyStreamlined] Client connected (not server, no spawn)");
        }
    }

    private Node SpawnAgent(Variant d)
    {
        long peerId = (long)d;
        var packedScene = GD.Load<PackedScene>(AgentSpawner._SpawnableScenes[0]);
        var node = packedScene.Instantiate();
        node.SetMultiplayerAuthority((int)peerId, true);
        return node;
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"[LobbyStreamlined] Peer disconnected: {id}");

        if (!IsWanLobbyServer)
            return;

        Node spawnRoot = GetNode(AgentSpawner.GetPath() + "/" + AgentSpawner.SpawnPath);

        foreach (Node child in spawnRoot.GetChildren())
        {
            if (child.GetMultiplayerAuthority() == id)
            {
                GD.Print($"[LobbyStreamlined] Freeing agent owned by {id}");
                child.QueueFree();
            }
        }
    }

    public async Task CheckIPAddresses()
    {
        GD.Print("[LobbyStreamlined] Attempting to connect to public IP.");
        GD.Print("[LobbyStreamlined] Trying Public IP Address: " + PublicIP.ToString());
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        System.Net.NetworkInformation.PingOptions po = new System.Net.NetworkInformation.PingOptions();
        po.DontFragment = true;
        string data = "HELLLLOOOOO!";
        byte[] buffer = ASCIIEncoding.ASCII.GetBytes(data);
        int timeout = 500;
        System.Net.NetworkInformation.PingReply pr = ping.Send(PublicIP, timeout, buffer, po);
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        GD.Print("[LobbyStreamlined] Ping Return: " + pr.Status.ToString());
        if (pr.Status == System.Net.NetworkInformation.IPStatus.Success)
        {
            GD.Print("[LobbyStreamlined] The public IP responded with a roundtrip time of: " + pr.RoundtripTime);
            UsePublic = true;
            LobbyServerIP = PublicIP;
        }
        else
        {
            GD.Print("[LobbyStreamlined] The public IP failed to respond");
       
            if (!UsePublic)
            {
                GD.Print("[LobbyStreamlined] Trying Private Address: " + PrivateIP.ToString());
                pr = ping.Send(PrivateIP, timeout, buffer, po);
                await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
                GD.Print("[LobbyStreamlined] Ping Return: " + pr.Status.ToString());
                if (pr.Status.ToString() == "Success")
                {
                    GD.Print("[LobbyStreamlined] The Private IP responded with a roundtrip time of: " + pr.RoundtripTime);
                    UsePrivate = true;
                    LobbyServerIP = PrivateIP;
                }
                else
                {
                    LobbyServerIP = "127.0.0.1";
                    GD.Print("[LobbyStreamlined] The Private IP failed to respond");
                    UsePrivate = false;
                }
            }
        }
        if (JoinLobbyServer() != Error.Ok)
        {
            LobbyServerIP = "127.0.0.1";
            JoinLobbyServer();
        }
    }

    private Error JoinLobbyServer()
    {
        GD.Print($"[LobbyStreamlined] Attempting to connect to {LobbyServerIP}:{PortMinimum}");
        AgentPeer = new ENetMultiplayerPeer();

        Error error = AgentPeer.CreateClient(LobbyServerIP, PortMinimum);
        AgentAPI.MultiplayerPeer = AgentPeer;
        if (error != Error.Ok)
            return error;

        GD.Print("[LobbyStreamlined] Connected to MASTER");

        IsWanLobbyConnected = true;
        return Error.Ok;
    }

    public Error CreateMasterServer()
    {
        GD.Print("[LobbyStreamlined] Attempting to create lobby system at port: " + PortMinimum);
        AgentPeer = new ENetMultiplayerPeer();

        Error err = AgentPeer.CreateServer(PortMinimum, 1000);
        AgentAPI.MultiplayerPeer = AgentPeer;
        if (err != Error.Ok)
        {
            GD.Print("[LobbyStreamlined] " + err.ToString());
            return err;
        }
        GD.Print("[LobbyStreamlined] Master Server Created!");
        IsWanLobbyConnected = true;
        IsWanLobbyServer = true;
        return Error.Ok;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        AgentAPI.Poll();
        
        if (!IsWanLobbyServer)
        {
            UpdateVBoxChildren((VBoxContainer)GetNode(AgentSpawner.GetPath() + "/" + AgentSpawner.SpawnPath));
        }
       
        if (GenericCore.Instance.IsGenericCoreConnected || IsWanLobbyServer)
        {
            ((Control)GetChild(0)).Visible = false;
            foreach(Node n in GenericCore.Instance.GetChildren())
            {
                if (n is CanvasItem canvasItem)
                {
                    canvasItem.Visible = true;
                }
                else if (n is Node3D node3D)
                {
                    node3D.Visible = true;
                }
                else if (n is CanvasLayer canvasLayer)
                {
                    canvasLayer.Visible = true;
                }
            }
        }
        else
        {
            ((Control)GetChild(0)).Visible = true;
            foreach (Node n in GenericCore.Instance.GetChildren())
            {
                if (n is CanvasItem canvasItem)
                {
                    canvasItem.Visible = true;
                }
                else if (n is Node3D node3D)
                {
                    node3D.Visible = true;
                }
                else if (n is CanvasLayer canvasLayer)
                {
                    canvasLayer.Visible = true;
                }
            }
        }
    }

    private void UpdateVBoxChildren(VBoxContainer vbox)
    {
        foreach (Node c in vbox.GetChildren())
        {
            if ((c is Control))
            {
                Control child = (Control)c;

                Button btn = child.GetNode<Button>("Button");

                if (btn != null && btn.Visible)
                {
                    // Visible button
                }
                else
                {
                    // Hidden button
                }
            }
        }

        vbox.QueueSort();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ProcessSpawnServerSide(String n)
    {
        if (IsWanLobbyServer)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.UseShellExecute = true;     
                string[] args = OS.GetCmdlineArgs();
                proc.StartInfo.FileName = OS.GetExecutablePath();
                
                string sanitizedName = SanitizeGameName(n);
                proc.StartInfo.Arguments += $"--headless GAMESERVER {PortMinimum + portOffset} GAMENAME#{sanitizedName} > {sanitizedName}.log";
                
                GD.Print("[LobbyStreamlined] Starting Game Server With: " + proc.StartInfo.Arguments);
                portOffset++;
                if(PortMinimum + portOffset > PortMaximum)
                {
                    portOffset = 0;
                }
                Rpc("UpdatePortOffset", portOffset);
                proc.Start();
                if (MaxGameTime > 0)
                {
                    GameMonitor(proc);
                }
            }
            catch (System.Exception e)
            {
                GD.Print("[LobbyStreamlined] EXCEPTION - in creating a game!!! - " + e.ToString());
            }
        }
    }

    private string SanitizeGameName(string name)
    {
        string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        var result = new System.Text.StringBuilder();
        
        foreach (char c in name)
        {
            result.Append(valid.Contains(c) ? c : '-');
        }
        
        return result.ToString();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void UpdatePortOffset(int p)
    {
        if (!IsWanLobbyServer)
        {
            portOffset = p;
        }
    }

    public async void GameMonitor(System.Diagnostics.Process proc)
    {
        await ToSignal(GetTree().CreateTimer(MaxGameTime), SceneTreeTimer.SignalName.Timeout);
        if(!proc.HasExited)
        {
            proc.Kill();
        }
    }

    public void CreateNewGameServer()
    {
        if (GameNameBox.Text.Length < 2)
        {
            return;
        }
        
        RpcId(1, "ProcessSpawnServerSide", GameNameBox.Text);
        WaitForGameToStart(portOffset);
    }

    public async void WaitForGameToStart(int p)
    {
        GD.Print($"[LobbyStreamlined] Waiting for game to start on port {p + PortMinimum}");
        
        GenericCore.Instance.SetPort((p + PortMinimum).ToString());
        GenericCore.Instance.SetIP(LobbyServerIP);
        
        GD.Print($"[LobbyStreamlined] Set connection to {LobbyServerIP}:{p + PortMinimum}");
        
        int waitCount = 0;
        while (p == portOffset && waitCount < 50)
        {
            await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
            waitCount++;
        }
        
        if (waitCount >= 50)
        {
            GD.PrintErr($"[LobbyStreamlined] Timeout waiting for game server on port {p + PortMinimum}");
        }
        
        await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
        
        GD.Print($"[LobbyStreamlined] Attempting to join game at {LobbyServerIP}:{GenericCore.Instance.GetPort()}");
        GenericCore.Instance.JoinGame();
    }

    public void DisconnectFromLobbySystem()
    {
        if (AgentAPI.MultiplayerPeer != null)
        {
            GD.Print("[LobbyStreamlined] Disconnecting from ENet session<Lobby>");

            AgentAPI.MultiplayerPeer.Close();
            AgentAPI.MultiplayerPeer = null;
        }
    }
}
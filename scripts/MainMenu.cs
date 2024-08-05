using Godot;
using System;

public partial class MainMenu : Control
{
	[Export]
	private int port = 9999;
	
	[Export]
	private string address = "127.0.0.1";
	
	[Export]
	private LineEdit NameField;
	
	private ENetMultiplayerPeer peer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private void PeerConnected(long id)
	{
		GD.Print("Player Connected: " + id.ToString());
	}
	
	private void PeerDisconnected(long id)
	{
		GD.Print("Player Disconnected: " + id.ToString());
	}
	
	private void ConnectedToServer()
	{
		GD.Print("Connected To Server");
		RpcId(1, "sendPlayerInformation", NameField.Text, Multiplayer.GetUniqueId());
	}
	
	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}
	
	private void _on_start_pressed()
	{
		Rpc("startGame");
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame()
	{
		GetTree().ChangeSceneToFile("res://scenes/world.tscn");
		foreach (var item in GameManager.Players) 
		{
			GD.Print(item.Name + " is playing");
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void sendPlayerInformation(string name, int id)
	{
		PlayerInfo playerInfo = new PlayerInfo() {
			Name = NameField.Text,
			Id = id
		};
		
		if (!GameManager.Players.Contains(playerInfo))
		{
			GameManager.Players.Add(playerInfo);
		}
		
		if (Multiplayer.IsServer())
		{
			foreach (var item in GameManager.Players)
			{
				Rpc("sendPlayerInformation", item.Name, item.Id);
			}
		}
	}
	
	private void _on_host_pressed()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 4);
		if (error != Error.Ok)
		{
			GD.Print("Error cannot host: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for players...");
		sendPlayerInformation(NameField.Text, 1);
	}
	
	private void _on_join_pressed()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);
		
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining game...");
	}
}

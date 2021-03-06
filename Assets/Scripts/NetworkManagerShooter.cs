﻿using UnityEngine;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkManagerShooter : NetworkManager
{
    [Header("Room")]
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private NetworkRoomPlayerShooter roomPlayerPrefab = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public string SceneLobbyName;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerShooter gamePlayerPrefab = null;
    //[SerializeField] private GameObject playerSpawnSystem = null;
    //[SerializeField] private GameObject roundSystem = null;

    public List<NetworkRoomPlayerShooter> RoomPlayers { get; } = new List<NetworkRoomPlayerShooter>();
    public List<NetworkGamePlayerShooter> GamePlayers { get; } = new List<NetworkGamePlayerShooter>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().name != SceneLobbyName)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name == SceneLobbyName)
        {
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerShooter roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
		{
            var player = conn.identity.GetComponent<NetworkRoomPlayerShooter>();
            RoomPlayers.Remove(player);
            NotifyPlayersOfReadyState();
		}
        base.OnServerDisconnect(conn);
    }

	public override void OnStopServer()
	{
        RoomPlayers.Clear();
	}

    public void NotifyPlayersOfReadyState()
	{
        foreach (var player in RoomPlayers)
		{
            player.HandleReadyToStart(IsReadyToStart());
		}
	}

    private bool IsReadyToStart()
	{
        if (numPlayers < minPlayers) { return false; }

        foreach(var player in RoomPlayers)
		{
            if (!player.IsReady) { return false; }
		}

        return true;
	}

    public void StartGame()
	{
        if(SceneManager.GetActiveScene().name == SceneLobbyName)
		{
            if (!IsReadyToStart()) { return; }
            ServerChangeScene("Scene_Map_01");
		}
	}

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (SceneManager.GetActiveScene().name == SceneLobbyName && newSceneName.StartsWith("Scene_Map"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }
}
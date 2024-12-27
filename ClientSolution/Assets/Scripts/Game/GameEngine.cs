using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Game;
using NUnit.Framework;
using SharedLibrary;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SharedLibrary.Common;
using static SharedLibrary.Common.HubMsg;

public class GameEngine : NetworkBehaviour
{
    private bool isClosed = false;
    private int clientCount = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (SignalRConnectionManager.MyRoom == null || SignalRConnectionManager.MyPlayer == null)
        {
            SignalRConnectionManager.InitializeConnectionTest();
            //throw new System.Exception("SignalRConnectionManager is null");
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log($"OnNetworkSpawn: {NetworkManager.Singleton.IsListening}, {IsServer}, {IsClient}");
            if (IsServer)
            {
                SpawnPlayer(SignalRConnectionManager.MyPlayer.Role, NetworkManager.ServerClientId);
                
                NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
                {
                    clientCount++;
                    Debug.Log($"Client {clientId} connected. {clientCount}");
                    if (clientCount == 2)
                    {
                        SpawnNPC();
                    }
                };
            }
            else
            {
                RequestSpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
        else
            Debug.LogError("NetworkManager is not in this scene.");
    }

    private void SpawnNPC()
    {
        Debug.Log($"mup SpawnNPC");
        GameObject clone = Instantiate(Resources.Load<GameObject>(ClientCommon.File.CatPrefab));
        clone.transform.position = new Vector3(2.0f, 3.0f, 0);
        clone.GetComponent<NetworkObject>().Spawn(true);
    }

    private void SpawnPlayer(Role role, ulong clientId)
    {
        Debug.Log($"mup SpawnPlayer: {role}, {clientId}");
        GameObject clone;
        if (role == Common.Role.Human)
        {
            clone = Instantiate(Resources.Load<GameObject>(ClientCommon.File.HumanPrefab));
            clone.transform.position = new Vector3(-2.0f, 0, 0);

        }
        else if (role == Common.Role.Cat)
        {
            clone = Instantiate(Resources.Load<GameObject>(ClientCommon.File.CatPrefab));
            clone.transform.position = new Vector3(2.0f, 0, 0);
        }
        else {
            return;
        }
        clone.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnPlayerServerRpc(ulong clientId)
    {
        Debug.Log($"Spawning player for client {clientId}, {IsServer}");
        if (!IsServer) return;

        SpawnPlayer(Role.Cat, clientId);
    }

    private void HandleMatchClosed()
    {
        try
        {
            Debug.Log($"HandleMatchClosed");
            isClosed = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }

    private void OnGameClose()
    {
        try
        {
            Debug.Log($"OnGameClose");
            SceneManager.LoadScene("StartScene");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }

    }

    void Update()
    {
        if (isClosed)
        {
            isClosed = false;
            OnGameClose();
        }
    }

    private void OnEnable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchClosed += HandleMatchClosed;
        }
    }

    private void OnDisable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchClosed -= HandleMatchClosed;
        }
    }

}

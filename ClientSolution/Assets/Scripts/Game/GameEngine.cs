using System;
using SharedLibrary;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SharedLibrary.Common;

public class GameEngine : NetworkBehaviour
{
    [SerializeField] 
    private Canvas canvas;
    private GameTimer gameTimer;
    private MessMeter messMeter;

    private bool isClosed = false;
    private int clientCount = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SpawnNGOs();

        if (SignalRConnectionManager.MyRoom.Id == "-1")
        {
            SpawnPlayer(Role.Human, NetworkManager.ServerClientId);
            SpawnPlayer(Role.Cat, NetworkManager.ServerClientId, true);
            SpawnNPC();
            InitNGOs();
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
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
                        InitNGOs();
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

    private void SpawnNGOs()
    {
        SpawnGameTimer();
        SpawnMessMeter();
    }

    private void InitNGOs()
    {
        InitGameTimer();
        InitMessMeter();
    }

    private void SpawnNPC(bool spectate = false)
    {
        GameObject clone = Instantiate(Resources.Load<GameObject>(ClientCommon.File.CatPrefab));
        clone.transform.position = new Vector3(5.0f, 3.0f, 0);
        //clone.GetComponent<CharacterController>().toSpectate = spectate;
        clone.GetComponent<NetworkObject>().Spawn(true);
    }

    private void SpawnPlayer(Role role, ulong clientId, bool disable = false)
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
            clone.transform.position = new Vector3(5.0f, 0, 0);
        }
        else {
            return;
        }

        //clone.GetComponent<CharacterController>().toPlayerControl = !disable;
        //clone.GetComponent<CharacterController>().toSpectate = !disable;
        clone.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    private void SpawnGameTimer()
    {
        if (IsServer)
        {
            // Spawn the GameTimer on the network
            var instance = Instantiate(Resources.Load<GameObject>(ClientCommon.File.GameTimerPrefab));
            var networkObject = instance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn(); // Spawns the object across the network
            }

            gameTimer = instance.GetComponent<GameTimer>();
        }
    }

    private void InitGameTimer()
    {
        gameTimer.transform.SetParent(canvas.transform, false);
        gameTimer.OnTimerEnded += HandleGameTimerEnded;
        gameTimer.StartTimer();
    }

    private void SpawnMessMeter()
    {
        if (IsServer)
        {
            var instance = Instantiate(Resources.Load<GameObject>(ClientCommon.File.MessMeterPrefab));
            var networkObject = instance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn();
            }

            messMeter = instance.GetComponent<MessMeter>();
        }
    }

    private void InitMessMeter()
    {
        messMeter.transform.SetParent(canvas.transform, false);
        messMeter.OnMaxMessReached += HandleGameTimerEnded;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnPlayerServerRpc(ulong clientId)
    {
        Debug.Log($"Request Server to spawn my player: {clientId}, {IsServer}");
        if (!IsServer) return;

        SpawnPlayer(Role.Cat, clientId, true);
    }

    private void HandleSeverMatchClosed()
    {
        try
        {
            Debug.Log($"HandleSeverMatchClosed: Draw");
            isClosed = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }

    private void HandleGameTimerEnded()
    {
        try
        {
            Debug.Log($"HandleGameTimerEnded: Human wins");
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }

    private void HandleMaxMessReached()
    {
        try
        {
            Debug.Log($"HandleMaxMessReached: Cat wins");

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
            SignalRConnectionManager.Instance.OnMatchClosed += HandleSeverMatchClosed;
        }
    }

    private void OnDisable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchClosed -= HandleSeverMatchClosed;
        }
    }

    public override void OnDestroy()
    {
        if (gameTimer != null)
        {
            gameTimer.OnTimerEnded -= HandleGameTimerEnded;
        }

        base.OnDestroy();
    }
}

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

    private bool isClosed = false;
    private int clientCount = 0;

    public event Action OnGameStart;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SpawnGameTimer();

        if (SignalRConnectionManager.MyRoom.Id == "-1")
        {
            SpawnPlayer(Role.Human, NetworkManager.ServerClientId);
            SpawnPlayer(Role.Cat, NetworkManager.ServerClientId, true);
            SpawnNPC();
            gameTimer.transform.SetParent(canvas.transform, false);
            gameTimer.StartTimer();
            gameTimer.OnTimerEnded += HandleGameTimerEnded;
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
                        gameTimer.transform.SetParent(canvas.transform, false);
                        gameTimer.StartTimer();
                        gameTimer.OnTimerEnded += HandleGameTimerEnded;
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

    private void SpawnNPC(bool spectate = false)
    {
        Debug.Log($"mup SpawnNPC");
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
            var timerInstance = Instantiate(Resources.Load<GameObject>(ClientCommon.File.GameTimerPrefab));
            var networkObject = timerInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn(); // Spawns the object across the network
            }

            gameTimer = timerInstance.GetComponent<GameTimer>();
        }
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
            Debug.Log($"HandleSeverMatchClosed");
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

        if (gameTimer != null)
        {
            gameTimer.OnTimerEnded += HandleGameTimerEnded;
        }
    }

    private void OnDisable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchClosed -= HandleSeverMatchClosed;
        }

        if (gameTimer != null)
        {
            gameTimer.OnTimerEnded -= HandleGameTimerEnded;
        }
    }

}

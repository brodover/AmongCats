using System;
using System.Threading.Tasks;
using SharedLibrary;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SharedLibrary.Common;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Ddol");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private async void Start()
    {
        await SignalRConnectionManager.Instance.InitializeConnection();
    }

    private async void OnApplicationQuit()
    {
        await SignalRConnectionManager.Instance.Disconnect();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.name}, Mode: {mode}");

        if (scene.name == ClientCommon.Scene.Game)
        {
            if (NetworkManager.Singleton != null)
            {
                if (SignalRConnectionManager.MyRoom == null || SignalRConnectionManager.MyPlayer == null)
                {
                    SignalRConnectionManager.InitializeConnectionTest();
                    //throw new System.Exception("SignalRConnectionManager is null");
                } 
                else if (SignalRConnectionManager.MyPlayer.Role == Role.Human && !NetworkManager.Singleton.IsListening)
                {
                    try
                    {
                        StartHost();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"NetworkManager failed: {ex.Message}");
                    }
                }
                else if (SignalRConnectionManager.MyPlayer.Role == Role.Cat)
                {
                    StartClientWithRetry();
                }
                else
                {
                    Debug.LogError("Invalid player role. Can't start match.");
                    return;
                }
            }
            else
            {
                Debug.LogError("Missing NetworkManager. Can't start match.");
                return;
            }
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    private async void StartClientWithRetry()
    {
        int maxRetries = 5;
        int delayBetweenRetries = 1000; // in milliseconds

        for (int i = 0; i < maxRetries; i++)
        {
            Debug.Log($"Attempting to connect... (Attempt {i + 1}/{maxRetries})");

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started successfully.");
                return;
            }

            await Task.Delay(delayBetweenRetries);
        }

        Debug.LogError("Failed to connect to the server after multiple attempts.");
    }
}

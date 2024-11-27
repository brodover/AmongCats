using UnityEngine;

public class GameStartup : MonoBehaviour
{
    private async void Start()
    {
        await SignalRConnectionManager.Instance.InitializeConnection();
    }

    private async void OnApplicationQuit()
    {
        await SignalRConnectionManager.Instance.Disconnect();
    }
}

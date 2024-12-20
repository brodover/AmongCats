using UnityEngine;

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
}

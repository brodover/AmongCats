using NUnit.Framework;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject lightRays; //the light game object.
    [SerializeField]
    private GameObject human;

    private GameObject myPlayer;
    private GameObject otherPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SignalRConnectionManager.MyRoom == null || SignalRConnectionManager.MyPlayer == null)
            throw new System.Exception("SignalRConnectionManager is null");

        var humanPrefab = Resources.Load<GameObject>("Prefabs/Human");
        var catPrefab = Resources.Load<GameObject>("Prefabs/Cat");

        foreach (var player in SignalRConnectionManager.MyRoom.Players)
        {
            if (player == null) continue;
            GameObject clone;
            if (player.Role == SharedLibrary.Common.Role.Human)
            {
                clone = Instantiate(humanPrefab);
                clone.transform.position = new Vector3(-2.0f, 0, 0);
                Debug.Log($"Human: {player.Id} == {SignalRConnectionManager.MyPlayer.Id}");
            }
            else if (player.Role == SharedLibrary.Common.Role.Cat)
            {
                clone = Instantiate(catPrefab);
                clone.transform.position = new Vector3(2.0f, 0, 0);
                Debug.Log($"Cat: {player.Id} == {SignalRConnectionManager.MyPlayer.Id}");
            }
            else
            {
                continue;
            }
            clone.transform.SetParent(this.transform);

            if (player.Id == SignalRConnectionManager.MyPlayer.Id)
            {
                myPlayer = clone;
                myPlayer.AddComponent<Move>();
                //myPlayer.AddComponent<CameraFollow>();
                //myPlayer.GetComponent<CameraFollow>().target = myPlayer.transform;
                myPlayer.AddComponent<lightcaster>();
                myPlayer.GetComponent<lightcaster>().lightRays = lightRays;
                myPlayer.GetComponentInChildren<SpriteRenderer>().sortingOrder = 2;
            }
            else
                otherPlayer = clone;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        if (myPlayer != null)
        {
            Destroy(myPlayer);
        }
        if (otherPlayer != null)
        {
            Destroy(otherPlayer);
        }
    }
}

using System;
using System.Linq;
using NUnit.Framework;
using SharedLibrary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainCamera;
    [SerializeField]
    private GameObject lightRays;

    private GameObject myPlayer;
    private GameObject otherPlayer;

    private bool isClosed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SignalRConnectionManager.MyRoom == null || SignalRConnectionManager.MyPlayer == null)
        {
            SignalRConnectionManager.InitializeConnectionTest();
            //throw new System.Exception("SignalRConnectionManager is null");
        }


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
                myPlayer.AddComponent<lightcaster>();
                myPlayer.GetComponent<lightcaster>().lightRays = lightRays;
                myPlayer.GetComponentInChildren<SpriteRenderer>().sortingOrder = 2;

                mainCamera.GetComponent<CameraFollow>().target = myPlayer.transform;
            }
            else
                otherPlayer = clone;
        }
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

    private void HandlePlayerMoveUpdated()
    {
        var otherP = SignalRConnectionManager.MyRoom.Players.FirstOrDefault(p => p.Id != SignalRConnectionManager.MyPlayer.Id);
        Debug.Log($"HandlePlayerMoveUpdated: {otherP.Position.X}");
        otherPlayer.transform.SetPositionAndRotation(new Vector3(otherP.Position.X, otherP.Position.Y, otherP.Position.Z), Quaternion.identity);
        otherPlayer.GetComponentInChildren<SpriteRenderer>().flipX = otherP.IsFaceRight;
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

    private void OnEnable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchClosed += HandleMatchClosed;
            SignalRConnectionManager.Instance.OnPlayerMoveUpdated += HandlePlayerMoveUpdated;
        }
    }

    private void OnDisable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchClosed -= HandleMatchClosed;
            SignalRConnectionManager.Instance.OnPlayerMoveUpdated += HandlePlayerMoveUpdated;
        }
    }

}

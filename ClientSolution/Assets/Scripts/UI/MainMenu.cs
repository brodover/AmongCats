using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Common;

public class MainMenu : MonoBehaviour
{
    private bool isQueuing = false;
    private Role role = Role.Random;
    private string url = Common.ServerUrl + "/check-queue";

    public Transform SelectMenu;
    public Transform QueueMenu;
    [Space]
    public Button humanButton;
    public Button catButton;
    public Button randomButton;
    [Space]
    public TMP_Text queueText;
    public Button cancelButton;

    void Start()
    {
        humanButton.onClick.AddListener(() => OnSelect(Role.Human));
        catButton.onClick.AddListener(() => OnSelect(Role.Cat));
        randomButton.onClick.AddListener(() => OnSelect(Role.Random));

        cancelButton.onClick.AddListener(() => OnCancel());
    }

    void OnSelect(Role role)
    {
        this.role = role;
        queueText.text = $"Queuing as {role}";
        Debug.Log($"Player is queuing as: {role}");
        StartCoroutine(Wait1Sec());
        //MatchmakingManager.Instance.PlayerReady(role);
        isQueuing = true;
    }

    IEnumerator Wait1Sec()
    {
        yield return new WaitForSeconds(1);
    }

    void OnCancel()
    {
        Debug.Log($"Player canceled queue: {role}");
        isQueuing = false;
    }

    public void OnGameStart()
    {
        Debug.Log($"Game starting with role: {role}");
        PlayerPrefs.SetString("PlayerRole", role.ToString()); // Save role for game scene
        SceneManager.LoadScene("GameScene");
    }

    private void Update()
    {

        if (isQueuing)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log("Queue Response: " + response);

                if (response.Contains("RoomId"))
                {
                    // Room ready, start the game
                    Debug.Log("Room found! Transitioning to game...");
                    isQueuing = false;

                    // Call scene change or other logic here

                }
            }
            else
            {
                Debug.LogError("Error checking queue: " + request.error);
            }
        }
    }
}

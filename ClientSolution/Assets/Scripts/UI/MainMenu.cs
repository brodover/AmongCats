using System;
using System.Collections;
using SharedLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SharedLibrary.Common;

public class MainMenu : MonoBehaviour
{
    private Role role = Role.Random;
    private bool isMatched = false;

    public GameObject selectMenu;
    public GameObject queueMenu;
    [Space]
    public Button humanButton;
    public Button catButton;
    public Button randomButton;
    [Space]
    public TMP_Text queueText;
    public Button cancelButton;

    private void Start()
    {
        ShowSelectMenu(true);

        humanButton.onClick.AddListener(() => OnSelect(Role.Human));
        catButton.onClick.AddListener(() => OnSelect(Role.Cat));
        randomButton.onClick.AddListener(() => OnSelect(Role.Random));

        cancelButton.onClick.AddListener(() => OnCancel());
    }

    private async void OnSelect(Role role)
    {
        this.role = role;
        queueText.text = $"Queuing as {role}";
        cancelButton.gameObject.SetActive(false);
        ShowSelectMenu(false);
        Debug.Log($"Player is queuing as: {role}");

        StartCoroutine(Wait1Sec());

        cancelButton.gameObject.SetActive(true);
        await SignalRConnectionManager.Instance.PlayerJoinQueue(role);
    }

    IEnumerator Wait1Sec()
    {
        yield return new WaitForSeconds(1);
    }

    private void OnCancel()
    {
        Debug.Log($"Player canceled queue: {role}");

        ShowSelectMenu(true);
    }

    private void ShowSelectMenu(bool toShow)
    {
        selectMenu.SetActive(toShow);
        queueMenu.SetActive(!toShow);
    }

    private void OnGameStart()
    {
        try
        {
            Debug.Log($"Game starting");
            queueText.text = "Starting match...";
            SceneManager.LoadScene("GameScene");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
        
    }

    private void HandleMatchCreated()
    {
        try
        {
            Debug.Log($"HandleMatchCreated");
            isMatched = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }

    private void OnEnable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchCreated += HandleMatchCreated;
        }
    }

    private void OnDisable()
    {
        if (SignalRConnectionManager.Instance != null)
        {
            SignalRConnectionManager.Instance.OnMatchCreated -= HandleMatchCreated;
        }
    }

    private void Update()
    {
        if (isMatched)
        {
            isMatched = false;
            OnGameStart();
        }
    }
}

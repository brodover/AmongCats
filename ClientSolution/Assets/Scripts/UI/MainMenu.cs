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
        ShowSelectMenu(false);

        this.role = role;
        queueText.text = $"Queuing as {role}";
        Debug.Log($"Player is queuing as: {role}");
        StartCoroutine(Wait1Sec());
        await SignalRConnectionManager.Instance.PlayerJoinQueue(role);
        isQueuing = true;
    }

    IEnumerator Wait1Sec()
    {
        yield return new WaitForSeconds(1);
    }

    private void OnCancel()
    {
        Debug.Log($"Player canceled queue: {role}");
        isQueuing = false;

        ShowSelectMenu(true);
    }

    private void OnGameStart()
    {
        Debug.Log($"Game starting with role: {role}");
        PlayerPrefs.SetString("PlayerRole", role.ToString()); // Save role for game scene
        SceneManager.LoadScene("GameScene");
    }

    private void ShowSelectMenu(bool toShow)
    {
        selectMenu.SetActive(toShow);
        queueMenu.SetActive(!toShow);
    }
}

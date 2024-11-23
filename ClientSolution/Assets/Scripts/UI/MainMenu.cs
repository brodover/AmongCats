using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button humanButton;
    public Button catButton;
    public Button randomButton;

    void Start()
    {
        humanButton.onClick.AddListener(() => OnSelect("Human"));
        catButton.onClick.AddListener(() => OnSelect("Cat"));
        randomButton.onClick.AddListener(() => OnSelect("Random"));
    }

    void OnSelect(string role)
    {
        Debug.Log($"Player is ready with role: {role}");
        //MatchmakingManager.Instance.PlayerReady(role);
    }

    public void OnGameStart(string role)
    {
        Debug.Log($"Game starting with role: {role}");
        PlayerPrefs.SetString("PlayerRole", role); // Save role for game scene
        SceneManager.LoadScene("GameScene");
    }
}

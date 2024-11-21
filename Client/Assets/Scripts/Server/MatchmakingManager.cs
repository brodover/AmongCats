using UnityEngine;
using UnityEngine.UI;

public class MatchmakingManager : MonoBehaviour
{
    public Button humanButton;
    public Button catButton;
    public Button randomButton;

    void Start()
    {
        humanButton.onClick.AddListener(() => OnSelectRole("Human"));
        catButton.onClick.AddListener(() => OnSelectRole("Cat"));
        randomButton.onClick.AddListener(() => OnSelectRole("Random"));
    }
    void OnSelectRole(string role)
    {
        Debug.Log($"Player is ready with role: {role}");
        //PlayerReady(role);
    }
}

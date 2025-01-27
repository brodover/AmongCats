using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : NetworkBehaviour
{
    private const float GAME_DURATION = 180f; // Total game time in seconds (3 minutes)
    private float timeRemaining = 0f;
    private NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(true);

    public TMP_Text timerText;
    //public GameObject endGameUI;

    public delegate void TimerEndedHandler();

    public event TimerEndedHandler OnTimerEnded;    // Inform GameEngine

    void Awake()
    {
        timeRemaining = GAME_DURATION;
        UpdateUI();
    }

    public void StartTimer()
    {
        if (IsServer)
        {
            isGameOver.Value = false;
            Debug.Log("Start timer.");
        }
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void EndGame()
    {
        isGameOver.Value = true;
        Debug.Log("Time's up! Game Over.");
        OnTimerEnded?.Invoke();
        //endGameUI.SetActive(true); // Show end game UI
        // Add logic for determining winner and stopping gameplay
    }

    void Update()
    {
        if (isGameOver.Value)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }
            UpdateUI();
        }
    }
}

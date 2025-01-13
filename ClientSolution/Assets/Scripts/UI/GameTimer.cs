using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    public float gameDuration = 180f; // Total game time in seconds (3 minutes)
    private float timeRemaining;

    public TMP_Text timerText; // UI Text to display the timer
    //public GameObject endGameUI; // UI element to show when the game ends

    private bool isGameOver = true;

    public delegate void TimerEndedHandler();

    public event TimerEndedHandler OnTimerEnded;    // Inform GameEngine

    void Start()
    {
        timeRemaining = gameDuration;
        UpdateTimerUI();
        //endGameUI.SetActive(false); // Hide end game UI at the start
    }

    public void StartTimer()
    {
        isGameOver = false;
        Debug.Log("Start timer.");
        
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void EndGame()
    {
        isGameOver = true;
        Debug.Log("Time's up! Game Over.");
        OnTimerEnded?.Invoke();
        //endGameUI.SetActive(true); // Show end game UI
        // Add logic for determining winner and stopping gameplay
    }

    void Update()
    {
        if (isGameOver)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }
            UpdateTimerUI();
        }
    }
}

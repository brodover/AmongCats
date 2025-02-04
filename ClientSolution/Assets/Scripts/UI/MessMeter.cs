using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static GameTimer;

public class MessMeter : NetworkBehaviour
{
    private const int MAX_MESS = 100;
    private const int MIN_MESS = 0;
    private const int MESS_CHANGE_UNIT = 10;

    private NetworkVariable<int> currentMess = new NetworkVariable<int>(0);

    public TMP_Text meterText;

    public event Action OnMaxMessReached;    // Inform GameEngine
    //public event Action<int> OnMessChanged;

    void UpdateUI()
    {
        meterText.text = $"{currentMess.Value}%";
    }

    public void ChangeMess(bool isIncrease)
    {
        if (isIncrease)
        {
            HandleMessChanged(MESS_CHANGE_UNIT);
        }
        else
        {
            HandleMessChanged(-MESS_CHANGE_UNIT);
        }
    }

    private void HandleMessChanged(int change)
    {
        currentMess.Value += change;
        if (currentMess.Value == MAX_MESS)
        {
            OnMaxMessReached?.Invoke();
        } 
        else if (currentMess.Value > MAX_MESS)
        {
            Debug.Log($"Mess overflow: {currentMess.Value} ({change})");
        }
        else if (currentMess.Value < MIN_MESS)
        {
            Debug.Log($"Mess underflow: {currentMess.Value} ({change})");
        }

        UpdateUI();
    }
}

using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static GameTimer;

public class MessMeter : NetworkBehaviour
{
    private const int MAX_MESS = 100;
    private const int MIN_MESS = 0;
    private const int INIT_MESS = 50;

    private NetworkVariable<int> currentMess = new NetworkVariable<int>(INIT_MESS);

    public TMP_Text meterText;

    public event Action OnMaxMessReached;    // Inform GameEngine
    //public event Action<int> OnMessChanged;

    void UpdateUI()
    {
        meterText.text = $"{currentMess.Value}%";
    }

    private void HandleMessChanged(int change)
    {
        currentMess.Value += change;
        if (currentMess.Value >= MAX_MESS)
        {
            OnMaxMessReached?.Invoke();
        }

        currentMess.Value = Math.Max(MIN_MESS, currentMess.Value);
    }
}

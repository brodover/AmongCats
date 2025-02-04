using System;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    private SpriteRenderer childSR;

    [SerializeField]
    private bool isMess;

    private Color _cleanColor = new Color(0.84f, 0.86f, 0.86f);
    private Color _messColor = new Color(0.83f, 0.48f, 0.41f);

    public Action<bool> OnStateChanged;    // Inform GameEngine

    private void Awake()
    {
        childSR = transform.GetChild(0).GetComponent<SpriteRenderer>();

        isMess = false;
    }

    public void CleanUp()
    {
        if (!isMess) { return; }

        isMess = false;
        UpdateVisual();
    }

    public void MessUp()
    {
        if (isMess) { return; }

        isMess = true;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (isMess)
        {
            childSR.color = _messColor;
        }
        else
        {
            childSR.color = _cleanColor;
        }

        OnStateChanged?.Invoke(isMess);
    }
}

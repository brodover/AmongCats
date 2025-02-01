using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    private Transform childT;

    private bool isMess;

    private void Awake()
    {
        childT = transform.GetChild(0).transform;

        isMess = false;
    }

    public void CleanUp()
    {
        Debug.Log($"mup CleanUp: {isMess}");
        if (!isMess) { return; }


    }

    public void MessUp()
    {
        Debug.Log($"mup MessUp: {isMess}");
        if (isMess) { return; }


    }
}

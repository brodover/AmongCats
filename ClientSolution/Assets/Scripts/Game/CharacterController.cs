using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterController : NetworkBehaviour
{
    public bool toPlayerControl = true;
    public bool toSpectate = true;

    public void InitPlayer(Button interactBtn, bool toPlayerControl=true, bool toSpectate=true)
    {
        if (toPlayerControl)
        {
            gameObject.AddComponent<MoveInput>();
            var interact = gameObject.AddComponent<InteractInput>();
            interactBtn.onClick.AddListener(interact.OnInteractClick);
        }
        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 3; // 1 higher than other characters

        if (toSpectate)
        {
            var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            gameObject.AddComponent<lightcaster>();
            mainCamera.GetComponent<CameraFollow>().target = gameObject.transform;
        }
    }

    public void InitNPC()
    {
        gameObject.AddComponent<NavMeshAgent>();
        gameObject.AddComponent<MoveNpc>();
    }
}

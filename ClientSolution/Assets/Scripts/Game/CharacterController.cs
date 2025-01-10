using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class CharacterController : NetworkBehaviour
{
    public bool toPlayerControl = true;
    public bool toSpectate = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // My Player
            if (GetComponent<NetworkObject>().IsPlayerObject)
            {
                if (toPlayerControl)
                {
                    gameObject.AddComponent<MoveInput>();
                }
                transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 3; // 1 higher than other characters

                if (toSpectate)
                {
                    var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                    gameObject.AddComponent<lightcaster>();
                    mainCamera.GetComponent<CameraFollow>().target = gameObject.transform;
                }
            }
            // NPC
            else
            {
                gameObject.AddComponent<NavMeshAgent>();
                gameObject.AddComponent<MoveNpc>();
            }
        }
    }
}

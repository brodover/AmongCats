using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class CharacterController : NetworkBehaviour
{
    public bool toDisable = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // My Player
            if (GetComponent<NetworkObject>().IsPlayerObject)
            {
                gameObject.AddComponent<MoveInput>();
                gameObject.AddComponent<lightcaster>();
                transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 3; // 1 higher than other characters

                if (!toDisable)
                {
                    var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
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

        if (toDisable)
        {
            gameObject.GetComponent<MoveInput>().enabled = false;
            gameObject.GetComponent<lightcaster>().enabled = false;
        }
    }
}

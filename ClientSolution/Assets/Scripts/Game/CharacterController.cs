using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class CharacterController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // Player
            if (GetComponent<NetworkObject>().IsPlayerObject)
            {
                gameObject.AddComponent<MoveInput>();
                gameObject.AddComponent<lightcaster>();

                var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                mainCamera.GetComponent<CameraFollow>().target = gameObject.transform;
            }
            // NPC
            else
            {
                gameObject.AddComponent<NavMeshAgent>();
                gameObject.AddComponent<MoveNpc>();
            }
        }
        
        enabled = false;
    }
}

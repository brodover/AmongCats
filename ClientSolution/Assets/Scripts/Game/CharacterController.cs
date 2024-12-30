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
            // Player
            if (GetComponent<NetworkObject>().IsPlayerObject)
            {
                gameObject.AddComponent<MoveInput>();
                gameObject.AddComponent<lightcaster>();

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
                var agent = gameObject.GetComponent<NavMeshAgent>();
                agent.speed = ClientCommon.Game.CatMovementSpeed; // 3.5
                agent.acceleration = agent.speed / ClientCommon.Game.TimeToMaxSpeed; //8
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

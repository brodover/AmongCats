using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class CharacterController : NetworkBehaviour
{
    private SpriteRenderer sr;
    private Vector3 prevPos;

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
                var agent = gameObject.GetComponent<NavMeshAgent>();
                agent.speed = 10; // 3.5
                agent.acceleration = 18; //8
                gameObject.AddComponent<MoveNpc>();
            }
        }

        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!sr.flipX && transform.position.x > prevPos.x)
            sr.flipX = true;
        else if (sr.flipX && transform.position.x < prevPos.x)
            sr.flipX = false;

        prevPos = transform.position;
    }
}

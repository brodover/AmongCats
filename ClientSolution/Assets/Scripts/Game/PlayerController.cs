using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            var lightRays = GameObject.FindGameObjectWithTag("LightMask");

            gameObject.AddComponent<MoveInput>();
            gameObject.AddComponent<lightcaster>();
            gameObject.GetComponent<lightcaster>().lightRays = lightRays;
            gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = 2;

            mainCamera.GetComponent<CameraFollow>().target = gameObject.transform;
        }
    }
}

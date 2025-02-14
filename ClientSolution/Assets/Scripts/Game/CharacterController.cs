using System;
using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterController : NetworkBehaviour
{
    private MoveInput moveI = null;

    public bool toPlayerControl = true;
    public bool toSpectate = true;

    private const float _RANGE = 2f;

    public void InitPlayer(Button interactBtn, Button speedBtn, bool toPlayerControl=true, bool toSpectate=true)
    {
        if (toPlayerControl)
        {
            moveI = gameObject.AddComponent<MoveInput>();
            interactBtn.onClick.AddListener(OnInteractClick);
            speedBtn.onClick.AddListener(OnSpeedClick);
            moveI.speedBtn = speedBtn;
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


    public void OnInteractClick()
    {
        var targetList = FindObjectsByType<Interactable>(FindObjectsSortMode.None);

        foreach (var target in targetList)
        {
            var distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > _RANGE) { continue; }

            if (gameObject.tag == "Human")
            {
                target.CleanUp();
            }
            else if (gameObject.tag == "Cat")
            {
                target.MessUp();
            }

            break;
        }
    }

    public void OnSpeedClick()
    {
        moveI.ChangeSpeed();
    }
}

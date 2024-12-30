using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Game;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MoveNpc : NetworkBehaviour
{
    public Transform target; // The target object (e.g., player)
    private Vector3 _currentTarget;
    private Vector3 _constrainedDirection;

    private float searchRadius = 10f; // How far to search for a spot
    private float waitTime = MAX_WAIT_TIME;
    private int toShuffleCountdown = 0;

    private const float MAX_WAIT_TIME = 3f;

    private Vector3[] randomCentrePoints = new [] { new Vector3(27f, 10f, 0f), new Vector3(-27f, 10f, 0f), new Vector3(27f, -10f, 0f), new Vector3(-27f, -10f, 0f), new Vector3(-18.9f, 0.7f, 0f), new Vector3(13.4f, 1f, 0f) }; 

    private NavMeshAgent agent;
    private Rigidbody rb;

    private GameObject testMoveToMarker;

    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Human").transform;

        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        //agent.updatePosition = false; // handle movement manually for 8-dir
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // TEST
        testMoveToMarker = GameObject.Find("Marker");
    }

    void MoveToOutOfSightSpot()
    {
        int attempts = 10; // Limit attempts to prevent infinite loop

        if (toShuffleCountdown == 0)
        {
            randomCentrePoints = randomCentrePoints.OrderBy(n => Guid.NewGuid()).ToArray();
            toShuffleCountdown = randomCentrePoints.Length / 2;
        }
        toShuffleCountdown--;

        Debug.Log($"mup toShuffleCountdown: {toShuffleCountdown} : {randomCentrePoints[toShuffleCountdown]}");
        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(randomCentrePoints[toShuffleCountdown], searchRadius);
            if (randomPoint != Vector3.zero && !IsVisibleToTarget(randomPoint))
            {
                randomPoint.z = agent.transform.position.z;
                agent.SetDestination(randomPoint);
                testMoveToMarker.transform.position = randomPoint; // TEST
                Debug.Log($"mup Success at #{i}!!! Moving to {randomPoint}");
                return;
            }
        }

        Debug.LogWarning("Failed to find an out-of-sight point after multiple attempts.");
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomDirection = (Vector3)UnityEngine.Random.insideUnitCircle * radius;
        randomDirection += center;

        NavMeshHit hit;

        int notWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
        int areaMask = ~(1 << notWalkableArea);

        if (NavMesh.SamplePosition(randomDirection, out hit, radius, areaMask))
        {
            return hit.position;
        }

        return Vector3.zero; // Invalid point
    }

    bool IsVisibleToTarget(Vector3 point)
    {
        Vector3 directionToPoint = point - target.position;
        Ray ray = new Ray(target.position, directionToPoint.normalized);
        RaycastHit hit;

        // Cast a ray from the target to the point
        if (Physics.Raycast(ray, out hit, directionToPoint.magnitude))
        {
            // If the ray hits something, the point is NOT visible
            Debug.Log($"mup Hit: {hit.collider.gameObject.name}");
            return !hit.collider.gameObject.CompareTag("Wall");
        }

        // If the ray doesn't hit anything, the point is VISIBLE
        return true;
    }

    Vector2 ConstrainDirection(Vector2 inputDirection)
    {
        Vector2 constrainedDirection;

        // Prioritize Horizontal or Vertical Movement
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(4f * inputDirection.y))
        {
            constrainedDirection = new Vector2(Mathf.Sign(inputDirection.x), 0); // Left or Right
        }
        else if (Mathf.Abs(inputDirection.y) > Mathf.Abs(4f * inputDirection.x))
        {
            constrainedDirection = new Vector2(0, Mathf.Sign(inputDirection.y)); // Up or Down
        }
        else
        {
            // Diagonal movement
            constrainedDirection = new Vector2(Mathf.Sign(inputDirection.x), Mathf.Sign(inputDirection.y));
        }

        // Renormalize to match original direction's magnitude
        constrainedDirection = constrainedDirection.normalized * inputDirection.magnitude;

        return constrainedDirection;
    }

    void OnAnimatorMove()
    {
        // Sync NavMeshAgent's position with Rigidbody
        agent.nextPosition = transform.position;
    }

    /*private void Update()
    {
        // wait/move
        if (agent.velocity.x == 0)
        {
            if (waitTime < 0f)
            {
                MoveToOutOfSightSpot();

                waitTime += UnityEngine.Random.Range(0f, MAX_WAIT_TIME);
                Debug.Log($"mup waitTime {waitTime}");
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        if (agent.pathPending || agent.remainingDistance <= 0.1f)
            return;

        // Step 1: Get Next Target Position
        if (_currentTarget != agent.steeringTarget)
        {
            _currentTarget = agent.steeringTarget;
            // Step 2: Calculate Movement Direction
            Vector3 direction = (_currentTarget - transform.position).normalized;

            _constrainedDirection = ConstrainDirection(new Vector3(direction.x, direction.y, 0f));
            Debug.Log($"{direction} ... {_constrainedDirection}");

            // facing direction
            *//*if (agent.velocity.x != 0)
            {
                float facingDir = Mathf.Sign(agent.velocity.x); // 1 for positive, -1 for negative
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -facingDir, transform.localScale.y, transform.localScale.z);
            }*//*
            float facingDir = Mathf.Sign(_constrainedDirection.x); // 1 for positive, -1 for negative
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -facingDir, transform.localScale.y, transform.localScale.z);
        }

        if (_constrainedDirection != Vector3.zero)
        {
            // Step 3: Apply Movement
            Vector3 movement = _constrainedDirection * 10 * Time.deltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }*/

    private void Update()
    {
        if (agent.velocity.x == 0)
        {
            if (waitTime < 0f)
            {
                MoveToOutOfSightSpot();

                waitTime += UnityEngine.Random.Range(0f, MAX_WAIT_TIME);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        if (agent.velocity.x != 0)
        {
            float direction = Mathf.Sign(agent.velocity.x); // 1 for positive, -1 for negative
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -direction, transform.localScale.y, transform.localScale.z);
        }
    }

}

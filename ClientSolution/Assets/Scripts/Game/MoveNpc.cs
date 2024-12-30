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
    public float searchRadius = 10f; // How far to search for a spot

    private float waitTime = MAX_WAIT_TIME;
    private int toShuffleCountdown = 0;

    private const float MAX_WAIT_TIME = 3f;

    private Vector3[] randomCentrePoints = new [] { new Vector3(27f, 10f, 0f), new Vector3(-27f, 10f, 0f), new Vector3(27f, -10f, 0f), new Vector3(-27f, -10f, 0f), new Vector3(-18.9f, 0.7f, 0f), new Vector3(13.4f, 1f, 0f) }; 

    private NavMeshAgent agent;

    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Human").transform;
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
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

        Debug.Log($"toShuffleCountdown: {toShuffleCountdown} : {randomCentrePoints[toShuffleCountdown]}");
        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(randomCentrePoints[toShuffleCountdown], searchRadius);
            if (randomPoint != Vector3.zero && !IsVisibleToTarget(randomPoint))
            {
                randomPoint.z = agent.transform.position.z;
                agent.SetDestination(randomPoint);
                Debug.Log($"Success at #{i}!!! Moving to {randomPoint}");
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

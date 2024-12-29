using Assets.Scripts.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MoveNpc : NetworkBehaviour
{
    public Transform target; // The target object (e.g., player)
    public float searchRadius = 10f; // How far to search for a spot
    private float waitTime = MAX_WAIT_TIME;

    private const float MAX_WAIT_TIME = 5f;

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

        for (int i = 0; i < attempts; i++)
        {
            Debug.Log($"MoveToOutOfSightSpot: {i}");
            Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, searchRadius);

            if (randomPoint != Vector3.zero && !IsVisibleToTarget(randomPoint))
            {
                randomPoint.z = agent.transform.position.z;
                agent.SetDestination(randomPoint);
                Debug.Log("Moving to out-of-sight point: " + randomPoint);
                return;
            }
        }

        Debug.LogWarning("Failed to find an out-of-sight point after multiple attempts.");
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomDirection = (Vector3)Random.insideUnitCircle * radius;
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
            Debug.Log($"Ray hit: {hit.collider.gameObject.name}");
            return false;
        }

        // If the ray doesn't hit anything, the point is VISIBLE
        return true;
    }
    private void Update()
    {
        if (waitTime < 0f)
        {
            MoveToOutOfSightSpot();

            waitTime += MAX_WAIT_TIME;
        }
        else
        {
            waitTime -= Time.deltaTime;
        }

        if (agent.velocity.x != 0)
        {
            float direction = Mathf.Sign(agent.velocity.x); // 1 for positive, -1 for negative
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -direction, transform.localScale.y, transform.localScale.z);
        }
    }

}

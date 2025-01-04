using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Game;
using NUnit.Framework;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MoveNpc : NetworkBehaviour
{
    public Transform target; // The target object (e.g., player)
    private Vector3 _currentTarget = Vector3.zero;
    private Vector3 _constrainedDirection;

    private float _searchRadius = 10f; // How far to search for a spot
    private float _waitTime = 1f;
    private int _toShuffleCountdown = 0;

    private const float MAX_WAIT_TIME = 5f;

    private int _currentCornerIndex = 0;
    private NavMeshPath _path;
    private Vector3[] _corners = new Vector3[] { };
    private Vector3[] _randomCentrePoints = new [] { new Vector3(27f, 10f, 0f), new Vector3(-27f, 10f, 0f), new Vector3(27f, -10f, 0f), new Vector3(-27f, -10f, 0f), new Vector3(-18.9f, 0.7f, 0f), new Vector3(13.4f, 1f, 0f) }; 

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform spriteTransform;

    private GameObject _testMoveToMarker;
    private GameObject _testSteerToMarker;

    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Human").transform;

        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        spriteTransform = transform.GetChild(0).transform;

        agent.updatePosition = false; // handle movement manually for 8-dir
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.autoBraking = false;
        agent.stoppingDistance = 0.1f;
        agent.angularSpeed = 10f;

        int notWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
        agent.areaMask = ~(1 << notWalkableArea);

        agent.speed = ClientCommon.Game.CatMovementSpeed;
        agent.acceleration = agent.speed / ClientCommon.Game.TimeToMaxSpeed;

        // TEST
        _testMoveToMarker = GameObject.Find("Marker");
        _testSteerToMarker = GameObject.Find("SteeringMarker");
    }

    private void MoveToOutOfSightSpot()
    {
        int attempts = 10; // Limit attempts to prevent infinite loop

        if (_toShuffleCountdown == 0)
        {
            _randomCentrePoints = _randomCentrePoints.OrderBy(n => Guid.NewGuid()).ToArray();
            _toShuffleCountdown = _randomCentrePoints.Length / 2;
        }
        _toShuffleCountdown--;

        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(_randomCentrePoints[_toShuffleCountdown], _searchRadius);
            if (randomPoint != Vector3.zero && !IsVisibleToTarget(randomPoint))
            {
                randomPoint.z = transform.position.z;
                _path = new NavMeshPath();
                if (agent.SetDestination(randomPoint))
                {
                    agent.CalculatePath(randomPoint, _path);
                    _currentCornerIndex = 0;
                    agent.isStopped = false;
                    Debug.Log($"_path.corners { _path.corners.Length}, { _path.status}: {string.Join(",", _path.corners)}");

                    if (_testMoveToMarker)
                        _testMoveToMarker.transform.position = randomPoint; // TEST
                    Debug.Log($"mup Success at #{i}!!! Moving to {randomPoint}");

                }

                //StartCoroutine(WaitAWhile(randomPoint, i));

                return;
            }
        }

        Debug.LogWarning("Failed to find an out-of-sight point after multiple attempts.");
    }

    IEnumerator WaitAWhile(Vector3 randomPoint, int i)
    {
        agent.SetDestination(randomPoint); // calculate path

        yield return new WaitForSeconds(0.1f);
        while (agent.pathPending)
        {
            yield return null;
        }

        Debug.Log($"agent.path.corners {agent.path.corners.Length}, {agent.path.status}: {string.Join(",", agent.path.corners)}");
        _corners = new Vector3[agent.path.corners.Length + 1];
        agent.path.corners.CopyTo(_corners, 0);
        _corners[agent.path.corners.Length] = randomPoint;
        Debug.Log($"_corners: {string.Join(",", _corners)}");

        agent.SetDestination(transform.position); // reset and use move instead
        _currentCornerIndex = 0;
        agent.isStopped = false;

        if (_testMoveToMarker)
            _testMoveToMarker.transform.position = randomPoint; // TEST
        Debug.Log($"mup Success at #{i}!!! Moving to {randomPoint}");
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomDirection = (Vector3)UnityEngine.Random.insideUnitCircle * radius;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, agent.areaMask))
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

    /*void OnAnimatorMove()
    {
        // Sync NavMeshAgent's position with Rigidbody
        agent.nextPosition = transform.position;
    }*/

    public float radius = 2.0f;
    public int samplePoints = 8; // More points for higher accuracy

    public bool IsCircularAreaWalkable(Vector3 center, float radius, int pointCount)
    {
        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * Mathf.PI * 2 / pointCount; // Evenly distribute points
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 samplePoint = center + offset;

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(samplePoint, out hit, 0.1f, NavMesh.AllAreas))
            {
                // Point is not valid on NavMesh
                return false;
            }

            // Optional: Ensure no obstacles block the path from center to samplePoint
            if (!NavMesh.Raycast(center, hit.position, out _, NavMesh.AllAreas))
            {
                // Raycast succeeded, path is clear
                continue;
            }
            else
            {
                // Obstacle in the way
                return false;
            }
        }

        return true; // All points valid and no obstructions detected
    }

    private void Update()
    {
        // wait/move
        if (agent.velocity.x == 0)
        {
            if (_waitTime < 0f)
            {
                MoveToOutOfSightSpot();

                _waitTime += UnityEngine.Random.Range(3f, MAX_WAIT_TIME);
            }
            else
            {
                _waitTime -= Time.deltaTime;
            }
        }

        //if (agent.pathPending || agent.remainingDistance <= agent.stoppingDistance)
        //    return;

        // Step 1: Get Next Target Position
        if (!agent.isStopped && _path != null)
        {
            if (_currentCornerIndex == 0 || Mathf.Abs(_currentTarget.x - transform.position.x) <= agent.stoppingDistance && Mathf.Abs(_currentTarget.y - transform.position.y) <= agent.stoppingDistance)
            {
                _currentCornerIndex++;
                if (_currentCornerIndex < _path.corners.Length)
                {
                    _currentTarget = _path.corners[_currentCornerIndex];

                    if (_testSteerToMarker)
                        _testSteerToMarker.transform.position = _currentTarget; // TEST

                    // Step 2: Calculate Movement Direction
                    Vector3 direction = (_currentTarget - transform.position).normalized;

                    _constrainedDirection = ConstrainDirection(new Vector3(direction.x, direction.y, 0f));
                    Debug.Log($"Initial dir #{_currentCornerIndex}: {direction} ... {_constrainedDirection}");

                    // facing direction
                    float facingDir = Mathf.Sign(_constrainedDirection.x); // 1 for positive, -1 for negative
                    spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x) * -facingDir, spriteTransform.localScale.y, transform.localScale.z);
                }
                else
                {
                    // stop
                    Debug.Log("Stopped");
                    _constrainedDirection = Vector3.zero;
                    agent.Move(_constrainedDirection);
                    agent.isStopped = true;
                    _corners = new Vector3[] { };
                    _path = null;
                    _currentTarget = Vector3.zero;
                }
            }
            else if (
                // was diagonal but in line on one axis now
                ((Math.Abs(_constrainedDirection.x) > agent.stoppingDistance && Math.Abs(_constrainedDirection.y) > agent.stoppingDistance) 
                && (Mathf.Abs(_currentTarget.x - transform.position.x) <= agent.stoppingDistance || Mathf.Abs(_currentTarget.y - transform.position.y) <= agent.stoppingDistance))
                ||
                // was going horizontal but in line on x axis now
                (Math.Abs(_constrainedDirection.x) > agent.stoppingDistance
                && Mathf.Abs(_currentTarget.x - transform.position.x) <= agent.stoppingDistance)
                ||
                // was going vertical but in line on y axis now
                (Math.Abs(_constrainedDirection.y) > agent.stoppingDistance
                && Mathf.Abs(_currentTarget.y - transform.position.y) <= agent.stoppingDistance)
                )
            {
                Vector3 direction = (_currentTarget - transform.position).normalized;

                _constrainedDirection = ConstrainDirection(new Vector3(direction.x, direction.y, 0f));
                Debug.Log($"Fixed dir #{_currentCornerIndex}: {direction} ... {_constrainedDirection}");

                // facing direction
                float facingDir = Mathf.Sign(_constrainedDirection.x); // 1 for positive, -1 for negative
                spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x) * -facingDir, spriteTransform.localScale.y, transform.localScale.z);
            }

            if (_constrainedDirection != Vector3.zero)
            {
                // Step 3: Apply Movement
                Vector3 movement = _constrainedDirection * agent.speed * Time.deltaTime;
                rb.MovePosition(rb.position + movement);
                //Debug.Log($"Move: {_constrainedDirection} /// {movement}");
                //agent.Move(movement);
                agent.nextPosition = transform.position;
            }
        }

        
    }

    /*private void Update()
    {
        if (_testSteerToMarker)
            _testSteerToMarker.transform.position = agent.steeringTarget; // TEST

        if (agent.velocity.x == 0)
        {
            if (_waitTime < 0f)
            {
                MoveToOutOfSightSpot();

                _waitTime += UnityEngine.Random.Range(0f, MAX_WAIT_TIME);
            }
            else
            {
                _waitTime -= Time.deltaTime;
            }
        }

        if (agent.velocity.x != 0)
        {
            float direction = Mathf.Sign(agent.velocity.x); // 1 for positive, -1 for negative
            spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x) * -direction, spriteTransform.localScale.y, spriteTransform.localScale.z);
        }
    }*/

}

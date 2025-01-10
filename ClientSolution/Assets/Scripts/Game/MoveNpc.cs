using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Game;
using NUnit.Framework;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MoveNpc : NetworkBehaviour
{
    public Transform target; // to avoid LOS

    private Vector3 _currentTarget = Vector3.zero;
    private Vector3 _constrainedDirection;

    private int _currentCornerIndex = 0;
    private NavMeshPath _path;

    private float _wait_time = 1f;
    private int _toShuffleCountdown = 0;
    private Vector3[] _randomCentrePoints = new[] { new Vector3(27f, 10f, 0f), new Vector3(-27f, 10f, 0f), new Vector3(27f, -10f, 0f), new Vector3(-27f, -10f, 0f), new Vector3(-18.9f, 0.7f, 0f), new Vector3(13.4f, 1f, 0f) };

    private const float SEARCH_RADIUS = 10f; // radius around a random centre point
    private const float MAX_WAIT_TIME = 5f;
    private const float STOPPING_DISTANCE = 0.3f;
    private const float FLOAT_ZERO_ERROR = 0.1f;
    private const float SLOW_TURNING_ANGLE = 10f;

    private NavMeshAgent agent;
    private Transform st;

    private GameObject _testMoveToMarker;
    private GameObject _testSteerToMarker;


    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Human").transform;
        st = transform.GetChild(0).transform;

        InitAgent();
        _path = new NavMeshPath();

        // TEST
        _testMoveToMarker = GameObject.Find("Marker");
        _testSteerToMarker = GameObject.Find("SteeringMarker");
    }

    private void InitAgent()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.isStopped = true;

        agent.speed = ClientCommon.Game.CatMovementSpeed;
        agent.acceleration = agent.speed / ClientCommon.Game.TimeToMaxSpeed;
        agent.stoppingDistance = STOPPING_DISTANCE;
        agent.angularSpeed = SLOW_TURNING_ANGLE;
        agent.autoBraking = false;

        int notWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
        agent.areaMask = ~(1 << notWalkableArea);

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
            Vector3 randomPoint = GetRandomPointOnNavMesh(_randomCentrePoints[_toShuffleCountdown], SEARCH_RADIUS);
            if (randomPoint != Vector3.zero && !IsVisibleToTarget(randomPoint))
            {
                randomPoint.z = transform.position.z;
                agent.CalculatePath(randomPoint, _path);

                if (_testMoveToMarker)
                    _testMoveToMarker.transform.position = randomPoint; // TEST
                //Debug.Log($"mup Success at #{i}!!! Set final target to {randomPoint}");

                _currentCornerIndex = 0;
                UpdateNextTarget();
                UpdateDirection(_currentTarget - transform.position);
                agent.isStopped = false;


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

    Vector2 ConstrainDirection(Vector2 move)
    {
        var constrainedDirection = move.normalized;
        var absMove = move.Abs();

        // Prioritize diagonal movement first
        if (absMove.x > agent.stoppingDistance && absMove.y > agent.stoppingDistance)
        {
            constrainedDirection = new Vector2(Mathf.Sign(constrainedDirection.x), Mathf.Sign(constrainedDirection.y));
        }
        // Then shorter distance next
        else if (absMove.x < absMove.y && absMove.x > agent.stoppingDistance)
        {
            constrainedDirection = new Vector2(Mathf.Sign(constrainedDirection.x), 0f); // Left or Right
        }
        else if (absMove.y < absMove.x && absMove.y > agent.stoppingDistance)
        {
            constrainedDirection = new Vector2(0f, Mathf.Sign(constrainedDirection.y)); // Up or Down
        }
        else
        {
            return constrainedDirection;
        }

        // Renormalize to match original direction's magnitude
        constrainedDirection = constrainedDirection.normalized;

        return constrainedDirection;
    }

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

    /// <summary>
    /// Handles waiting logic when the agent is stopped.
    /// </summary>
    private void HandleWaiting()
    {
        if (_wait_time < 0f)
        {
            MoveToOutOfSightSpot();
            _wait_time += UnityEngine.Random.Range(3f, MAX_WAIT_TIME);
        }
        else
        {
            _wait_time -= Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Checks if the agent has reached the current target corner.
    /// Changes tranform position if true
    /// </summary>
    private bool HasReachedCurrentTarget(Vector2 move)
    {
        if (move.magnitude <= agent.stoppingDistance ||
            ((_constrainedDirection.x < 0 && move.x > 0 || _constrainedDirection.x > 0 && move.x < 0) &&
            (_constrainedDirection.y < 0 && move.y > 0 || _constrainedDirection.y > 0 && move.y < 0)))
        {
            transform.position = new Vector2(_currentTarget.x, transform.position.y);
            agent.nextPosition = transform.position;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the agent is aligned along a single axis.
    /// Changes tranform position if true
    /// </summary>
    private bool IsAlignedWithTarget(Vector2 move)
    {
        // was going diagonal but in line on one axis now
        if (Mathf.Abs(_constrainedDirection.x) > FLOAT_ZERO_ERROR
             && Mathf.Abs(_constrainedDirection.y) > FLOAT_ZERO_ERROR)
        {
            // snap on x axis
            if (Mathf.Abs(move.x) <= agent.stoppingDistance || _constrainedDirection.x < 0 && move.x > 0 || _constrainedDirection.x > 0 && move.x < 0)
            {
                transform.position = new Vector2(_currentTarget.x, transform.position.y);
                agent.nextPosition = transform.position;
                return true;
            }
            // snap on y axis
            else if (Mathf.Abs(move.y) <= agent.stoppingDistance || _constrainedDirection.y < 0 && move.y > 0 || _constrainedDirection.y > 0 && move.y < 0)
            {
                transform.position = new Vector2(transform.position.x, _currentTarget.y);
                agent.nextPosition = transform.position;
                return true;
            }
        }
        // was going horizontal but in line on x axis now
        else if (Mathf.Abs(_constrainedDirection.x) > FLOAT_ZERO_ERROR)
        {
            // snap on x axis
            if (Mathf.Abs(move.x) <= agent.stoppingDistance || _constrainedDirection.x < 0 && move.x > 0 || _constrainedDirection.x > 0 && move.x < 0)
            {
                transform.position = new Vector2(_currentTarget.x, transform.position.y);
                agent.nextPosition = transform.position;
                return true;
            }
        }
        // was going vertical but in line on y axis now
        else if (Mathf.Abs(_constrainedDirection.y) > FLOAT_ZERO_ERROR)
        {
            // snap on y axis
            if (Mathf.Abs(move.y) <= agent.stoppingDistance || _constrainedDirection.y < 0 && move.y > 0 || _constrainedDirection.y > 0 && move.y < 0)
            {
                transform.position = new Vector2(transform.position.x, _currentTarget.y);
                agent.nextPosition = transform.position;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Updates target to the next corner in the path.
    /// </summary>
    private void UpdateNextTarget()
    {
        _currentCornerIndex++;
        if (_currentCornerIndex < _path.corners.Length)
        {
            _currentTarget = _path.corners[_currentCornerIndex];

            if (_testSteerToMarker)
                _testSteerToMarker.transform.position = _currentTarget; // TEST
            //Debug.Log($"Next target #{_currentCornerIndex} moving from {transform.position} to {_currentTarget}");
        }
    }

    /// <summary>
    /// Updates movement direction and sprite facing direction.
    /// </summary>
    private void UpdateDirection(Vector2 move)
    {
        _constrainedDirection = ConstrainDirection(move);
        UpdateSpriteFacingDirection();
        //Debug.Log($"#{_currentCornerIndex} Change dir moving from {transform.position} to {_currentTarget}: {move} ... {_constrainedDirection}");
    }

    /// <summary>
    /// Updates sprite facing direction based on movement direction.
    /// </summary>
    private void UpdateSpriteFacingDirection()
    {
        if (Mathf.Abs(_constrainedDirection.x) > 0.1f)
        {
            float facingDir = Mathf.Sign(_constrainedDirection.x); // 1 for positive, -1 for negative
            st.localScale = new Vector3(
                Mathf.Abs(st.localScale.x) * -facingDir,
                st.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void CheckUpdateMovement(Vector2 move)
    {
        // Check if the agent reached the current target
        if (HasReachedCurrentTarget(move))
        {
            UpdateNextTarget();
            if (_currentCornerIndex < _path.corners.Length)
            {
                UpdateDirection(_currentTarget - transform.position);
            }
            else
            {
                StopAgent();
            }
        }
        // Adjust direction if needed
        else if (IsAlignedWithTarget(move))
        {
            UpdateDirection(_currentTarget - transform.position);
        }

    }

    /// <summary>
    /// Applies movement using the constrained direction.
    /// </summary>
    private void ApplyMovement()
    {
        if (_constrainedDirection != Vector3.zero)
        {
            Vector3 movement = _constrainedDirection * agent.speed * Time.fixedDeltaTime;
            agent.Move(movement);
        }
    }

    /// <summary>
    /// Stops the agent, clears the path, and resets state.
    /// </summary>
    private void StopAgent()
    {
        //Debug.Log($"Stopped at {transform.position}");
        agent.isStopped = true;
        _constrainedDirection = Vector3.zero;
        _currentTarget = Vector3.zero;
        _path.ClearCorners();
    }

    void FixedUpdate()
    {
        if (agent.isStopped)
        {
            HandleWaiting();
            return;
        }

        CheckUpdateMovement(_currentTarget - transform.position);

        ApplyMovement();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public Transform waypointParent; // Assign the parent GameObject with waypoints as children
    public float speed = 5f;
    public bool loopPath = true;
    public float waypointReachThreshold = 0.2f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float currentSpeed;

    void Start()
    {
        if (waypointParent == null)
        {
            Debug.LogWarning("No waypoint parent assigned!");
            enabled = false;
            return;
        }

        // Get all child transforms as waypoints, sorted by hierarchy order
        waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }

        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints found in parent object!");
            enabled = false;
            return;
        }

        currentSpeed = speed;
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        MoveTowardsWaypoint();

        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < waypointReachThreshold)
        {
            ReachWaypoint();
        }
    }

    void MoveTowardsWaypoint()
    {
        Vector3 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, (Time.deltaTime * 0.5f) * speed);
    }

    void ReachWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loopPath)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                enabled = false;
            }
        }

        currentSpeed = speed;
    }
}

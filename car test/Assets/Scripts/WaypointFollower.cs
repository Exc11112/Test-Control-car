using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;  // Array to store waypoints
    public float speed = 5f;       // Base speed of the object
    public bool loopPath = true;   // Set to true if you want the object to loop back after reaching the last waypoint
    public float waypointReachThreshold = 0.2f; // Distance threshold to determine if waypoint has been reached

    private int currentWaypointIndex = 0; // Index of the current waypoint
    private float currentSpeed;           // Speed that can be adjusted at each waypoint

    void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            enabled = false;
            return;
        }

        currentSpeed = speed;
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        // Move towards the current waypoint
        MoveTowardsWaypoint();

        // Check if we have reached the current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < waypointReachThreshold)
        {
            ReachWaypoint();
        }
    }

    void MoveTowardsWaypoint()
    {
        // Calculate direction and move towards the current waypoint
        Vector3 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;

        // Rotate smoothly towards the waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }

    void ReachWaypoint()
    {
        // Call this method when reaching a waypoint
        Debug.Log("Reached waypoint: " + currentWaypointIndex);

        // Increase the waypoint index to move to the next one
        currentWaypointIndex++;

        // Check if we've reached the end of the waypoint array
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loopPath)
            {
                currentWaypointIndex = 0; // Loop back to the start
            }
            else
            {
                enabled = false; // Stop the script if path is complete and loop is disabled
            }
        }

        // Modify speed or other properties as needed here if you want per-waypoint control
        currentSpeed = speed; // Reset to base speed (optional, add custom logic here)
    }
}

using UnityEngine;
using System.Collections.Generic;

public class AIPathFollower : MonoBehaviour
{
    public List<Transform> waypoints;      // Waypoints for path-following
    public float waypointReachThreshold = 5f; // Distance at which to consider the waypoint reached
    public float steeringSensitivity = 0.5f; // Sensitivity for turning toward waypoints
    public float maxSpeed = 150f;           // Max speed the AI can reach

    public CarController2 carController;   // Reference to CarController2
    private int currentWaypointIndex = 0;

    private void Start()
    {
        // Reference CarController2
        carController = GetComponent<CarController2>();
    }

    private void Update()
    {
        if (waypoints.Count == 0) return;

        // Get the current waypoint and calculate distance and direction
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        //// Steer toward the waypoint
        //SteerTowardsWaypoint(directionToWaypoint);

        //// Control speed based on distance to waypoint
        //ControlSpeed(distanceToWaypoint);

        // Check if the waypoint is reached
        if (distanceToWaypoint < waypointReachThreshold)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
    }

    //    private void SteerTowardsWaypoint(Vector3 directionToWaypoint)
    //    {
    //        // Calculate the angle to the waypoint
    //        float angle = Vector3.SignedAngle(transform.forward, directionToWaypoint, Vector3.up);

    //        // Apply steering based on the angle
    //        float steeringInput = Mathf.Clamp(angle * steeringSensitivity, -1f, 1f);
    //        carController.Steer(steeringInput); // Call CarController2’s steering method
    //    }

    //    private void ControlSpeed(float distanceToWaypoint)
    //    {
    //        // Set speed control based on distance
    //        if (distanceToWaypoint < waypointReachThreshold * 1.5f) // Slow down near waypoint
    //        {
    //            carController.Accelerate(0.5f); // Adjust to half-throttle near waypoints
    //        }
    //        else
    //        {
    //            carController.Accelerate(1f); // Full throttle when farther from waypoints
    //        }

    //        // Ensure max speed limit
    //        if (carController.CurrentSpeed > maxSpeed)
    //        {
    //            carController.Brake(1f); // Apply brakes if speed exceeds limit
    //        }
    //    }
}

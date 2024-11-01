using UnityEngine;
using System.Collections.Generic;

public class AIPathFollower : MonoBehaviour
{
    public List<Transform> waypoints;
    public float waypointReachThreshold = 5f;
    public float steeringSensitivity = 0.5f;
    public float maxSpeed = 150f;

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    private int currentWaypointIndex = 0;
    public float currentSpeed;

    private void Update()
    {
        if (waypoints.Count == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        SteerTowardsWaypoint(directionToWaypoint);
        ControlSpeed(distanceToWaypoint);

        if (distanceToWaypoint < waypointReachThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
    }

    private void SteerTowardsWaypoint(Vector3 directionToWaypoint)
    {
        float angle = Vector3.SignedAngle(transform.forward, directionToWaypoint, Vector3.up);
        float steeringInput = Mathf.Clamp(angle * steeringSensitivity, -1f, 1f);

        // Apply steering to front wheels only
        frontLeftWheel.steerAngle = steeringInput * 30f;
        frontRightWheel.steerAngle = steeringInput * 30f;
    }

    private void ControlSpeed(float distanceToWaypoint)
    {
        currentSpeed = CalculateCurrentSpeed();

        if (distanceToWaypoint < waypointReachThreshold * 1.5f)
        {
            SetMotorTorque(0.5f);
        }
        else
        {
            SetMotorTorque(1f);
        }

        if (currentSpeed > maxSpeed)
        {
            ApplyBrakes(1f);
        }
        else
        {
            ApplyBrakes(0f);
        }
    }

    private void SetMotorTorque(float input)
    {
        float motorTorque = input * 1000f;

        rearLeftWheel.motorTorque = motorTorque;
        rearRightWheel.motorTorque = motorTorque;
    }

    private void ApplyBrakes(float brakeForce)
    {
        float brakeTorque = brakeForce * 3000f;

        rearLeftWheel.brakeTorque = brakeTorque;
        rearRightWheel.brakeTorque = brakeTorque;
        frontLeftWheel.brakeTorque = brakeTorque;
        frontRightWheel.brakeTorque = brakeTorque;
    }

    private float CalculateCurrentSpeed()
    {
        // Calculate current speed based on rear wheel rotation
        float leftSpeed = rearLeftWheel.rpm * (rearLeftWheel.radius * 2 * Mathf.PI) / 60;
        float rightSpeed = rearRightWheel.rpm * (rearRightWheel.radius * 2 * Mathf.PI) / 60;
        return (leftSpeed + rightSpeed) / 2f;
    }
}

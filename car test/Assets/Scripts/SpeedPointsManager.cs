using System.Collections;
using UnityEngine;

public class SpeedPointsManager : MonoBehaviour
{
    public CarController2 carController; // Reference to your CarController2 script
    public int points = 0; // Total points earned
    public float speedThreshold = 70f; // Speed threshold in km/h
    public float pointInterval = 5f; // Interval in seconds to gain points
    public int pointsPerInterval = 5; // Points earned per interval

    private bool isAtSpeed = false; // Tracks if the car is at the required speed
    private float speedCheckTimer = 0f; // Timer to track time at required speed

    void Update()
    {
        // Convert current speed from m/s to km/h
        float currentSpeedKmh = carController.currentSpeed;

        if (currentSpeedKmh >= speedThreshold)
        {
            // If at speed, increment the timer
            speedCheckTimer += Time.deltaTime;

            // If the timer exceeds the interval, award points
            if (speedCheckTimer >= pointInterval)
            {
                points += pointsPerInterval;
                speedCheckTimer = 0f; // Reset the timer
                Debug.Log($"Points awarded! Total points: {points}");
            }
        }
        else
        {
            // Reset the timer if speed drops below threshold
            speedCheckTimer = 0f;
        }
    }
}

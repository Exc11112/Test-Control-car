using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPointsManager : MonoBehaviour
{
    public CarController2 carController; // Reference to CarController2
    public float speedThreshold = 70f; // Speed threshold in km/h
    public float pointInterval = 5f; // Interval in seconds to gain points
    public int pointsPerInterval = 5; // Points earned per interval

    private float speedCheckTimer = 0f; // Timer to track time at required speed
    private DriftScore2[] driftScores; // Array to store all DriftScore2 instances

    void Start()
    {
        // Find all DriftScore2 scripts in the scene and store them
        driftScores = FindObjectsOfType<DriftScore2>();
    }

    void Update()
    {
        float currentSpeedKmh = carController.currentSpeed;

        if (currentSpeedKmh >= speedThreshold)
        {
            speedCheckTimer += Time.deltaTime;

            if (speedCheckTimer >= pointInterval)
            {
                // Award points to all DriftScore2 scripts
                foreach (DriftScore2 driftScore in driftScores)
                {
                    if (driftScore != null)
                    {
                        driftScore.AddPoints(pointsPerInterval);
                    }
                }

                speedCheckTimer = 0f; // Reset the timer
                Debug.Log($"Points awarded to all DriftScore2 instances!");
            }
        }
        else
        {
            speedCheckTimer = 0f;
        }
    }
}

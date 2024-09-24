using UnityEngine;
using System.Collections.Generic;

public class TrailSpawner : MonoBehaviour
{
    public List<TrailRenderer> trailRenderers;  // List of trail renderers for each wheel or point
    public CarController2 car;                  // Reference to CarController2 for accessing drifting state

    private bool trailsActive = false;

    private void Start()
    {
        StopTrails();
    }

    void Update()
    {
        float turnInputThreshold = 0.1f;  // Small tolerance for turn input
        float speedThreshold = car.driftThresholdSpeed;

        // Check if car is drifting or turning at a high speed
        bool isTurningAtSpeed = car.currentSpeed > speedThreshold && Mathf.Abs(car.turnInput) > turnInputThreshold;

        // Activate trails when drifting or when turning at a certain speed
        if (!trailsActive && (car.isDrifting || isTurningAtSpeed))
        {
            Debug.Log("on");
            StartTrails();
            trailsActive = true;
        }

        // Deactivate trails when no longer drifting or turning at high speed
        else if (trailsActive && (!car.isDrifting && !isTurningAtSpeed))
        {
            Debug.Log("off");
            StopTrails();
            trailsActive = false;
        }
    }

    // Enable all trail renderers
    void StartTrails()
    {
        foreach (TrailRenderer trail in trailRenderers)
        {
            trail.emitting = true;  // Start emitting the trail
        }
    }

    // Disable all trail renderers
    void StopTrails()
    {
        foreach (TrailRenderer trail in trailRenderers)
        {
            trail.emitting = false; // Stop emitting the trail
        }
    }
}

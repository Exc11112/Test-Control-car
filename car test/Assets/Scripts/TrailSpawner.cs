using UnityEngine;
using System.Collections.Generic;

public class TrailSpawner : MonoBehaviour
{
    public List<TrailRenderer> trailRenderers;  // List of trail renderers for each wheel or point
    public CarController2 car;                  // Reference to CarController2 for accessing drifting state

    private bool trailsActive = false;

    void Update()
    {
        // Activate trails when drifting starts
        if (car.isDrifting && !trailsActive)
        {
            StartTrails();
            trailsActive = true;
        }

        // Deactivate trails when drifting stops
        if (!car.isDrifting && trailsActive)
        {
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

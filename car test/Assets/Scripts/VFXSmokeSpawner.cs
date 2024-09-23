using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class VFXSmokeSpawner : MonoBehaviour
{
    public VisualEffect smokeEffectPrefab; // Reference to the VFX prefab
    public List<Transform> spawnPoints;    // List of spawn points
    private List<VisualEffect> activeSmokeEffects = new List<VisualEffect>(); // To store active smoke instances
    public CarController2 car;
    bool smokeActive = false;

    void Start()
    {
        if (car == null)
        {
            car = FindObjectOfType<CarController2>();
        }
    }
    void Update()
    {
        // Start smoke only if drifting starts and it's not already active
        if (car.isDrifting && !smokeActive)
        {
            StartSmoke();
            smokeActive = true;
        }

        // Stop smoke when drifting stops
        if (!car.isDrifting && smokeActive)
        {
            StopSmoke();
            smokeActive = false;
        }
    }


    // Method to spawn and start smoke at all spawn points
    void StartSmoke()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            // Instantiate the smoke effect at each spawn point
            VisualEffect smokeInstance = Instantiate(smokeEffectPrefab, spawnPoint.position, spawnPoint.rotation);

            // Parent the smoke effect to the spawn point so it follows its position
            smokeInstance.transform.SetParent(spawnPoint);

            // Play the smoke effect
            smokeInstance.Play();

            // Keep track of active smoke effects
            activeSmokeEffects.Add(smokeInstance);
        }
    }

    // Method to stop all active smoke effects
    void StopSmoke()
    {
        foreach (VisualEffect smokeEffect in activeSmokeEffects)
        {
            if (smokeEffect != null)
            {
                smokeEffect.Stop(); // Stop the smoke effect
                Destroy(smokeEffect.gameObject,1f); // Optionally, destroy after a delay
            }
        }

        activeSmokeEffects.Clear(); // Clear the list after stopping the effects
    }
}

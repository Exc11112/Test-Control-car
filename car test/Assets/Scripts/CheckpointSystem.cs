using UnityEngine;
using UnityEngine.SceneManagement; // For resetting the scene

public class CheckpointSystem : MonoBehaviour
{
    public string[] checkpointLayers = { "checkpoint1", "checkpoint2" };
    public string fpointLayer = "fpoint";
    public string car = "Car";

    private float[] checkpointSpeeds;
    private float totalSpeedAtCheckpoints = 0f;
    private float timer = 0f;
    private int checkpointIndex = 0;
    private bool isTimerRunning = false;
    private bool hasReachedCheckpoint = false;
    public CarController2 cars;

    private void Start()
    {
        checkpointSpeeds = new float[checkpointLayers.Length];
    }

    private void Update()
    {
        // Start the timer if car is moving (optional condition)
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the car hits any checkpoint
        if (checkpointIndex < checkpointLayers.Length && other.gameObject.layer == LayerMask.NameToLayer(checkpointLayers[checkpointIndex]))
        {
            Debug.Log("Car hit checkpoint: " + checkpointLayers[checkpointIndex]);

            // Collect the car's speed at this checkpoint
            float currentSpeed = cars.currentSpeed; // Assuming speed is in km/h
            checkpointSpeeds[checkpointIndex] = currentSpeed;
            totalSpeedAtCheckpoints += currentSpeed;

            Debug.Log("Checkpoint " + (checkpointIndex + 1) + " speed: " + currentSpeed + " km/h");
            Debug.Log("Total speed so far: " + totalSpeedAtCheckpoints);

            // Deactivate the checkpoint after collecting speed
            other.gameObject.SetActive(false);

            // Move to the next checkpoint
            checkpointIndex++;
            hasReachedCheckpoint = true;

            // Start the timer if it's the first checkpoint
            if (!isTimerRunning)
            {
                isTimerRunning = true;
            }
        }
        // Check if the car reaches the finish point (fpoint)
        else if (other.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            Debug.Log("Car reached finish point.");

            if (hasReachedCheckpoint)
            {
                // Stop the timer
                isTimerRunning = false;

                // Display total speed and time
                Debug.Log("Grand total speed at checkpoints: " + totalSpeedAtCheckpoints);
                Debug.Log("Total time to reach finish point: " + timer + " seconds");
            }
            else
            {
                // Reset the scene if no checkpoints were hit
                Debug.Log("No checkpoints reached. Resetting scene.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}

using UnityEngine.SceneManagement;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    public string[] checkpointLayers = { "checkpoint1", "checkpoint2" };
    public string fpointLayer = "fpoint";
    public string car = "Car";

    private float[] checkpointSpeeds;
    private float totalSpeedAtCheckpoints = 0f;
    private float timer = 0f;
    private int checkpointIndex = 0;
    private int currentLap = 1;
    private bool isTimerRunning = false;
    private bool hasReachedCheckpoint = false;
    public CarController2 cars;
    public SwitchParth sp1;
    public SwitchParth sp2;

    private void Start()
    {
        checkpointSpeeds = new float[checkpointLayers.Length];
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (checkpointIndex < checkpointLayers.Length && other.gameObject.layer == LayerMask.NameToLayer(checkpointLayers[checkpointIndex]))
        {
            float currentSpeed = cars.currentSpeed;
            checkpointSpeeds[checkpointIndex] = currentSpeed;
            totalSpeedAtCheckpoints += currentSpeed;

            Debug.Log("Checkpoint " + (checkpointIndex + 1) + " speed: " + currentSpeed + " km/h");
            Debug.Log("Total speed so far: " + totalSpeedAtCheckpoints);

            other.gameObject.SetActive(false);
            checkpointIndex++;
            hasReachedCheckpoint = true;

            if (!isTimerRunning)
            {
                isTimerRunning = true;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            if (hasReachedCheckpoint)
            {
                isTimerRunning = false;

                if (currentLap == 1)
                {
                    // Prepare for Lap 2
                    ReactivateAllSwitchParthLayers();
                    ReactivateCheckpoints();
                    currentLap++;
                    checkpointIndex = 0;
                    hasReachedCheckpoint = false;
                    isTimerRunning = true;
                    Debug.Log("Starting Lap 2");
                }
                else
                {
                    // Final lap complete, display total score and time
                    Debug.Log("Grand total speed at checkpoints: " + totalSpeedAtCheckpoints);
                    Debug.Log("Total time to reach finish point: " + timer + " seconds");
                }
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void ReactivateCheckpoints()
    {
        foreach (string checkpointLayer in checkpointLayers)
        {
            GameObject[] checkpoints = GameObject.FindGameObjectsWithTag(checkpointLayer);

            foreach (GameObject checkpoint in checkpoints)
            {
                checkpoint.SetActive(true);
            }
        }
    }

    private void ReactivateAllSwitchParthLayers()
    {
        sp1.ReactivateDeactivatedLayers();
        sp2.ReactivateDeactivatedLayers();
    }
}

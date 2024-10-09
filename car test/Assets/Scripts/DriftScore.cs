using UnityEngine;
using UnityEngine.UI;
using System.Collections;  // Required for Coroutines

public class DriftScore : MonoBehaviour
{
    private bool isDrifting = false;
    private float driftStartTime;
    private float driftScore = 0f;
    private bool hasPassedDriftStart = false;
    public CarController2 car;

    public Text driftScoreText;  // UI text to display the score
    public Text countdownText;   // UI text to display the countdown

    public string driftstart = "driftstart";
    public string driftend = "driftend";
    public string wall = "wall";

    private bool driftEndDeactivated = false;  // To track if driftend has been deactivated
    private Coroutine driftEndCoroutine = null;  // Coroutine to handle delayed drift end
    private Coroutine countdownCoroutine = null; // Coroutine for the countdown timer

    void Update()
    {
        // If the car is drifting, calculate the score based on drift duration and speed
        if (isDrifting)
        {
            float speed = GetComponent<Rigidbody>().velocity.magnitude;

            // Accumulate drift score over time while drifting (e.g., drift time * speed)
            driftScore += Time.deltaTime * speed;
            driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
        }

        // If the car stops drifting but hasn't hit driftend, start delay to end drifting
        if (!car.isDrifting && isDrifting && driftEndCoroutine == null)
        {
            // Start a coroutine to delay drift end by 1.5 seconds
            driftEndCoroutine = StartCoroutine(DelayedEndDrift(1.5f));
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(driftstart))
        {
            // Start a countdown when hitting the driftstart trigger
            if (countdownCoroutine == null)
            {
                countdownCoroutine = StartCoroutine(StartDriftCountdown(3f));  // 3 seconds countdown
            }
            hasPassedDriftStart = true;
            DeactivateObjectsInLayer(driftstart);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer(driftend))
        {
            EndDrifting();
            DeactivateObjectsInLayer(driftend);
            driftEndDeactivated = true;  // Mark as deactivated since collision occurred
            StopDelayedEndDrift();  // Stop the delay if the car collides with driftend
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(wall) && hasPassedDriftStart)
        {
            // Stop the countdown if it is still running
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
                countdownText.text = "";  // Clear the countdown display
            }

            EndDrifting();  // Immediately end drifting
            DeactivateObjectsInLayer(driftend);
            driftEndDeactivated = true;
            StopDelayedEndDrift();  // Stop any delayed drift end coroutine if running
        }
    }


    // Coroutine to handle the countdown before drifting begins
    private IEnumerator StartDriftCountdown(float countdownTime)
    {
        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            countdownText.text = "Drift! (" + Mathf.Ceil(remainingTime).ToString() + ")";

            // If car starts drifting during the countdown, stop the countdown and start drifting
            if (car.isDrifting)
            {
                StartDrifting();
                yield break;  // Exit the countdown coroutine early
            }

            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        // Countdown is over, check if car is drifting
        if (car.isDrifting)
        {
            StartDrifting();
        }
        else
        {
            countdownText.text = "Failed to Drift!";
            EndDrifting();  // End drift if no drift detected after countdown
        }

        countdownCoroutine = null;  // Reset the countdown coroutine
    }

    void StartDrifting()
    {
        // Stop the countdown if it's still running
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        isDrifting = true;
        driftStartTime = Time.time;
        driftEndDeactivated = false;  // Reset the driftend deactivation flag
        driftEndCoroutine = null;  // Reset the coroutine when drifting starts
        countdownText.text = "";  // Clear countdown text when drifting starts
    }

    void EndDrifting()
    {
        isDrifting = false;
        hasPassedDriftStart = false;
        driftScoreText.text = "Final Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
    }

    private void DeactivateObjectsInLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objectsInLayer)
        {
            if (obj.layer == layer)
            {
                obj.SetActive(false);
            }
        }
    }

    // Coroutine to delay ending the drift
    private IEnumerator DelayedEndDrift(float delay)
    {
        yield return new WaitForSeconds(delay);

        // After delay, check again if the car is still not drifting
        if (!car.isDrifting)
        {
            EndDrifting();
            if (!driftEndDeactivated)
            {
                DeactivateObjectsInLayer(driftend);
                driftEndDeactivated = true;  // Ensure deactivation happens only once
            }
        }

        driftEndCoroutine = null;  // Reset coroutine reference after it completes
    }

    // Method to stop the coroutine if needed (e.g., when the car hits driftend)
    private void StopDelayedEndDrift()
    {
        if (driftEndCoroutine != null)
        {
            StopCoroutine(driftEndCoroutine);
            driftEndCoroutine = null;
        }
    }
}

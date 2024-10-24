using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DriftScore : MonoBehaviour
{
    private bool isDrifting = false;
    private float driftStartTime;
    private float driftScore = 0f;
    private bool hasPassedDriftStart = false;
    public CarController2 car;

    public Text driftScoreText;  // UI text to display the score
    public Text countdownText;   // UI text to display the countdown

    public string wall = "wall";

    private Coroutine driftEndCoroutine = null;  // Coroutine to handle delayed drift end
    private Coroutine countdownCoroutine = null; // Coroutine for the countdown timer

    // Class to store drift start and end pairs
    [System.Serializable]
    public class DriftPair
    {
        public GameObject driftStart;  // The driftstart GameObject
        public GameObject driftEnd;    // The driftend GameObject
    }

    public List<DriftPair> driftPairs = new List<DriftPair>(); // List to hold pairs of driftstart and driftend

    private int currentPairIndex = 0;  // Track which pair is active

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
        // Ensure currentPairIndex is initialized properly and within bounds
        if (currentPairIndex >= 0 && currentPairIndex < driftPairs.Count)
        {
            GameObject currentDriftStart = driftPairs[currentPairIndex].driftStart;
            GameObject currentDriftEnd = driftPairs[currentPairIndex].driftEnd;

            // Check if the car hits the current driftstart trigger
            if (collision.gameObject == currentDriftStart)
            {
                // Start a countdown when hitting the driftstart trigger
                if (countdownCoroutine == null)
                {
                    countdownCoroutine = StartCoroutine(StartDriftCountdown(3f));  // 3 seconds countdown
                }
                hasPassedDriftStart = true;
                currentDriftStart.SetActive(false);  // Deactivate the current drift start
            }
            // Check if the car hits the current driftend trigger
            else if (collision.gameObject == currentDriftEnd)
            {
                EndDrifting();
                currentDriftEnd.SetActive(false);  // Deactivate the current drift end
                StopDelayedEndDrift();  // Stop the delay if the car collides with driftend
                currentPairIndex++;  // Move to the next drift pair

                // Make sure currentPairIndex doesn't go out of bounds
                if (currentPairIndex >= driftPairs.Count)
                {
                    currentPairIndex = driftPairs.Count - 1; // Prevent out-of-range errors
                }
            }
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
            if (currentPairIndex >= 0 && currentPairIndex < driftPairs.Count)
            {
                driftPairs[currentPairIndex].driftEnd.SetActive(false);  // Deactivate the current drift end
            }
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

            // Deactivate driftEnd if the car didn't start drifting
            if (currentPairIndex >= 0 && currentPairIndex < driftPairs.Count)
            {
                driftPairs[currentPairIndex].driftEnd.SetActive(false);  // Deactivate the current drift end
            }
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
        countdownText.text = "";  // Clear countdown text when drifting starts
    }

    void EndDrifting()
    {
        isDrifting = false;
        hasPassedDriftStart = false;
        driftScoreText.text = "Final Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
    }

    // Coroutine to delay ending the drift
    private IEnumerator DelayedEndDrift(float delay)
    {
        yield return new WaitForSeconds(delay);

        // After delay, check again if the car is still not drifting
        if (!car.isDrifting)
        {
            EndDrifting();
            if (currentPairIndex >= 0 && currentPairIndex < driftPairs.Count)
            {
                driftPairs[currentPairIndex].driftEnd.SetActive(false);  // Ensure deactivation happens only once
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

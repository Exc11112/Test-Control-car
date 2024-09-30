using UnityEngine;
using UnityEngine.UI;
using System.Collections;  // Required for Coroutines

public class DriftScore : MonoBehaviour
{
    private bool isDrifting = false;
    private float driftStartTime;
    private float driftScore = 0f;
    public CarController2 car;

    public Text driftScoreText;  // UI text to display the score

    public string driftstart = "driftstart";
    public string driftend = "driftend";

    private bool driftEndDeactivated = false;  // To track if driftend has been deactivated
    private Coroutine driftEndCoroutine = null;  // Coroutine to handle delayed drift end

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(driftstart))
        {
            StartDrifting();
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

    void StartDrifting()
    {
        if (car.isDrifting)
        {
            isDrifting = true;
            driftStartTime = Time.time;
            driftEndDeactivated = false;  // Reset the driftend deactivation flag
            driftEndCoroutine = null;  // Reset the coroutine when drifting starts
        }
    }

    void EndDrifting()
    {
        isDrifting = false;
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

using UnityEngine;
using UnityEngine.UI;

public class DriftScore2 : MonoBehaviour
{
    private bool isDrifting = false;
    private float driftScore = 0f;
    public CarController2 car;

    public Text driftScoreText;  // UI text to display the score

    public string fpointLayer = "fpoint"; // Name of the layer for the final point

    void Update()
    {
        // If the car is drifting, calculate the score based on drift duration and speed
        if (car.isDrifting)
        {
            isDrifting = true;
            float speed = GetComponent<Rigidbody>().velocity.magnitude;

            // Accumulate drift score over time while drifting (e.g., drift time * speed)
            driftScore += Time.deltaTime * speed;
            driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
        }
        else
        {
            // Stop updating the drift score when not drifting but do not reset it
            isDrifting = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the car hits the "fpoint" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            // Display the final drift score
            driftScoreText.text = "Final Drift Score: " + Mathf.RoundToInt(driftScore).ToString();

            // Optionally, you could stop scoring entirely here if needed:
            // enabled = false;
        }
    }
}

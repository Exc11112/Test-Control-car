using UnityEngine;
using UnityEngine.UI;

public class DriftScore2 : MonoBehaviour
{
    public SpeedDisplay speedDisplay; // Reference to SpeedDisplay script
    private bool isDrifting = false;
    private float driftScore = 0f;
    public CarController2 car;

    public Text driftScoreText;  // UI text to display the score

    public string fpointLayer = "fpoint"; // Name of the layer for the final point

    public Slider bar1; // Slider for the first bar
    public Slider bar2; // Slider for the second bar
    public Slider bar3; // Slider for the third bar
    public Slider bar4; // Slider for the fourth bar

    public float maxBar1 = 100f;  // Max points for bar1
    public float maxBar2 = 500f; // Max points for bar2
    public float maxBar3 = 1000f; // Max points for bar3
    public float maxBar4 = 2500f; // Max points for bar4

    private float progressBar2To4 = 0f; // Points used for bar2, bar3, and bar4

    public float Plustime = 0f;
    private void Start()
    {
        // Initialize sliders
        bar1.maxValue = maxBar1;
        bar2.maxValue = maxBar2;
        bar3.maxValue = maxBar3;
        bar4.maxValue = maxBar4;

        bar1.value = 0f;
        bar2.value = 0f;
        bar3.value = 0f;
        bar4.value = 0f;
    }

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

        UpdateBar1();  // Call to update bar 1
        UpdateBar2To4(); // Call to update bars 2 to 4
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
    private void UpdateBar1()
    {
        if (!car.isDrifting) return; // Do nothing if the car is not drifting

        // Increase the bar value based on driftScore, but scale it down to a fixed rate
        float increment = Time.deltaTime * driftScore / 100f; // Scale the driftScore to a manageable increment
        bar1.value = Mathf.Min(bar1.value + increment, maxBar1); // Update bar 1 value

        if (bar1.value >= maxBar1)
        {
            // Reset the first bar and increase the timer in SpeedDisplay
            bar1.value = 0f;

            if (speedDisplay != null)
            {
                speedDisplay.countdownTime += Plustime; // Increase the timer
            }
        }
    }

    private void UpdateBar2To4()
    {
        if (!car.isDrifting) return; // Do nothing if the car is not drifting

        // Increase the collective progress for bars 2 to 4 based on driftScore, scaled to a fixed rate
        float increment = Time.deltaTime * driftScore / 500f; // Scale the driftScore to a manageable increment
        progressBar2To4 = Mathf.Min(progressBar2To4 + increment, maxBar2 + maxBar3 + maxBar4);

        // Fill the second bar
        if (progressBar2To4 <= maxBar2)
        {
            bar2.value = progressBar2To4;
        }
        // Fill the third bar
        else if (progressBar2To4 <= maxBar2 + maxBar3)
        {
            bar2.value = maxBar2;
            bar3.value = progressBar2To4 - maxBar2;
        }
        // Fill the fourth bar
        else if (progressBar2To4 <= maxBar2 + maxBar3 + maxBar4)
        {
            bar2.value = maxBar2;
            bar3.value = maxBar3;
            bar4.value = progressBar2To4 - (maxBar2 + maxBar3);
        }
        else
        {
            // Stop increasing when the fourth bar is full
            bar2.value = maxBar2;
            bar3.value = maxBar3;
            bar4.value = maxBar4;
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DriftScore2 : MonoBehaviour
{
    public SpeedDisplay speedDisplay; // Reference to SpeedDisplay script
    private float driftScore = 0f;
    public CarController2 car;

    public Text driftScoreText; // UI text to display the score
    public Text multiplierText; // UI text to display the multiplier

    public string fpointLayer = "fpoint"; // Name of the layer for the final point
    public string wallTag = "wall"; // Tag for wall objects

    public Slider bar1; // Slider for the first bar
    public Slider bar2; // Slider for the second bar
    public Slider bar3; // Slider for the third bar
    public Slider bar4; // Slider for the fourth bar

    public float maxBar1 = 100f; // Max points for bar1
    public float maxBar2 = 500f; // Max points for bar2
    public float maxBar3 = 1000f; // Max points for bar3
    public float maxBar4 = 2500f; // Max points for bar4

    private float progressBar2To4 = 0f; // Points used for bar2, bar3, and bar4

    public float Plustime = 0f;
    public float EnergyIncreaseRate = 10f;
    public float HeartIncreaseRate = 50f;

    public float timePenalty = 5f; // Time reduction when hitting a wall
    public float scorePenalty = 50f; // Drift score reduction when hitting a wall
    public float wallCooldown = 5f; // Cooldown time between penalties

    public float timePlusIncrement = 10f; // Amount to increase bar1 for TimePlus
    public float heartPlusIncrement = 50f; // Amount to increase bar2-bar4 for HeartPlus

    private float lastWallHitTime = -Mathf.Infinity; // Track the last wall collision time
    private float driftMultiplier = 1f; // Current score multiplier
    private float driftTime = 0f; // Total drift duration
    private float multiplierIncreaseInterval = 2f; // Time required to increase multiplier
    private int currentMultiplier = 1; // Current score multiplier
    private const int maxMultiplier = 5; // Maximum multiplier

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
        if (car.isDrifting)
        {
            float speed = GetComponent<Rigidbody>().velocity.magnitude; // Assume car has CurrentSpeed property
            driftScore += Time.deltaTime * speed * driftMultiplier;
            driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();

            // Update drift time and multiplier
            driftTime += Time.deltaTime;
            if (driftTime >= multiplierIncreaseInterval && driftMultiplier < 5f)
            {
                driftMultiplier++;
                driftTime = 0f;
            }
        }
        else
        {
            driftTime = 0f; // Reset drift time when not drifting
        }

        UpdateBar1();
        UpdateBar2To4();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the car hits the final point layer
        if (collision.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            driftScoreText.text = "Final Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
        }

        // Check if the car hits a wall
        if (collision.gameObject.CompareTag(wallTag))
        {
            if (Time.time >= lastWallHitTime + wallCooldown)
            {
                lastWallHitTime = Time.time;

                // Reduce drift score and reset multiplier
                driftScore = Mathf.Max(0, driftScore - scorePenalty);
                driftMultiplier = 1f;
                driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();

                if (speedDisplay != null)
                {
                    speedDisplay.countdownTime = Mathf.Max(0, speedDisplay.countdownTime - timePenalty);
                }

                bar1.value = Mathf.Max(0, bar1.value - scorePenalty);
                progressBar2To4 = Mathf.Max(0, progressBar2To4 - scorePenalty);
            }
        }

        // Handle collisions with TimePlus and HeartPlus objects (same as before)
        if (collision.gameObject.CompareTag("TimePlus"))
        {
            bar1.value = Mathf.Min(bar1.value + timePlusIncrement, maxBar1);

            if (bar1.value >= maxBar1)
            {
                bar1.value = 0f;

                if (speedDisplay != null)
                {
                    speedDisplay.countdownTime += Plustime;
                }
            }

            StartCoroutine(ReactivateGameObject(collision.gameObject, Random.Range(15f, 20f)));
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("HeartPlus"))
        {
            progressBar2To4 = Mathf.Min(progressBar2To4 + heartPlusIncrement, maxBar2 + maxBar3 + maxBar4);

            if (progressBar2To4 <= maxBar2)
            {
                bar2.value = progressBar2To4;
            }
            else if (progressBar2To4 <= maxBar2 + maxBar3)
            {
                bar2.value = maxBar2;
                bar3.value = progressBar2To4 - maxBar2;
            }
            else if (progressBar2To4 <= maxBar2 + maxBar3 + maxBar4)
            {
                bar2.value = maxBar2;
                bar3.value = maxBar3;
                bar4.value = progressBar2To4 - (maxBar2 + maxBar3);
            }
            else
            {
                bar2.value = maxBar2;
                bar3.value = maxBar3;
                bar4.value = maxBar4;
            }

            StartCoroutine(ReactivateGameObject(collision.gameObject, Random.Range(15f, 20f)));
            collision.gameObject.SetActive(false);
        }
        // Reset multiplier
        currentMultiplier = 1;
        driftTime = 0f;
        UpdateMultiplierText();
    }

    // Coroutine to reactivate the game object
    private IEnumerator ReactivateGameObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    private void UpdateBar1()
    {
        if (!car.isDrifting) return;

        float increment = Time.deltaTime * EnergyIncreaseRate; // Fixed increment rate for bar1
        bar1.value += increment;

        if (bar1.value >= maxBar1)
        {
            bar1.value = 0f;

            if (speedDisplay != null)
            {
                speedDisplay.countdownTime += Plustime;
            }
        }
    }

    private void UpdateBar2To4()
    {
        if (!car.isDrifting) return;

        float increment = Time.deltaTime * HeartIncreaseRate; // Fixed increment rate for bars 2 to 4
        progressBar2To4 = Mathf.Min(progressBar2To4 + increment, maxBar2 + maxBar3 + maxBar4);

        if (progressBar2To4 <= maxBar2)
        {
            bar2.value = progressBar2To4;
        }
        else if (progressBar2To4 <= maxBar2 + maxBar3)
        {
            bar2.value = maxBar2;
            bar3.value = progressBar2To4 - maxBar2;
        }
        else if (progressBar2To4 <= maxBar2 + maxBar3 + maxBar4)
        {
            bar2.value = maxBar2;
            bar3.value = maxBar3;
            bar4.value = progressBar2To4 - (maxBar2 + maxBar3);
        }
        else
        {
            bar2.value = maxBar2;
            bar3.value = maxBar3;
            bar4.value = maxBar4;
        }
    }
    private void UpdateMultiplierText()
    {
        multiplierText.text = "x" + currentMultiplier;
    }
}

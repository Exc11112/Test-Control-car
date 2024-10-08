using UnityEngine;
using UnityEngine.UI; // Include this namespace for UI elements

public class SpeedDisplay : MonoBehaviour
{
    public Rigidbody target;
    public float maxSpeed = 0.0f; // The maximum speed of the target ** IN KM/H **
    public CarController2 car;

    [Header("UI")]
    public Text speedLabel; // The label that displays the speed
    public Text rpmLabel;   // The label that displays the RPM
    public Text gearLabel;  // The label that displays the current gear
    public Text timers;
    public Text tstart;

    private float speed = 0.0f;
    private float tStartnew;
    private bool goDisplayed = false; // Track if "GO!" is currently displayed
    private float goDuration = 2.0f;  // Duration to show "GO!" in seconds
    private float goTimer = 0f;       // Timer to track "GO!" display time

    private void Start()
    {
        tStartnew = car.tStart; // Initialize tStartnew with car.tStart directly
    }

    private void Update()
    {
        // Reduce tStartnew over time if not displaying "GO!"
        if (!goDisplayed)
        {
            tStartnew -= Time.deltaTime;
        }

        // Display speed in km/h
        speed = target.velocity.magnitude * 3.6f;

        if (speedLabel != null)
            speedLabel.text = Mathf.FloorToInt(speed * 1.5f) + " km/h";

        if (rpmLabel != null && car != null)
            rpmLabel.text = Mathf.FloorToInt(car.currentRPM) + " RPM";

        if (gearLabel != null && car != null)
            gearLabel.text = "Gear: " + (car.currentGear + 1); // Display gears starting from 1

        if (timers != null)
            timers.text = Mathf.FloorToInt(car.timer) + " sec";

        // Update the countdown or display "GO!" text
        if (tstart != null)
        {
            if (tStartnew > 0)
            {
                // Show countdown until it reaches zero
                tstart.text = Mathf.Max(tStartnew, 0).ToString("F2") + "!";
            }
            else if (!goDisplayed)
            {
                // When countdown reaches zero, display "GO!" and start goTimer
                tstart.text = "GO!";
                goDisplayed = true; // Set flag to start "GO!" timer
            }
            else if (goDisplayed)
            {
                // Update "GO!" timer
                goTimer += Time.deltaTime;

                // Deactivate tstart text after "GO!" duration
                if (goTimer >= goDuration)
                {
                    tstart.gameObject.SetActive(false);
                }
            }
        }
    }
}

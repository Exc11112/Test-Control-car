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

    private float speed = 0.0f;

    private void Update()
    {
        // 3.6f to convert to kilometers per hour
        speed = target.velocity.magnitude * 3.6f;

        if (speedLabel != null)
            speedLabel.text = Mathf.FloorToInt(speed) + " km/h";

        if (rpmLabel != null && car != null)
            rpmLabel.text = Mathf.FloorToInt(car.currentRPM) + " RPM";

        if (gearLabel != null && car != null)
            gearLabel.text = "Gear: " + (car.currentGear + 1); // Add 1 to display gears starting from 1

        if (timers != null)
            timers.text = Mathf.FloorToInt(car.timer) + " sec";
    }
}
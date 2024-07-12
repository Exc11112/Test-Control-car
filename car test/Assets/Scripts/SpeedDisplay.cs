using UnityEngine;
using UnityEngine.UI; // Include this namespace for UI elements

public class SpeedDisplay : MonoBehaviour
{
    public Rigidbody target;
    public float maxSpeed = 0.0f; // The maximum speed of the target ** IN KM/H **

    [Header("UI")]
    public Text speedLabel; // The label that displays the speed

    private float speed = 0.0f;

    private void Update()
    {
        // 3.6f to convert to kilometers per hour
        // ** The speed must be clamped by the car controller **
        speed = target.velocity.magnitude * 3.6f;

        if (speedLabel != null)
            speedLabel.text = Mathf.FloorToInt(speed) + " km/h";
    }
}

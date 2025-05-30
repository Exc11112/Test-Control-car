using UnityEngine;
using UnityEngine.UI;

public class SpeedDisplay : MonoBehaviour
{
    public Rigidbody target;
    public float maxSpeed = 0.0f; // The maximum speed of the target ** IN KM/H **
    public CarController2 car;

    [Header("UI")]
    public Text speedLabel; // The label that displays the speed
    public Text rpmLabel;   // The label that displays the RPM
    public Text gearLabel;  // The label that displays the current gear
    public Text timers;     // The label that displays the timer
    public Text tstart;     // The label for the countdown/start text
    public RectTransform arrow; // The arrow in the speedometer

    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    private float speed = 0.0f;
    private bool goDisplayed = false; // Track if "GO!" is currently displayed
    private float goDuration = 2.0f;  // Duration to show "GO!" in seconds
    private float goTimer = 0f;       // Timer to track "GO!" display time

    public float countdownTime = 300f; // Countdown timer starting at 5 minutes
    private bool isTimerRunning = false;
    [System.NonSerialized] public bool gameEnded = false;
    private bool isNeutral = false; // Update this value based on your game logic
    private bool hasStartedTimer = false;
    private float localTStart;      // Local copy of car.tStart for countdown handling
    private int debugOPressCount = 0;
    private float debugOPressResetTime = 1.5f; // Reset if not pressed within 1.5 sec
    private float lastLPressTime = 0f;

    private void Start()
    {
        // Find the car dynamically if not set
        if (car == null)
        {
            car = FindObjectOfType<CarController2>();
        }

        if (car != null)
        {
            localTStart = car.tStart;
        }
        else
        {
            Debug.LogError("CarController2 reference is missing. Please assign it in the Inspector.");
        }

        // Find the target Rigidbody dynamically if not set
        if (target == null)
        {
            target = car?.GetComponent<Rigidbody>();
        }

        if (speedLabel == null || rpmLabel == null || gearLabel == null || tstart == null || timers == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the SpeedDisplay script.");
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (Time.time - lastLPressTime > debugOPressResetTime)
            {
                debugOPressCount = 0; // Reset if too much time passed between presses
            }

            debugOPressCount++;
            lastLPressTime = Time.time;

            if (debugOPressCount >= 3)
            {
                countdownTime = 1;
                debugOPressCount = 0; // Reset counter
            }
        }
        // Display speed in km/h
        speed = target.velocity.magnitude * 3.6f;

        if (speedLabel != null)
            speedLabel.text = Mathf.FloorToInt(speed * 1.5f) + "";

        if (rpmLabel != null && car != null)
            rpmLabel.text = Mathf.FloorToInt(car.currentRPM) + "";

        if (gearLabel != null && car != null)
            gearLabel.text = "" + (car.currentGear + 1); // Display gears starting from 1

        if (arrow != null)
            arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, car.currentRPM / car.maxRPM));

        // Countdown before "GO!" display
        if (tstart != null && !goDisplayed && localTStart > 0)
        {
            localTStart -= Time.deltaTime; // Reduce local copy
            tstart.text = localTStart.ToString("F2") + "!";
        }
        else if (tstart != null && !goDisplayed && localTStart <= 0)
        {
            tstart.text = "GO!";
            goDisplayed = true; // Set flag to start "GO!" timer
        }
        else if (goDisplayed)
        {
            goTimer += Time.deltaTime;
            if (goTimer >= goDuration)
            {
                tstart.gameObject.SetActive(false); // Hide "GO!" after duration
            }
        }

        // Timer logic for game countdown
        if (!isNeutral && !isTimerRunning && !hasStartedTimer && localTStart <= 0)
        {
            isTimerRunning = true;
            hasStartedTimer = true;
        }

        if (isTimerRunning && !gameEnded)
        {
            countdownTime -= Time.deltaTime;

            if (timers != null)
            {
                int minutes = Mathf.FloorToInt(countdownTime / 60f);
                int seconds = Mathf.FloorToInt(countdownTime % 60f);
                timers.text = $"{minutes:00}:{seconds:00}"; // Format the timer as MM:SS
            }

            if (countdownTime <= 0f)
            {
                countdownTime = 0f;
                isTimerRunning = false;
                gameEnded = true;
                OnGameOver();
            }
        }
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over! You Lose.");
        // Add any additional game-over logic here (e.g., UI display, stopping input).
    }
}
using UnityEngine;

public class CarController4Wa : MonoBehaviour
{
    public float moveInput;
    public float turnInput;
    public bool isCarGrounded;
    public float currentSpeed;
    public float currentTurnSpeed;
    public int currentGear { get; private set; }
    public float currentRPM { get; private set; }

    public float airDrag;
    public float groundDrag;

    public float maxFwdSpeed;
    public float maxRevSpeed;
    public float baseAcceleration;
    private float currentAcceleration;
    public float deceleration;
    public float brakeForce;

    public float turnSpeed;
    public float defaultTurnSpeed;
    public float highTurnSpeed;
    public float lowTurnSpeed;
    public float highTurnRadiusAt;
    public float lowTurnRadiusAt;
    public float driftThresholdSpeed;

    public float turnAcceleration;
    public float turnDeceleration;
    public float dForce;
    public float alignToGroundTime;
    public LayerMask groundLayer;

    public Rigidbody rb;

    public float pStartRpmSpeed;
    public float tStart;

    public float[] rpmDeceleration;
    public float[] gearRatios;
    public float shiftUpRPM;
    public float shiftDownRPM;
    public float maxRPM;
    public float minRPM;

    private float shiftDelay = 1f;
    private float lastShiftTime;

    public float maxRPMRateIncrease;

    private bool isManual = false;
    private bool isNeutral = true; // Start in Neutral gear
    private float neutralStartTime;

    private bool isBoosted = false; // Indicates if the temporary acceleration boost is active
    private float boostStartTime; // Time when the acceleration boost started
    private float gear1Acceleration; // Acceleration for gear 1
    private float gear1Deceleration; // Deceleration for gear 1

    private bool isCollidingWithWall = false;
    private float originalBaseAcceleration;

    public int maxCheckpoints = 5; // Example: 5 checkpoints
    private float[] checkpointTimes;
    private int checkpointIndex = 0;

    public float timer = 0f;
    public bool isTimerRunning = false;
    private float finishPointTime;

    public string checkpointLayer = "checkpoint";
    public string fpointLayer = "fpoint";
    private bool hasStartedTimer = false; // Flag to ensure the timer starts only once

    private bool isEnhancedTurning = false;
    private float enhancedTurnStartTime;
    private bool isSKeyPressed = false;
    private float sKeyPressTime;
    private float maxTimeBetweenSAndW = 1.0f; // Maximum time allowed between pressing S and W
    private float totalSpeedAtCheckpoints = 0f; // Stores the sum of speeds at all checkpoints

    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        checkpointTimes = new float[maxCheckpoints];
        currentGear = 1;
        currentRPM = 0;
        originalBaseAcceleration = baseAcceleration;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Handle gear shifting
        HandleGearShift();

        // Handle car movement
        HandleMovement();

        // Handle turning
        HandleTurning();

        // Update wheel positions
        UpdateWheelPositions();
    }

    void FixedUpdate()
    {
        ApplyDrive();
        ApplySteering();
        AlignWheelsToGround();
    }

    private void ApplyDrive()
    {
        float motorTorque = currentAcceleration * moveInput * 1000f;
        foreach (var wheel in wheelColliders)
        {
            wheel.motorTorque = motorTorque;
        }
    }

    private void ApplySteering()
    {
        float steeringAngle = currentTurnSpeed * turnInput;
        wheelColliders[0].steerAngle = steeringAngle;
        wheelColliders[1].steerAngle = steeringAngle;
    }

    private void HandleGearShift()
    {
        if (Time.time - lastShiftTime > shiftDelay)
        {
            if (currentRPM >= shiftUpRPM && currentGear < gearRatios.Length)
            {
                ShiftUp();
            }
            else if (currentRPM <= shiftDownRPM && currentGear > 1)
            {
                ShiftDown();
            }
        }

        // RPM calculation based on speed and gear ratios
        currentRPM = Mathf.Clamp(rb.velocity.magnitude * gearRatios[currentGear - 1], minRPM, maxRPM);
    }

    private void ShiftUp()
    {
        currentGear++;
        lastShiftTime = Time.time;
    }

    private void ShiftDown()
    {
        currentGear--;
        lastShiftTime = Time.time;
    }

    private void HandleMovement()
    {
        currentSpeed = rb.velocity.magnitude * 3.6f; // Convert to km/h
        currentAcceleration = Mathf.Lerp(currentAcceleration, baseAcceleration, Time.deltaTime * deceleration);
    }

    private void HandleTurning()
    {
        float speedFactor = Mathf.InverseLerp(lowTurnRadiusAt, highTurnRadiusAt, currentSpeed);
        currentTurnSpeed = Mathf.Lerp(lowTurnSpeed, highTurnSpeed, speedFactor);

        if (moveInput > 0 && isSKeyPressed && (Time.time - sKeyPressTime) <= maxTimeBetweenSAndW)
        {
            if (!isEnhancedTurning)
            {
                enhancedTurnStartTime = Time.time;
                isEnhancedTurning = true;
            }
        }

        if (isEnhancedTurning)
        {
            currentTurnSpeed = highTurnSpeed;
            if (Time.time - enhancedTurnStartTime > 2f)
            {
                isEnhancedTurning = false;
            }
        }
    }

    private void AlignWheelsToGround()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(wheelColliders[i].transform.position, -wheelColliders[i].transform.up, out hit, wheelColliders[i].radius + wheelColliders[i].suspensionDistance, groundLayer))
            {
                isCarGrounded = true;
                wheelTransforms[i].position = hit.point + wheelColliders[i].transform.up * wheelColliders[i].radius;
            }
            else
            {
                isCarGrounded = false;
            }
        }
    }

    private void UpdateWheelPositions()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            UpdateWheelPosition(wheelColliders[i], wheelTransforms[i]);
        }
    }

    private void UpdateWheelPosition(WheelCollider collider, Transform transform)
    {
        Vector3 pos;
        Quaternion quat;
        collider.GetWorldPose(out pos, out quat);
        transform.position = pos;
        transform.rotation = quat;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            isCollidingWithWall = true;
            baseAcceleration = 5f;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            isCollidingWithWall = false;
            baseAcceleration = originalBaseAcceleration;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(checkpointLayer))
        {
            if (!hasStartedTimer)
            {
                timer = 0f;
                isTimerRunning = true;
                hasStartedTimer = true;
            }
            if (checkpointIndex < maxCheckpoints)
            {
                checkpointTimes[checkpointIndex] = timer;
                totalSpeedAtCheckpoints += currentSpeed;
                checkpointIndex++;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            if (isTimerRunning)
            {
                finishPointTime = timer;
                isTimerRunning = false;
                DisplayFinalTime();
            }
        }
    }

    private void DisplayFinalTime()
    {
        Debug.Log($"Final Time: {finishPointTime} seconds");
        Debug.Log($"Total Speed at Checkpoints: {totalSpeedAtCheckpoints} km/h");
    }
}

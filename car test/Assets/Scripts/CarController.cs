using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float moveInput;
    private float turnInput;
    private bool isCarGrounded;
    private float currentSpeed;
    private float currentTurnSpeed;
    public int currentGear { get; private set; }
    public float currentRPM { get; private set; }

    public float airDrag;
    public float groundDrag;
    public float DiftDrag;

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

    public Rigidbody sphereRB;

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

    public float suspensionStrength = 1000f; // How strong the suspension is
    public float suspensionDamping = 100f;   // Damping factor to reduce oscillation
    public float suspensionLength = 0.5f;    // Length of the suspension (distance to ground)

    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;


    void Start()
    {
        // Detach the sphere from the car object
        sphereRB.transform.parent = null;
        currentSpeed = 0f;
        currentTurnSpeed = 0f;
        currentGear = 0; // Start at first gear
        currentRPM = minRPM;
        lastShiftTime = Time.time;

        // Initialize acceleration and deceleration for gear 1
        gear1Acceleration = baseAcceleration * gearRatios[0];
        gear1Deceleration = deceleration;

        // Store the original base acceleration
        originalBaseAcceleration = baseAcceleration;

        // Start in neutral for 3 seconds
        isNeutral = true;
        neutralStartTime = Time.time;

        checkpointTimes = new float[maxCheckpoints];
    }

    void Update()
    {
        // Toggle manual mode with 'R' key
        if (Input.GetKeyDown(KeyCode.R))
        {
            isManual = !isManual;
        }

        // Update wheel rotations based on the car's current speed
        UpdateWheelRotations();

        // Update the turning of the front wheels
        UpdateFrontWheelTurning();

        // Get input for movement and turning
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        // Timer management
        if (!isNeutral && !isTimerRunning && !hasStartedTimer && Time.time - neutralStartTime > tStart)
        {
            isTimerRunning = true;
            timer = 0f;
            hasStartedTimer = true; // Set the flag to true so this block won't run again
        }
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
        }

        if (isNeutral)
        {
            // In Neutral state, only RPM can go up to max 10000 without affecting speed
            if (moveInput > 0)
            {
                currentRPM += gear1Acceleration * Time.deltaTime * pStartRpmSpeed; // Increase RPM faster in neutral
                currentRPM = Mathf.Clamp(currentRPM, minRPM, 10000);
            }
            else
            {
                currentRPM -= gear1Deceleration * Time.deltaTime * pStartRpmSpeed;
                currentRPM = Mathf.Max(currentRPM, minRPM);
            }

            // Check if 3 seconds have passed to switch to normal state
            if (Time.time - neutralStartTime > tStart)
            {
                isNeutral = false;

                // Check if the player kept RPM between 4000 and 5000 during neutral state
                if (currentRPM >= 4000 && currentRPM <= 5000)
                {
                    isBoosted = true;
                    boostStartTime = Time.time;
                }
            }
        }
        else
        {
            // Calculate current RPM based on speed and gear ratios
            float targetRPM = Mathf.Abs(currentSpeed) / maxFwdSpeed * maxRPM * gearRatios[currentGear];

            // Cap the RPM to the maxRPM
            currentRPM = Mathf.Min(targetRPM, maxRPM);

            // Prevent speed increase if RPM is at its maximum
            bool isAtMaxRPM = currentRPM >= maxRPM;

            // Gradually adjust current speed based on moveInput and RPM cap
            if (moveInput > 0 && !isAtMaxRPM)
            {
                currentSpeed += currentAcceleration * Time.deltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, -maxRevSpeed, maxFwdSpeed);
            }
            else if (moveInput < 0)
            {
                currentSpeed -= brakeForce * Time.deltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, -maxRevSpeed, maxFwdSpeed);
            }
            else
            {
                // Decelerate if no input is provided
                if (currentSpeed > 0)
                {
                    currentSpeed -= deceleration * Time.deltaTime;
                    if (currentSpeed < 0) currentSpeed = 0;
                }
                else if (currentSpeed < 0)
                {
                    currentSpeed += deceleration * Time.deltaTime;
                    if (currentSpeed > 0) currentSpeed = 0;
                }
            }

            // Check if the temporary acceleration boost should be active
            if (isBoosted)
            {
                if (Time.time - boostStartTime < 3f)
                {
                    // Double the acceleration during the boost period
                    currentAcceleration = gear1Acceleration * 0.6f;
                }
                else
                {
                    // End the boost period
                    isBoosted = false;
                    AdjustAcceleration();
                }
            }

            // Adjust turnSpeed based on currentSpeed
            float targetTurnSpeed = defaultTurnSpeed;

            if (currentSpeed > lowTurnRadiusAt)
            {
                targetTurnSpeed = lowTurnSpeed;
            }
            else if (currentSpeed < highTurnRadiusAt)
            {
                targetTurnSpeed = highTurnSpeed;
            }

            // Lerp to the target turn speed
            turnSpeed = Mathf.Lerp(turnSpeed, targetTurnSpeed, Time.deltaTime * 2f);

            // Gradually adjust current turn speed based on turn input
            if (turnInput != 0)
            {
                currentRPM -= rpmDeceleration[0] * Time.deltaTime * 10000f;
                currentRPM = Mathf.Clamp(currentRPM, 0, maxRPM);
                currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
                currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -turnSpeed, turnSpeed);
                currentSpeed -= currentSpeed * 0.5f * Time.deltaTime;
            }
            else
            {
                // Decelerate turn speed if no input is provided
                if (currentTurnSpeed > 0)
                {
                    currentTurnSpeed -= turnDeceleration * Time.deltaTime;
                    if (currentTurnSpeed < 0) currentTurnSpeed = 0;
                }
                else if (currentTurnSpeed < 0)
                {
                    currentTurnSpeed += turnDeceleration * Time.deltaTime;
                    if (currentTurnSpeed > 0) currentTurnSpeed = 0;
                }
            }

            // Update car position to match the sphere's position
            transform.position = sphereRB.transform.position;

            if (isCarGrounded)
            {
                // Rotate the car based on current turn speed
                float newRotation = currentTurnSpeed * Time.deltaTime * (currentSpeed / maxFwdSpeed);
                transform.Rotate(0, newRotation, 0, Space.World);
            }

            // Check if the car is grounded
            RaycastHit hit;
            isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

            // Align the car with the ground normal
            if (isCarGrounded)
            {
                Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);
            }

            // Adjust drag based on whether the car is grounded
            sphereRB.drag = isCarGrounded ? groundDrag : airDrag;

            if (!isManual)
            {
                // Automatic gear shifting logic with time delay
                if (Time.time - lastShiftTime > shiftDelay)
                {
                    if (currentRPM > shiftUpRPM && currentGear < gearRatios.Length - 1)
                    {
                        ShiftUp();
                    }
                    else if (currentRPM < shiftDownRPM && currentGear > 0)
                    {
                        ShiftDown();
                    }
                }
            }
            else
            {
                // Manual gear shifting logic
                if (Input.GetKeyDown(KeyCode.LeftShift) && currentGear < gearRatios.Length - 1)
                {
                    ShiftUp();
                }
                else if (Input.GetKeyDown(KeyCode.LeftControl) && currentGear > 0)
                {
                    ShiftDown();
                }
            }
        }

        // Adjust current acceleration based on the current gear if not boosted
        if (!isBoosted)
        {
            AdjustAcceleration();
        }

        // Check if S is pressed and the current speed is above 100 km/h
        if (!isEnhancedTurning && currentSpeed > 100f && Input.GetKeyDown(KeyCode.S))
        {
            isSKeyPressed = true;
            sKeyPressTime = Time.time;
        }

        // Check if W is pressed within the allowed time after S
        if (!isEnhancedTurning && isSKeyPressed && Time.time - sKeyPressTime <= maxTimeBetweenSAndW && Input.GetKeyDown(KeyCode.W))
        {
            isEnhancedTurning = true;
            enhancedTurnStartTime = Time.time;

            // Double the turn-related parameters
            lowTurnSpeed *= 2;
            turnAcceleration *= 2;
            turnDeceleration *= 2;
            sphereRB.drag = groundDrag = 1.5f;
            currentSpeed = Mathf.Max(5f, currentSpeed - Time.deltaTime * 10000f);
            Debug.Log(sphereRB.drag);

            isSKeyPressed = false; // Reset after successful sequence
        }

        // Reset the S key flag if W isn't pressed within the allowed time
        if (isSKeyPressed && Time.time - sKeyPressTime > maxTimeBetweenSAndW)
        {
            isSKeyPressed = false;
        }

        // Handle enhanced turning behavior
        if (isEnhancedTurning)
        {
            // Check if the enhanced turning has been active for more than 1.5 seconds
            if (Time.time - enhancedTurnStartTime > 1.5f)
            {
                // Reset the turn-related parameters to their original values
                lowTurnSpeed /= 2;
                turnAcceleration /= 2;
                turnDeceleration /= 2;
                sphereRB.drag = groundDrag = 3f;
                Debug.Log(sphereRB.drag);
                isEnhancedTurning = false;
            }
            //else if (turnInput != 0)
            //{
            //    // Reset the timer if there is turn input
            //    enhancedTurnStartTime = Time.time;
            //}
        }
    }

    private void FixedUpdate()
    {
        if (isCarGrounded)
        {
            // Apply forward force when grounded
            sphereRB.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);

            // Calculate suspension force
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, suspensionLength, groundLayer))
            {
                // Calculate how much the suspension is compressed
                float compressionRatio = 1.0f - (hit.distance / suspensionLength);

                // Apply suspension force
                Vector3 suspensionForce = transform.up * compressionRatio * suspensionStrength;
                sphereRB.AddForce(suspensionForce, ForceMode.Acceleration);

                // Apply damping to prevent oscillation
                Vector3 suspensionVelocity = Vector3.Project(sphereRB.velocity, transform.up);
                Vector3 dampingForce = -suspensionVelocity * suspensionDamping;
                sphereRB.AddForce(dampingForce, ForceMode.Acceleration);
            }
        }
        else
        {
            // Apply downward force when in the air
            sphereRB.AddForce(transform.up * dForce);
        }
    }

    private void ShiftUp()
    {
        currentGear++;
        lastShiftTime = Time.time;
        AdjustAcceleration();
        currentRPM = Mathf.Clamp(currentRPM / gearRatios[currentGear], minRPM, maxRPM);
    }

    private void ShiftDown()
    {
        currentGear--;
        lastShiftTime = Time.time;
        AdjustAcceleration();
        currentRPM = Mathf.Clamp(currentRPM * gearRatios[currentGear], minRPM, maxRPM);
    }

    private void AdjustAcceleration()
    {
        // Continuously reduce the current speed to 5 if colliding with a wall
        if (isCollidingWithWall)
        {
            Debug.Log("wwwwwww");
            currentSpeed = Mathf.Max(5f, currentSpeed - Time.deltaTime * 50f);
            moveInput = 0f;
            //currentRPM = 0f;
        }
        //else
        //{
        //    currentSpeed = Mathf.Lerp(currentSpeed, originalBaseAcceleration, Time.deltaTime * 2f);
        //}

        currentAcceleration = baseAcceleration * gearRatios[currentGear] * rpmDeceleration[currentGear];
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Check if the car collides with a wall
        if (collision.gameObject.CompareTag("wall"))
        {
            Debug.Log("asd");
            isCollidingWithWall = true;
        }

        // Check if the car hits a checkpoint
        if (collision.gameObject.layer == LayerMask.NameToLayer(checkpointLayer))
        {
            if (checkpointIndex < maxCheckpoints)
            {
                checkpointTimes[checkpointIndex] = timer;
                Debug.Log("Checkpoint " + (checkpointIndex + 1) + " Time: " + checkpointTimes[checkpointIndex]);

                float currentSpeedAtCheckpoint = currentSpeed; // Get the current speed at this checkpoint
                totalSpeedAtCheckpoints += currentSpeedAtCheckpoint; // Add it to the total speed
                Debug.Log("Speed at Checkpoint " + (checkpointIndex + 1) + ": " + currentSpeedAtCheckpoint + " km/h");
                Debug.Log("Total Speed after Checkpoint " + (checkpointIndex + 1) + ": " + totalSpeedAtCheckpoints + " km/h");

                DeactivateObject(collision.gameObject); // Deactivate the specific checkpoint
                checkpointIndex++; // Move to the next checkpoint
            }
            else
            {
                Debug.LogWarning("All checkpoints have been hit already.");
            }
        }

        // Check if the car hits the finish point
        if (collision.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            finishPointTime = timer;
            Debug.Log("Finish Point Time: " + finishPointTime);
            Debug.Log("Grand Total Speed at all Checkpoints: " + totalSpeedAtCheckpoints + " km/h");
            isTimerRunning = false; // Stop the timer
            DeactivateObjectsInLayer(fpointLayer);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        // Check if the car stops colliding with a wall
        if (collision.gameObject.CompareTag("wall"))
        {
            isCollidingWithWall = false;
        }
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

    private void DeactivateObject(GameObject obj)
    {
        obj.SetActive(false);
    }
    private void UpdateWheelRotations()
    {
        // Calculate the rotation angle for the wheels based on the current speed
        float rotationAngle = currentSpeed * Time.deltaTime * 360f / (2 * Mathf.PI * 0.5f); // Assuming wheel radius of 0.5 units

        // Rotate the rear wheels (no turning, just rotation)
        rearLeftWheel.Rotate(Vector3.right, rotationAngle);
        rearRightWheel.Rotate(Vector3.right, rotationAngle);

        // Store the current rotation of the front wheels
        Quaternion leftRotation = frontLeftWheel.localRotation;
        Quaternion rightRotation = frontRightWheel.localRotation;

        // Rotate the front wheels for spinning (without affecting Y-axis rotation)
        frontLeftWheel.localRotation = leftRotation * Quaternion.Euler(rotationAngle, 0, 0);
        frontRightWheel.localRotation = rightRotation * Quaternion.Euler(rotationAngle, 0, 0);
    }

    private void UpdateFrontWheelTurning()
    {
        // Calculate the desired turning angle based on the current turn input
        float turnAngle = turnInput * turnSpeed;

        // Store the current rotation of the front wheels
        Quaternion leftRotation = frontLeftWheel.localRotation;
        Quaternion rightRotation = frontRightWheel.localRotation;

        // Apply the turn to the front wheels (preserving the X-axis rotation)
        frontLeftWheel.localRotation = Quaternion.Euler(leftRotation.eulerAngles.x, turnAngle, 0);
        frontRightWheel.localRotation = Quaternion.Euler(rightRotation.eulerAngles.x, turnAngle, 0);
    }
}
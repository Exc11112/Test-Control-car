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

    public float turnAcceleration;
    public float turnDeceleration;
    public float dForce;
    public LayerMask groundLayer;

    public Rigidbody sphereRB;

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

        // Start in neutral for 3 seconds
        isNeutral = true;
        neutralStartTime = Time.time;
    }

    void Update()
    {
        // Toggle manual mode with 'R' key
        if (Input.GetKeyDown(KeyCode.R))
        {
            isManual = !isManual;
        }

        // Get input for movement and turning
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        if (isNeutral)
        {
            // In Neutral state, only RPM can go up to max 10000 without affecting speed
            if (moveInput > 0)
            {
                currentRPM += gear1Acceleration * Time.deltaTime * 25; // Increase RPM faster in neutral
                currentRPM = Mathf.Clamp(currentRPM, minRPM, 10000);
            }
            else
            {
                currentRPM -= gear1Deceleration * Time.deltaTime * 25;
                currentRPM = Mathf.Max(currentRPM, minRPM);
            }

            // Check if 3 seconds have passed to switch to normal state
            if (Time.time - neutralStartTime > 3f)
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
                currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
                currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -turnSpeed, turnSpeed);
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
                transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
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
    }

    private void FixedUpdate()
    {
        if (isCarGrounded)
        {
            // Apply forward force when grounded
            sphereRB.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
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
        currentAcceleration = baseAcceleration * gearRatios[currentGear] * rpmDeceleration[currentGear];
    }
}
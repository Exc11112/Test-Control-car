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

    public float[] gearRatios; // Gear ratios
    public float shiftUpRPM; // RPM to shift up
    public float shiftDownRPM; // RPM to shift down
    public float maxRPM; // Maximum RPM
    public float minRPM; // Minimum RPM

    private float shiftDelay = 0.5f; // Time delay between shifts to avoid rapid shifting
    private float lastShiftTime; // Time of the last shift

    void Start()
    {
        // Detach the sphere from the car object
        sphereRB.transform.parent = null;
        currentSpeed = 0f;
        currentTurnSpeed = 0f;
        currentGear = 0; // Start at first gear
        currentRPM = minRPM;
        lastShiftTime = Time.time;
        currentAcceleration = baseAcceleration * gearRatios[currentGear];
    }

    void Update()
    {
        // Get input for movement and turning
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        // Calculate current RPM based on speed and gear ratios
        currentRPM = Mathf.Abs(currentSpeed) / maxFwdSpeed * maxRPM * gearRatios[currentGear];

        // Gear shifting logic with time delay
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

        // Adjust current acceleration based on the current gear
        AdjustAcceleration();

        // Gradually adjust current speed based on moveInput
        if (moveInput > 0)
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
        // Adjust the base acceleration for higher gears
        if (currentGear >= 4) // Assuming gear 5 is index 4
        {
            currentAcceleration = baseAcceleration * gearRatios[currentGear] * 0.1f; // Decrease acceleration
        }
        else
        {
            currentAcceleration = baseAcceleration * gearRatios[currentGear];
        }
    }
}
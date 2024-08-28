using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController2 : MonoBehaviour
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
    public float driftDrag;

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

    public Rigidbody carRigidbody;

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
    private bool isNeutral = true;
    private float neutralStartTime;

    private bool isBoosted = false;
    private float boostStartTime;
    private float gear1Acceleration;
    private float gear1Deceleration;

    private bool isCollidingWithWall = false;
    private float originalBaseAcceleration;

    public int maxCheckpoints = 5;
    private float[] checkpointTimes;
    private int checkpointIndex = 0;

    public float timer = 0f;
    public bool isTimerRunning = false;
    private float finishPointTime;

    public string checkpointLayer = "checkpoint";
    public string fpointLayer = "fpoint";
    private bool hasStartedTimer = false;

    private bool isEnhancedTurning = false;
    private float enhancedTurnStartTime;
    private bool isSKeyPressed = false;
    private float sKeyPressTime;
    private float maxTimeBetweenSAndW = 1.0f;
    private float totalSpeedAtCheckpoints = 0f;

    public float suspensionStrength = 1000f;
    public float suspensionDamping = 100f;
    public float suspensionLength = 0.5f;

    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0); // »ÃÑº¨Ø´ÈÙ¹Âì¶èÇ§¢Í§Ã¶
        currentSpeed = 0f;
        currentTurnSpeed = 0f;
        currentGear = 0;
        currentRPM = minRPM;
        lastShiftTime = Time.time;

        gear1Acceleration = baseAcceleration * gearRatios[0];
        gear1Deceleration = deceleration;

        originalBaseAcceleration = baseAcceleration;

        isNeutral = true;
        neutralStartTime = Time.time;

        checkpointTimes = new float[maxCheckpoints];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isManual = !isManual;
        }

        UpdateWheelRotations();
        UpdateFrontWheelTurning();

        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        if (!isNeutral && !isTimerRunning && !hasStartedTimer && Time.time - neutralStartTime > tStart)
        {
            isTimerRunning = true;
            timer = 0f;
            hasStartedTimer = true;
        }
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
        }

        if (isNeutral)
        {
            if (moveInput > 0)
            {
                currentRPM += gear1Acceleration * Time.deltaTime * pStartRpmSpeed;
                currentRPM = Mathf.Clamp(currentRPM, minRPM, 10000);
            }
            else
            {
                currentRPM -= gear1Deceleration * Time.deltaTime * pStartRpmSpeed;
                currentRPM = Mathf.Max(currentRPM, minRPM);
            }

            if (Time.time - neutralStartTime > tStart)
            {
                isNeutral = false;

                if (currentRPM >= 4000 && currentRPM <= 5000)
                {
                    isBoosted = true;
                    boostStartTime = Time.time;
                }
            }
        }
        else
        {
            float targetRPM = Mathf.Abs(currentSpeed) / maxFwdSpeed * maxRPM * gearRatios[currentGear];
            currentRPM = Mathf.Min(targetRPM, maxRPM);
            bool isAtMaxRPM = currentRPM >= maxRPM;

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

            if (isBoosted)
            {
                if (Time.time - boostStartTime < 3f)
                {
                    currentAcceleration = gear1Acceleration * 0.6f;
                }
                else
                {
                    isBoosted = false;
                    AdjustAcceleration();
                }
            }

            float targetTurnSpeed = defaultTurnSpeed;

            if (currentSpeed > lowTurnRadiusAt)
            {
                targetTurnSpeed = lowTurnSpeed;
            }
            else if (currentSpeed < highTurnRadiusAt)
            {
                targetTurnSpeed = highTurnSpeed;
            }

            turnSpeed = Mathf.Lerp(turnSpeed, targetTurnSpeed, Time.deltaTime * 2f);

            if (turnInput != 0)
            {
                currentRPM -= rpmDeceleration[0] * Time.deltaTime * 10000f;
                currentRPM = Mathf.Clamp(currentRPM, 0, maxRPM);
                currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
                currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -turnSpeed, turnSpeed);
                currentSpeed -= currentSpeed * 0.2f * Time.deltaTime;
            }
            else
            {
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

            if (isCarGrounded)
            {
                float newRotation = currentTurnSpeed * Time.deltaTime * (currentSpeed / maxFwdSpeed);
                transform.Rotate(0, newRotation, 0, Space.World);
            }

            RaycastHit hit;
            isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

            if (isCarGrounded)
            {
                Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);
            }

            carRigidbody.drag = isCarGrounded ? groundDrag : airDrag;

            if (!isManual)
            {
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

        if (!isBoosted)
        {
            AdjustAcceleration();
        }

        if (!isEnhancedTurning && currentSpeed > 100f && Input.GetKeyDown(KeyCode.S))
        {
            isSKeyPressed = true;
            sKeyPressTime = Time.time;
        }

        if (!isEnhancedTurning && isSKeyPressed && Time.time - sKeyPressTime <= maxTimeBetweenSAndW && Input.GetKeyDown(KeyCode.W))
        {
            isEnhancedTurning = true;
            enhancedTurnStartTime = Time.time;

            lowTurnSpeed *= 2;
            turnAcceleration *= 2;
            carRigidbody.drag = groundDrag = 2f;
            currentSpeed = Mathf.Max(5f, currentSpeed - Time.deltaTime * 700f);

            isSKeyPressed = false;
        }

        if (isSKeyPressed && Time.time - sKeyPressTime > maxTimeBetweenSAndW)
        {
            isSKeyPressed = false;
        }

        if (isEnhancedTurning)
        {
            if (turnInput == 0 && Time.time - enhancedTurnStartTime > 0.5f)
            {
                lowTurnSpeed /= 2;
                turnAcceleration /= 2;
                carRigidbody.drag = groundDrag = 3f;
                isEnhancedTurning = false;
            }
            else if (turnInput != 0)
            {
                enhancedTurnStartTime = Time.time;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isCarGrounded)
        {
            carRigidbody.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
        }
        else
        {
            carRigidbody.AddForce(transform.up * dForce);
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
        if (isCollidingWithWall)
        {
            currentSpeed = Mathf.Max(5f, currentSpeed - Time.deltaTime * 50f);
            moveInput = 0f;
        }

        currentAcceleration = baseAcceleration * gearRatios[currentGear] * rpmDeceleration[currentGear];
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            isCollidingWithWall = true;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(checkpointLayer))
        {
            if (checkpointIndex < maxCheckpoints)
            {
                checkpointTimes[checkpointIndex] = timer;
                float currentSpeedAtCheckpoint = currentSpeed;
                totalSpeedAtCheckpoints += currentSpeedAtCheckpoint;

                DeactivateObject(collision.gameObject);
                checkpointIndex++;
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            finishPointTime = timer;
            isTimerRunning = false;
            DeactivateObjectsInLayer(fpointLayer);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
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
        float rotationAngle = currentSpeed * Time.deltaTime * 360f / (2 * Mathf.PI * 0.5f);

        rearLeftWheel.Rotate(Vector3.right, rotationAngle);
        rearRightWheel.Rotate(Vector3.right, rotationAngle);

        Quaternion leftRotation = frontLeftWheel.localRotation;
        Quaternion rightRotation = frontRightWheel.localRotation;

        frontLeftWheel.localRotation = leftRotation * Quaternion.Euler(rotationAngle, 0, 0);
        frontRightWheel.localRotation = rightRotation * Quaternion.Euler(rotationAngle, 0, 0);
    }

    private void UpdateFrontWheelTurning()
    {
        float turnAngle = turnInput * turnSpeed;

        Quaternion leftRotation = frontLeftWheel.localRotation;
        Quaternion rightRotation = frontRightWheel.localRotation;

        frontLeftWheel.localRotation = Quaternion.Euler(leftRotation.eulerAngles.x, turnAngle, 0);
        frontRightWheel.localRotation = Quaternion.Euler(rightRotation.eulerAngles.x, turnAngle, 0);
    }
}

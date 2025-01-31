using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController2 : MonoBehaviour
{
    private float moveInput;
    public float turnInput;
    private bool isCarGrounded;
    public float currentSpeed;
    private float currentTurnSpeed;
    public int currentGear { get; private set; }
    public float currentRPM { get; private set; }

    public float airDrag;
    public float groundDrag;

    //public float driftLateralFriction = 0.5f; // Friction when drifting
    public float normalLateralFriction = 1.0f; // Normal friction for tires
    //public float oversteerMultiplier = 1.2f; // Multiplier for oversteer effect
    //public float understeerMultiplier = 0.8f; // Multiplier for understeer effect
    public float maxSteerAngle = 30f; // Maximum steering angle for wheels
    public float driftSteerAngle = 45f; // Steering angle during drift
    public bool isDrifting = false;
    public float steerAngle;
    public float driftSteerAngleMultiplier;

    public float maxFwdSpeed;
    public float maxRevSpeed;
    public float baseAcceleration;
    private float currentAcceleration;
    public float deceleration;
    public float brakeForce;

    public float turnSpeed;
    public float defaultTurnSpeed;
    //public float highTurnSpeed;
    //public float lowTurnSpeed;
    //public float highTurnRadiusAt;
    //public float lowTurnRadiusAt;
    public float driftThresholdSpeed;
    private float lastTurnInputTime;
    private float turnResetDelay = 0.2f;

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

    //public float maxRPMRateIncrease;

    private bool isManual = false;
    private bool isNeutral = true;
    private float neutralStartTime;

    private bool isBoosted = false;
    private float boostStartTime;
    private float gear1Acceleration;
    private float gear1Deceleration;

    //private bool isCollidingWithWall = false;
    private float originalBaseAcceleration;

    public int maxCheckpoints = 5;
    private float[] checkpointTimes;
    private int checkpointIndex = 0;

    public float timer = 0f;
    public bool isTimerRunning = false;
    private float finishPointTime;
    private float countdownTime = 300f; // 5 minutes in seconds
    private bool gameEnded = false;

    public string checkpointLayer = "checkpoint";
    public string fpointLayer = "fpoint";
    //private bool hasStartedTimer = false;

    private bool isEnhancedTurning = false;

    private bool isSKeyPressed = false;
    private float sKeyPressTime;
    private float maxTimeBetweenSAndW = 1.0f;
    private float totalSpeedAtCheckpoints = 0f;

    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    public enum DriveMode { RearWheelDrive, FrontWheelDrive, FourWheelDrive }
    public DriveMode driveMode; // Selectable in Inspector

    public Transform[] frontRayOrigins;
    public Transform[] backRayOrigins;
    public Transform[] RightRayOrigins;
    public Transform[] LeftRayOrigins;
    public Animator carAnimator;

    private bool isPlayingRightAnim = false;
    private bool isPlayingLeftAnim = false;


    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0); // Adjust car's center of mass
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
        currentAcceleration = baseAcceleration * gearRatios[currentGear];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isManual = !isManual;
        }

        UpdateWheelRotations();
        UpdateFrontWheelTurning();
        HandleTurning();  // Handle turning based on wheel colliders

        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

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

        if (!isEnhancedTurning && currentSpeed > driftThresholdSpeed && Input.GetKeyDown(KeyCode.S))
        {
            isSKeyPressed = true;
            sKeyPressTime = Time.time;
        }

        if (isSKeyPressed && Input.GetKeyDown(KeyCode.W) && (Time.time - sKeyPressTime) <= maxTimeBetweenSAndW)
        {
            isEnhancedTurning = true;
            isSKeyPressed = false;
        }

        if (isEnhancedTurning)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                isDrifting = true;
                lastTurnInputTime = Time.time;
                //turnSpeed *= 1.2f;
                //currentAcceleration *= 1.2f;
            }
            else
            {
                if (Time.time - lastTurnInputTime >= turnResetDelay)
                {
                    isDrifting = false;
                    isEnhancedTurning = false;
                    AdjustAcceleration();
                }
            }
        }

        float raycastDistance = 0.3f;
        LayerMask wallLayer = LayerMask.GetMask("wall");

        foreach (Transform frontRay in frontRayOrigins)
        {
            Debug.DrawRay(frontRay.position, frontRay.forward * raycastDistance, Color.red);

            if (Physics.Raycast(frontRay.position, frontRay.forward, out RaycastHit frontHit, raycastDistance, wallLayer))
            {
                HandleWallCollision(frontHit.normal);
                break;
            }
        }

        // Rear collision detection (3 rays)
        foreach (Transform backRay in backRayOrigins)
        {
            Debug.DrawRay(backRay.position, -backRay.forward * raycastDistance, Color.blue);

            if (Physics.Raycast(backRay.position, -backRay.forward, out RaycastHit backHit, raycastDistance, wallLayer))
            {
                HandleWallCollision(backHit.normal);
                break;
            }
        }

        void HandleWallCollision(Vector3 collisionNormal)
        {
            Vector3 incomingVelocity = carRigidbody.velocity;

            // Perfect mirror reflection with velocity preservation
            if (Vector3.Dot(incomingVelocity.normalized, collisionNormal) < 0)
            {
                Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, collisionNormal) * 0.7f;
                carRigidbody.velocity = reflectedVelocity;

                // Add rotational force for more realistic bounce
                Vector3 torque = Vector3.Cross(collisionNormal, incomingVelocity.normalized) * 50f;
                carRigidbody.AddTorque(torque, ForceMode.Impulse);
            }
        }

        HandleDrifting();
        HandleOversteerUndersteer();
        AdjustAcceleration();
        HandleRaycasts();
    }

    private void FixedUpdate()
    {
        if (isCarGrounded)
        {
            // Apply motor torque based on selected drive mode
            switch (driveMode)
            {
                case DriveMode.RearWheelDrive:
                    ApplyRearWheelDrive();
                    break;

                case DriveMode.FrontWheelDrive:
                    ApplyFrontWheelDrive();
                    break;

                case DriveMode.FourWheelDrive:
                    ApplyFourWheelDrive();
                    break;
            }

            // Apply the steer angle to the front wheels
            frontLeftWheelCollider.steerAngle = steerAngle;
            frontRightWheelCollider.steerAngle = steerAngle;

            // Apply forward force to the car based on the rear-wheel drive
            carRigidbody.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
        }
        else
        {
            carRigidbody.AddForce(transform.up * dForce);
        }

        if (isDrifting && isCarGrounded)
        {
            float driftDirection = Input.GetAxis("Horizontal");

            Vector3 driftForce = -transform.right * (driftDirection * currentSpeed * 0.1f);
            carRigidbody.AddForce(driftForce, ForceMode.Acceleration);
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
    }

    void HandleRaycasts()
    {
        float raycastDistance = 0.3f;
        LayerMask wallLayer = LayerMask.GetMask("wall");
        bool rightHit = false;
        bool leftHit = false;
        Debug.Log("Wall LayerMask value: " + wallLayer.value);

        foreach (Transform rightRay in RightRayOrigins)
        {
            Vector3 direction = rightRay.forward;
            Debug.DrawRay(rightRay.position, direction * raycastDistance, Color.green);

            if (Physics.Raycast(rightRay.position, direction, out RaycastHit hit, raycastDistance, wallLayer))
            {
                Debug.Log("hitr - Hit " + hit.collider.name);
                rightHit = true;
                break;
            }
        }

        foreach (Transform leftRay in LeftRayOrigins)
        {
            Vector3 direction = leftRay.forward;
            Debug.DrawRay(leftRay.position, direction * raycastDistance, Color.yellow);

            if (Physics.Raycast(leftRay.position, direction, out RaycastHit hit, raycastDistance, wallLayer))
            {
                Debug.Log("hitl - Hit " + hit.collider.name);
                leftHit = true;
                break;
            }
        }

        HandleAnimation(rightHit, leftHit);
    }

    void HandleAnimation(bool rightHit, bool leftHit)
    {
        if (rightHit)
        {
            Debug.Log("Setting Right Trigger");
            //carAnimator.SetTrigger("Ivy Hit Right");
            carAnimator.SetTrigger("Ivy Hit Right");

            // Check if trigger is set correctly
            AnimatorStateInfo stateInfo = carAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current Animator State: " + stateInfo.fullPathHash);
        }
        else if (leftHit)
        {
            Debug.Log("Setting Left Trigger");
            carAnimator.SetTrigger("Ivy Hit Left");

            AnimatorStateInfo stateInfo = carAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current Animator State: " + stateInfo.fullPathHash);
        }
        else
        {
            Debug.Log("Setting Idle Trigger");
            carAnimator.SetTrigger("Ivy Idle");

            AnimatorStateInfo stateInfo = carAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current Animator State: " + stateInfo.fullPathHash);
        }
    }

    private void UpdateWheelRotations()
    {
        float rotationAngle = currentSpeed * Time.deltaTime * 360f / (2 * Mathf.PI * 0.5f);

        // Rotate rear wheels (which are driven by the motor)
        rearLeftWheel.Rotate(Vector3.right, rotationAngle);
        rearRightWheel.Rotate(Vector3.right, rotationAngle);

        // Rotate front wheels to simulate their movement
        frontLeftWheel.Rotate(Vector3.right, rotationAngle);
        frontRightWheel.Rotate(Vector3.right, rotationAngle);
    }

    private void UpdateFrontWheelTurning()
    {
        // Adjust front wheels' turning angle based on steerAngle set in the Inspector
        frontLeftWheel.localRotation = Quaternion.Euler(frontLeftWheel.localRotation.eulerAngles.x, steerAngle, 0);
        frontRightWheel.localRotation = Quaternion.Euler(frontRightWheel.localRotation.eulerAngles.x, steerAngle, 0);
    }


    void HandleTurning()
    {
        float targetTurnSpeed = isDrifting ? defaultTurnSpeed * driftSteerAngleMultiplier : defaultTurnSpeed;

        // Smoothly adjust turn speed
        turnSpeed = Mathf.Lerp(turnSpeed, targetTurnSpeed, Time.deltaTime * 2f);

        if (turnInput != 0)
        {
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
            currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -turnSpeed, turnSpeed);

            float SteerAngle = steerAngle * currentTurnSpeed / turnSpeed;
            frontLeftWheelCollider.steerAngle = SteerAngle;
            frontRightWheelCollider.steerAngle = SteerAngle;
        }
        else
        {
            if (currentTurnSpeed > 0)
            {
                currentTurnSpeed -= turnDeceleration;
                if (currentTurnSpeed < 0) currentTurnSpeed = 0;
            }
            else if (currentTurnSpeed < 0)
            {
                currentTurnSpeed += turnDeceleration;
                if (currentTurnSpeed > 0) currentTurnSpeed = 0;
            }

            float SteerAngle = steerAngle * currentTurnSpeed / turnSpeed;
            frontLeftWheelCollider.steerAngle = SteerAngle;
            frontRightWheelCollider.steerAngle = SteerAngle;
        }
    }


    private void AdjustAcceleration()
    {
        currentAcceleration = baseAcceleration * gearRatios[currentGear] * rpmDeceleration[currentGear];
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(checkpointLayer) && isTimerRunning)
        {
            float timeAtCheckpoint = timer;
            checkpointTimes[checkpointIndex] = timeAtCheckpoint;
            totalSpeedAtCheckpoints += currentSpeed;
            checkpointIndex = (checkpointIndex + 1) % maxCheckpoints;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer(fpointLayer) && isTimerRunning)
        {
            finishPointTime = timer;
            isTimerRunning = false;
            float grandTotalSpeed = totalSpeedAtCheckpoints;
            Debug.Log("Grand total speed at checkpoints: " + grandTotalSpeed);
            Debug.Log("Total time to reach finish point: " + finishPointTime);
        }
    }

    void HandleDrifting()
    {
        if (isDrifting)
        {
            // Reduce lateral friction when drifting
            WheelFrictionCurve driftFriction = rearLeftWheelCollider.sidewaysFriction;
            driftFriction.stiffness = normalLateralFriction = 0.5f;
            rearLeftWheelCollider.sidewaysFriction = driftFriction;
            rearRightWheelCollider.sidewaysFriction = driftFriction;

            // Adjust the steering angle based on the current input
            float targetSteerAngle = maxSteerAngle * turnInput; // steerInput is between -1 and 1
            steerAngle = Mathf.Lerp(steerAngle, targetSteerAngle * driftSteerAngleMultiplier, Time.deltaTime * 2f);
        }
        else
        {
            // Restore normal friction when not drifting
            WheelFrictionCurve normalFriction = rearLeftWheelCollider.sidewaysFriction;
            normalFriction.stiffness = normalLateralFriction = 2f;
            rearLeftWheelCollider.sidewaysFriction = normalFriction;
            rearRightWheelCollider.sidewaysFriction = normalFriction;

            // Reset the steering angle based on input
            steerAngle = Mathf.Lerp(steerAngle, maxSteerAngle * turnInput, Time.deltaTime * 10f);
            if (Mathf.Abs(steerAngle) < 1f)
            {
                steerAngle = 0f;
            }

        }

        // Apply the adjusted steer angle to the front wheels
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }


    private void HandleOversteerUndersteer()
    {
        if (isDrifting)
        {
            // Oversteer: Reduce rear tire friction
            WheelFrictionCurve rearFriction = rearLeftWheelCollider.sidewaysFriction;
            rearFriction.stiffness = normalLateralFriction = 0f;
            rearLeftWheelCollider.sidewaysFriction = rearFriction;
            rearRightWheelCollider.sidewaysFriction = rearFriction;
        }
        else
        {
            // Gradually restore rear friction when not drifting
            WheelFrictionCurve rearFriction = rearLeftWheelCollider.sidewaysFriction;
            rearFriction.stiffness = Mathf.Lerp(rearFriction.stiffness, 2f, Time.deltaTime * 5f);
            rearLeftWheelCollider.sidewaysFriction = rearFriction;
            rearRightWheelCollider.sidewaysFriction = rearFriction;

            // Gradually reset steering angle
            steerAngle = Mathf.Lerp(steerAngle, maxSteerAngle * turnInput, Time.deltaTime * 10f);

            // Dampen the angular velocity and sideways velocity after drifting
            carRigidbody.angularVelocity = Vector3.Lerp(carRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 2f);
            Vector3 sidewaysVelocity = Vector3.Dot(carRigidbody.velocity, transform.right) * transform.right;
            carRigidbody.velocity -= sidewaysVelocity * Time.deltaTime * 2f;

            // Snap steering back to zero if near center
            if (Mathf.Abs(steerAngle) < 1f)
            {
                steerAngle = 0f;
            }
        }
    }

    // Function for Rear-Wheel Drive (RWD)
    private void ApplyRearWheelDrive()
    {
        rearLeftWheelCollider.motorTorque = moveInput * currentAcceleration;
        rearRightWheelCollider.motorTorque = moveInput * currentAcceleration;
    }

    // Function for Front-Wheel Drive (FWD)
    private void ApplyFrontWheelDrive()
    {
        frontLeftWheelCollider.motorTorque = moveInput * currentAcceleration;
        frontRightWheelCollider.motorTorque = moveInput * currentAcceleration;
    }

    // Function for Four-Wheel Drive (AWD)
    private void ApplyFourWheelDrive()
    {
        // Apply half of the torque to the front wheels
        frontLeftWheelCollider.motorTorque = moveInput * currentAcceleration / 2f;
        frontRightWheelCollider.motorTorque = moveInput * currentAcceleration / 2f;

        // Apply half of the torque to the rear wheels
        rearLeftWheelCollider.motorTorque = moveInput * currentAcceleration / 2f;
        rearRightWheelCollider.motorTorque = moveInput * currentAcceleration / 2f;
    }
}

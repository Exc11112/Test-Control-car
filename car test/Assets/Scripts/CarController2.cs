using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController2 : MonoBehaviour
{
    public float moveInput;
    public float turnInput;
    private bool isCarGrounded;
    public float currentSpeed;
    private float currentTurnSpeed;
    public int currentGear { get; private set; }
    public float currentRPM { get; private set; }

    public float airDrag;
    public float groundDrag;

    public float normalLateralFriction = 1.0f; // Normal friction for tires
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
    public float driftThresholdSpeed;
    private float lastTurnInputTime;
    private float turnResetDelay = 0.2f;
    public float turnAcceleration;
    public float turnDeceleration;
    [Header("Turn Speed Adjustment Settings")]
    [Range(0f, 1f)] public float turnSpeedMinFactor = 0.7f; // 70% of defaultTurnSpeed at max speed
    [Range(1f, 1.5f)] public float turnSpeedMaxFactor = 1.3f; // 130% of defaultTurnSpeed at 0 speed

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

    private bool isManual = false;
    private bool isNeutral = true;
    private float neutralStartTime;

    private bool isBoosted = false;
    private float boostStartTime;
    private float gear1Acceleration;
    private float gear1Deceleration;

    private float originalBaseAcceleration;

    public int maxCheckpoints = 5;
    private float[] checkpointTimes;
    private int checkpointIndex = 0;

    public float timer = 0f;
    public bool isTimerRunning = false;
    private float finishPointTime;
    private bool gameEnded = false;

    public string checkpointLayer = "checkpoint";
    public string fpointLayer = "fpoint";

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
    public Animator[] carAnimators;
    public GameObject Backlight;

    public AudioClip[] collisionSounds; // Assign two sounds in the Inspector
    public AudioClip gcrash;
    public AudioClip[] Bgm;
    public AudioSource audioSource;
    private bool canPlaySound = true; // Cooldown control
    private bool isSlowingDown = false;
    public float slowDownRate = 0f; // Adjust for slower or faster deceleration
    private bool wasAirborne = false; // Tracks if the car was previously in the air
    private float gameStartTime;
    private bool canPlayGcrash = false;
    private GameObject[] mtObjects;
    private GameObject[] atObjects;


    void Start()
    {
        gameStartTime = Time.time; // Record when the game starts
        StartCoroutine(EnableGcrashAfterDelay(10f)); // Enable Gcrash after 10 seconds
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
        mtObjects = GameObject.FindGameObjectsWithTag("MT");
        atObjects = GameObject.FindGameObjectsWithTag("AT");

        isNeutral = true;
        neutralStartTime = Time.time;

        checkpointTimes = new float[maxCheckpoints];
        currentAcceleration = baseAcceleration * gearRatios[currentGear];
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        PlayBGM();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isManual = !isManual;
        }
        GearmodeUi();
        UpdateWheelRotations();
        UpdateFrontWheelTurning();
        HandleTurning();  // Handle turning based on wheel colliders

        // Read input from both keyboard and Xbox controller
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal") + Input.GetAxis("ControllerHorizontal");

        // Read acceleration and braking from Xbox controller triggers
        float accelerationInput = Input.GetAxis("ControllerTriggerRight");
        // Ensure triggers don't cause unexpected reversing
        if (accelerationInput > 0.1f)
        {
            moveInput = accelerationInput;
        }
        else if (Mathf.Abs(moveInput) < 0.1f)
        {
            moveInput = 0; // Prevent small drift input from causing unintended movement
        }
        if (Input.GetButton("ControllerA"))
        {
            moveInput = -1;
        }

        if (Input.GetButtonDown("ControllerRB") && currentGear < gearRatios.Length - 1)
        {
            ShiftUp();
        }

        // Shift Down (LB)
        if (Input.GetButtonDown("ControllerLB") && currentGear > 0)
        {
            ShiftDown();
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
        isEnhancedTurning = Input.GetKey(KeyCode.Space) && currentSpeed > driftThresholdSpeed || Input.GetAxis("ControllerTriggerLeft") > 0.1f && currentSpeed > driftThresholdSpeed;
        isDrifting = isEnhancedTurning;

        if (isDrifting)
        {
            lastTurnInputTime = Time.time; // Keep drifting as long as Space is held
                                           // Extend drift time by pressing A/D (optional)
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Mathf.Abs(Input.GetAxis("ControllerHorizontal")) > 0.1f)
            {
                lastTurnInputTime = Time.time; // Reset drift timer while steering
            }
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

        if (moveInput < 0)
        {
            Backlight.SetActive(true);
        }
        else
        {
            Backlight.SetActive(false);
        }

        float raycastDistance = 0.2f;
        LayerMask wallLayer = LayerMask.GetMask("wall");

        foreach (Transform frontRay in frontRayOrigins)
        {
            Debug.DrawRay(frontRay.position, frontRay.forward * raycastDistance, Color.red);

            if (Physics.Raycast(frontRay.position, frontRay.forward, out RaycastHit frontHit, raycastDistance, wallLayer))
            {
                HandleWallCollision(frontHit.normal, true, false);
                break;
            }
        }

        // Rear collision detection (3 rays)
        foreach (Transform backRay in backRayOrigins)
        {
            Debug.DrawRay(backRay.position, -backRay.forward * raycastDistance, Color.blue);

            if (Physics.Raycast(backRay.position, -backRay.forward, out RaycastHit backHit, raycastDistance, wallLayer))
            {
                HandleWallCollision(backHit.normal, false, true);
                break;
            }
        }

        if (isSlowingDown)
        {
            // Stop slowing if player tries to reverse
            if (moveInput < 0 || moveInput > 0)
            {
                isSlowingDown = false;
            }
            else
            {
                // Gradually reduce speed and RPM to 0
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, slowDownRate * Time.deltaTime);
                currentRPM = Mathf.MoveTowards(currentRPM, 0, slowDownRate * 100 * Time.deltaTime);

                // Stop slowing when speed reaches 0
                if (currentSpeed <= 0.1f)
                {
                    currentSpeed = 0;
                    currentRPM = 0;
                    isSlowingDown = false; // Stop process
                }
            }
        }
        HandleDrifting();
        HandleOversteerUndersteer();
        AdjustAcceleration();
        if(currentSpeed >= 50f || currentSpeed <= -20f)
        {
            HandleRaycasts();
        }
    }

    private void GearmodeUi()
    {
        // Deactivate all first
        foreach (GameObject mt in mtObjects)
        {
            if (mt != null) mt.SetActive(false);
        }

        foreach (GameObject at in atObjects)
        {
            if (at != null) at.SetActive(false);
        }

        // Then activate according to isManual
        if (isManual)
        {
            foreach (GameObject mt in mtObjects)
            {
                if (mt != null) mt.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject at in atObjects)
            {
                if (at != null) at.SetActive(true);
            }
        }
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

        bool previouslyGrounded = isCarGrounded;

        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        if (!previouslyGrounded && isCarGrounded) // Car just landed
        {
            if (gcrash != null && audioSource != null && canPlayGcrash)
            {
                audioSource.PlayOneShot(gcrash);
            }
            wasAirborne = false; // Reset airborne flag
        }
        else if (previouslyGrounded && !isCarGrounded) // Car just became airborne
        {
            wasAirborne = true;
        }

        if (isCarGrounded)
        {
            Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);
        }
    }

    void HandleWallCollision(Vector3 collisionNormal, bool isFront, bool isBack)
    {
        // Only start slowing down if speed is greater than 40
        if (!isSlowingDown && ((isFront && currentSpeed > 40f) || (isBack && currentSpeed < -20f)))
        {
            isSlowingDown = true;
        }


        // Reflect velocity for a slight bounce effect
        Vector3 incomingVelocity = carRigidbody.velocity;
        if (Vector3.Dot(incomingVelocity.normalized, collisionNormal) < 0)
        {
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, collisionNormal) * 0.7f;
            carRigidbody.velocity = reflectedVelocity;

            // Add rotational force for a realistic bounce
            Vector3 torque = Vector3.Cross(collisionNormal, incomingVelocity.normalized) * 50f;
            carRigidbody.AddTorque(torque, ForceMode.Impulse);
        }

        isDrifting = false; // Stop drifting after hitting a wall

        // Play a random collision sound if available
        if (canPlaySound && collisionSounds.Length > 0 && (currentSpeed >= 50f || currentSpeed <= -20f))
        {
            // Play sound IMMEDIATELY
            int randomIndex = Random.Range(0, collisionSounds.Length);
            audioSource.PlayOneShot(collisionSounds[randomIndex]);

            // Start cooldown
            StartCoroutine(SoundCooldown());
        }
    }
    IEnumerator SoundCooldown()
    {
        canPlaySound = false; // Block new sounds
        yield return new WaitForSeconds(0.5f);
        canPlaySound = true; // Re-enable sounds
    }

    void HandleRaycasts()
    {
        float raycastDistance = 0.2f;
        LayerMask wallLayer = LayerMask.GetMask("wall");
        bool rightHit = false;
        bool leftHit = false;
        bool frontHit = false;
        bool backHit = false;


        // Front collision detection (Triggers slowdown if moving forward fast)
        foreach (Transform frontRay in frontRayOrigins)
        {
            Debug.DrawRay(frontRay.position, frontRay.forward * raycastDistance, Color.red);
            if (Physics.Raycast(frontRay.position, frontRay.forward, out RaycastHit hit, raycastDistance, wallLayer))
            {
                frontHit = true;
                HandleWallCollision(hit.normal, true, false); // Front hit, check slowdown for forward movement
                break;
            }
        }

        // Back collision detection (Triggers slowdown if reversing fast)
        foreach (Transform backRay in backRayOrigins)
        {
            Debug.DrawRay(backRay.position, -backRay.forward * raycastDistance, Color.blue);
            if (Physics.Raycast(backRay.position, -backRay.forward, out RaycastHit hit, raycastDistance, wallLayer))
            {
                backHit = true;
                HandleWallCollision(hit.normal, false, true); // Back hit, check slowdown for reverse movement
                break;
            }
        }

        // Right collision detection (Only Reflects)
        foreach (Transform rightRay in RightRayOrigins)
        {
            Debug.DrawRay(rightRay.position, rightRay.forward * raycastDistance, Color.green);
            if (Physics.Raycast(rightRay.position, rightRay.forward, out RaycastHit hit, raycastDistance, wallLayer))
            {
                rightHit = true;
                HandleWallCollision(hit.normal, false, false); // Only Reflection, no slowdown
                break;
            }
        }

        // Left collision detection (Only Reflects)
        foreach (Transform leftRay in LeftRayOrigins)
        {
            Debug.DrawRay(leftRay.position, leftRay.forward * raycastDistance, Color.yellow);
            if (Physics.Raycast(leftRay.position, leftRay.forward, out RaycastHit hit, raycastDistance, wallLayer))
            {
                leftHit = true;
                HandleWallCollision(hit.normal, false, false); // Only Reflection, no slowdown
                break;
            }
        }

        HandleAnimation(rightHit, leftHit, frontHit);

    }

    void HandleAnimation(bool rightHit, bool leftHit, bool frontHit)
    {
        foreach (Animator animator in carAnimators)
        {
            animator.ResetTrigger("Ivy Hit Right");
            animator.ResetTrigger("Ivy Hit Left");
            animator.ResetTrigger("Ivy Idle");
            animator.ResetTrigger("Ivy Hit Front");
            animator.ResetTrigger("Iris Hit Front");
            animator.ResetTrigger("Iris Hit Right");
            animator.ResetTrigger("Iris Hit Left");
            animator.ResetTrigger("Iris Idle");
            animator.ResetTrigger("May Hit Front");
            animator.ResetTrigger("May Hit Right");
            animator.ResetTrigger("May Hit Left");
            animator.ResetTrigger("May Idle");

            if (rightHit)
            {
                Debug.Log("Setting Right Trigger");
                animator.SetTrigger("Ivy Hit Right");
                animator.SetTrigger("Iris Hit Right");
                animator.SetTrigger("May Hit Right");
            }
            else if (leftHit)
            {
                Debug.Log("Setting Left Trigger");
                animator.SetTrigger("Ivy Hit Left");
                animator.SetTrigger("Iris Hit Left");
                animator.SetTrigger("May Hit Left");
            }
            else if (frontHit)
            {
                animator.SetTrigger("Ivy Hit Front");
                animator.SetTrigger("Iris Hit Front");
                animator.SetTrigger("May Hit Front");
            }
            else
            {
                Debug.Log("Setting Idle Trigger");
                animator.SetTrigger("Ivy Idle");
                animator.SetTrigger("Iris Idle");
                animator.SetTrigger("May Idle");
            }
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
        float speedRatio = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxFwdSpeed);
        float speedAdjustedTurnFactor = Mathf.Lerp(turnSpeedMaxFactor, turnSpeedMinFactor, speedRatio);
        float targetTurnSpeed = isDrifting ? defaultTurnSpeed * driftSteerAngleMultiplier : defaultTurnSpeed * speedAdjustedTurnFactor;

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
        StartCoroutine(ShiftUpCoroutine());
    }

    private IEnumerator ShiftUpCoroutine()
    {
        currentGear++;
        lastShiftTime = Time.time;
        AdjustAcceleration();

        float rpmDrop = 0f;
        switch (currentGear)
        {
            case 1: rpmDrop = 2000f; break;
            case 2: rpmDrop = 1800f; break;
            case 3: rpmDrop = 1500f; break;
            case 4: rpmDrop = 1200f; break;
            default: rpmDrop = 1000f; break;
        }

        float targetRPM = Mathf.Clamp(currentRPM - rpmDrop, minRPM, maxRPM);
        float startRPM = currentRPM;
        float elapsedTime = 0f;
        float duration = 0.5f; // Time in seconds to smoothly drop RPM
        bool isHoldingRPM = true;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentRPM = Mathf.Lerp(startRPM, targetRPM, elapsedTime / duration);
            yield return null;
        }

        currentRPM = targetRPM;

        // **Hold the dropped RPM for a short duration before allowing recalculations**
        yield return new WaitForSeconds(0.5f);
        isHoldingRPM = false;
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
            // Reduce lateral friction
            WheelFrictionCurve driftFriction = rearLeftWheelCollider.sidewaysFriction;
            driftFriction.stiffness = 0.5f;
            rearLeftWheelCollider.sidewaysFriction = driftFriction;
            rearRightWheelCollider.sidewaysFriction = driftFriction;

            // Adjust steering based on A/D input
            float driftDirection = Input.GetAxis("Horizontal");
            steerAngle = driftDirection * driftSteerAngle;
        }
        else
        {
            // Restore normal friction
            WheelFrictionCurve normalFriction = rearLeftWheelCollider.sidewaysFriction;
            normalFriction.stiffness = 2f;
            rearLeftWheelCollider.sidewaysFriction = normalFriction;
            rearRightWheelCollider.sidewaysFriction = normalFriction;
            steerAngle = maxSteerAngle * turnInput; // Default steering
        }

        // Apply steering angle
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
    void PlayBGM()
    {
        if (Bgm.Length > 0 && SelectionData.SelectedCharacterIndex < Bgm.Length)
        {
            audioSource.clip = Bgm[SelectionData.SelectedCharacterIndex];
            audioSource.loop = true; // Loop the BGM
            audioSource.Play();
        }
    }
    private IEnumerator EnableGcrashAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canPlayGcrash = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController4W : MonoBehaviour
{
    // Public variables for car handling
    public float airDrag = 0.5f;
    public float groundDrag = 3f;
    public float maxForwardSpeed = 280f;
    public float maxReverseSpeed = 100f;
    public float acceleration = 70f;
    public float deceleration = 30f;
    public float brakeForce = 155f;
    public float turnSpeed = 100f;
    public float dForce = -30f;

    // Turn behavior variables
    public float turnAcceleration = 5f;
    public float turnDeceleration = 2f;
    public float highTurnSpeedAt = 150f;
    public float lowTurnSpeedAt = 50f;

    // Wheel Colliders and Transforms
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    // Private variables
    private float moveInput;
    private float turnInput;
    private float currentAcceleration;
    private bool isCarGrounded;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        UpdateWheelPoses();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleTurnBehavior();
        ApplyDrag();
        GroundCheck();
    }

    void HandleMovement()
    {
        if (isCarGrounded)
        {
            float motorTorque = moveInput * currentAcceleration;
            float steeringAngle = turnInput * turnSpeed;

            // Apply motor torque to rear wheels
            rearLeftWheel.motorTorque = motorTorque;
            rearRightWheel.motorTorque = motorTorque;

            // Apply steering to front wheels
            frontLeftWheel.steerAngle = steeringAngle;
            frontRightWheel.steerAngle = steeringAngle;

            // Handle braking
            if (moveInput < 0)
            {
                rearLeftWheel.brakeTorque = brakeForce;
                rearRightWheel.brakeTorque = brakeForce;
            }
            else
            {
                rearLeftWheel.brakeTorque = 0;
                rearRightWheel.brakeTorque = 0;
            }

            // Limit speed
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, moveInput > 0 ? maxForwardSpeed : maxReverseSpeed);
        }
    }

    void HandleTurnBehavior()
    {
        if (rb.velocity.magnitude > highTurnSpeedAt)
        {
            turnSpeed -= turnDeceleration;
        }
        else if (rb.velocity.magnitude < lowTurnSpeedAt)
        {
            turnSpeed += turnAcceleration;
        }

        turnSpeed = Mathf.Clamp(turnSpeed, 0, 100);
    }

    void ApplyDrag()
    {
        if (isCarGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
            rb.AddForce(Vector3.up * dForce, ForceMode.Acceleration);
        }
    }

    void GroundCheck()
    {
        isCarGrounded = frontLeftWheel.isGrounded && frontRightWheel.isGrounded && rearLeftWheel.isGrounded && rearRightWheel.isGrounded;
    }

    void UpdateWheelPoses()
    {
        UpdateWheelPose(frontLeftWheel, frontLeftTransform);
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(rearLeftWheel, rearLeftTransform);
        UpdateWheelPose(rearRightWheel, rearRightTransform);
    }

    void UpdateWheelPose(WheelCollider collider, Transform transform)
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
            currentAcceleration = 5f;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            currentAcceleration = acceleration;
        }
    }
}
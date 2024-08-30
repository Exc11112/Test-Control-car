using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2 : MonoBehaviour
{
    public Transform centerOfMass;

    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;

    public WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
    public Transform frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;

    public float suspensionStrength = 5000f;
    public float suspensionDamping = 500f;
    public float suspensionTravel = 0.2f;

    public float driftFactor = 0.95f;
    public float driftThreshold = 10f;

    public float oversteerFactor = 1.2f;
    public float understeerFactor = 1.2f;

    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isBraking;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        ApplySuspension();
        HandleDrifting();
        HandleOversteerUndersteer();
    }

    private void HandleMotor()
    {
        float motorInput = Input.GetAxis("Vertical");
        frontLeftWheelCollider.motorTorque = motorInput * motorForce;
        frontRightWheelCollider.motorTorque = motorInput * motorForce;

        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * Input.GetAxis("Horizontal");
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheel);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheel);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheel);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheel);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void ApplySuspension()
    {
        // Apply suspension forces to each wheel
        foreach (WheelCollider wheelCollider in new WheelCollider[] { frontLeftWheelCollider, frontRightWheelCollider, rearLeftWheelCollider, rearRightWheelCollider })
        {
            WheelHit hit;
            if (wheelCollider.GetGroundHit(out hit))
            {
                float compression = (wheelCollider.transform.position.y - hit.point.y) / suspensionTravel;
                float suspensionForce = suspensionStrength * compression - suspensionDamping * wheelCollider.attachedRigidbody.velocity.y;
                rb.AddForceAtPosition(wheelCollider.transform.up * suspensionForce, wheelCollider.transform.position);
            }
        }
    }

    private void HandleDrifting()
    {
        // Detect and apply drift mechanics
        foreach (WheelCollider wheelCollider in new WheelCollider[] { rearLeftWheelCollider, rearRightWheelCollider })
        {
            WheelHit hit;
            if (wheelCollider.GetGroundHit(out hit))
            {
                if (Mathf.Abs(hit.sidewaysSlip) > driftThreshold)
                {
                    rb.AddForce(-transform.right * driftFactor * hit.sidewaysSlip);
                }
            }
        }
    }

    private void HandleOversteerUndersteer()
    {
        // Adjust wheel friction based on oversteer or understeer
        AdjustFriction(frontLeftWheelCollider, understeerFactor);
        AdjustFriction(frontRightWheelCollider, understeerFactor);
        AdjustFriction(rearLeftWheelCollider, oversteerFactor);
        AdjustFriction(rearRightWheelCollider, oversteerFactor);
    }

    private void AdjustFriction(WheelCollider wheelCollider, float factor)
    {
        WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
        sidewaysFriction.stiffness *= factor;
        wheelCollider.sidewaysFriction = sidewaysFriction;
    }
}

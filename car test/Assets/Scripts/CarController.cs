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

    public float airDrag;
    public float groundDrag;

    public float maxFwdSpeed;
    public float maxRevSpeed;
    public float acceleration;
    public float deceleration;
    public float brakeForce;

    public float turnSpeed;
    public float turnSpeeddef;
    public float turnSpeedHight;//วงการเลี้ยวแคบ(เป็นค่าความเร็วในการเลี้ยว ยิ่งเยอะยิ่งวงเลี้ยวแคบ)
    public float turnSpeedlow;//วงการเลี้ยวกว้าง(เป็นค่าความเร็วในการเลี้ยว ยิ่งน้อยยิ่งวงเลี้ยวกว้าง)
    public float hightturnRadiusAt;//ถ้าความต่ำกว่าค่าที่ตั้งนี้วงเลี้ยวจะแคบ
    public float lowturnRadiusAt;//ถ้าความสูงกว่าค่าที่ตั้งนี้วงเลี้ยวจะกว้าง

    public float turnAcceleration; // Acceleration of turning force
    public float turnDeceleration; // Deceleration of turning force
    public float dForce; // Downward force when in the air
    public LayerMask groundLayer;

    public Rigidbody sphereRB;

    // Start is called before the first frame update
    void Start()
    {
        // Detach the sphere from the car object
        sphereRB.transform.parent = null;
        currentSpeed = 0f;
        currentTurnSpeed = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Get input for movement and turning
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        // Gradually adjust current speed based on moveInput
        if (moveInput > 0)
        {
            currentSpeed += acceleration * Time.deltaTime;
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
        float targetTurnSpeed = 0f;

        if (currentSpeed > lowturnRadiusAt)
        {
            targetTurnSpeed = turnSpeedlow;
        }
        else if (currentSpeed < hightturnRadiusAt)
        {
            targetTurnSpeed = turnSpeedHight;
        }
        else if (currentSpeed != hightturnRadiusAt || currentSpeed != lowturnRadiusAt)
        {
            targetTurnSpeed = turnSpeeddef; // Default turn speed
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
}
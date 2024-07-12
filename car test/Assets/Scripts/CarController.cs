using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float moveInput;
    private float turnInput;
    private bool isCarGrounded;
    private float currentSpeed;

    public float airDrag;
    public float groundDrag;

    public float maxFwdSpeed;
    public float maxRevSpeed;
    public float acceleration;
    public float deceleration;
    public float brakeForce;
    public float turnSpeed;
    public float dForce;
    public LayerMask groundLayer;

    public Rigidbody sphereRB;

    // Start is called before the first frame update
    void Start()
    {
        // Detach the sphere from the car object
        sphereRB.transform.parent = null;
        currentSpeed = 0f;
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

        // Update car position to match the sphere's position
        transform.position = sphereRB.transform.position;

        if (isCarGrounded)
        {
            // Rotate the car based on turn input and vertical input
            float newRotation = turnInput * turnSpeed * Time.deltaTime * (currentSpeed / maxFwdSpeed);
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
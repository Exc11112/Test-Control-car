using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiCar : MonoBehaviour
{
    public Transform path;
    public float maxSteerAngle = 45f;
    public float turnSpeed = 5f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public float maxMotorTorque = 80f;
    public float maxBrakeTorque = 150f;
    public float currentSpeed;
    public float maxSpeed = 100f;
    public Vector3 centerOfMass;
    public bool isBraking = false;
    public Renderer carRenderer;
    public float waypointReachThreshold = 0.5f;

    public float sensorLength = 3f;
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
    public float frontSideSensorPosition = 0.2f;
    public float frontSensorAngle = 30f;

    private List<Transform> nodes;
    private int currentNode = 0;
    private bool avoiding = false;
    private float targetSteerAngle = 0;

    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }
    }

    private void FixedUpdate()
    {
        Sensor();
        ApplySteer();
        Drive();
        CheckWaypointDistance();
        Braking();
        LerpToSteerAngle();
    }

    private void Sensor()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position + transform.forward * frontSensorPosition.z + transform.up * frontSensorPosition.y;
        float avoidMultiplier = 0;
        avoiding = false;
        LayerMask avoidLayers = LayerMask.GetMask("groundLayer", "checkpoint1", "checkpoint2", "fpoint", "driftend", "driftstart", "switch1", "switch2");

        // Front center sensor
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if ((avoidLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;

                // Decide steer direction based on hit normal
                avoidMultiplier = hit.normal.x < 0 ? 1.5f : -1.5f;
            }
        }

        // Front right sensor
        Vector3 rightSensorPos = sensorStartPos + transform.right * frontSideSensorPosition;
        if (Physics.Raycast(rightSensorPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if ((avoidLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                Debug.DrawLine(rightSensorPos, hit.point);
                avoiding = true;
                avoidMultiplier = -1.5f; // steer left if obstacle on the right
            }
        }

        // Front left sensor
        Vector3 leftSensorPos = sensorStartPos - transform.right * frontSideSensorPosition;
        if (Physics.Raycast(leftSensorPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if ((avoidLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                Debug.DrawLine(leftSensorPos, hit.point);
                avoiding = true;
                avoidMultiplier = 1.5f; // steer right if obstacle on the left
            }
        }

        // Apply avoidance steering angle if avoiding
        if (avoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidMultiplier * Time.deltaTime;
            if(currentSpeed > 15) 
            {
                isBraking = true; // Optionally apply braking when avoiding
            }

        }
        else
        {
            isBraking = false;
        }
    }


    private void ApplySteer()
    {
        if (!avoiding) // Only steer toward the next node if we're not avoiding
        {
            Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
            float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
            targetSteerAngle = newSteer;
        }
    }


    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;

        if (currentSpeed < maxSpeed)
        {
            wheelFL.motorTorque = maxMotorTorque;
            wheelFR.motorTorque = maxMotorTorque;
        }
        else
        {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
    }

    private void CheckWaypointDistance()
    {
        if(Vector3.Distance(transform.position, nodes[currentNode].position) < waypointReachThreshold)
        {
            if(currentNode == nodes.Count- 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }

    private void Braking()
    {
        if (isBraking)
        {
            wheelRL.brakeTorque = maxBrakeTorque;
            wheelRR.brakeTorque = maxBrakeTorque;
        }
        else 
        {
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
        }
    }

    private void LerpToSteerAngle()
    {
        wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }
}

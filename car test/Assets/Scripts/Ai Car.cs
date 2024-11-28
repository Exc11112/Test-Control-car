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

    private List<Path.PathNode> nodes;
    private int currentNodeIndex = 0;
    private bool avoiding = false;
    private float targetSteerAngle = 0;

    public bool isReversing = false;
    private float reverseTimer = 0f;
    private float reverseDuration = 2f; // Time to reverse
    private float stuckSpeedThreshold = 10f; // Speed threshold to detect being stuck
    private float stuckTimeThreshold = 1.5f; // Time before triggering reverse
    private float stuckTimer = 0f;
    private Path.PathNode nextPathNode = null;

    private void Start()
    {
        isReversing = true;
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        Path pathScript = path.GetComponent<Path>();
        nodes = pathScript.pathNodes; // Load path nodes from the Path script
    }

    private void FixedUpdate()
    {
        Sensor();
        CheckIfStuck();
        ApplySteer();
        Drive();
        CheckWaypointDistance();
        Braking();
        LerpToSteerAngle();
        Debug.Log("Reversing: " + isReversing + ", Reverse Timer: " + reverseTimer);
    }

    private void Sensor()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position + transform.forward * frontSensorPosition.z + transform.up * frontSensorPosition.y;
        float avoidMultiplier = 0;
        avoiding = false;
        LayerMask avoidLayers = LayerMask.GetMask("groundLayer", "checkpoint1", "checkpoint2", "fpoint", "driftend", "driftstart", "switch1", "switch2");

        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if ((avoidLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier = hit.normal.x < 0 ? 1.5f : -1.5f;
            }
        }

        Vector3 rightSensorPos = sensorStartPos + transform.right * frontSideSensorPosition;
        if (Physics.Raycast(rightSensorPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if ((avoidLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                Debug.DrawLine(rightSensorPos, hit.point);
                avoiding = true;
                avoidMultiplier = -1.5f;
            }
        }

        Vector3 leftSensorPos = sensorStartPos - transform.right * frontSideSensorPosition;
        if (Physics.Raycast(leftSensorPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if ((avoidLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                Debug.DrawLine(leftSensorPos, hit.point);
                avoiding = true;
                avoidMultiplier = 1.5f;
            }
        }

        if (avoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidMultiplier * (Time.deltaTime * 30f);
            if (currentSpeed > 15)
            {
                isBraking = true;
            }
        }
        else
        {
            isBraking = false;
        }
    }

    private void ApplySteer()
    {
        if (!avoiding)
        {
            Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNodeIndex].nodeTransform.position);
            float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
            targetSteerAngle = newSteer;
        }
    }

    private void CheckIfStuck()
    {
        if (avoiding && currentSpeed < stuckSpeedThreshold)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckTimeThreshold)
            {
                isReversing = true;
                reverseTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
            isReversing = false;
        }
    }

    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;

        if (isReversing)
        {
            wheelFL.motorTorque = -maxMotorTorque;
            wheelFR.motorTorque = -maxMotorTorque;
            reverseTimer += Time.deltaTime;
            if (reverseTimer > reverseDuration)
            {
                isReversing = false;
                stuckTimer = 0f;
            }
        }
        else if (currentSpeed < maxSpeed)
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
        if (Vector3.Distance(transform.position, nodes[currentNodeIndex].nodeTransform.position) < waypointReachThreshold)
        {
            Path.PathNode currentNode = nodes[currentNodeIndex];

            if (nextPathNode == null) // No path chosen yet for this branch
            {
                if (currentNode.nextNodes.Count > 0)
                {
                    // Select a specific next node only once
                    int specificIndex = 21; // Example: Change this to select a specific path index
                    Transform nextTransform = currentNode.nextNodes[specificIndex];

                    // Find the corresponding PathNode in the nodes list
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (nodes[i].nodeTransform == nextTransform)
                        {
                            nextPathNode = nodes[i];
                            currentNodeIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    // If no branching nodes, move to the next node sequentially
                    currentNodeIndex = (currentNodeIndex + 1) % nodes.Count;
                }
            }
            else
            {
                // Continue following the already chosen branch
                currentNodeIndex = nodes.IndexOf(nextPathNode);
            }
        }
    }


    private void Braking()
    {
        if (isReversing)
        {
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
            isBraking = false;
        }
        else if (isBraking)
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

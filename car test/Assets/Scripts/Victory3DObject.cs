using UnityEngine;

public class Victory3DObject : MonoBehaviour
{
    public GameManager gameManager;
    public DriftScore2[] associatedDriftScore;
    public LayerMask carLayer;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Victory3DObject] Trigger Enter detected from: {other.name}");

        // Ensure the collider is part of the car layer
        if (((1 << other.gameObject.layer) & carLayer) == 0)
        {
            Debug.Log($"[Victory3DObject] {other.name} is not in the car layer, ignoring.");
            return;
        }

        // Retrieve the Rigidbody and CarController2 component
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.Log($"[Victory3DObject] {other.name} has no Rigidbody, ignoring.");
            return;
        }

        CarController2 car = rb.GetComponent<CarController2>();
        if (car == null)
        {
            Debug.Log($"[Victory3DObject] {rb.name} has a Rigidbody but no CarController2 component.");
            return;
        }

        Debug.Log($"[Victory3DObject] Car {car.name} entered the victory zone.");

        // Check if associated drift scores are assigned
        if (associatedDriftScore == null || associatedDriftScore.Length == 0)
        {
            Debug.LogWarning("[Victory3DObject] associatedDriftScore is null or empty.");
            return;
        }

        // Determine the victory index based on the object's name
        int victoryIndex = 0;
        switch (gameObject.name)
        {
            case "Finish Point1":
                victoryIndex = 1;
                break;
            case "Finish Point2":
                victoryIndex = 2;
                break;
            case "Finish Point3":
                victoryIndex = 3;
                break;
            default:
                Debug.LogWarning("[Victory3DObject] Unknown finish point name, ignoring.");
                return;
        }

        // Send the victory index to GameManager
        Debug.Log($"[Victory3DObject] Sending victory index {victoryIndex} to GameManager.");
        if (gameManager != null)
        {
            gameManager.ReceiveVictoryIndex(victoryIndex);
        }
        else
        {
            Debug.LogError("[Victory3DObject] gameManager is not assigned! Cannot send victory index.");
        }

        // Notify the GameManager about the car entering the 3D object area
        bool carFound = false;
        foreach (DriftScore2 driftScore in associatedDriftScore)
        {
            if (driftScore != null && car == driftScore.car)
            {
                Debug.Log($"[Victory3DObject] Notifying GameManager for DriftScore: {driftScore.name}");
                carFound = true;

                if (gameManager != null)
                {
                    gameManager.OnCarEnter3DObject(car, driftScore);
                }
                else
                {
                    Debug.LogError("[Victory3DObject] gameManager is not assigned! Cannot notify enter event.");
                }
            }
        }

        if (!carFound)
        {
            Debug.Log($"[Victory3DObject] No matching DriftScore2 object found for car {car.name}.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Victory3DObject] Trigger Exit detected from: {other.name}");

        // Ensure the collider is part of the car layer
        if (((1 << other.gameObject.layer) & carLayer) == 0)
        {
            Debug.Log($"[Victory3DObject] {other.name} is not in the car layer, ignoring.");
            return;
        }

        // Retrieve the Rigidbody and CarController2 component
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.Log($"[Victory3DObject] {other.name} has no Rigidbody, ignoring.");
            return;
        }

        CarController2 car = rb.GetComponent<CarController2>();
        if (car == null)
        {
            Debug.Log($"[Victory3DObject] {rb.name} has a Rigidbody but no CarController2 component.");
            return;
        }

        Debug.Log($"[Victory3DObject] Car {car.name} exited the victory zone.");

        // Notify the GameManager about the car exiting the 3D object area
        foreach (DriftScore2 driftScore in associatedDriftScore)
        {
            if (driftScore != null && car == driftScore.car)
            {
                Debug.Log($"[Victory3DObject] Notifying GameManager that {car.name} exited {driftScore.name}");

                if (gameManager != null)
                {
                    gameManager.OnCarExit3DObject(car, driftScore);
                }
                else
                {
                    Debug.LogError("[Victory3DObject] gameManager is not assigned! Cannot notify exit event.");
                }
            }
        }
    }
}

using UnityEngine;

public class Victory3DObject : MonoBehaviour
{
    public GameManager gameManager;
    public DriftScore2[] associatedDriftScore;
    public LayerMask carLayer;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Victory3DObject] Trigger Enter detected from: {other.name}");

        // 1. Check layer first
        if (((1 << other.gameObject.layer) & carLayer) == 0)
        {
            Debug.Log($"[Victory3DObject] {other.name} is not in the car layer, ignoring.");
            return;
        }

        // 2. Find the Rigidbody at the root level
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.Log($"[Victory3DObject] {other.name} has no Rigidbody, ignoring.");
            return;
        }

        // 3. Get CarController2 from the Rigidbody's GameObject
        CarController2 car = rb.GetComponent<CarController2>();
        if (car == null)
        {
            Debug.Log($"[Victory3DObject] {rb.name} has a Rigidbody but no CarController2 component.");
            return;
        }

        Debug.Log($"[Victory3DObject] Car {car.name} entered the victory zone.");

        // 4. Ensure associatedDriftScore is not null or empty
        if (associatedDriftScore == null || associatedDriftScore.Length == 0)
        {
            Debug.LogWarning("[Victory3DObject] associatedDriftScore is null or empty.");
            return;
        }

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
                    Debug.LogError("[Victory3DObject] gameManager is not assigned! Make sure it's set in the Inspector.");
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

        // 1. Check layer first
        if (((1 << other.gameObject.layer) & carLayer) == 0)
        {
            Debug.Log($"[Victory3DObject] {other.name} is not in the car layer, ignoring.");
            return;
        }

        // 2. Find the Rigidbody at the root level
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.Log($"[Victory3DObject] {other.name} has no Rigidbody, ignoring.");
            return;
        }

        // 3. Get CarController2 from the Rigidbody's GameObject
        CarController2 car = rb.GetComponent<CarController2>();
        if (car == null)
        {
            Debug.Log($"[Victory3DObject] {rb.name} has a Rigidbody but no CarController2 component.");
            return;
        }

        Debug.Log($"[Victory3DObject] Car {car.name} exited the victory zone.");

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

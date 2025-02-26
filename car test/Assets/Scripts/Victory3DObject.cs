using UnityEngine;

public class Victory3DObject : MonoBehaviour
{
    public GameManager gameManager;
    public DriftScore2[] associatedDriftScore;
    public LayerMask carLayer;

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & carLayer) == 0)
        {
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            return;
        }

        CarController2 car = rb.GetComponent<CarController2>();
        if (car == null)
        {
            return;
        }

        if (associatedDriftScore == null || associatedDriftScore.Length == 0)
        {
            return;
        }

        //int victoryIndex = 0;
        //switch (gameObject.name)
        //{
        //    case "Finish Point1":
        //        victoryIndex = 0;
        //        break;
        //    case "Finish Point2":
        //        victoryIndex = 1;
        //        break;
        //    case "Finish Point3":
        //        victoryIndex = 2;
        //        break;
        //    case "Finish Point4":
        //        victoryIndex = 0;
        //        break;
        //    case "Finish Point5":
        //        victoryIndex = 1;
        //        break;
        //    case "Finish Point6":
        //        victoryIndex = 2;
        //        break;
        //    default:
        //        Debug.LogError("Unexpected gameObject.name: " + gameObject.name); // Log the error first
        //        return;  // Exit after logging
        //}

        //if (victoryIndex == 0 || gameManager == null)
        //{
        //    return;
        //}

        foreach (DriftScore2 driftScore in associatedDriftScore)
        {
            if (driftScore != null && car == driftScore.car && gameManager != null)
            {
                // Set the UI index based on the finish point name
                switch (gameObject.name)
                {
                    case "Finish Point1":
                        driftScore.currentUIIndex = 0; // Maps to victoryUIObjects[0]
                        break;
                    case "Finish Point2":
                        driftScore.currentUIIndex = 1; // Maps to victoryUIObjects[1]
                        break;
                    case "Finish Point3":
                        driftScore.currentUIIndex = 2; // Maps to victoryUIObjects[2]
                        break;
                        // Add more cases as needed
                }
                Debug.Log(driftScore.currentUIIndex);
                gameManager.OnCarEnter3DObject(car, driftScore);
                gameManager.ReceiveVictoryIndex(driftScore); // Pass the updated DriftScore2
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & carLayer) == 0)
        {
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            return;
        }

        CarController2 car = rb.GetComponent<CarController2>();
        if (car == null)
        {
            return;
        }

        foreach (DriftScore2 driftScore in associatedDriftScore)
        {
            if (driftScore != null && car == driftScore.car && gameManager != null)
            {
                gameManager.OnCarExit3DObject(car, driftScore);
            }
        }
    }
}

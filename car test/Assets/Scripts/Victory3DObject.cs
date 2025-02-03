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
                return;
        }

        if (victoryIndex == 0 || gameManager == null)
        {
            return;
        }

        gameManager.ReceiveVictoryIndex(victoryIndex);

        foreach (DriftScore2 driftScore in associatedDriftScore)
        {
            if (driftScore != null && car == driftScore.car && gameManager != null)
            {
                gameManager.OnCarEnter3DObject(car, driftScore);
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

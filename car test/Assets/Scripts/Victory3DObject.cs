using UnityEngine;

public class Victory3DObject : MonoBehaviour
{
    public GameManager gameManager;
    public DriftScore2[] associatedDriftScore; // Array remains

    void OnTriggerEnter(Collider other)
    {
        CarController2 car = other.GetComponent<CarController2>();
        if (car != null)
        {
            // Check all drift scores in the array
            foreach (DriftScore2 driftScore in associatedDriftScore)
            {
                if (driftScore != null && car == driftScore.car)
                {
                    gameManager.OnCarEnter3DObject(car, driftScore);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        CarController2 car = other.GetComponent<CarController2>();
        if (car != null)
        {
            // Check all drift scores in the array
            foreach (DriftScore2 driftScore in associatedDriftScore)
            {
                if (driftScore != null && car == driftScore.car)
                {
                    gameManager.OnCarExit3DObject(car, driftScore);
                }
            }
        }
    }
}
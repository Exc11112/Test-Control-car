using System.Collections.Generic;
using UnityEngine;

public class CarCameraTargets : MonoBehaviour
{
    public Transform lookAtTarget;   // Transform to look at
    public Transform positionTarget; // Transform for camera position

    public GameObject objectToActivate;
    public List<GameObject> objectsToDeactivate;

    void Awake()
    {
        // Activate the specified GameObject
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        // Deactivate all GameObjects in the list
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}

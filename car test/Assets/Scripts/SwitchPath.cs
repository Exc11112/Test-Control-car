using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchParth : MonoBehaviour
{
    public string carLayer = "car";
    public string switch1Layer = "switch1";
    public string switch2Layer = "switch2";
    public string swap1Layer = "swap1";
    public string swap2Layer = "swap2";

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collider belongs to the "Car" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer(carLayer))
        {
            // Check if the collided object belongs to the "Switch1" layer
            if (gameObject.layer == LayerMask.NameToLayer(switch1Layer))
            {
                DeactivateObjectsInLayer(swap1Layer);
                DeactivateObjectsInLayer(switch1Layer);
                DeactivateObjectsInLayer(switch2Layer);
            }

            // Check if the collided object belongs to the "Switch2" layer
            if (gameObject.layer == LayerMask.NameToLayer(switch2Layer))
            {
                DeactivateObjectsInLayer(swap2Layer);
                DeactivateObjectsInLayer(switch1Layer);
                DeactivateObjectsInLayer(switch2Layer);
            }
        }
    }

    private void DeactivateObjectsInLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objectsInLayer)
        {
            if (obj.layer == layer)
            {
                obj.SetActive(false);
            }
        }
    }
}
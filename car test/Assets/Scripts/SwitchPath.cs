using System.Collections.Generic;
using UnityEngine;

public class SwitchParth : MonoBehaviour
{
    public string carLayer = "Car";
    public string switch1Layer = "switch1";
    public string switch2Layer = "switch2";
    public string swap1Layer = "swap1";
    public string swap2Layer = "swap2";

    private List<GameObject> deactivatedObjects = new List<GameObject>();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(carLayer))
        {
            if (gameObject.layer == LayerMask.NameToLayer(switch1Layer))
            {
                DeactivateObjectsInLayer(swap1Layer);
                DeactivateObjectsInLayer(switch1Layer);
                DeactivateObjectsInLayer(switch2Layer);
            }

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
                deactivatedObjects.Add(obj);
            }
        }
    }

    public void ReactivateDeactivatedLayers()
    {
        foreach (GameObject obj in deactivatedObjects)
        {
            obj.SetActive(true);
        }
        deactivatedObjects.Clear();
    }
}

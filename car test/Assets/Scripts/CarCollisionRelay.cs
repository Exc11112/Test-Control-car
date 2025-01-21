using System.Collections.Generic;
using UnityEngine;

public class CarCollisionRelay : MonoBehaviour
{
    public List<DriftScore2> driftScoreUIs = new List<DriftScore2>(); // List of DriftScore2 instances

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var driftScoreUI in driftScoreUIs)
        {
            if (driftScoreUI != null)
            {
                driftScoreUI.HandleCarCollision(collision); // Forward the collision to each DriftScore2
            }
        }
    }
}

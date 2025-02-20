using System.Collections.Generic;
using UnityEngine;

public class CarUIManager : MonoBehaviour
{
    public List<UITypeManager> uiTypeManagers = new List<UITypeManager>();

    public void RegisterUITypeManager(UITypeManager uiManager)
    {
        if (!uiTypeManagers.Contains(uiManager))
        {
            uiTypeManagers.Add(uiManager);
        }
    }

    public void UpdateDriftScore()
    {
        foreach (var manager in uiTypeManagers)
        {
            manager.UpdateDriftScore(); // Remove parameter
        }
    }
    public void ApplyWallPenalty()
    {
        foreach (var manager in uiTypeManagers)
        {
            manager.ApplyWallPenalty(); // Call the function for each UI type
        }
    }

}

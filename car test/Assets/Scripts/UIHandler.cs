using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public int UIType; // 1 = UI1, 2 = UI2, etc.
    public Text driftScoreText;
    public Text multiplierText;
    public Slider bar1;
    public Slider bar2;
    public Slider bar3;
    public Slider bar4;

    private void Start()
    {
        // Try to find CarUIManager in the parent (for UIs inside the car)
        CarUIManager carUIManager = GetComponentInParent<CarUIManager>();

        // If not found, search for the closest car
        if (carUIManager == null)
        {
            carUIManager = FindClosestCarUIManager();
        }

        // If still null, log an error and return
        if (carUIManager == null)
        {
            Debug.LogError("UIHandler: No CarUIManager found for UI object: " + gameObject.name, gameObject);
            return;
        }

        // Find the correct UITypeManager inside the Car
        UITypeManager[] allUIManagers = carUIManager.GetComponentsInChildren<UITypeManager>();
        UITypeManager uiTypeManager = null;

        foreach (var manager in allUIManagers)
        {
            if (manager.UIType == UIType)
            {
                uiTypeManager = manager;
                break;
            }
        }

        // If UITypeManager is missing, log an error and return
        if (uiTypeManager == null)
        {
            Debug.LogError($"UIHandler: No UITypeManager found with UIType {UIType} in {carUIManager.gameObject.name}", gameObject);
            return;
        }

        // Register UI elements
        uiTypeManager.RegisterUI(driftScoreText, multiplierText, bar1, bar2, bar3, bar4);
        carUIManager.RegisterUITypeManager(uiTypeManager);
    }

    // Finds the closest CarUIManager in the scene (for cameras outside the car)
    private CarUIManager FindClosestCarUIManager()
    {
        CarUIManager[] allCarUIs = FindObjectsOfType<CarUIManager>();
        CarUIManager closestCarUI = null;
        float minDistance = float.MaxValue;

        foreach (CarUIManager carUI in allCarUIs)
        {
            float distance = Vector3.Distance(transform.position, carUI.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCarUI = carUI;
            }
        }

        return closestCarUI;
    }
}

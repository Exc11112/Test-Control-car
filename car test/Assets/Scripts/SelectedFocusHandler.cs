using UnityEngine;

public class SelectedFocusHandler : MonoBehaviour
{
    void Start()
    {
        int selectedFocusIndex = PlayerPrefs.GetInt("SelectedFocusIndex", -1);

        if (selectedFocusIndex == -1)
        {
            Debug.LogError("No focus point selected!");
            return;
        }

        Debug.Log($"Selected focus index in new scene: {selectedFocusIndex}");
        // Use this index to configure the scene as needed
    }
}

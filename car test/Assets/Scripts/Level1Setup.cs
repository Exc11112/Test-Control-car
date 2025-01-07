using UnityEngine;

public class Level1Setup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Selected Car Index: " + SelectionData.SelectedCarIndex);
        Debug.Log("Selected Character Index: " + SelectionData.SelectedCharacterIndex);

        // Use these values to set up the scene (e.g., load the appropriate car and character).
    }
}

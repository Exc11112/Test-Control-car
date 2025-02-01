using UnityEngine;

public class Level1Setup : MonoBehaviour
{
    [Header("Debug Mode")]
    public bool debugMode = true; // Toggle for debug functionality

    [Header("Cars")]
    public GameObject[] cars;

    [Header("Characters")]
    public GameObject[] characters1; // UI elements for character index 0
    public GameObject[] characters2; // UI elements for character index 1

    [Header("Camera")]
    public DriftCamera driftCamera; // Reference to the DriftCamera in the scene

    void Start()
    {
        SetupScene();

        // Update the DriftCamera with the new active car
        if (driftCamera != null)
        {
            driftCamera.UpdateActiveCar();
        }
        else
        {
            Debug.LogError("DriftCamera reference is missing in Level1Setup.");
        }
    }

    void Update()
    {
        if (!debugMode) return; // Skip debug functionality if debugMode is false

        // Debug: Handle input for selecting cars
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectionData.SelectedCarIndex = 0;
            Debug.Log("Car Index set to 0");
            SetupScene();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectionData.SelectedCarIndex = 1;
            Debug.Log("Car Index set to 1");
            SetupScene();
        }

        // Debug: Handle input for selecting characters
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            SelectionData.SelectedCharacterIndex = 0;
            Debug.Log("Character Index set to 0");
            SetupScene();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            SelectionData.SelectedCharacterIndex = 1;
            Debug.Log("Character Index set to 1");
            SetupScene();
        }
    }

    private void SetupScene()
    {
        // Activate the correct car
        if (SelectionData.SelectedCarIndex >= 0 && SelectionData.SelectedCarIndex < cars.Length)
        {
            ActivateGameObject(cars, SelectionData.SelectedCarIndex);
        }

        // Activate the correct UI based on SelectedCharacterIndex
        if (SelectionData.SelectedCharacterIndex == 0)
        {
            ActivateAllGameObjects(characters1);
            DeactivateAllGameObjects(characters2);
        }
        else if (SelectionData.SelectedCharacterIndex == 1)
        {
            ActivateAllGameObjects(characters2);
            DeactivateAllGameObjects(characters1);
        }
    }

    private void ActivateGameObject(GameObject[] objects, int activeIndex)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(i == activeIndex);
            }
        }
    }

    private void ActivateAllGameObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void DeactivateAllGameObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}

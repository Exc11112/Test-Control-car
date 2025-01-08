using System.Collections;
using UnityEngine;

public class DriftCamera : MonoBehaviour
{
    public Transform lookAtTarget;
    public Transform positionTarget;
    public float smoothing = 20f;
    public KeyCode switchViewKey = KeyCode.V;

    public Camera mainCamera;
    public Camera secondCamera;
    public Camera thirdCamera;

    private int currentCameraIndex = 0;

    private void Start()
    {
        // Delay the setup to allow Level1Setup to finish activating objects
        StartCoroutine(DelayedSetup());
    }

    private IEnumerator DelayedSetup()
    {
        // Wait for one frame
        yield return null;

        // Update the active car after Level1Setup
        UpdateActiveCar();

        // Ensure the default camera is active
        SetActiveCamera(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchViewKey))
        {
            currentCameraIndex = (currentCameraIndex + 1) % 3; // Cycle through cameras
            SetActiveCamera(currentCameraIndex);
        }
    }

    public void UpdateActiveCar()
    {
        // Find the active car in the scene
        GameObject activeCar = null;
        foreach (GameObject car in FindObjectsOfType<GameObject>())
        {
            if (car.activeSelf && car.CompareTag("car"))
            {
                activeCar = car;
                break;
            }
        }

        if (activeCar == null)
        {
            Debug.LogError("No active car found!");
            return;
        }

        // Debug: Log the hierarchy of the active car
        Debug.Log("Active car found: " + activeCar.name);
        foreach (Transform child in activeCar.transform)
        {
            Debug.Log("Child object: " + child.name);
        }

        // Set camera targets
        lookAtTarget = activeCar.transform.Find("LookAtTarget");
        positionTarget = activeCar.transform.Find("PositionTarget");

        if (lookAtTarget == null || positionTarget == null)
        {
            Debug.LogError("LookAtTarget or PositionTarget is missing on the active car!");
        }
        else
        {
            Debug.Log("Camera targets successfully set for " + activeCar.name);
        }
    }


    private void UpdateCamera()
    {
        if (currentCameraIndex == 0 && lookAtTarget != null && positionTarget != null)
        {
            // Smoothly move and rotate the main camera
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, positionTarget.position, Time.deltaTime * smoothing);
            mainCamera.transform.LookAt(lookAtTarget);
        }
    }

    private void FixedUpdate()
    {
        UpdateCamera();
    }

    private void SetActiveCamera(int index)
    {
        mainCamera.enabled = (index == 0);
        secondCamera.enabled = (index == 1);
        thirdCamera.enabled = (index == 2);
    }
}

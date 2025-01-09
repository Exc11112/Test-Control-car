using System;
using UnityEngine;
using static DriftCamera;

public class DriftCamera : MonoBehaviour
{
    public float smoothing = 20f;
    public Transform lookAtTarget; // Target the camera looks at
    public Transform positionTarget; // Position the camera follows
    public Camera mainCamera; // The main camera, independent of cars
    private Camera secondCamera;
    private Camera thirdCamera;

    public AdvancedOptions advancedOptions;

    private int currentCameraIndex = 0;
    public KeyCode switchViewKey = KeyCode.V;
    [Serializable]
    public class AdvancedOptions
    {
        public bool updateCameraInUpdate;
        public bool updateCameraInFixedUpdate = true;
        public bool updateCameraInLateUpdate;
    }

        private void Start()
    {
        UpdateActiveCar(); // Initialize camera setup
    }

    private void FixedUpdate()
    {
        if (advancedOptions.updateCameraInFixedUpdate)
        {
            if (currentCameraIndex == 0 && mainCamera != null && positionTarget != null && lookAtTarget != null)
            {
                FollowCar();
            }
        }
    }

    private void Update()
    {
        // Switch camera when the key is pressed
        if (Input.GetKeyDown(switchViewKey))
        {
            CycleCamera();
        }

        // Make the main camera follow the car if it is active

    }

    private void FollowCar()
    {
        //// Smoothly move the camera to the position target
        //Vector3 targetPosition = positionTarget.position;
        //mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, smoothing * Time.deltaTime);

        //// Smoothly rotate the camera to look at the lookAtTarget
        //Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget.position - mainCamera.transform.position);
        //mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, smoothing * Time.deltaTime);
        //// Only update the main camera's position and rotation

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, positionTarget.position, Time.deltaTime * smoothing);
            mainCamera.transform.LookAt(lookAtTarget);
        
    }

    public void UpdateActiveCar()
    {
        // Ensure main camera is always assigned
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is missing!");
            return;
        }

        // Locate second and third cameras dynamically based on the active car
        if (lookAtTarget != null && positionTarget != null)
        {
            secondCamera = lookAtTarget.GetComponentInChildren<Camera>();
            thirdCamera = positionTarget.GetComponentInChildren<Camera>();
        }

        if (secondCamera == null || thirdCamera == null)
        {
            Debug.LogError("Second or Third Camera is missing on the active car!");
        }

        SetActiveCamera(0); // Start with the main camera
    }

    private void CycleCamera()
    {
        currentCameraIndex = (currentCameraIndex + 1) % 3; // Cycle between 0, 1, 2
        SetActiveCamera(currentCameraIndex);
    }

    private void SetActiveCamera(int index)
    {
        // Disable all cameras first
        if (mainCamera != null) mainCamera.enabled = false;
        if (secondCamera != null) secondCamera.enabled = false;
        if (thirdCamera != null) thirdCamera.enabled = false;

        // Enable the correct camera based on the index
        switch (index)
        {
            case 0:
                mainCamera.enabled = true;
                break;
            case 1:
                if (secondCamera != null) secondCamera.enabled = true;
                break;
            case 2:
                if (thirdCamera != null) thirdCamera.enabled = true;
                break;
        }
    }
}

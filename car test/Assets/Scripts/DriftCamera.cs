using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftCamera : MonoBehaviour
{
    [Serializable]
    public class AdvancedOptions
    {
        public bool updateCameraInUpdate;
        public bool updateCameraInFixedUpdate = true;
        public bool updateCameraInLateUpdate;
        public KeyCode switchViewKey = KeyCode.V;  // Press V to switch views
    }

    public float smoothing = 20f;
    public Transform lookAtTarget;
    public Transform positionTarget;
    public Camera mainCamera;  // The camera that moves and looks at the target
    public Camera secondCamera;  // Another camera that doesn't move dynamically
    public Camera thirdCamera;  // Another static camera
    public AdvancedOptions advancedOptions;

    private int currentCameraIndex;

    private void Start()
    {
        // Ensure that only the main camera is enabled at the start
        SetActiveCamera(0);
    }

    private void FixedUpdate()
    {
        if (advancedOptions.updateCameraInFixedUpdate)
            UpdateCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(advancedOptions.switchViewKey))
        {
            currentCameraIndex = (currentCameraIndex + 1) % 3;  // Cycle between 0, 1, and 2
            SetActiveCamera(currentCameraIndex);
        }

        if (advancedOptions.updateCameraInUpdate)
            UpdateCamera();
    }

    private void LateUpdate()
    {
        if (advancedOptions.updateCameraInLateUpdate)
            UpdateCamera();
    }

    private void UpdateCamera()
    {
        // Only update the main camera's position and rotation
        if (currentCameraIndex == 0)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, positionTarget.position, Time.deltaTime * smoothing);
            mainCamera.transform.LookAt(lookAtTarget);
        }
    }

    private void SetActiveCamera(int index)
    {
        // Enable only the selected camera
        mainCamera.enabled = (index == 0);
        secondCamera.enabled = (index == 1);
        thirdCamera.enabled = (index == 2);
    }
}
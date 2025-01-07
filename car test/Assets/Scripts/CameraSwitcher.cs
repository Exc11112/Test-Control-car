using UnityEngine;

public class CameraSwitcherUI : MonoBehaviour
{
    [Header("Focus Points")]
    public Transform[] focusPoints; // List of points to focus on

    [Header("Transition Settings")]
    public float transitionSpeed = 5f; // Speed of transition

    private int currentFocusIndex = 0; // Current focus point index
    private Transform targetFocus; // Target focus point

    // Public getter for currentFocusIndex to allow external access
    public int CurrentFocusIndex => currentFocusIndex;

    void Start()
    {
        if (focusPoints.Length == 0)
        {
            Debug.LogError("No focus points assigned!");
            return;
        }

        // Initialize the target focus to the first point
        targetFocus = focusPoints[currentFocusIndex];
    }

    void Update()
    {
        if (focusPoints.Length == 0) return;

        //// Check for the X key press
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    Debug.Log("SwitchFocus triggered with direction: 1");
        //    SwitchFocus(1); // Move to the next point
        //}

        // Smoothly move the camera towards the target focus
        SmoothTransition();
    }

    public void SwitchFocusLeft()
    {
        SwitchFocus(-1); // Move to the previous point
    }

    public void SwitchFocusRight()
    {
        SwitchFocus(1); // Move to the next point
    }

    private void SwitchFocus(int direction)
    {
        // Update the current focus index
        currentFocusIndex += direction;

        // Wrap around if out of bounds
        if (currentFocusIndex >= focusPoints.Length)
        {
            currentFocusIndex = 0;
        }
        else if (currentFocusIndex < 0)
        {
            currentFocusIndex = focusPoints.Length - 1;
        }

        // Set the new target focus
        targetFocus = focusPoints[currentFocusIndex];

        // Log the current focus index for debugging
        Debug.Log($"Switched focus to index: {currentFocusIndex}, Target: {targetFocus.name}");
    }

    private void SmoothTransition()
    {
        // Smoothly interpolate the camera position
        transform.position = Vector3.Lerp(transform.position, targetFocus.position, Time.deltaTime * transitionSpeed);

        // Smoothly interpolate the camera rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetFocus.rotation, Time.deltaTime * transitionSpeed);
    }
}

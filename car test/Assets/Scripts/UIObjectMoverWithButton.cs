using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIObjectMoverWithActivation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float startY = -1080f; // Starting Y position
    public float targetY = 0f;    // Target Y position
    public float duration = 1f;  // Time in seconds to complete the move
    public float delayAfterButtonPress = 1f; // Delay before moving back down

    [Header("UI Object Settings")]
    public GameObject uiToActivate; // UI GameObject to activate

    private RectTransform rectTransform; // Reference to RectTransform
    private float elapsedTime = 0f; // Tracks the animation time
    private bool isMoving = false; // Controls whether the animation is active

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("This script must be attached to a UI object with a RectTransform!");
            return;
        }

        if (uiToActivate != null)
        {
            uiToActivate.SetActive(false); // Deactivate the UI object at start
        }

        // Set the starting position
        Vector2 startPosition = rectTransform.anchoredPosition;
        startPosition.y = startY;
        rectTransform.anchoredPosition = startPosition;

        // Start moving up when the scene starts
        StartCoroutine(MoveToPosition(targetY));
    }

    public void OnButtonPressed()
    {
        // Wait for a delay and then move down
        StartCoroutine(HandleButtonPress());
    }

    private IEnumerator HandleButtonPress()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayAfterButtonPress);

        // Move back to the starting position
        yield return StartCoroutine(MoveToPosition(startY));

        // Reactivate the UI object after the movement finishes
        if (uiToActivate != null)
        {
            uiToActivate.SetActive(true);
        }
    }

    private IEnumerator MoveToPosition(float destinationY)
    {
        isMoving = true;
        elapsedTime = 0f;

        // Get the current position of the UI element
        Vector2 initialPosition = rectTransform.anchoredPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // Interpolate the position
            Vector2 newPosition = initialPosition;
            newPosition.y = Mathf.Lerp(initialPosition.y, destinationY, progress);
            rectTransform.anchoredPosition = newPosition;

            yield return null; // Wait for the next frame
        }

        // Ensure the final position is exactly the target
        Vector2 finalPosition = rectTransform.anchoredPosition;
        finalPosition.y = destinationY;
        rectTransform.anchoredPosition = finalPosition;

        isMoving = false;
    }
}

using UnityEngine;

public class SpriteVisibilityController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float delay = 10f; // Time in seconds to reactivate the sprite

    void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Check if SpriteRenderer is assigned
        if (spriteRenderer != null)
        {
            // Deactivate the sprite at the start
            spriteRenderer.enabled = false;

            // Reactivate the sprite after a delay
            Invoke("ActivateSprite", delay);
        }
        else
        {
            Debug.LogError("No SpriteRenderer component found on the GameObject.");
        }
    }

    void ActivateSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
}

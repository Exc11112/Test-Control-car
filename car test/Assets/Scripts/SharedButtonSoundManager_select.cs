using UnityEngine;
using UnityEngine.UI;

public class SharedButtonSoundManager : MonoBehaviour
{
    public static SharedButtonSoundManager Instance; // Singleton instance

    public AudioSource audioSource;  // Audio player
    public AudioClip[] soundClips;   // Array of button sounds
    private int currentClipIndex = 0; // Tracks the current sound

    [Range(0f, 1f)] public float buttonSoundVolume = 1.0f; // Volume control (0 = mute, 1 = max)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep instance across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayNextSound()
    {
        if (soundClips.Length == 0 || audioSource == null) return; // Do nothing if no sounds

        audioSource.volume = buttonSoundVolume; // Apply the volume setting
        audioSource.clip = soundClips[currentClipIndex]; // Set the next sound
        audioSource.Play(); // Play sound

        currentClipIndex = (currentClipIndex + 1) % soundClips.Length; // Loop through sounds
    }
}

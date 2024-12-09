using UnityEngine;

public class ToggleMusic : MonoBehaviour
{
    public AudioSource audioSource; // เชื่อมโยงกับ Audio Source
    private bool isPlaying = true;  // สถานะเพลง

    public void ToggleMusicState()
    {
        if (isPlaying)
        {
            audioSource.Pause(); // พักเพลง
            isPlaying = false;
        }
        else
        {
            audioSource.Play(); // เล่นเพลง
            isPlaying = true;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class AudioSlider : MonoBehaviour
{
    private static AudioSource[] audioSources;
    private static Slider[] soundSliders;
    private Slider slider;

    // Reset static variables when the runtime initializes (on scene load)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticVariables()
    {
        audioSources = null;
        soundSliders = null;
    }

    void Start()
    {
        slider = GetComponent<Slider>();

        if (slider == null)
        {
            Debug.LogError("AudioSlider script is attached to an object without a Slider component!");
            return;
        }

        // Re-fetch AudioSources if the array is null, empty, or contains null entries
        if (audioSources == null || audioSources.Length == 0 || audioSources.Any(a => a == null))
        {
            audioSources = FindObjectsOfType<AudioSource>();
        }

        // Re-fetch Sliders if the array is null, empty, or contains null entries
        if (soundSliders == null || soundSliders.Length == 0 || soundSliders.Any(s => s == null))
        {
            soundSliders = FindObjectsOfType<Slider>().Where(s => s.gameObject.name == "Sound Slider").ToArray();
        }

        if (audioSources.Length > 0)
        {
            slider.value = audioSources[0].volume;
        }

        slider.onValueChanged.AddListener(UpdateAllAudioVolumes);
    }

    void UpdateAllAudioVolumes(float volume)
    {
        foreach (AudioSource audio in audioSources)
        {
            if (audio != null)
            {
                audio.volume = volume;
            }
        }

        foreach (Slider soundSlider in soundSliders)
        {
            if (soundSlider != null && soundSlider != slider && soundSlider.value != volume)
            {
                soundSlider.value = volume;
            }
        }
    }
}
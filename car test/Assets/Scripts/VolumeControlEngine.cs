using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControlEngine : MonoBehaviour
{
    public AudioMixer audioMixer;
    public string exposedParameter = "Engine";
    public List<Slider> volumeSliders = new List<Slider>();

    private void Start()
    {
        float volume;
        audioMixer.GetFloat(exposedParameter, out volume);
        float linearVolume = Mathf.Pow(10, volume / 20f); // Convert from dB to 0-1

        // Set all sliders to the current volume
        foreach (Slider slider in volumeSliders)
        {
            slider.value = linearVolume;
            slider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat(exposedParameter, dB);

        // Update all sliders so they stay in sync
        foreach (Slider slider in volumeSliders)
        {
            if (slider.value != value)
                slider.value = value;
        }
    }
}

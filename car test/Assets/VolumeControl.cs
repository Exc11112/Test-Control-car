using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public AudioMixer audioMixer; // Drag your AudioMixer here
    public Slider volumeSlider;   // Drag the UI slider

    void Start()
    {
        float volume;
        audioMixer.GetFloat("SpeechVolume", out volume);
        volumeSlider.value = Mathf.Pow(10, volume / 20f); // Convert from dB to 0-1
    }

    public void SetSpeechVolume(float volume)
    {
        float dB = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        audioMixer.SetFloat("SpeechVolume", dB);
    }
}


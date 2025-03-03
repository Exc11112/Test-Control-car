using UnityEngine;

public class CarSoundsmini : MonoBehaviour
{
    public float minPitch = 0.8f;
    public float maxPitch = 2.0f;
    public float engineVolume = 0.5f;
    public float bgmVolume = 1.0f;

    private AudioSource engineAudio;
    private AudioSource bgmAudio; // New AudioSource for BGM
    private AudioSource driftAudio;
    private CarController2 carController;

    public AudioClip driftClip; // Assign drift sound in Inspector
    public float driftVolume = 0.5f; // Adjustable drift sound volume (default = 50%)

    void Start()
    {
        carController = GetComponent<CarController2>();
        engineAudio = GetComponent<AudioSource>();

        if (engineAudio == null)
        {
            Debug.LogError("No AudioSource found on the car!");
        }

        // Create a separate AudioSource for BGM
        bgmAudio = gameObject.AddComponent<AudioSource>();
        bgmAudio.loop = true;
        bgmAudio.playOnAwake = false;
        bgmAudio.volume = bgmVolume;

        // Assign BGM from CarController2
        if (carController.Bgm.Length > 0 && SelectionData.SelectedCharacterIndex < carController.Bgm.Length)
        {
            bgmAudio.clip = carController.Bgm[SelectionData.SelectedCharacterIndex];
            bgmAudio.Play();
        }

        // Create a separate AudioSource for drifting
        driftAudio = gameObject.AddComponent<AudioSource>();
        driftAudio.clip = driftClip;
        driftAudio.loop = true;
        driftAudio.playOnAwake = false;
        driftAudio.volume = driftVolume;

        engineAudio.volume = engineVolume;
    }


    void Update()
    {
        EngineSound();
        HandleDriftingSound();
        bgmAudio.volume = bgmVolume; // Update BGM volume separately
    }

    void EngineSound()
    {
        if (carController == null || engineAudio == null) return;

        float rpmNormalized = Mathf.InverseLerp(carController.minRPM, carController.maxRPM, carController.currentRPM);
        engineAudio.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
    }

    void HandleDriftingSound()
    {
        if (carController == null || driftAudio == null || driftClip == null) return;

        driftAudio.volume = driftVolume; // Update volume dynamically

        if (carController.isDrifting)
        {
            if (!driftAudio.isPlaying)
            {
                driftAudio.Play();
            }
        }
        else
        {
            if (driftAudio.isPlaying)
            {
                driftAudio.Stop();
            }
        }
    }
}

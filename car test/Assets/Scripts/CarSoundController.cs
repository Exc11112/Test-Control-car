using UnityEngine;

public class CarSoundController : MonoBehaviour
{
    public AudioSource engineSound;
    public AudioClip shiftUpClip;
    public AudioClip shiftDownClip;
    public AudioClip driftClip;
    public AudioClip idleClip;

    private CarController2 carController;
    private float basePitch = 1f;
    private float maxPitch = 2f;
    private float minPitch = 0.5f;
    private bool isDriftingPlaying = false;
    private bool isIdlePlaying = false;

    void Start()
    {
        carController = GetComponent<CarController2>();

        if (engineSound == null)
        {
            Debug.LogError("Engine Sound AudioSource is not assigned!");
        }
    }

    void Update()
    {
        UpdateEngineSound();
        HandleShiftingSounds();
        HandleDriftingSound();
        HandleIdleSound();
    }

    void UpdateEngineSound()
    {
        if (engineSound == null) return;

        float rpmNormalized = Mathf.InverseLerp(carController.minRPM, carController.maxRPM, carController.currentRPM);
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
    }

    void HandleShiftingSounds()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && carController.currentGear < carController.gearRatios.Length - 1)
        {
            PlayShiftSound(shiftUpClip);
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && carController.currentGear > 0)
        {
            PlayShiftSound(shiftDownClip);
        }
    }

    void HandleDriftingSound()
    {
        if (carController.isDrifting && !isDriftingPlaying)
        {
            engineSound.PlayOneShot(driftClip);
            isDriftingPlaying = true;
        }
        else if (!carController.isDrifting)
        {
            isDriftingPlaying = false;
        }
    }

    void HandleIdleSound()
    {
        if (carController.currentSpeed == 0 && !Input.anyKey && !isIdlePlaying)
        {
            engineSound.PlayOneShot(idleClip);
            isIdlePlaying = true;
        }
        else if (carController.currentSpeed > 0 || Input.anyKey)
        {
            isIdlePlaying = false;
        }
    }

    void PlayShiftSound(AudioClip clip)
    {
        if (clip != null)
        {
            engineSound.PlayOneShot(clip);
        }
    }
}

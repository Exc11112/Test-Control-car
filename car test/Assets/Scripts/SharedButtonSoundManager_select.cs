using UnityEngine;
using UnityEngine.UI;

public class SharedButtonSoundManager : MonoBehaviour
{
    public static SharedButtonSoundManager Instance; // Singleton สำหรับแชร์ตัวจัดการเสียง

    public AudioSource audioSource;  // ตัวเล่นเสียง
    public AudioClip[] soundClips;   // Array เก็บเสียง
    private int currentClipIndex = 0; // ตัวนับว่าตอนนี้ถึงเสียงที่เท่าไหร่

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayNextSound()
    {
        if (soundClips.Length == 0) return; // ถ้าไม่มีเสียง ไม่ต้องทำอะไร

        audioSource.clip = soundClips[currentClipIndex]; // ตั้งเสียงที่ต้องเล่น
        audioSource.Play(); // เล่นเสียง

        currentClipIndex = (currentClipIndex + 1) % soundClips.Length; // เปลี่ยนไปเสียงถัดไป (วนลูป)
    }
}

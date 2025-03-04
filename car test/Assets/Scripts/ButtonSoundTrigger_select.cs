using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundTrigger : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySharedSound);
    }

    void PlaySharedSound()
    {
        if (SharedButtonSoundManager.Instance != null)
        {
            SharedButtonSoundManager.Instance.PlayNextSound();
        }
    }
}

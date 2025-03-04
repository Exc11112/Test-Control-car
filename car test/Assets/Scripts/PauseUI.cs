using UnityEngine;
using System.Linq;

public class PauseUI : MonoBehaviour
{
    private GameObject[] stopUIObjects;
    private bool isPaused = false;

    void Start()
    {
        // Find all GameObjects named "StopUI" in the scene
        stopUIObjects = FindObjectsOfType<GameObject>().Where(obj => obj.name == "StopUI").ToArray();

        // Deactivate them initially
        SetObjectsActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        SetObjectsActive(isPaused);

        // Pause or Resume the game
        Time.timeScale = isPaused ? 0 : 1;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void SetObjectsActive(bool state)
    {
        foreach (GameObject obj in stopUIObjects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }
    public void ResumeGame()
    {
        TogglePause();
    }
}

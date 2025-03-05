using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Check the stored day value and load the corresponding scene
        int day = PlayerPrefs.GetInt("day", 1); // Default to 1 if not set

        switch (day)
        {
            case 1:
                SceneManager.LoadScene("Level1");
                break;
            case 2:
                SceneManager.LoadScene("Level2");
                break;
            case 3:
                SceneManager.LoadScene("Level3");
                break;
            default:
                Debug.LogWarning("Invalid day value, loading Level1 as default.");
                SceneManager.LoadScene("Level1");
                break;
        }
    }

    public void ExitGame()
    {
        Debug.Log("Game Exited");
        Application.Quit();
    }

    public void SelectScene()
    {
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1")
        {
            PlayerPrefs.SetInt("day", 2);
            PlayerPrefs.Save();
        }
        else if (currentScene == "Level2")
        {
            PlayerPrefs.SetInt("day", 3);
            PlayerPrefs.Save();
        }
        else if (currentScene == "Level3")
        {
            PlayerPrefs.SetInt("day", 1);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("Select");
    }


    public void BlackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("start_manu");
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

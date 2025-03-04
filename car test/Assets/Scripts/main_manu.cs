using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load Scene ชื่อ "GameScene"
        SceneManager.LoadScene("Level1");
    }

    public void ExitGame()
    {
        // ปิดโปรแกรม
        Debug.Log("Game Exited");
        Application.Quit();
    }

    public void SelectScene()
    {
        SceneManager.LoadScene("Select");
    }

    public void BlackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("start_manu");
    }

    public void ReloadScene()
    {
        // Reload the current active scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

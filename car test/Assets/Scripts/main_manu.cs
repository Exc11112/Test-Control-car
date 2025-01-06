using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load Scene ชื่อ "GameScene"
        SceneManager.LoadScene("Level1");
    }

    public void OpenOptions()
    {
        // เปิดเมนู Options
        Debug.Log("Options Menu Opened");
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
}

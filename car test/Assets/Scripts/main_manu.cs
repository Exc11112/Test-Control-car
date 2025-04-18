using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool isCutscene = false;
    public void StartGame()
    {
        int day = PlayerPrefs.GetInt("day", 1); // Read the stored day

        Debug.Log("Starting Game - Day: " + day); // Debug to check value

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
            DetermineMostChosenCharacter();
            return; // Exit function early to avoid loading "Select"
        }

        SceneManager.LoadScene("Select");
    }

    public void ToCutscene()
    {
        if (!isCutscene)
        {
            isCutscene = true;  // Mark as having seen the cutscene
            SceneManager.LoadScene("Cutscene");  // Load the cutscene
            return;  // Stop the rest of the function
        }

        SelectScene();  // If cutscene already seen, go directly to select scene
    }
    public void BlackToMenu()
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
            DetermineMostChosenCharacter();
            return; // Exit function early to avoid loading "Select"
        }
        SceneManager.LoadScene("start_manu");
    }
    public void BlackToMenuButReset()
    {
        // Reset character selection counts
        PlayerPrefs.SetInt("Character_0", 0);
        PlayerPrefs.SetInt("Character_1", 0);
        PlayerPrefs.SetInt("Character_2", 0);

        // Reset the most chosen character
        PlayerPrefs.SetInt("MostChosenCharacter", 0);

        // Save changes
        PlayerPrefs.Save();

        Debug.Log("All character selection data has been reset.");

        // Return to main menu
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("day", 1);
        SceneManager.LoadScene("start_manu");
    }
    public void BlackToMenuButStopUI()
    {
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;

        // Keep the day set to the current level
        if (currentScene == "Level1")
        {
            PlayerPrefs.SetInt("day", 1);
        }
        else if (currentScene == "Level2")
        {
            PlayerPrefs.SetInt("day", 2);
        }
        else if (currentScene == "Level3")
        {
            PlayerPrefs.SetInt("day", 3);
        }

        // Undo the last character selection
        CharacterSelector.UndoLastCharacterSelection();

        PlayerPrefs.Save();

        // Go back to main menu
        SceneManager.LoadScene("start_manu");
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void DetermineMostChosenCharacter()
    {
        // Get the selection counts for all three characters
        int char1Count = PlayerPrefs.GetInt("Character_0", 0);
        int char2Count = PlayerPrefs.GetInt("Character_1", 0);
        int char3Count = PlayerPrefs.GetInt("Character_2", 0);

        Debug.Log("Character 1 Count: " + char1Count);
        Debug.Log("Character 2 Count: " + char2Count);
        Debug.Log("Character 3 Count: " + char3Count);

        int selectedCharacter = 0; // Default to character 1 (index 0)

        // Find the highest count
        int maxCount = Mathf.Max(char1Count, char2Count, char3Count);

        // Check for ties
        bool isChar1Max = (char1Count == maxCount);
        bool isChar2Max = (char2Count == maxCount);
        bool isChar3Max = (char3Count == maxCount);

        int[] possibleWinners = new int[3];
        int winnerCount = 0;

        if (isChar1Max) possibleWinners[winnerCount++] = 0;
        if (isChar2Max) possibleWinners[winnerCount++] = 1;
        if (isChar3Max) possibleWinners[winnerCount++] = 2;

        // Randomly choose if there's a tie
        if (winnerCount > 1)
        {
            selectedCharacter = possibleWinners[Random.Range(0, winnerCount)];
        }
        else
        {
            selectedCharacter = possibleWinners[0]; // Only one winner
        }

        PlayerPrefs.SetInt("MostChosenCharacter", selectedCharacter);

        string galleryKey = selectedCharacter switch
        {
            0 => "Character1_Revealed",
            1 => "Character2_Revealed",
            2 => "Character3_Revealed",
            _ => throw new System.ArgumentOutOfRangeException()
        };

        string revealedData = PlayerPrefs.GetString(galleryKey, "00000");
        char[] dataArray = revealedData.ToCharArray();

        // Ensure index 3 exists (for the 4th image)
        if (dataArray.Length > 3)
        {
            dataArray[3] = '1'; // Set index 3 to uncovered
            PlayerPrefs.SetString(galleryKey, new string(dataArray));
        }

        PlayerPrefs.Save();

        SceneManager.LoadScene("EndScene");
    }
}

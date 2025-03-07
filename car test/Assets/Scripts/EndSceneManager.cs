using UnityEngine;

public class EndSceneManager : MonoBehaviour
{
    public GameObject character1Ending;
    public GameObject character2Ending;
    public GameObject character3Ending;

    void Start()
    {
        int mostChosenCharacter = PlayerPrefs.GetInt("MostChosenCharacter", 0);

        // Deactivate all endings initially
        character1Ending.SetActive(false);
        character2Ending.SetActive(false);
        character3Ending.SetActive(false);

        // Activate the corresponding GameObject
        if (mostChosenCharacter == 0)
        {
            character1Ending.SetActive(true);
        }
        else if (mostChosenCharacter == 1)
        {
            character2Ending.SetActive(true);
        }
        else
        {
            character3Ending.SetActive(true);
        }
    }
}

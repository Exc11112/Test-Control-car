using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public int SelectedCharacterIndex { get; private set; } = 0;

    public void SetCharacterSelection(int characterIndex)
    {
        SelectedCharacterIndex = characterIndex;
        Debug.Log("Character selected: " + SelectedCharacterIndex);

        // Increase the selection count for the chosen character
        string key = "Character_" + characterIndex;
        int count = PlayerPrefs.GetInt(key, 0);
        PlayerPrefs.SetInt(key, count + 1);

        // Mark this character as the most recent selection
        PlayerPrefs.SetInt("MostChosenCharacter", characterIndex);

        // Save both the new count and most recent selection
        PlayerPrefs.Save();
    }
    public static void UndoLastCharacterSelection()
    {
        int mostRecentChar = PlayerPrefs.GetInt("MostChosenCharacter", -1);
        if (mostRecentChar != -1)  // Check if a valid character was selected
        {
            string key = "Character_" + mostRecentChar;
            int count = PlayerPrefs.GetInt(key, 0);

            // Subtract 1 but don't let it drop below -1
            PlayerPrefs.SetInt(key, Mathf.Max(-1, count - 1));

            Debug.Log("Undo selection for Character_" + mostRecentChar + ". New count: " + Mathf.Max(-1, count - 1));
        }

        // Reset the marker so the last choice is cleared
        PlayerPrefs.SetInt("MostChosenCharacter", 0);
        PlayerPrefs.Save();
    }
}
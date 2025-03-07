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
        PlayerPrefs.Save();
    }
}
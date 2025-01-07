using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public int SelectedCharacterIndex { get; private set; } = 0;

    public void SetCharacterSelection(int characterIndex)
    {
        SelectedCharacterIndex = characterIndex;
        Debug.Log("Character selected: " + SelectedCharacterIndex);
    }
}

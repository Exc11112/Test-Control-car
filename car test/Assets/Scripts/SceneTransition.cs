using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public CameraSwitcherUI cameraSwitcher;
    public CharacterSelector characterSelector;

    public void ConfirmSelectionAndTransfer()
    {
        // Save selections
        SelectionData.SelectedCarIndex = cameraSwitcher.CurrentFocusIndex;
        SelectionData.SelectedCharacterIndex = characterSelector.SelectedCharacterIndex;

        // Log for confirmation
        Debug.Log($"Car: {SelectionData.SelectedCarIndex}, Character: {SelectionData.SelectedCharacterIndex}");

        // Load the next scene
        SceneManager.LoadScene("Level1");
    }
}

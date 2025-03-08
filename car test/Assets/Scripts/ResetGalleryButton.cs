using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetGalleryButton : MonoBehaviour
{
    public Text warningText; // Assign this in the Inspector
    private bool isWarningActive = false;

    public void ResetGallery()
    {
        if (isWarningActive)
        {
            // Second click within 7 seconds -> Reset the day
            PlayerPrefs.SetString("Character1_Revealed", "00000");
            PlayerPrefs.SetString("Character2_Revealed", "00000");
            PlayerPrefs.SetString("Character3_Revealed", "00000");

            PlayerPrefs.Save();
            Debug.Log("Gallery has been reset.");
            warningText.text = "Gallery reset";
            StartCoroutine(ClearWarningAfter(2f)); // Clear message after 2 seconds
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // First click -> Show warning
            isWarningActive = true;
            warningText.text = "Are you sure?\nThis will reset all gallery you have collected.\n(Click again to reset)";
            warningText.gameObject.SetActive(true);

            // Start countdown to remove the warning after 7 seconds
            StartCoroutine(WarningCountdown());
        }
    }
    private IEnumerator WarningCountdown()
    {
        yield return new WaitForSeconds(7f);
        isWarningActive = false;
        warningText.gameObject.SetActive(false);
    }

    private IEnumerator ClearWarningAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        warningText.gameObject.SetActive(false);
    }
}

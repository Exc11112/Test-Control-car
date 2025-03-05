using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ResetDayButton : MonoBehaviour
{
    public Text warningText; // Assign this in the Inspector
    private bool isWarningActive = false;

    public void OnResetButtonClick()
    {
        if (isWarningActive)
        {
            // Second click within 7 seconds -> Reset the day
            PlayerPrefs.SetInt("day", 1);
            PlayerPrefs.Save();
            warningText.text = "Days reset to 1.";
            StartCoroutine(ClearWarningAfter(2f)); // Clear message after 2 seconds
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // First click -> Show warning
            isWarningActive = true;
            warningText.text = "Are you sure?\nThis will reset the days you have played.\n(Click again to reset)";
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
